using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Models;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Services;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Presistence.Entities;
using Xunit;

namespace CoreMe.Tests
{
    public class FakeBBBGetMeetings : IBBBGetMeetings
    {
        public List<RecordingInfo> RecordingsToReturn { get; set; } = new List<RecordingInfo>();
        public bool LastPublishState { get; set; }
        public string? LastPublishedRecordId { get; set; }
        public string? LastDeletedRecordId { get; set; }
        public Dictionary<string, string> LastUpdatedMetadata { get; set; } = new Dictionary<string, string>();
        public string? LastUpdatedRecordId { get; set; }

        public Task<List<RecordingInfo>> GetRecordingsByMeetingId(string meetingId)
        {
            return Task.FromResult(RecordingsToReturn.Where(r => r.MeetingID == meetingId).ToList());
        }

        public Task<List<RecordingInfo>> GetRecordingsAsync(string? meetingId = null, string? recordId = null, string? state = null)
        {
            var query = RecordingsToReturn.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(meetingId))
                query = query.Where(r => r.MeetingID == meetingId);
            if (!string.IsNullOrWhiteSpace(recordId))
                query = query.Where(r => r.RecordID == recordId);
            return Task.FromResult(query.ToList());
        }

        public Task<bool> PublishRecordingsAsync(string recordId, bool publish)
        {
            LastPublishedRecordId = recordId;
            LastPublishState = publish;
            var rec = RecordingsToReturn.FirstOrDefault(r => r.RecordID == recordId);
            if (rec != null) rec.Published = publish;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteRecordingsAsync(string recordId)
        {
            LastDeletedRecordId = recordId;
            RecordingsToReturn.RemoveAll(r => r.RecordID == recordId);
            return Task.FromResult(true);
        }

        public Task<bool> UpdateRecordingsAsync(string recordId, Dictionary<string, string> metadata)
        {
            LastUpdatedRecordId = recordId;
            LastUpdatedMetadata = metadata;
            var rec = RecordingsToReturn.FirstOrDefault(r => r.RecordID == recordId);
            if (rec != null)
            {
                foreach (var kvp in metadata)
                {
                    rec.Metadata[kvp.Key] = kvp.Value;
                }
            }
            return Task.FromResult(true);
        }

        public Task<string> GetRecordingTextTracksAsync(string recordId)
        {
            return Task.FromResult("<response><returncode>SUCCESS</returncode><tracks></tracks></response>");
        }

        public Task<List<MiniRoomJSON>> Request(GetLivesRequest req, UserTBL user) => Task.FromResult(new List<MiniRoomJSON>());
        public Task<List<MiniRoomJSON>> Request(GetLivesOfProfileRequest req, UserTBL user, UserTBL currentUser) => Task.FromResult(new List<MiniRoomJSON>());
        public Task<List<MiniRoomJSON>> Request(GetLivesRequest req) => Task.FromResult(new List<MiniRoomJSON>());
        public Task<List<MiniRoomJSON>> RequestAll(GetLivesRequest req) => Task.FromResult(new List<MiniRoomJSON>());
        public Task<List<MiniRoomJSON>> Request(GetLiveWithIDRequest req) => Task.FromResult(new List<MiniRoomJSON>());
        public Task<List<MiniRoomJSON>> GetLivesShort(GetLivesRequest req) => Task.FromResult(new List<MiniRoomJSON>());
        public Task<MeetJSON> CreateMeetingAsync(RoomTBL room) => Task.FromResult(new MeetJSON());
        public Task<string> GetJoinUrlAsync(MeetingTBL meet, string fullName, string userId, string nationalCode, string phone = "", string vc2AuthNorm = "", bool asModerator = false, string moderatorPassowrd = "") => Task.FromResult("https://fake.url");
        public Task GetMeeting(string meetingId) => Task.CompletedTask;
        public Task<bool> EndMeetingAsync(string meetingId, string moderatorPass = "") => Task.FromResult(true);
        public Task<AttendeeInfo> GetAttendeeByUserId(string roomId, string userId) => Task.FromResult(new AttendeeInfo());
        public Task<List<AttendeeInfo>> GetMeetingAttendeesByRoomId(string roomId) => Task.FromResult(new List<AttendeeInfo>());
        public Task<BBBHookResponse> CreateWebhookAsync(string callbackURL, string meetingID = null) => Task.FromResult(new BBBHookResponse());
        public Task<BBBHookResponse> CreateWebhookWithEventsAsync(string callbackURL, List<string> eventIDs, string meetingID = null) => Task.FromResult(new BBBHookResponse());
        public Task<List<BBBHookInfo>> ListWebhooksAsync(string meetingID = null) => Task.FromResult(new List<BBBHookInfo>());
        public Task<bool> DestroyWebhookAsync(string hookID) => Task.FromResult(true);
        public Task<int> DestroyAllWebhooksForMeetingAsync(string meetingID) => Task.FromResult(1);
        public Task<BBBHookInfo> GetWebhookInfoAsync(string hookID) => Task.FromResult(new BBBHookInfo());
        public Task<bool> UpdateWebhookCallbackURLAsync(string hookID, string newCallbackURL) => Task.FromResult(true);
    }

    public class AdminMeetingRecordingTests
    {
        private DBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new DBContext(options);
        }

        private (DBContext db, UserTBL superAdmin, UserTBL ownerUser, UserTBL unauthorizedUser, MeetingTBL meeting, FakeBBBGetMeetings fakeBbb) SetupTestData()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());

            var superAdmin = new UserTBL
            {
                ID = 1,
                UUID = "uuid-superadmin",
                Username = "admin_super",
                Firstname = "مدیر",
                LastName = "کل",
                Type = MainEntity_Types.USER_ADMINSTRATOR,
                Status = MainEntity_Status.Succedd
            };

            var ownerUser = new UserTBL
            {
                ID = 2,
                UUID = "uuid-owner",
                Username = "room_owner",
                Firstname = "مالک",
                LastName = "اتاق",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var unauthorizedUser = new UserTBL
            {
                ID = 3,
                UUID = "uuid-unauth",
                Username = "unauthorized_user",
                Firstname = "کاربر",
                LastName = "غریبه",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            db.UserTBLs.AddRange(superAdmin, ownerUser, unauthorizedUser);

            var room = new RoomTBL
            {
                ID = 101,
                FriendlyID = "room-test-fid",
                BBB_MeetID = "bbb-meet-room-101",
                Title = "اتاق تست وبینار",
                IDOwnerTBL = ownerUser.ID,
                OwnerTBL = ownerUser,
                Status = MainEntity_Status.Succedd,
                Type = MainEntity_Types.ROOM_NORMAL,
                RecordRoom = true
            };
            db.RoomTBLs.Add(room);

            var meeting = new MeetingTBL
            {
                ID = 1001,
                MeetID = "meet-random-1001",
                Title = "جلسه ضبط شده آزمایشی",
                RoomFriendlyID = room.FriendlyID,
                RoomTBL = room,
                IDCreator = ownerUser.ID,
                Creator = ownerUser,
                Type = MainEntity_Types.MEET_GREEN_LIGHT,
                Status = MainEntity_Status.MEETING_END,
                DateInsert = DateTime.UtcNow.AddHours(-3),
                IsRecorded = true
            };
            db.MeetingTBLs.Add(meeting);
            db.SaveChanges();

            var fakeBbb = new FakeBBBGetMeetings();
            fakeBbb.RecordingsToReturn.Add(new RecordingInfo
            {
                RecordID = "rec-guid-12345",
                MeetingID = room.BBB_MeetID,
                Name = meeting.Title,
                Published = true,
                State = "published",
                StartTime = DateTime.UtcNow.AddHours(-3),
                EndTime = DateTime.UtcNow.AddHours(-1),
                LengthMinutes = 120,
                Participants = 25,
                PlaybackFormat = "presentation",
                PlaybackUrl = "https://vc.hekmateislami.com/playback/presentation/2.3/rec-guid-12345",
                Size = 150000000,
                Metadata = new Dictionary<string, string>
                {
                    { "downloadable", "true" },
                    { "view_permission", "all" }
                }
            });

            return (db, superAdmin, ownerUser, unauthorizedUser, meeting, fakeBbb);
        }

        [Fact]
        public async Task GetMeetingRecordings_WhenUserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            var (db, _, _, unauthorizedUser, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await service.GetMeetingRecordingsAsync(meeting.ID, unauthorizedUser);
            });
        }

        [Fact]
        public async Task GetMeetingRecordings_WhenSuperAdmin_ReturnsRecordingsAndSettings()
        {
            var (db, superAdmin, _, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            var result = await service.GetMeetingRecordingsAsync(meeting.ID, superAdmin);

            Assert.NotNull(result);
            Assert.Equal(meeting.ID, result.MeetingId);
            Assert.True(result.HasRecordings);
            Assert.Single(result.Recordings);
            Assert.Equal("rec-guid-12345", result.Recordings[0].RecordId);
            Assert.Equal("https://vc.hekmateislami.com/playback/presentation/2.3/rec-guid-12345", result.Recordings[0].PlaybackUrl);
            Assert.Equal("all", result.Settings.ViewPermission);
            Assert.True(result.Settings.IsDownloadable);
        }

        [Fact]
        public async Task GetMeetingRecordings_WhenRoomOwner_CanAccessRecordings()
        {
            var (db, _, ownerUser, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            var result = await service.GetMeetingRecordingsAsync(meeting.ID, ownerUser);

            Assert.NotNull(result);
            Assert.Equal(meeting.ID, result.MeetingId);
            Assert.True(result.HasRecordings);
        }

        [Fact]
        public async Task UpdateRecordingSettings_UpdatesDbLogAndCallsBbb()
        {
            var (db, superAdmin, _, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            var req = new UpdateRecordingSettingsRequestDTO
            {
                MeetingId = meeting.ID,
                RecordId = "rec-guid-12345",
                ViewPermission = "members",
                IsDownloadable = false,
                Publish = true
            };

            var settings = await service.UpdateRecordingSettingsAsync(req, superAdmin);

            Assert.NotNull(settings);
            Assert.Equal("members", settings.ViewPermission);
            Assert.False(settings.IsDownloadable);

            // Verify BBB was called with meta tags
            Assert.Equal("rec-guid-12345", fakeBbb.LastUpdatedRecordId);
            Assert.Equal("members", fakeBbb.LastUpdatedMetadata["meta_view_permission"]);
            Assert.Equal("false", fakeBbb.LastUpdatedMetadata["meta_downloadable"]);

            // Verify DB MeetingLogTBL entry exists
            var log = await db.MeetingLogTBLs.FirstOrDefaultAsync(l => l.IDMeetingTBL == meeting.ID && l.Key == MeetingLogTBL.KEY_BBB_RECORD);
            Assert.NotNull(log);
            Assert.Contains("members", log.Value);
            Assert.Equal(MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN, log.Type);
        }

        [Fact]
        public async Task UpdateRecordingSettings_WhenInvalidViewPermission_ThrowsArgumentException()
        {
            var (db, superAdmin, _, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            var req = new UpdateRecordingSettingsRequestDTO
            {
                MeetingId = meeting.ID,
                RecordId = "rec-guid-12345",
                ViewPermission = "invalid_role",
                IsDownloadable = true
            };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.UpdateRecordingSettingsAsync(req, superAdmin);
            });
        }

        [Fact]
        public async Task PublishRecording_WhenAuthorized_CallsBbbPublishAndUpdatesSettings()
        {
            var (db, superAdmin, _, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            var req = new PublishRecordingRequestDTO
            {
                MeetingId = meeting.ID,
                RecordId = "rec-guid-12345",
                Publish = false
            };

            var result = await service.PublishRecordingAsync(req, superAdmin);

            Assert.True(result);
            Assert.Equal("rec-guid-12345", fakeBbb.LastPublishedRecordId);
            Assert.False(fakeBbb.LastPublishState);
        }

        [Fact]
        public async Task DeleteRecording_WhenAuthorized_CallsBbbDeleteAndCleansSettings()
        {
            var (db, superAdmin, _, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            var req = new DeleteRecordingRequestDTO
            {
                MeetingId = meeting.ID,
                RecordId = "rec-guid-12345"
            };

            var result = await service.DeleteRecordingAsync(req, superAdmin);

            Assert.True(result);
            Assert.Equal("rec-guid-12345", fakeBbb.LastDeletedRecordId);
            Assert.Empty(fakeBbb.RecordingsToReturn);
        }

        [Fact]
        public async Task GetMeetingsAsync_MapsBbbMeetIdAndRecordingSettingsToItemDTO()
        {
            var (db, superAdmin, _, _, meeting, fakeBbb) = SetupTestData();
            var service = new AdminMeetingService(db, fakeBbb);

            // Add BBB record settings in DB for meeting
            db.MeetingLogTBLs.Add(new MeetingLogTBL
            {
                ID = 999,
                IDMeetingTBL = meeting.ID,
                Key = MeetingLogTBL.KEY_BBB_RECORD,
                Value = System.Text.Json.JsonSerializer.Serialize(new
                {
                    recordId = "rec-guid-12345",
                    playbackUrl = "https://vc.hekmateislami.com/playback/presentation/2.3/rec-guid-12345",
                    viewPermission = "members",
                    downloadable = false,
                    published = true
                }),
                Type = MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.GetMeetingsAsync(superAdmin, page: 1, count: 10);

            Assert.NotNull(result);
            var item = result.Items.FirstOrDefault(m => m.Id == meeting.ID);
            Assert.NotNull(item);
            Assert.Equal("bbb-meet-room-101", item.BbbMeetID);
            Assert.Equal("https://vc.hekmateislami.com/playback/presentation/2.3/rec-guid-12345", item.RecordingPlaybackUrl);
            Assert.Equal("rec-guid-12345", item.RecordingRecordId);
            Assert.Equal("members", item.RecordingViewPermission);
            Assert.False(item.RecordingDownloadable);
            Assert.True(item.RecordingPublished);
        }
    }
}
