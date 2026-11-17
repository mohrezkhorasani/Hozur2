using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.General;
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
        Task<AdminMeetingStatsDTO> GetMeetingStatsAsync(
            UserTBL user,
            int? type = null,
            string q = "",
            long? ownerId = null,
            long? teacherId = null,
            string? roomFriendlyId = null,
            int? minAttendees = null,
            int? maxAttendees = null,
            string? attendeeTypes = null,
            int? minDurationMinutes = null,
            int? maxDurationMinutes = null,
            bool includeIgnored = false);

        Task<AdminMeetingsResultDTO> GetMeetingsAsync(
            UserTBL user,
            int page = 1,
            int count = 15,
            int? type = null,
            string q = "",
            long? ownerId = null,
            long? teacherId = null,
            string? roomFriendlyId = null,
            int? minAttendees = null,
            int? maxAttendees = null,
            string? attendeeTypes = null,
            int? minDurationMinutes = null,
            int? maxDurationMinutes = null,
            bool includeIgnored = false);

        Task<AdminMeetingFilterOptionsDTO> GetFilterOptionsAsync(UserTBL user);
        Task<bool> BatchUpdateMeetingTypeAsync(UserTBL user, List<long> meetingIds, int newType);
        Task<int> RecalculateRoomSpentMinutesAsync(string roomFriendlyId);
        Task<List<UpcomingPlanItemDTO>> GetUpcomingPlansAsync(UserTBL user);
        Task<List<AdminMeetingItemDTO>> GetRoomRecordingsAsync(string roomFriendlyId, UserTBL user);
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
                   (user.Username != null && user.Username.Equals("mohrez", StringComparison.OrdinalIgnoreCase)) ||
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

            var meeting = await _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                .FirstOrDefaultAsync(m => m.ID == meetingId);

            if (meeting == null) return false;

            // Check if user is owner of the room or creator of the meeting
            if (meeting.RoomTBL != null && meeting.RoomTBL.IDOwnerTBL == user.ID) return true;
            if (meeting.IDCreator == user.ID) return true;

            // Check if user has room access in RoomTBLToUserTBL (any role except deleted/deactivated)
            if (await _db.RoomTBLToUserTBLs.AnyAsync(r =>
                (r.FriendlyID == meeting.RoomFriendlyID || r.FriendlyIDRoomTBL == meeting.RoomFriendlyID || (meeting.RoomTBL != null && (r.FriendlyID == meeting.RoomTBL.FriendlyID || r.FriendlyIDRoomTBL == meeting.RoomTBL.FriendlyID))) &&
                (r.UUIDParticipatorTBL == user.UUID || (r.ParticipatorTBL != null && r.ParticipatorTBL.ID == user.ID)) &&
                r.Status != MainEntity_Status.Deactive &&
                r.Status != MainEntity_Status.Delete))
            {
                return true;
            }

            return false;
        }

        public IQueryable<MeetingTBL> BuildScopedQuery(
            UserTBL user,
            int? type = null,
            string q = "",
            long? ownerId = null,
            long? teacherId = null,
            string? roomFriendlyId = null,
            int? minAttendees = null,
            int? maxAttendees = null,
            string? attendeeTypes = null,
            int? minDurationMinutes = null,
            int? maxDurationMinutes = null,
            bool includeIgnored = false)
        {
            IQueryable<MeetingTBL> query = _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                    .ThenInclude(r => r.OwnerTBL)
                .Include(m => m.Teacher)
                .Include(m => m.Creator)
                .Include(m => m.UserTBLToMeetingTBLs);

            // Scope restriction: Non-superadmin users only see meetings in rooms they have access to (as room owner, admin/member/subscribed in RoomTBLToUserTBL, creator, or teacher)
            if (!IsSuperAdmin(user))
            {
                var accessibleRoomFids = _db.RoomTBLToUserTBLs
                    .Where(r =>
                        (r.UUIDParticipatorTBL == user.UUID || (r.ParticipatorTBL != null && r.ParticipatorTBL.ID == user.ID)) &&
                        r.Status != MainEntity_Status.Deactive &&
                        r.Status != MainEntity_Status.Delete)
                    .Select(r => r.FriendlyIDRoomTBL ?? r.FriendlyID)
                    .Distinct();

                query = query.Where(m =>
                    (m.RoomTBL != null && m.RoomTBL.IDOwnerTBL == user.ID) ||
                    accessibleRoomFids.Contains(m.RoomFriendlyID) ||
                    (m.RoomTBL != null && accessibleRoomFids.Contains(m.RoomTBL.FriendlyID)) ||
                    m.IDCreator == user.ID ||
                    m.IDTeacher == user.ID);
            }

            // Filters
            if (type.HasValue && type.Value != 0)
            {
                query = query.Where(m => (int)m.Type == type.Value);
            }
            else if (!includeIgnored)
            {
                // نادیده گرفته شده‌ها کلاً نشان داده نشوند مگر اینکه در فیلترها تیک خورده باشد
                query = query.Where(m => (int)m.Type != -99);
            }

            if (!string.IsNullOrWhiteSpace(roomFriendlyId))
            {
                var rfid = roomFriendlyId.Trim();
                query = query.Where(m => m.RoomFriendlyID == rfid || (m.RoomTBL != null && m.RoomTBL.FriendlyID == rfid));
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

            // Duration filters (in minutes)
            if (minDurationMinutes.HasValue && minDurationMinutes.Value > 0)
            {
                query = query.Where(m => m.EndDate != null && EF.Functions.DateDiffMinute(m.DateInsert, m.EndDate.Value) >= minDurationMinutes.Value);
            }

            if (maxDurationMinutes.HasValue && maxDurationMinutes.Value > 0)
            {
                query = query.Where(m => m.EndDate != null && EF.Functions.DateDiffMinute(m.DateInsert, m.EndDate.Value) <= maxDurationMinutes.Value);
            }

            // Attendees filters
            List<int>? parsedAttendeeTypes = null;
            if (!string.IsNullOrWhiteSpace(attendeeTypes))
            {
                parsedAttendeeTypes = attendeeTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var val) ? (int?)val : null)
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .ToList();
            }

            if (minAttendees.HasValue && minAttendees.Value >= 0)
            {
                if (parsedAttendeeTypes != null && parsedAttendeeTypes.Count > 0)
                {
                    query = query.Where(m => m.UserTBLToMeetingTBLs.Count(u => !u.IsRemoved && parsedAttendeeTypes.Contains((int)u.Type)) >= minAttendees.Value);
                }
                else
                {
                    query = query.Where(m => m.UserTBLToMeetingTBLs.Count(u => !u.IsRemoved &&
                        u.Type != MainEntity_Types.USER_TEACHER &&
                        u.Type != MainEntity_Types.USER_DABIR &&
                        u.Type != MainEntity_Types.USER_ADMINSTRATOR &&
                        u.Type != MainEntity_Types.USER_MODERATOR) >= minAttendees.Value);
                }
            }

            if (maxAttendees.HasValue && maxAttendees.Value >= 0)
            {
                if (parsedAttendeeTypes != null && parsedAttendeeTypes.Count > 0)
                {
                    query = query.Where(m => m.UserTBLToMeetingTBLs.Count(u => !u.IsRemoved && parsedAttendeeTypes.Contains((int)u.Type)) <= maxAttendees.Value);
                }
                else
                {
                    query = query.Where(m => m.UserTBLToMeetingTBLs.Count(u => !u.IsRemoved &&
                        u.Type != MainEntity_Types.USER_TEACHER &&
                        u.Type != MainEntity_Types.USER_DABIR &&
                        u.Type != MainEntity_Types.USER_ADMINSTRATOR &&
                        u.Type != MainEntity_Types.USER_MODERATOR) <= maxAttendees.Value);
                }
            }

            return query;
        }

        public async Task<AdminMeetingStatsDTO> GetMeetingStatsAsync(
            UserTBL user,
            int? type = null,
            string q = "",
            long? ownerId = null,
            long? teacherId = null,
            string? roomFriendlyId = null,
            int? minAttendees = null,
            int? maxAttendees = null,
            string? attendeeTypes = null,
            int? minDurationMinutes = null,
            int? maxDurationMinutes = null,
            bool includeIgnored = false)
        {
            var query = BuildScopedQuery(
                user, type, q, ownerId, teacherId, roomFriendlyId,
                minAttendees, maxAttendees, attendeeTypes, minDurationMinutes, maxDurationMinutes, includeIgnored);

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

        public async Task<AdminMeetingsResultDTO> GetMeetingsAsync(
            UserTBL user,
            int page = 1,
            int count = 15,
            int? type = null,
            string q = "",
            long? ownerId = null,
            long? teacherId = null,
            string? roomFriendlyId = null,
            int? minAttendees = null,
            int? maxAttendees = null,
            string? attendeeTypes = null,
            int? minDurationMinutes = null,
            int? maxDurationMinutes = null,
            bool includeIgnored = false)
        {
            if (page < 1) page = 1;
            if (count < 1) count = 15;
            if (count > 10000) count = 10000;

            var query = BuildScopedQuery(
                user, type, q, ownerId, teacherId, roomFriendlyId,
                minAttendees, maxAttendees, attendeeTypes, minDurationMinutes, maxDurationMinutes, includeIgnored);

            var stats = await GetMeetingStatsAsync(
                user, type, q, ownerId, teacherId, roomFriendlyId,
                minAttendees, maxAttendees, attendeeTypes, minDurationMinutes, maxDurationMinutes, includeIgnored);
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
                    idCreator = m.IDCreator,
                    creator = m.Creator != null ? new AdminMeetingOwnerDTO
                    {
                        Id = m.Creator.ID,
                        Name = m.Creator.GetName(),
                        Username = m.Creator.Username,
                        Avatar = m.Creator.AvatarPath
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
                    IDCreator = m.idCreator,
                    Creator = m.creator,
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
                    RecordingPublished = recPublished,
                    DurationMinutes = (m.endDate.HasValue && m.endDate.Value > m.dateInsert) ? (int?)(int)(m.endDate.Value - m.dateInsert).TotalMinutes : null,
                    HasRecording = m.isRecorded || !string.IsNullOrWhiteSpace(recPlaybackUrl) || !string.IsNullOrWhiteSpace(recRecordId)
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

        public async Task<AdminMeetingFilterOptionsDTO> GetFilterOptionsAsync(UserTBL user)
        {
            var result = new AdminMeetingFilterOptionsDTO();
            if (user == null) return result;

            IQueryable<RoomTBL> roomsQuery = _db.RoomTBLs.Include(r => r.RoomMetaTBLs);
            if (!IsSuperAdmin(user))
            {
                var accessibleRoomFids = _db.RoomTBLToUserTBLs
                    .Where(r => (r.UUIDParticipatorTBL == user.UUID || (r.ParticipatorTBL != null && r.ParticipatorTBL.ID == user.ID)) &&
                                r.Status != MainEntity_Status.Deactive &&
                                r.Status != MainEntity_Status.Delete)
                    .Select(r => r.FriendlyIDRoomTBL ?? r.FriendlyID)
                    .Distinct();

                roomsQuery = roomsQuery.Where(r => r.IDOwnerTBL == user.ID || accessibleRoomFids.Contains(r.FriendlyID));
            }

            var roomsList = await roomsQuery.OrderBy(r => r.Title).ToListAsync();
            foreach (var r in roomsList)
            {
                int spentMin = 0;
                var spentMeta = r.RoomMetaTBLs?.FirstOrDefault(m => m.Key == "spent_minutes");
                if (spentMeta != null && int.TryParse(spentMeta.Value, out var sm))
                {
                    spentMin = sm;
                }

                result.Rooms.Add(new AdminMeetingRoomOptionDTO
                {
                    Id = r.ID,
                    FriendlyId = r.FriendlyID,
                    Title = !string.IsNullOrWhiteSpace(r.Title) ? r.Title : r.FriendlyID,
                    SpentMinutes = spentMin
                });
            }

            // Distinct teachers from meetings in accessible rooms
            var accessibleFids = result.Rooms.Select(r => r.FriendlyId).ToList();
            var teachersList = await _db.MeetingTBLs
                .Where(m => accessibleFids.Contains(m.RoomFriendlyID) && m.IDTeacher != null && m.Teacher != null)
                .Select(m => new AdminMeetingTeacherDTO
                {
                    Id = m.Teacher.ID,
                    Name = m.Teacher.GetName(),
                    Avatar = m.Teacher.AvatarPath
                })
                .Distinct()
                .ToListAsync();

            result.Teachers = teachersList
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .OrderBy(t => t.Name)
                .ToList();

            return result;
        }

        public async Task<int> RecalculateRoomSpentMinutesAsync(string roomFriendlyId)
        {
            if (string.IsNullOrWhiteSpace(roomFriendlyId)) return 0;

            var room = await _db.RoomTBLs
                .Include(r => r.RoomMetaTBLs)
                .FirstOrDefaultAsync(r => r.FriendlyID == roomFriendlyId);

            if (room == null) return 0;

            var meetings = await _db.MeetingTBLs
                .Where(m => (m.RoomFriendlyID == room.FriendlyID || (m.RoomTBL != null && m.RoomTBL.FriendlyID == room.FriendlyID)) &&
                            ((int)m.Type == 100 || (int)m.Type == 101) &&
                            m.EndDate != null &&
                            m.EndDate > m.DateInsert)
                .Select(m => new { m.DateInsert, EndDate = m.EndDate.Value })
                .ToListAsync();

            int totalMinutes = meetings.Sum(m =>
            {
                var mins = (int)(m.EndDate - m.DateInsert).TotalMinutes;
                return mins > 1440 ? 120 : mins;
            });

            var existingMeta = room.RoomMetaTBLs?.FirstOrDefault(m => m.Key == "spent_minutes")
                ?? await _db.RoomMetaTBLs.FirstOrDefaultAsync(m => m.IDRoomTBL == room.ID && m.Key == "spent_minutes");

            if (existingMeta != null)
            {
                existingMeta.Value = totalMinutes.ToString();
                existingMeta.UpdatedAt = DateTime.Now;
                existingMeta.Status = MainEntity_Status.Enabled;
            }
            else
            {
                _db.RoomMetaTBLs.Add(new RoomMetaTBL
                {
                    IDRoomTBL = room.ID,
                    FriendlyID = room.FriendlyID,
                    Key = "spent_minutes",
                    Value = totalMinutes.ToString(),
                    DataType = MainEntity_Types.META_INT,
                    DateInsert = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Status = MainEntity_Status.Enabled
                });
            }

            await _db.SaveChangesAsync();
            return totalMinutes;
        }

        public async Task<bool> BatchUpdateMeetingTypeAsync(UserTBL user, List<long> meetingIds, int newType)
        {
            if (user == null || meetingIds == null || meetingIds.Count == 0) return false;

            var meetings = await _db.MeetingTBLs
                .Include(m => m.RoomTBL)
                .Where(m => meetingIds.Contains(m.ID))
                .ToListAsync();

            var affectedRoomFids = new HashSet<string>();

            foreach (var meeting in meetings)
            {
                if (await CanUserEditMeetingAsync(user, meeting.ID))
                {
                    meeting.Type = (MainEntity_Types)newType;
                    if (!string.IsNullOrEmpty(meeting.RoomFriendlyID))
                        affectedRoomFids.Add(meeting.RoomFriendlyID);
                    else if (meeting.RoomTBL != null && !string.IsNullOrEmpty(meeting.RoomTBL.FriendlyID))
                        affectedRoomFids.Add(meeting.RoomTBL.FriendlyID);
                }
            }

            await _db.SaveChangesAsync();

            // Recalculate spent minutes for each affected room
            foreach (var rfid in affectedRoomFids)
            {
                try
                {
                    await RecalculateRoomSpentMinutesAsync(rfid);
                }
                catch { }
            }

            return true;
        }

        public async Task<List<UpcomingPlanItemDTO>> GetUpcomingPlansAsync(UserTBL user)
        {
            var list = new List<UpcomingPlanItemDTO>();
            if (user == null) return list;

            IQueryable<RoomTBLPlanTBL> query = _db.RoomTBLPlanTBLs
                .Include(p => p.RoomTBL)
                .Where(p => p.StartDate != null &&
                            p.StartDate >= DateTime.UtcNow.AddHours(-3) &&
                            !string.IsNullOrWhiteSpace(p.Title) &&
                            (p.Status == MainEntity_Status.Enabled || p.Status == MainEntity_Status.MEETING_STARTED_LATER) &&
                            (int)p.Status != -400 &&
                            (int)p.Type != -400 &&
                            p.Type != MainEntity_Types.PLAN_DELETED &&
                            p.Type != MainEntity_Types.MEET_IGNORE &&
                            (int)p.Type != -99 &&
                            p.RoomTBL != null &&
                            ((int)p.RoomTBL.Type == 1 || (int)p.RoomTBL.Type == 2) &&
                            (int)p.RoomTBL.Status == 100);

            if (!IsSuperAdmin(user))
            {
                // فقط جلسات پیش‌رو مربوط به گروه‌ها و اتاق‌های اکانت فعلی برای کاربران عادی
                var accessibleRoomFids = _db.RoomTBLToUserTBLs
                    .Where(r => (r.UUIDParticipatorTBL == user.UUID || (r.ParticipatorTBL != null && r.ParticipatorTBL.ID == user.ID)) &&
                                r.Status != MainEntity_Status.Deactive &&
                                r.Status != MainEntity_Status.Delete)
                    .Select(r => r.FriendlyIDRoomTBL ?? r.FriendlyID)
                    .Distinct();

                query = query.Where(p => p.RoomTBL != null &&
                    (p.RoomTBL.IDOwnerTBL == user.ID || accessibleRoomFids.Contains(p.RoomTBL.FriendlyID) || p.RoomTBL.IsPublicRoom));
            }

            var plans = await query
                .OrderBy(p => p.StartDate)
                .Take(50)
                .ToListAsync();

            foreach (var p in plans)
            {
                var (tehranDate, tehranTime, _) = PlanTimeHelper.ToTehranJalaliStrings(p.StartDate.Value);
                list.Add(new UpcomingPlanItemDTO
                {
                    PlanId = p.ID,
                    RoomId = p.RoomTBL != null ? p.RoomTBL.ID : p.IDRoomTBL,
                    RoomFriendlyId = p.RoomTBL != null ? p.RoomTBL.FriendlyID : "",
                    RoomTitle = p.RoomTBL != null ? p.RoomTBL.Title : "اتاق",
                    Title = p.Title,
                    Teachers = p.Teachers,
                    Dabir = p.Dabir,
                    StartDate = p.StartDate.HasValue ? DateTime.SpecifyKind(p.StartDate.Value, DateTimeKind.Utc) : null,
                    JalaliDate = tehranDate,
                    Time = tehranTime,
                    Type = (int)p.Type,
                    RoomType = p.RoomTBL != null ? (int)p.RoomTBL.Type : 0,
                    RoomStatus = p.RoomTBL != null ? (int)p.RoomTBL.Status : 0
                });
            }

            return list;
        }

        public async Task<List<AdminMeetingItemDTO>> GetRoomRecordingsAsync(string roomFriendlyId, UserTBL user)
        {
            var list = new List<AdminMeetingItemDTO>();
            if (string.IsNullOrWhiteSpace(roomFriendlyId) || user == null) return list;

            var room = await _db.RoomTBLs.FirstOrDefaultAsync(r => r.FriendlyID == roomFriendlyId);
            if (room == null && long.TryParse(roomFriendlyId, out var numId))
            {
                room = await _db.RoomTBLs.FirstOrDefaultAsync(r => r.ID == numId);
            }
            if (room == null) return list;

            // 1. استعلام مستقیم رکوردهای ضبط شده از سرور بیگ‌بلو‌باتن
            var bbbRecordings = new List<RecordingInfo>();
            if (_bbb != null)
            {
                if (!string.IsNullOrWhiteSpace(room.BBB_MeetID))
                {
                    try
                    {
                        var recs = await _bbb.GetRecordingsAsync(meetingId: room.BBB_MeetID);
                        if (recs != null && recs.Count > 0)
                            bbbRecordings.AddRange(recs);
                    }
                    catch (Exception ex)
                    {
                        Statics.Log("GetRoomRecordingsAsync BBB Error (BBB_MeetID)", ex.Message);
                    }
                }

                if (!string.IsNullOrWhiteSpace(room.FriendlyID))
                {
                    try
                    {
                        var recs = await _bbb.GetRecordingsAsync(meetingId: room.FriendlyID);
                        if (recs != null && recs.Count > 0)
                        {
                            foreach (var r in recs)
                            {
                                if (!bbbRecordings.Any(existing => existing.RecordID == r.RecordID))
                                    bbbRecordings.Add(r);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Statics.Log("GetRoomRecordingsAsync BBB Error (FriendlyID)", ex.Message);
                    }
                }
            }

            // 2. واکشی اطلاعات نشست‌های این اتاق از جدول MeetingTBL
            var roomMeetings = await _db.MeetingTBLs
                .Include(m => m.Teacher)
                .Include(m => m.RoomTBLPlanTBL)
                .Where(m => (m.RoomFriendlyID == room.FriendlyID || (m.RoomTBL != null && m.RoomTBL.FriendlyID == room.FriendlyID)) &&
                            (int)m.Type != -99)
                .ToListAsync();

            var meetingsByMeetId = roomMeetings
                .Where(m => !string.IsNullOrEmpty(m.MeetID))
                .GroupBy(m => m.MeetID)
                .ToDictionary(g => g.Key, g => g.First());

            var matchedMeetingIds = new HashSet<long>();

            // 3. تطبیق هر ضبط BBB بر اساس recordID == meeting.MeetID
            foreach (var rec in bbbRecordings)
            {
                MeetingTBL? matchedMeeting = null;
                if (!string.IsNullOrEmpty(rec.RecordID) && meetingsByMeetId.TryGetValue(rec.RecordID, out var mFound))
                {
                    matchedMeeting = mFound;
                    matchedMeetingIds.Add(mFound.ID);
                }

                // عنوان نشست: اولویت با عنوان جدول میت، سپس نام جلسه در BBB یا متادیتا
                string title = "";
                if (matchedMeeting != null && !string.IsNullOrWhiteSpace(matchedMeeting.Title))
                    title = matchedMeeting.Title;
                else if (rec.Metadata != null && rec.Metadata.TryGetValue("meetingName", out var mn) && !string.IsNullOrWhiteSpace(mn))
                    title = mn;
                else if (rec.Metadata != null && rec.Metadata.TryGetValue("title", out var mt) && !string.IsNullOrWhiteSpace(mt))
                    title = mt;
                else if (!string.IsNullOrWhiteSpace(rec.Name))
                    title = rec.Name;
                else
                    title = room.Title;

                DateTime startDate = matchedMeeting?.DateInsert ?? rec.StartTime;
                DateTime? endDate = matchedMeeting?.EndDate ?? (rec.EndTime > DateTime.MinValue ? rec.EndTime : null);

                var (tehranDate, startTimeStr, _) = PlanTimeHelper.ToTehranJalaliStrings(startDate);
                string endTimeStr = "-";
                if (endDate.HasValue && endDate.Value > DateTime.MinValue)
                {
                    var (_, etStr, _) = PlanTimeHelper.ToTehranJalaliStrings(endDate.Value);
                    endTimeStr = etStr;
                }
                else if (rec.LengthMinutes > 0)
                {
                    var calculatedEnd = startDate.AddMinutes(rec.LengthMinutes);
                    var (_, etStr, _) = PlanTimeHelper.ToTehranJalaliStrings(calculatedEnd);
                    endTimeStr = etStr;
                }

                // نام استاد
                string teacherName = "";
                if (matchedMeeting?.Teacher != null)
                {
                    teacherName = matchedMeeting.Teacher.GetName();
                }
                else if (matchedMeeting?.RoomTBLPlanTBL != null && !string.IsNullOrWhiteSpace(matchedMeeting.RoomTBLPlanTBL.Teachers))
                {
                    teacherName = matchedMeeting.RoomTBLPlanTBL.Teachers;
                }
                else if (rec.Metadata != null && rec.Metadata.TryGetValue("teachers", out var tJson) && !string.IsNullOrWhiteSpace(tJson))
                {
                    try
                    {
                        var tList = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(tJson);
                        if (tList != null && tList.Count > 0)
                        {
                            teacherName = string.Join("، ", tList.Select(x => x.GetValueOrDefault("name", "")).Where(s => !string.IsNullOrWhiteSpace(s)));
                        }
                    }
                    catch
                    {
                        teacherName = tJson;
                    }
                }

                if (string.IsNullOrWhiteSpace(teacherName))
                    teacherName = "تعیین نشده";

                int durationMinutes = rec.LengthMinutes;
                if (durationMinutes <= 0 && endDate.HasValue && endDate.Value > startDate)
                {
                    durationMinutes = (int)Math.Max(1, (endDate.Value - startDate).TotalMinutes);
                }

                list.Add(new AdminMeetingItemDTO
                {
                    Id = matchedMeeting?.ID ?? (long.TryParse(rec.RecordID.Split('-').LastOrDefault(), out var genId) ? genId : 0),
                    MeetID = rec.RecordID,
                    Title = title,
                    RoomFriendlyID = room.FriendlyID,
                    RoomTitle = room.Title,
                    Teacher = new AdminMeetingTeacherDTO { Name = teacherName },
                    DateInsert = startDate,
                    EndDate = endDate,
                    JalaliDate = tehranDate,
                    DurationMinutes = durationMinutes,
                    ParticipantsCount = rec.Participants,
                    IsRecorded = true,
                    HasRecording = true,
                    RecordingPlaybackUrl = rec.PlaybackUrl,
                    VideoUrl = rec.PlaybackUrl,
                    RecordingRecordId = rec.RecordID,
                    Status = matchedMeeting != null ? (int)matchedMeeting.Status : 100,
                    Type = matchedMeeting != null ? (int)matchedMeeting.Type : 1
                });
            }

            // 4. همچنین اضافه کردن نشست‌هایی در دیتابیس که فلگ IsRecorded دارند ولی در لیست BBB نبوده‌اند
            foreach (var m in roomMeetings.Where(m => m.IsRecorded && !matchedMeetingIds.Contains(m.ID)))
            {
                var (tehranDate, _, _) = PlanTimeHelper.ToTehranJalaliStrings(m.DateInsert);
                int dur = 0;
                if (m.EndDate.HasValue && m.EndDate.Value > m.DateInsert)
                {
                    dur = (int)Math.Max(1, (m.EndDate.Value - m.DateInsert).TotalMinutes);
                }

                string mTeacherName = m.Teacher?.GetName() ?? m.RoomTBLPlanTBL?.Teachers ?? "تعیین نشده";

                list.Add(new AdminMeetingItemDTO
                {
                    Id = m.ID,
                    MeetID = m.MeetID,
                    Title = !string.IsNullOrWhiteSpace(m.Title) ? m.Title : room.Title,
                    RoomFriendlyID = room.FriendlyID,
                    RoomTitle = room.Title,
                    Teacher = new AdminMeetingTeacherDTO { Name = mTeacherName },
                    DateInsert = m.DateInsert,
                    EndDate = m.EndDate,
                    JalaliDate = tehranDate,
                    DurationMinutes = dur,
                    IsRecorded = true,
                    HasRecording = true,
                    Status = (int)m.Status,
                    Type = (int)m.Type
                });
            }

            // فقط نشست‌های ضبط‌شده و مرتب‌شده به صورت نزولی بر اساس تاریخ
            return list.OrderByDescending(x => x.DateInsert).ToList();
        }
    }
}
