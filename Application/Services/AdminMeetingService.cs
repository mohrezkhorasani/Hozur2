using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interface.IInfrastructure;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IAdminMeetingService
    {
        bool IsSuperAdmin(UserTBL user);
        Task<bool> CanUserAccessMeetingsPageAsync(UserTBL user);
        Task<bool> CanUserEditMeetingAsync(UserTBL user, long meetingId);
        Task<AdminMeetingStatsDTO> GetMeetingStatsAsync(UserTBL user, int? type = null, string q = "", long? ownerId = null, long? teacherId = null);
        Task<AdminMeetingsResultDTO> GetMeetingsAsync(UserTBL user, int page = 1, int count = 15, int? type = null, string q = "", long? ownerId = null, long? teacherId = null);
        Task<MeetingRecordingsResponseDTO> GetMeetingRecordingsAsync(long meetingId, UserTBL user);
        Task<MeetingRecordingSettingsDTO> UpdateRecordingSettingsAsync(UpdateRecordingSettingsRequestDTO req, UserTBL user);
        Task<bool> PublishRecordingAsync(PublishRecordingRequestDTO req, UserTBL user);
        Task<bool> DeleteRecordingAsync(DeleteRecordingRequestDTO req, UserTBL user);
        Task<string> GetRecordingTracksAsync(string recordId, UserTBL user);
    }

    public class AdminMeetingService : IAdminMeetingService
    {
        private readonly IDBContext _db;
        private readonly IBBBGetMeetings? _bbb;

        public AdminMeetingService(IDBContext db, IBBBGetMeetings? bbb = null)
        {
            _db = db;
            _bbb = bbb;
        }

        public bool IsSuperAdmin(UserTBL user)
        {
            if (user == null) return false;
            return user.Type == MainEntity_Types.USER_ADMINSTRATOR ||
                   user.VerifyUserPermissions("manage_admins");
        }

        public Task<bool> CanUserAccessMeetingsPageAsync(UserTBL user)
        {
            if (user == null) return Task.FromResult(false);
            // Any authenticated user can access the page; their meeting list will be restricted to what they own/admin
            return Task.FromResult(true);
        }

        public async Task<bool> CanUserEditMeetingAsync(UserTBL user, long meetingId)
        {
            if (user == null || meetingId <= 0) return false;

            if (IsSuperAdmin(user)) return true;

            var permissions = user.GetUserPermissions();
            if (permissions.Contains("create_rooms") ||
                user.Type == MainEntity_Types.USER_MODERATOR ||
                user.Type == MainEntity_Types.USER_ADMIN)
            {
                return true;
            }

            var meeting = await _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                .FirstOrDefaultAsync(m => m.ID == meetingId);

            if (meeting == null) return false;

            // Check if user is owner of the room
            if (meeting.RoomTBL != null && meeting.RoomTBL.IDOwnerTBL == user.ID) return true;

            // Check if user is room admin in RoomTBLToUserTBL
            if (await _db.RoomTBLToUserTBLs.AnyAsync(r =>
                (r.FriendlyID == meeting.RoomFriendlyID || r.FriendlyIDRoomTBL == meeting.RoomFriendlyID || (meeting.RoomTBL != null && r.FriendlyID == meeting.RoomTBL.FriendlyID)) &&
                (r.UUIDParticipatorTBL == user.UUID || (r.ParticipatorTBL != null && r.ParticipatorTBL.ID == user.ID)) &&
                (r.Type == MainEntity_Types.ROOMTOUSER_ADMIN) &&
                r.Status == MainEntity_Status.Succedd))
            {
                return true;
            }

            return false;
        }

        public IQueryable<MeetingTBL> BuildScopedQuery(UserTBL user, int? type = null, string q = "", long? ownerId = null, long? teacherId = null)
        {
            IQueryable<MeetingTBL> query = _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                    .ThenInclude(r => r.OwnerTBL)
                .Include(m => m.Teacher)
                .Include(m => m.Creator)
                .Include(m => m.UserTBLToMeetingTBLs);

            // Scope restriction: Non-superadmin users only see meetings in rooms they own or admin
            if (!IsSuperAdmin(user))
            {
                var adminRoomFids = _db.RoomTBLToUserTBLs
                    .Where(r =>
                        (r.UUIDParticipatorTBL == user.UUID || (r.ParticipatorTBL != null && r.ParticipatorTBL.ID == user.ID)) &&
                        (r.Type == MainEntity_Types.ROOMTOUSER_ADMIN) &&
                        r.Status == MainEntity_Status.Succedd)
                    .Select(r => r.FriendlyID)
                    .Distinct();

                query = query.Where(m =>
                    (m.RoomTBL != null && m.RoomTBL.IDOwnerTBL == user.ID) ||
                    adminRoomFids.Contains(m.RoomFriendlyID) ||
                    (m.RoomTBL != null && adminRoomFids.Contains(m.RoomTBL.FriendlyID)));
            }

            // Filters
            if (type.HasValue && type.Value > 0)
            {
                query = query.Where(m => (int)m.Type == type.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(m =>
                    (m.Title != null && m.Title.Contains(term)) ||
                    (m.MeetID != null && m.MeetID.Contains(term)) ||
                    (m.RoomFriendlyID != null && m.RoomFriendlyID.Contains(term)) ||
                    (m.RoomTBL != null && m.RoomTBL.Title != null && m.RoomTBL.Title.Contains(term)) ||
                    (m.Teacher != null && ((m.Teacher.Firstname + " " + m.Teacher.LastName).Contains(term) || (m.Teacher.Username != null && m.Teacher.Username.Contains(term)))) ||
                    (m.RoomTBL != null && m.RoomTBL.OwnerTBL != null && ((m.RoomTBL.OwnerTBL.Firstname + " " + m.RoomTBL.OwnerTBL.LastName).Contains(term) || (m.RoomTBL.OwnerTBL.Username != null && m.RoomTBL.OwnerTBL.Username.Contains(term)))));
            }

            if (ownerId.HasValue && ownerId.Value > 0)
            {
                query = query.Where(m => m.RoomTBL != null && m.RoomTBL.IDOwnerTBL == ownerId.Value);
            }

            if (teacherId.HasValue && teacherId.Value > 0)
            {
                query = query.Where(m => m.IDTeacher == teacherId.Value);
            }

            return query;
        }

        public async Task<AdminMeetingStatsDTO> GetMeetingStatsAsync(UserTBL user, int? type = null, string q = "", long? ownerId = null, long? teacherId = null)
        {
            var query = BuildScopedQuery(user, type, q, ownerId, teacherId);

            var totalMeetings = await query.CountAsync();

            var withVoiceCount = await query.CountAsync(m => m.MeetingLogTBLs.Any(l =>
                (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL") &&
                !string.IsNullOrEmpty(l.Value) &&
                l.Status != MainEntity_Status.LOG_REMOVED));

            var withVideoCount = await query.CountAsync(m => m.MeetingLogTBLs.Any(l =>
                (l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL") &&
                !string.IsNullOrEmpty(l.Value) &&
                l.Status != MainEntity_Status.LOG_REMOVED));

            var withReportCount = await query.CountAsync(m => m.MeetingLogTBLs.Any(l =>
                (l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL") &&
                !string.IsNullOrEmpty(l.Value) &&
                l.Status != MainEntity_Status.LOG_REMOVED));

            return new AdminMeetingStatsDTO
            {
                TotalMeetings = totalMeetings,
                WithVoiceCount = withVoiceCount,
                WithVideoCount = withVideoCount,
                WithReportCount = withReportCount
            };
        }

        public async Task<AdminMeetingsResultDTO> GetMeetingsAsync(UserTBL user, int page = 1, int count = 15, int? type = null, string q = "", long? ownerId = null, long? teacherId = null)
        {
            if (page < 1) page = 1;
            if (count < 1) count = 15;
            if (count > 100) count = 100;

            var query = BuildScopedQuery(user, type, q, ownerId, teacherId);

            var stats = await GetMeetingStatsAsync(user, type, q, ownerId, teacherId);
            var totalCount = stats.TotalMeetings;
            var totalPages = (int)Math.Ceiling((double)totalCount / count);

            var meetings = await query
                .OrderByDescending(e => e.DateInsert)
                .Skip((page - 1) * count)
                .Take(count)
                .Select(m => new
                {
                    id = m.ID,
                    meetID = m.MeetID,
                    title = m.Title,
                    type = (int)m.Type,
                    status = (int)m.Status,
                    statusTitle = MeetingTBL.GetStatusString(m.Status),
                    dateInsert = m.DateInsert,
                    endDate = m.EndDate,
                    jalaliDate = m.JalaliDate,
                    isRecorded = m.IsRecorded,
                    explain = m.Explain,
                    roomFriendlyID = m.RoomFriendlyID,
                    roomTitle = m.RoomTBL != null ? m.RoomTBL.Title : null,
                    bbbMeetID = m.RoomTBL != null ? m.RoomTBL.BBB_MeetID : null,
                    owner = m.RoomTBL != null && m.RoomTBL.OwnerTBL != null ? new AdminMeetingOwnerDTO
                    {
                        Id = m.RoomTBL.OwnerTBL.ID,
                        Name = m.RoomTBL.OwnerTBL.GetName(),
                        Username = m.RoomTBL.OwnerTBL.Username,
                        Avatar = m.RoomTBL.OwnerTBL.AvatarPath
                    } : null,
                    teacher = m.Teacher != null ? new AdminMeetingTeacherDTO
                    {
                        Id = m.Teacher.ID,
                        Name = m.Teacher.GetName(),
                        Avatar = m.Teacher.AvatarPath
                    } : null,
                    participantsCount = m.UserTBLToMeetingTBLs.Count(u =>
                        !u.IsRemoved &&
                        u.Type != MainEntity_Types.USER_TEACHER &&
                        u.Type != MainEntity_Types.USER_DABIR &&
                        u.Type != MainEntity_Types.USER_ADMINSTRATOR &&
                        u.Type != MainEntity_Types.USER_MODERATOR)
                })
                .ToListAsync();

            var meetIds = meetings.Select(m => m.id).ToList();
            var mediaLogs = await _db.MeetingLogTBLs
                .Where(l => meetIds.Contains(l.IDMeetingTBL) &&
                       (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL" ||
                        l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL" ||
                        l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL" ||
                        l.Key == MeetingLogTBL.KEY_BBB_RECORD || l.Key == "bbb_record_settings") &&
                       l.Status != MainEntity_Status.LOG_REMOVED)
                .OrderByDescending(l => l.ID)
                .ToListAsync();

            var items = meetings.Select(m =>
            {
                var voiceLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL"));
                var videoLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL"));
                var reportLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"));
                var recordLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_BBB_RECORD || l.Key == "bbb_record_settings"));

                string? recPlaybackUrl = null;
                string? recRecordId = null;
                string? recViewPermission = null;
                bool? recDownloadable = null;
                bool? recPublished = null;

                if (recordLog != null && !string.IsNullOrWhiteSpace(recordLog.Value))
                {
                    try
                    {
                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(recordLog.Value);
                        var root = jsonDoc.RootElement;
                        if (root.TryGetProperty("playbackUrl", out var pUrl)) recPlaybackUrl = pUrl.GetString();
                        if (root.TryGetProperty("recordId", out var rId)) recRecordId = rId.GetString();
                        if (root.TryGetProperty("viewPermission", out var vPerm)) recViewPermission = vPerm.GetString();
                        if (root.TryGetProperty("downloadable", out var dL)) recDownloadable = dL.GetBoolean();
                        if (root.TryGetProperty("published", out var pub)) recPublished = pub.GetBoolean();
                    }
                    catch
                    {
                        recPlaybackUrl = recordLog.Value;
                        recDownloadable = recordLog.Type == MainEntity_Types.MEETLOG_DOWNLOADABLE;
                    }
                }

                return new AdminMeetingItemDTO
                {
                    Id = m.id,
                    MeetID = m.meetID,
                    Title = m.title,
                    Type = m.type,
                    Status = m.status,
                    StatusTitle = m.statusTitle,
                    DateInsert = m.dateInsert,
                    EndDate = m.endDate,
                    JalaliDate = m.jalaliDate,
                    IsRecorded = m.isRecorded,
                    Explain = m.explain,
                    RoomFriendlyID = m.roomFriendlyID,
                    RoomTitle = m.roomTitle,
                    BbbMeetID = m.bbbMeetID,
                    Owner = m.owner,
                    Teacher = m.teacher,
                    ParticipantsCount = m.participantsCount,
                    VoiceUrl = voiceLog?.Value,
                    VoiceType = voiceLog != null ? (int)voiceLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    VoiceLogId = voiceLog?.ID,
                    VoiceExplain = voiceLog?.Explain,
                    VideoUrl = videoLog?.Value,
                    VideoType = videoLog != null ? (int)videoLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    VideoLogId = videoLog?.ID,
                    VideoExplain = videoLog?.Explain,
                    ReportUrl = reportLog?.Value,
                    ReportType = reportLog != null ? (int)reportLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    ReportLogId = reportLog?.ID,
                    ReportExplain = reportLog?.Explain,
                    RecordingPlaybackUrl = recPlaybackUrl,
                    RecordingRecordId = recRecordId,
                    RecordingViewPermission = recViewPermission,
                    RecordingDownloadable = recDownloadable,
                    RecordingPublished = recPublished
                };
            }).ToList();

            return new AdminMeetingsResultDTO
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = page,
                Count = count,
                Stats = stats
            };
        }

        public async Task<MeetingRecordingsResponseDTO> GetMeetingRecordingsAsync(long meetingId, UserTBL user)
        {
            if (meetingId <= 0)
                throw new ArgumentException("شناسه جلسه نامعتبر است.");

            bool canAccess = await CanUserEditMeetingAsync(user, meetingId);
            if (!canAccess)
                throw new UnauthorizedAccessException("شما دسترسی مشاهده و مدیریت رکورد این جلسه را ندارید.");

            var meeting = await _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                .FirstOrDefaultAsync(m => m.ID == meetingId);

            if (meeting == null)
                throw new KeyNotFoundException("جلسه مورد نظر یافت نشد.");

            var response = new MeetingRecordingsResponseDTO
            {
                MeetingId = meeting.ID,
                MeetId = meeting.MeetID,
                RoomFriendlyId = meeting.RoomFriendlyID,
                RoomTitle = meeting.RoomTBL?.Title,
                BbbMeetId = meeting.RoomTBL?.BBB_MeetID
            };

            // 1. Check DB settings log
            var recordLog = await _db.MeetingLogTBLs
                .Where(l => l.IDMeetingTBL == meetingId && (l.Key == MeetingLogTBL.KEY_BBB_RECORD || l.Key == "bbb_record_settings") && l.Status != MainEntity_Status.LOG_REMOVED)
                .OrderByDescending(l => l.ID)
                .FirstOrDefaultAsync();

            string? savedRecordId = null;
            if (recordLog != null && !string.IsNullOrWhiteSpace(recordLog.Value))
            {
                try
                {
                    using var jsonDoc = System.Text.Json.JsonDocument.Parse(recordLog.Value);
                    var root = jsonDoc.RootElement;
                    if (root.TryGetProperty("recordId", out var rId)) savedRecordId = rId.GetString();
                    if (root.TryGetProperty("viewPermission", out var vPerm)) response.Settings.ViewPermission = vPerm.GetString() ?? "all";
                    if (root.TryGetProperty("downloadable", out var dL)) response.Settings.IsDownloadable = dL.GetBoolean();
                    if (root.TryGetProperty("published", out var pub)) response.Settings.Published = pub.GetBoolean();
                    if (root.TryGetProperty("playbackUrl", out var pUrl)) response.Settings.PlaybackUrl = pUrl.GetString();
                }
                catch
                {
                    response.Settings.PlaybackUrl = recordLog.Value;
                    response.Settings.IsDownloadable = recordLog.Type == MainEntity_Types.MEETLOG_DOWNLOADABLE;
                }
                response.Settings.RecordId = savedRecordId;
                response.Settings.LastUpdated = recordLog.DateInsert;
            }

            // 2. Query BBB Recordings
            var bbbRecordings = new List<RecordingInfo>();
            if (_bbb != null)
            {
                // Try with MeetID
                if (!string.IsNullOrWhiteSpace(meeting.MeetID))
                {
                    var recs = await _bbb.GetRecordingsAsync(meetingId: meeting.MeetID);
                    if (recs != null && recs.Count > 0)
                        bbbRecordings.AddRange(recs);
                }

                // If none or room has different BBB_MeetID, try with room.BBB_MeetID
                if (meeting.RoomTBL != null && !string.IsNullOrWhiteSpace(meeting.RoomTBL.BBB_MeetID) &&
                    meeting.RoomTBL.BBB_MeetID != meeting.MeetID)
                {
                    var recs = await _bbb.GetRecordingsAsync(meetingId: meeting.RoomTBL.BBB_MeetID);
                    if (recs != null && recs.Count > 0)
                    {
                        foreach (var r in recs)
                        {
                            if (!bbbRecordings.Any(existing => existing.RecordID == r.RecordID))
                                bbbRecordings.Add(r);
                        }
                    }
                }

                // If still not found and savedRecordId is available, query directly by recordId
                if (!string.IsNullOrWhiteSpace(savedRecordId) && !bbbRecordings.Any(r => r.RecordID == savedRecordId))
                {
                    var recs = await _bbb.GetRecordingsAsync(recordId: savedRecordId);
                    if (recs != null && recs.Count > 0)
                        bbbRecordings.AddRange(recs);
                }
            }

            // 3. Map recordings
            foreach (var r in bbbRecordings)
            {
                string durStr = r.LengthMinutes > 0 ? $"{r.LengthMinutes} دقیقه" : "-";
                string sizeStr = r.Size > 0 ? $"{Math.Round(r.Size / (1024.0 * 1024.0), 1)} مگابایت" : "-";

                response.Recordings.Add(new BBBRecordingItemDTO
                {
                    RecordId = r.RecordID,
                    MeetingId = r.MeetingID,
                    Name = r.Name,
                    Published = r.Published,
                    State = r.State,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    DurationMinutes = r.LengthMinutes,
                    FormattedDuration = durStr,
                    SizeBytes = r.Size,
                    FormattedSize = sizeStr,
                    PlaybackFormat = r.PlaybackFormat,
                    PlaybackUrl = r.PlaybackUrl,
                    Thumbnail = r.Thumbnail,
                    ParticipantsCount = r.Participants,
                    Metadata = r.Metadata
                });
            }

            response.HasRecordings = response.Recordings.Count > 0;

            // Default settings if no settings were previously saved
            if (recordLog == null && response.HasRecordings)
            {
                var primary = response.Recordings[0];
                response.Settings.RecordId = primary.RecordId;
                response.Settings.PlaybackUrl = primary.PlaybackUrl;
                response.Settings.Published = primary.Published;
                if (primary.Metadata.TryGetValue("view_permission", out var vp)) response.Settings.ViewPermission = vp;
                if (primary.Metadata.TryGetValue("downloadable", out var dl)) response.Settings.IsDownloadable = dl.ToLower() == "true";
            }

            return response;
        }

        public async Task<MeetingRecordingSettingsDTO> UpdateRecordingSettingsAsync(UpdateRecordingSettingsRequestDTO req, UserTBL user)
        {
            if (req == null || req.MeetingId <= 0)
                throw new ArgumentException("درخواست نامعتبر است.");

            var validPermissions = new[] { "all", "members", "admins", "none" };
            if (!validPermissions.Contains(req.ViewPermission?.ToLower()))
                throw new ArgumentException("مقدار دسترسی تماشای رکورد نامعتبر است. مقادیر مجاز: all, members, admins, none");

            bool canAccess = await CanUserEditMeetingAsync(user, req.MeetingId);
            if (!canAccess)
                throw new UnauthorizedAccessException("شما دسترسی به تغییر تنظیمات این جلسه را ندارید.");

            var meeting = await _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                .FirstOrDefaultAsync(m => m.ID == req.MeetingId);

            if (meeting == null)
                throw new KeyNotFoundException("جلسه یافت نشد.");

            string? playbackUrl = null;

            // 1. Update BBB if recordId is provided
            if (_bbb != null && !string.IsNullOrWhiteSpace(req.RecordId))
            {
                var metaUpdates = new Dictionary<string, string>
                {
                    { "meta_view_permission", req.ViewPermission },
                    { "meta_downloadable", req.IsDownloadable ? "true" : "false" }
                };
                await _bbb.UpdateRecordingsAsync(req.RecordId, metaUpdates);

                if (req.Publish.HasValue)
                {
                    await _bbb.PublishRecordingsAsync(req.RecordId, req.Publish.Value);
                }

                var recs = await _bbb.GetRecordingsAsync(recordId: req.RecordId);
                playbackUrl = recs?.FirstOrDefault()?.PlaybackUrl;
            }

            // 2. Persist in MeetingLogTBL
            var recordLog = await _db.MeetingLogTBLs
                .FirstOrDefaultAsync(l => l.IDMeetingTBL == req.MeetingId && (l.Key == MeetingLogTBL.KEY_BBB_RECORD || l.Key == "bbb_record_settings") && l.Status != MainEntity_Status.LOG_REMOVED);

            var settingsPayload = new
            {
                recordId = req.RecordId,
                viewPermission = req.ViewPermission,
                downloadable = req.IsDownloadable,
                published = req.Publish ?? true,
                playbackUrl = playbackUrl,
                updatedAt = DateTime.UtcNow
            };
            string jsonVal = System.Text.Json.JsonSerializer.Serialize(settingsPayload);

            if (recordLog == null)
            {
                recordLog = new MeetingLogTBL
                {
                    IDMeetingTBL = req.MeetingId,
                    Key = MeetingLogTBL.KEY_BBB_RECORD,
                    Value = jsonVal,
                    Type = req.IsDownloadable ? MainEntity_Types.MEETLOG_DOWNLOADABLE : MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN,
                    Status = MainEntity_Status.LOG_NORMAL,
                    DateInsert = DateTime.UtcNow,
                    Explain = "تنظیمات رکورد جلسه BigBlueButton"
                };
                _db.MeetingLogTBLs.Add(recordLog);
            }
            else
            {
                recordLog.Value = jsonVal;
                recordLog.Type = req.IsDownloadable ? MainEntity_Types.MEETLOG_DOWNLOADABLE : MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN;
                recordLog.DateInsert = DateTime.UtcNow;
            }

            meeting.IsRecorded = true;
            await _db.SaveChangesAsync();

            return new MeetingRecordingSettingsDTO
            {
                MeetingId = req.MeetingId,
                RecordId = req.RecordId,
                ViewPermission = req.ViewPermission,
                IsDownloadable = req.IsDownloadable,
                Published = req.Publish ?? true,
                PlaybackUrl = playbackUrl,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<bool> PublishRecordingAsync(PublishRecordingRequestDTO req, UserTBL user)
        {
            if (req == null || req.MeetingId <= 0 || string.IsNullOrWhiteSpace(req.RecordId))
                throw new ArgumentException("اطلاعات درخواست انتشار ناقص است.");

            bool canAccess = await CanUserEditMeetingAsync(user, req.MeetingId);
            if (!canAccess)
                throw new UnauthorizedAccessException("شما دسترسی لازم برای تغییر وضعیت انتشار را ندارید.");

            bool bbbSuccess = true;
            if (_bbb != null)
            {
                bbbSuccess = await _bbb.PublishRecordingsAsync(req.RecordId, req.Publish);
            }

            var recordLog = await _db.MeetingLogTBLs
                .FirstOrDefaultAsync(l => l.IDMeetingTBL == req.MeetingId && (l.Key == MeetingLogTBL.KEY_BBB_RECORD || l.Key == "bbb_record_settings") && l.Status != MainEntity_Status.LOG_REMOVED);

            if (recordLog != null && !string.IsNullOrWhiteSpace(recordLog.Value))
            {
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(recordLog.Value);
                    if (dict != null)
                    {
                        dict["published"] = req.Publish;
                        dict["updatedAt"] = DateTime.UtcNow;
                        recordLog.Value = System.Text.Json.JsonSerializer.Serialize(dict);
                        await _db.SaveChangesAsync();
                    }
                }
                catch { }
            }

            return bbbSuccess;
        }

        public async Task<bool> DeleteRecordingAsync(DeleteRecordingRequestDTO req, UserTBL user)
        {
            if (req == null || req.MeetingId <= 0 || string.IsNullOrWhiteSpace(req.RecordId))
                throw new ArgumentException("اطلاعات درخواست حذف ناقص است.");

            bool canAccess = await CanUserEditMeetingAsync(user, req.MeetingId);
            if (!canAccess)
                throw new UnauthorizedAccessException("شما دسترسی لازم برای حذف رکورد را ندارید.");

            bool bbbSuccess = true;
            if (_bbb != null)
            {
                bbbSuccess = await _bbb.DeleteRecordingsAsync(req.RecordId);
            }

            var recordLog = await _db.MeetingLogTBLs
                .FirstOrDefaultAsync(l => l.IDMeetingTBL == req.MeetingId && (l.Key == MeetingLogTBL.KEY_BBB_RECORD || l.Key == "bbb_record_settings") && l.Status != MainEntity_Status.LOG_REMOVED);

            if (recordLog != null)
            {
                recordLog.Status = MainEntity_Status.LOG_REMOVED;
                await _db.SaveChangesAsync();
            }

            return bbbSuccess;
        }

        public async Task<string> GetRecordingTracksAsync(string recordId, UserTBL user)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("recordId الزامی است.");

            if (_bbb == null)
                return "<response><returncode>SUCCESS</returncode><tracks></tracks></response>";

            return await _bbb.GetRecordingTextTracksAsync(recordId);
        }
    }
}
