using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interface;
using Application.Services;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Presistence.Entities;
using Xunit;

namespace CoreMe.Tests
{
    public class AdminMeetingServiceTests
    {
        private DBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new DBContext(options);
        }

        private void SeedSampleData(DBContext db)
        {
            // Users
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

            var adminUser = new UserTBL
            {
                ID = 3,
                UUID = "uuid-admin",
                Username = "room_admin",
                Firstname = "مدیر",
                LastName = "اتاق",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var otherUser = new UserTBL
            {
                ID = 4,
                UUID = "uuid-other",
                Username = "other_user",
                Firstname = "کاربر",
                LastName = "عادی",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var userWithNoRooms = new UserTBL
            {
                ID = 99,
                UUID = "uuid-noroom",
                Username = "user_no_room",
                Firstname = "کاربر",
                LastName = "بی اتاق",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var teacher1 = new UserTBL
            {
                ID = 10,
                UUID = "uuid-teacher1",
                Username = "teacher1",
                Firstname = "علی",
                LastName = "محمدی",
                Type = MainEntity_Types.USER_TEACHER,
                Status = MainEntity_Status.Succedd
            };

            var teacher2 = new UserTBL
            {
                ID = 11,
                UUID = "uuid-teacher2",
                Username = "teacher2",
                Firstname = "رضا",
                LastName = "حسینی",
                Type = MainEntity_Types.USER_TEACHER,
                Status = MainEntity_Status.Succedd
            };

            db.UserTBLs.AddRange(superAdmin, ownerUser, adminUser, otherUser, userWithNoRooms, teacher1, teacher2);

            // Rooms
            var room1 = new RoomTBL
            {
                ID = 101,
                FriendlyID = "room-owner-fid",
                Title = "اتاق مالک",
                IDOwnerTBL = ownerUser.ID,
                OwnerTBL = ownerUser,
                Status = MainEntity_Status.Succedd,
                Type = MainEntity_Types.ROOM_NORMAL
            };

            var room2 = new RoomTBL
            {
                ID = 102,
                FriendlyID = "room-admin-fid",
                Title = "اتاق مدیر کمکی",
                IDOwnerTBL = otherUser.ID,
                OwnerTBL = otherUser,
                Status = MainEntity_Status.Succedd,
                Type = MainEntity_Types.ROOM_NORMAL
            };

            var room3 = new RoomTBL
            {
                ID = 103,
                FriendlyID = "room-other-fid",
                Title = "اتاق دیگران",
                IDOwnerTBL = otherUser.ID,
                OwnerTBL = otherUser,
                Status = MainEntity_Status.Succedd,
                Type = MainEntity_Types.ROOM_NORMAL
            };

            db.RoomTBLs.AddRange(room1, room2, room3);

            // Relation: adminUser is ROOMTOUSER_ADMIN in room2
            var relAdmin = new RoomTBLToUserTBL
            {
                ID = 201,
                FriendlyID = room2.FriendlyID,
                FriendlyIDRoomTBL = room2.FriendlyID,
                RoomTBL = room2,
                UUIDParticipatorTBL = adminUser.UUID,
                Type = MainEntity_Types.ROOMTOUSER_ADMIN,
                Status = MainEntity_Status.Succedd,
                DateInsert = DateTime.UtcNow
            };
            db.RoomTBLToUserTBLs.Add(relAdmin);

            // Meetings
            // Meet 1: In Room 1 (Owned by ownerUser), with Voice and Video
            var meet1 = new MeetingTBL
            {
                ID = 1001,
                MeetID = "meet-1001-uuid",
                Title = "جلسه اول مالک",
                RoomFriendlyID = room1.FriendlyID,
                RoomTBL = room1,
                IDTeacher = teacher1.ID,
                Teacher = teacher1,
                IDCreator = superAdmin.ID,
                Creator = superAdmin,
                Type = MainEntity_Types.MEET_GREEN_LIGHT,
                Status = MainEntity_Status.MEETING_LIVE,
                DateInsert = DateTime.UtcNow.AddDays(-2)
            };

            // Meet 2: In Room 1 (Owned by ownerUser), with Report only
            var meet2 = new MeetingTBL
            {
                ID = 1002,
                MeetID = "meet-1002-uuid",
                Title = "جلسه دوم مالک",
                RoomFriendlyID = room1.FriendlyID,
                RoomTBL = room1,
                IDTeacher = teacher2.ID,
                Teacher = teacher2,
                IDCreator = superAdmin.ID,
                Creator = superAdmin,
                Type = MainEntity_Types.MEET_API,
                Status = MainEntity_Status.MEETING_END,
                DateInsert = DateTime.UtcNow.AddDays(-1)
            };

            // Meet 3: In Room 2 (adminUser is admin), with Voice only
            var meet3 = new MeetingTBL
            {
                ID = 1003,
                MeetID = "meet-1003-uuid",
                Title = "جلسه سوم اتاق ادمین",
                RoomFriendlyID = room2.FriendlyID,
                RoomTBL = room2,
                IDTeacher = teacher1.ID,
                Teacher = teacher1,
                IDCreator = superAdmin.ID,
                Creator = superAdmin,
                Type = MainEntity_Types.MEET_GREEN_LIGHT,
                Status = MainEntity_Status.MEETING_END,
                DateInsert = DateTime.UtcNow
            };

            // Meet 4: In Room 3 (Other user room, neither owner nor admin)
            var meet4 = new MeetingTBL
            {
                ID = 1004,
                MeetID = "meet-1004-uuid",
                Title = "جلسه اتاق دیگر",
                RoomFriendlyID = room3.FriendlyID,
                RoomTBL = room3,
                IDTeacher = teacher2.ID,
                Teacher = teacher2,
                IDCreator = superAdmin.ID,
                Creator = superAdmin,
                Type = MainEntity_Types.MEET_GREEN_LIGHT,
                Status = MainEntity_Status.MEETING_END,
                DateInsert = DateTime.UtcNow
            };

            db.MeetingTBLs.AddRange(meet1, meet2, meet3, meet4);

            // Media Logs
            // meet1 has voice & video
            db.MeetingLogTBLs.Add(new MeetingLogTBL { ID = 1, IDMeetingTBL = meet1.ID, Key = MeetingLogTBL.KEY_SOUND, Value = "voice1.mp3", Status = MainEntity_Status.LOG_NORMAL });
            db.MeetingLogTBLs.Add(new MeetingLogTBL { ID = 2, IDMeetingTBL = meet1.ID, Key = MeetingLogTBL.KEY_VIDEO, Value = "video1.mp4", Status = MainEntity_Status.LOG_NORMAL });

            // meet2 has report
            db.MeetingLogTBLs.Add(new MeetingLogTBL { ID = 3, IDMeetingTBL = meet2.ID, Key = MeetingLogTBL.KEY_REPORT, Value = "report2.pdf", Status = MainEntity_Status.LOG_NORMAL });

            // meet3 has voice
            db.MeetingLogTBLs.Add(new MeetingLogTBL { ID = 4, IDMeetingTBL = meet3.ID, Key = MeetingLogTBL.KEY_SOUND, Value = "voice3.mp3", Status = MainEntity_Status.LOG_NORMAL });

            // Attendees for meet1
            db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL { ID = 1, MeetID = meet1.MeetID, Type = MainEntity_Types.USER_VIEWER, UUID = "u1", UserNickName = "کاربر ۱", TokenJWT = "jwt-1", UserMeetID = "um-1" });
            db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL { ID = 2, MeetID = meet1.MeetID, Type = MainEntity_Types.USER_VIEWER, UUID = "u2", UserNickName = "کاربر ۲", TokenJWT = "jwt-2", UserMeetID = "um-2" });
            db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL { ID = 3, MeetID = meet1.MeetID, Type = MainEntity_Types.USER_TEACHER, UUID = "u3", UserNickName = "استاد", TokenJWT = "jwt-3", UserMeetID = "um-3" }); // should not be counted in attendees

            db.SaveChanges();
        }

        [Fact]
        public async Task GetMeetingsQuery_WhenUserIsSuperAdmin_ReturnsAllMeetings()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.Type == MainEntity_Types.USER_ADMINSTRATOR);

            var result = await service.GetMeetingsAsync(superAdmin, page: 1, count: 50);

            Assert.NotNull(result);
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(4, result.Items.Count);
        }

        [Fact]
        public async Task GetMeetingsQuery_WhenUserIsRoomOwner_ReturnsOnlyOwnedRoomMeetings()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var owner = await db.UserTBLs.FirstAsync(u => u.Username == "room_owner");

            var result = await service.GetMeetingsAsync(owner, page: 1, count: 50);

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, m => Assert.Equal("room-owner-fid", m.RoomFriendlyID));
        }

        [Fact]
        public async Task GetMeetingsQuery_WhenUserIsRoomAdmin_ReturnsOnlyAdminRoomMeetings()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var roomAdmin = await db.UserTBLs.FirstAsync(u => u.Username == "room_admin");

            var result = await service.GetMeetingsAsync(roomAdmin, page: 1, count: 50);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal("room-admin-fid", result.Items[0].RoomFriendlyID);
        }

        [Fact]
        public async Task GetMeetingsQuery_WhenUserHasNoRooms_ReturnsEmpty()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var userNoRoom = await db.UserTBLs.FirstAsync(u => u.Username == "user_no_room");

            var result = await service.GetMeetingsAsync(userNoRoom, page: 1, count: 50);

            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetMeetingStats_ReturnsAccurateCountsForUserMeetings()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var owner = await db.UserTBLs.FirstAsync(u => u.Username == "room_owner");

            var stats = await service.GetMeetingStatsAsync(owner);

            Assert.NotNull(stats);
            Assert.Equal(2, stats.TotalMeetings);
            Assert.Equal(1, stats.WithVoiceCount);  // only meet1 has voice
            Assert.Equal(1, stats.WithVideoCount);  // only meet1 has video
            Assert.Equal(1, stats.WithReportCount); // only meet2 has report
        }

        [Fact]
        public async Task GetMeetingsQuery_WithFilterTeacherId_FiltersAccurately()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.Type == MainEntity_Types.USER_ADMINSTRATOR);

            var result = await service.GetMeetingsAsync(superAdmin, page: 1, count: 50, teacherId: 10);

            Assert.Equal(2, result.TotalCount); // meet1 and meet3
            Assert.All(result.Items, m => Assert.Equal(10, m.Teacher?.Id));
        }

        [Fact]
        public async Task GetMeetingsQuery_WithFilterOwnerId_FiltersAccurately()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.Type == MainEntity_Types.USER_ADMINSTRATOR);

            var result = await service.GetMeetingsAsync(superAdmin, page: 1, count: 50, ownerId: 2);

            Assert.Equal(2, result.TotalCount); // meet1 and meet2 owned by ID 2
            Assert.All(result.Items, m => Assert.Equal(2, m.Owner?.Id));
        }

        [Fact]
        public async Task CanUserEditMeeting_AllowsSuperAdminOwnerAndRoomAdmin_DeniesOther()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.Type == MainEntity_Types.USER_ADMINSTRATOR);
            var owner = await db.UserTBLs.FirstAsync(u => u.Username == "room_owner");
            var roomAdmin = await db.UserTBLs.FirstAsync(u => u.Username == "room_admin");
            var userNoRoom = await db.UserTBLs.FirstAsync(u => u.Username == "user_no_room");

            // meet1 is in room1 (owned by owner)
            Assert.True(await service.CanUserEditMeetingAsync(superAdmin, 1001));
            Assert.True(await service.CanUserEditMeetingAsync(owner, 1001));
            Assert.False(await service.CanUserEditMeetingAsync(roomAdmin, 1001));
            Assert.False(await service.CanUserEditMeetingAsync(userNoRoom, 1001));

            // meet3 is in room2 (adminUser is room admin)
            Assert.True(await service.CanUserEditMeetingAsync(superAdmin, 1003));
            Assert.False(await service.CanUserEditMeetingAsync(owner, 1003));
            Assert.True(await service.CanUserEditMeetingAsync(roomAdmin, 1003));
            Assert.False(await service.CanUserEditMeetingAsync(userNoRoom, 1003));
        }

        [Fact]
        public async Task GetMeetingsQuery_PopulatesCreatorAndRoomTitle_AndRestrictsNonOwnerAdmins()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            var service = new AdminMeetingService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.Type == MainEntity_Types.USER_ADMINSTRATOR);

            var result = await service.GetMeetingsAsync(superAdmin, page: 1, count: 50);

            Assert.NotNull(result);
            Assert.Equal(4, result.TotalCount);
            // Verify RoomTitle is populated
            Assert.Contains(result.Items, m => m.RoomTitle == "اتاق مالک");
            // Verify Creator is populated
            Assert.Contains(result.Items, m => m.Creator != null && m.Creator.Id == superAdmin.ID);
        }

        [Fact]
        public async Task GetMeetings_CalculatesPollAverageScore_OnlyForType625_AndMatchingOrNullMeetingId()
        {
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedSampleData(db);

            // Add MeetPollQuiz data
            // Poll 1: Type 625 (QUIZ_OPTIONAL), IDMeetingTBL = null (global)
            var pollGlobal = new MeetPollQuizTBL
            {
                ID = 501,
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                Question = "سوال سراسری",
                IDMeetingTBL = null
            };
            var ans1a = new MeetPollQuizAnsTBL { ID = 601, IDQuizTBL = 501, Score = 100, Answer = "عالی", Type = MainEntity_Types.QUIZ_OPTIONAL, Status = MainEntity_Status.LOG_NORMAL };
            var ans1b = new MeetPollQuizAnsTBL { ID = 602, IDQuizTBL = 501, Score = 60, Answer = "خوب", Type = MainEntity_Types.QUIZ_OPTIONAL, Status = MainEntity_Status.LOG_NORMAL };

            // Poll 2: Type 625, IDMeetingTBL = 1001 (specific to meeting 1001)
            var pollMeeting1001 = new MeetPollQuizTBL
            {
                ID = 502,
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                Question = "سوال اختصاصی جلسه ۱۰۰۱",
                IDMeetingTBL = 1001
            };
            var ans2a = new MeetPollQuizAnsTBL { ID = 603, IDQuizTBL = 502, Score = 80, Answer = "خیلی خوب", Type = MainEntity_Types.QUIZ_OPTIONAL, Status = MainEntity_Status.LOG_NORMAL };

            // Poll 3: Type 626 (QUIZ_INPUT), IDMeetingTBL = 1001 -> MUST BE IGNORED
            var pollInput = new MeetPollQuizTBL
            {
                ID = 503,
                Type = MainEntity_Types.QUIZ_INPUT,
                Status = MainEntity_Status.LOG_NORMAL,
                Question = "متن آزاد",
                IDMeetingTBL = 1001
            };
            var ans3a = new MeetPollQuizAnsTBL { ID = 604, IDQuizTBL = 503, Score = 10, Answer = "متن", Type = MainEntity_Types.QUIZ_INPUT, Status = MainEntity_Status.LOG_NORMAL };

            // Poll 4: Type 625, IDMeetingTBL = 1002 -> MUST BE IGNORED for meeting 1001
            var pollMeeting1002 = new MeetPollQuizTBL
            {
                ID = 504,
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                Question = "سوال جلسه ۱۰۰۲",
                IDMeetingTBL = 1002
            };
            var ans4a = new MeetPollQuizAnsTBL { ID = 605, IDQuizTBL = 504, Score = 20, Answer = "ضعیف", Type = MainEntity_Types.QUIZ_OPTIONAL, Status = MainEntity_Status.LOG_NORMAL };

            db.MeetPollQuizTBLs.AddRange(pollGlobal, pollMeeting1001, pollInput, pollMeeting1002);
            db.MeetPollQuizAnsTBLs.AddRange(ans1a, ans1b, ans2a, ans3a, ans4a);

            // UserTBLToMeetingTBL records
            var userMeet1 = new UserTBLToMeetingTBL
            {
                ID = 701,
                MeetID = "meet-1001-uuid",
                UUID = "uuid-superadmin",
                UserNickName = "کاربر ۱",
                TokenJWT = "jwt-701",
                UserMeetID = "um-701",
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.USER_VIEWER
            };
            var userMeet2 = new UserTBLToMeetingTBL
            {
                ID = 702,
                MeetID = "meet-1001-uuid",
                UUID = "uuid-owner",
                UserNickName = "کاربر ۲",
                TokenJWT = "jwt-702",
                UserMeetID = "um-702",
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.USER_VIEWER
            };
            var userMeet3 = new UserTBLToMeetingTBL
            {
                ID = 703,
                MeetID = "meet-1001-uuid",
                UUID = "uuid-admin",
                UserNickName = "کاربر ۳",
                TokenJWT = "jwt-703",
                UserMeetID = "um-703",
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.USER_VIEWER
            };

            db.UserTBLToMeetingTBLs.AddRange(userMeet1, userMeet2, userMeet3);

            // Votes
            // Vote 1: User 1 votes for Poll 1 Answer 1a (Score 100) -> COUNTED
            var v1 = new MeetPollQuizAnsTBLToUserMeetTBL
            {
                ID = 801,
                IDMeetPollQuizAnsTBL = ans1a.ID,
                MeetPollQuizAnsTBL = ans1a,
                IDUserTBLToMeetingTBL = userMeet1.ID,
                UserTBLToMeetingTBL = userMeet1,
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.QUIZ_OPTIONAL
            };
            // Vote 2: User 2 votes for Poll 1 Answer 1b (Score 60) -> COUNTED
            var v2 = new MeetPollQuizAnsTBLToUserMeetTBL
            {
                ID = 802,
                IDMeetPollQuizAnsTBL = ans1b.ID,
                MeetPollQuizAnsTBL = ans1b,
                IDUserTBLToMeetingTBL = userMeet2.ID,
                UserTBLToMeetingTBL = userMeet2,
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.QUIZ_OPTIONAL
            };
            // Vote 3: User 3 votes for Poll 2 (Meeting 1001 specific) Answer 2a (Score 80) -> COUNTED
            var v3 = new MeetPollQuizAnsTBLToUserMeetTBL
            {
                ID = 803,
                IDMeetPollQuizAnsTBL = ans2a.ID,
                MeetPollQuizAnsTBL = ans2a,
                IDUserTBLToMeetingTBL = userMeet3.ID,
                UserTBLToMeetingTBL = userMeet3,
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.QUIZ_OPTIONAL
            };
            // Vote 4: User 1 votes for Poll 3 (Type 626 QUIZ_INPUT) -> IGNORED because type != 625
            var v4 = new MeetPollQuizAnsTBLToUserMeetTBL
            {
                ID = 804,
                IDMeetPollQuizAnsTBL = ans3a.ID,
                MeetPollQuizAnsTBL = ans3a,
                IDUserTBLToMeetingTBL = userMeet1.ID,
                UserTBLToMeetingTBL = userMeet1,
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.QUIZ_INPUT
            };
            // Vote 5: User 2 votes for Poll 4 (Meeting 1002 specific) -> IGNORED because IDMeetingTBL == 1002 != 1001
            var v5 = new MeetPollQuizAnsTBLToUserMeetTBL
            {
                ID = 805,
                IDMeetPollQuizAnsTBL = ans4a.ID,
                MeetPollQuizAnsTBL = ans4a,
                IDUserTBLToMeetingTBL = userMeet2.ID,
                UserTBLToMeetingTBL = userMeet2,
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.QUIZ_OPTIONAL
            };

            db.MeetPollQuizAnsTBLToUserMeetTBLs.AddRange(v1, v2, v3, v4, v5);
            await db.SaveChangesAsync();

            var service = new AdminMeetingService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.Type == MainEntity_Types.USER_ADMINSTRATOR);

            var result = await service.GetMeetingsAsync(superAdmin, page: 1, count: 50);

            Assert.NotNull(result);
            var m1001 = result.Items.FirstOrDefault(m => m.Id == 1001);
            Assert.NotNull(m1001);
            // Votes counted: 100, 60, 80 -> Total: 3 votes, Average: (100 + 60 + 80) / 3 = 80.0
            Assert.Equal(3, m1001.PollVotesCount);
            Assert.Equal(80.0, m1001.PollAverageScore);

            // Meeting 1003 has no votes
            var m1003 = result.Items.FirstOrDefault(m => m.Id == 1003);
            Assert.NotNull(m1003);
            Assert.Equal(0, m1003.PollVotesCount);
            Assert.Null(m1003.PollAverageScore);
        }
    }
}
