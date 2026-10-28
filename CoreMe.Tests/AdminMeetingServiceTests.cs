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
    }
}
