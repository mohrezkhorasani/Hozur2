using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.Services;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Presistence.Entities;
using Xunit;

namespace CoreMe.Tests
{
    public class AdminParticipantServiceTests
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
            // 1. Users (Managers & Participants)
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

            var roomOwner = new UserTBL
            {
                ID = 2,
                UUID = "uuid-owner",
                Username = "room_owner",
                Firstname = "مالک",
                LastName = "اتاق",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var roomAdmin = new UserTBL
            {
                ID = 3,
                UUID = "uuid-admin",
                Username = "room_admin",
                Firstname = "مدیر",
                LastName = "اتاق",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var userNoRooms = new UserTBL
            {
                ID = 4,
                UUID = "uuid-noroom",
                Username = "user_noroom",
                Firstname = "کاربر",
                LastName = "بدون اتاق",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            // Participant Users
            var participant12 = new UserTBL
            {
                ID = 12,
                UUID = "uuid-part-12",
                Username = "alirezai",
                Firstname = "علیرضا",
                LastName = "رضایی",
                PhoneNumber = "09121111111",
                NationalCode = "0011223344",
                Email = "alireza@example.com",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var participant13 = new UserTBL
            {
                ID = 13,
                UUID = "uuid-part-13",
                Username = "zahram",
                Firstname = "زهرا",
                LastName = "محمدی",
                PhoneNumber = "", // missing phone
                NationalCode = "0022334455",
                Email = "zahra@example.com",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var participant14 = new UserTBL
            {
                ID = 14,
                UUID = "uuid-part-14",
                Username = "hasana",
                Firstname = "حسن",
                LastName = "احمدی",
                PhoneNumber = "09122222222",
                NationalCode = "", // missing national code
                Email = "hasan@example.com",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            var participantOtherRoomOnly = new UserTBL
            {
                ID = 15,
                UUID = "uuid-part-15",
                Username = "maryams",
                Firstname = "مریم",
                LastName = "سلیمانی",
                PhoneNumber = "09123333333",
                NationalCode = "0033445566",
                Email = "maryam@example.com",
                Type = MainEntity_Types.USER_USER,
                Status = MainEntity_Status.Succedd
            };

            db.UserTBLs.AddRange(superAdmin, roomOwner, roomAdmin, userNoRooms,
                participant12, participant13, participant14, participantOtherRoomOnly);

            // 2. Rooms
            var ownedRoom = new RoomTBL
            {
                ID = 10,
                FriendlyID = "room-owned-10",
                Title = "اتاق مالک ۲",
                IDOwnerTBL = 2,
                Status = MainEntity_Status.Succedd
            };

            var adminRoom = new RoomTBL
            {
                ID = 20,
                FriendlyID = "room-admin-20",
                Title = "اتاق ادمین ۳",
                IDOwnerTBL = 999, // Owned by someone else
                Status = MainEntity_Status.Succedd
            };

            var otherRoom = new RoomTBL
            {
                ID = 30,
                FriendlyID = "room-other-30",
                Title = "اتاق شخص ثالث",
                IDOwnerTBL = 888,
                Status = MainEntity_Status.Succedd
            };

            db.RoomTBLs.AddRange(ownedRoom, adminRoom, otherRoom);

            // 3. Room to User Mapping (room admin for user 3)
            var adminMapping = new RoomTBLToUserTBL
            {
                ID = 101,
                FriendlyID = "room-admin-20",
                FriendlyIDRoomTBL = "room-admin-20",
                UUIDParticipatorTBL = "uuid-admin",
                Type = MainEntity_Types.ROOMTOUSER_ADMIN, // 551
                Status = MainEntity_Status.Succedd
            };
            db.RoomTBLToUserTBLs.Add(adminMapping);

            // 4. Meetings
            // In Owned Room (2 meetings)
            var meet1 = new MeetingTBL
            {
                ID = 1001,
                MeetID = "meet-1001",
                Title = "جلسه اول اتاق تحت مالکیت",
                RoomFriendlyID = "room-owned-10",
                DateInsert = DateTime.UtcNow.AddDays(-5),
                Status = MainEntity_Status.Succedd
            };
            var meet2 = new MeetingTBL
            {
                ID = 1002,
                MeetID = "meet-1002",
                Title = "جلسه دوم اتاق تحت مالکیت",
                RoomFriendlyID = "room-owned-10",
                DateInsert = DateTime.UtcNow.AddDays(-2),
                Status = MainEntity_Status.Succedd
            };

            // In Admin Room (1 meeting)
            var meet3 = new MeetingTBL
            {
                ID = 2001,
                MeetID = "meet-2001",
                Title = "جلسه اول اتاق ادمین",
                RoomFriendlyID = "room-admin-20",
                DateInsert = DateTime.UtcNow.AddDays(-1),
                Status = MainEntity_Status.Succedd
            };

            // In Other Room (1 meeting)
            var meet4 = new MeetingTBL
            {
                ID = 3001,
                MeetID = "meet-3001",
                Title = "جلسه اتاق دیگران",
                RoomFriendlyID = "room-other-30",
                DateInsert = DateTime.UtcNow,
                Status = MainEntity_Status.Succedd
            };

            db.MeetingTBLs.AddRange(meet1, meet2, meet3, meet4);

            // 5. UserTBLToMeetingTBL attendances
            // Participant 12 attended meet1 and meet2 (in owned room) AND meet3 (in admin room) -> Total 3 meetings
            var att1 = new UserTBLToMeetingTBL
            {
                ID = 5001,
                UUID = "uuid-part-12",
                MeetID = "meet-1001",
                DateInsert = DateTime.UtcNow.AddDays(-5),
                TimeSpendSeconds = 1200,
                TokenJWT = "jwt-1",
                UserMeetID = "umid-1",
                UserNickName = "علیرضا"
            };
            var att2 = new UserTBLToMeetingTBL
            {
                ID = 5002,
                UUID = "uuid-part-12",
                MeetID = "meet-1002",
                DateInsert = DateTime.UtcNow.AddDays(-2),
                TimeSpendSeconds = 2400,
                TokenJWT = "jwt-2",
                UserMeetID = "umid-2",
                UserNickName = "علیرضا"
            };
            var att3 = new UserTBLToMeetingTBL
            {
                ID = 5003,
                UUID = "uuid-part-12",
                MeetID = "meet-2001",
                DateInsert = DateTime.UtcNow.AddDays(-1),
                TimeSpendSeconds = 3600,
                TokenJWT = "jwt-3",
                UserMeetID = "umid-3",
                UserNickName = "علیرضا"
            };

            // Participant 13 attended meet1 (in owned room)
            var att4 = new UserTBLToMeetingTBL
            {
                ID = 5004,
                UUID = "uuid-part-13",
                MeetID = "meet-1001",
                DateInsert = DateTime.UtcNow.AddDays(-5),
                TimeSpendSeconds = 900,
                TokenJWT = "jwt-4",
                UserMeetID = "umid-4",
                UserNickName = "زهرا"
            };

            // Participant 14 attended meet3 (in admin room)
            var att5 = new UserTBLToMeetingTBL
            {
                ID = 5005,
                UUID = "uuid-part-14",
                MeetID = "meet-2001",
                DateInsert = DateTime.UtcNow.AddDays(-1),
                TimeSpendSeconds = 1500,
                TokenJWT = "jwt-5",
                UserMeetID = "umid-5",
                UserNickName = "حسن"
            };

            // Participant 15 only attended meet4 (in other room)
            var att6 = new UserTBLToMeetingTBL
            {
                ID = 5006,
                UUID = "uuid-part-15",
                MeetID = "meet-3001",
                DateInsert = DateTime.UtcNow,
                TimeSpendSeconds = 800,
                TokenJWT = "jwt-6",
                UserMeetID = "umid-6",
                UserNickName = "مریم"
            };

            db.UserTBLToMeetingTBLs.AddRange(att1, att2, att3, att4, att5, att6);

            db.SaveChanges();
        }

        [Fact]
        public async Task GetParticipantsAsync_SuperAdmin_ReturnsAllDistinctParticipantsAcrossAllRooms()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantsAsync_SuperAdmin_ReturnsAllDistinctParticipantsAcrossAllRooms));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 1);

            var result = await service.GetParticipantsAsync(superAdmin, page: 1, count: 10);

            // Total 4 distinct participants: 12, 13, 14, 15
            Assert.Equal(4, result.TotalCount);
            Assert.Equal(4, result.Items.Count);

            // User 12 attended meet1, meet2, meet3 (3 meetings total)
            var item12 = result.Items.FirstOrDefault(i => i.Id == 12);
            Assert.NotNull(item12);
            Assert.Equal(3, item12.AttendedMeetingsCount);

            // Verify user 12 appears ONLY ONCE
            Assert.Single(result.Items.Where(i => i.Id == 12));
        }

        [Fact]
        public async Task GetParticipantsAsync_RoomOwner_ReturnsOnlyParticipantsFromOwnedRooms()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantsAsync_RoomOwner_ReturnsOnlyParticipantsFromOwnedRooms));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var roomOwner = await db.UserTBLs.FirstAsync(u => u.ID == 2);

            var result = await service.GetParticipantsAsync(roomOwner, page: 1, count: 10);

            // In owned room (room-owned-10), participants are 12 (attended meet1 and meet2) and 13 (attended meet1).
            // Participant 14 is only in admin room. Participant 15 is only in other room.
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);

            Assert.Contains(result.Items, i => i.Id == 12);
            Assert.Contains(result.Items, i => i.Id == 13);
            Assert.DoesNotContain(result.Items, i => i.Id == 14);
            Assert.DoesNotContain(result.Items, i => i.Id == 15);

            // In the owner's rooms, user 12 attended 2 meetings
            var item12 = result.Items.First(i => i.Id == 12);
            Assert.Equal(2, item12.AttendedMeetingsCount);
        }

        [Fact]
        public async Task GetParticipantsAsync_RoomAdmin_ReturnsOnlyParticipantsFromAdminRooms()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantsAsync_RoomAdmin_ReturnsOnlyParticipantsFromAdminRooms));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var roomAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 3);

            var result = await service.GetParticipantsAsync(roomAdmin, page: 1, count: 10);

            // In admin room (room-admin-20), participants are 12 and 14 (meet-2001).
            Assert.Equal(2, result.TotalCount);
            Assert.Contains(result.Items, i => i.Id == 12);
            Assert.Contains(result.Items, i => i.Id == 14);
            Assert.DoesNotContain(result.Items, i => i.Id == 13);
            Assert.DoesNotContain(result.Items, i => i.Id == 15);

            // In the admin's rooms, user 12 attended 1 meeting (meet3)
            var item12 = result.Items.First(i => i.Id == 12);
            Assert.Equal(1, item12.AttendedMeetingsCount);
        }

        [Fact]
        public async Task GetParticipantsAsync_UserWithNoRooms_ReturnsEmpty()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantsAsync_UserWithNoRooms_ReturnsEmpty));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var userNoRooms = await db.UserTBLs.FirstAsync(u => u.ID == 4);

            var result = await service.GetParticipantsAsync(userNoRooms, page: 1, count: 10);

            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetParticipantsAsync_Pagination_ReturnsCorrect10ItemsPerPageAndTotalCount()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantsAsync_Pagination_ReturnsCorrect10ItemsPerPageAndTotalCount));
            SeedSampleData(db);

            // Add 25 more participants to meet1
            for (int i = 100; i < 125; i++)
            {
                var p = new UserTBL
                {
                    ID = i,
                    UUID = $"uuid-part-{i}",
                    Username = $"user_{i}",
                    Firstname = $"نام_{i}",
                    LastName = $"فامیل_{i}",
                    Status = MainEntity_Status.Succedd
                };
                db.UserTBLs.Add(p);
                db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL
                {
                    ID = 10000 + i,
                    UUID = $"uuid-part-{i}",
                    MeetID = "meet-1001",
                    DateInsert = DateTime.UtcNow,
                    TokenJWT = $"jwt-{i}",
                    UserMeetID = $"umid-{i}",
                    UserNickName = $"nick_{i}"
                });
            }
            db.SaveChanges();

            var service = new AdminParticipantService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 1);

            // Page 1: 10 items
            var page1 = await service.GetParticipantsAsync(superAdmin, page: 1, count: 10);
            Assert.Equal(29, page1.TotalCount); // 4 initial + 25 added
            Assert.Equal(10, page1.Items.Count);
            Assert.Equal(1, page1.Page);
            Assert.Equal(10, page1.PageSize);
            Assert.Equal(3, page1.TotalPages);

            // Page 2: 10 items
            var page2 = await service.GetParticipantsAsync(superAdmin, page: 2, count: 10);
            Assert.Equal(29, page2.TotalCount);
            Assert.Equal(10, page2.Items.Count);
            Assert.Equal(2, page2.Page);

            // Page 3: 9 items
            var page3 = await service.GetParticipantsAsync(superAdmin, page: 3, count: 10);
            Assert.Equal(29, page3.TotalCount);
            Assert.Equal(9, page3.Items.Count);
            Assert.Equal(3, page3.Page);
        }

        [Fact]
        public async Task GetParticipantsAsync_SearchQuery_FiltersCorrectly()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantsAsync_SearchQuery_FiltersCorrectly));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 1);

            // Search by name "علیرضا"
            var searchByName = await service.GetParticipantsAsync(superAdmin, page: 1, count: 10, q: "علیرضا");
            Assert.Single(searchByName.Items);
            Assert.Equal(12, searchByName.Items[0].Id);

            // Search by national code
            var searchByNatCode = await service.GetParticipantsAsync(superAdmin, page: 1, count: 10, q: "0011223344");
            Assert.Single(searchByNatCode.Items);
            Assert.Equal(12, searchByNatCode.Items[0].Id);

            // Search by phone
            var searchByPhone = await service.GetParticipantsAsync(superAdmin, page: 1, count: 10, q: "09121111111");
            Assert.Single(searchByPhone.Items);
            Assert.Equal(12, searchByPhone.Items[0].Id);
        }

        [Fact]
        public async Task GetParticipantMeetingsAsync_ReturnsMeetingsInAccessibleRooms_OrderedNewestFirst()
        {
            using var db = CreateInMemoryContext(nameof(GetParticipantMeetingsAsync_ReturnsMeetingsInAccessibleRooms_OrderedNewestFirst));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var roomOwner = await db.UserTBLs.FirstAsync(u => u.ID == 2);

            // User 12 attended meet1 and meet2 in owned room, and meet3 in admin room
            var meetings = await service.GetParticipantMeetingsAsync(roomOwner, participantUserId: 12);

            // Owner should only see meet1 and meet2, NOT meet3
            Assert.Equal(2, meetings.Count);
            Assert.Contains(meetings, m => m.MeetId == "meet-1001");
            Assert.Contains(meetings, m => m.MeetId == "meet-1002");
            Assert.DoesNotContain(meetings, m => m.MeetId == "meet-2001");

            // Ordered newest first (meet-1002 is -2 days, meet-1001 is -5 days)
            Assert.Equal("meet-1002", meetings[0].MeetId);
            Assert.Equal("meet-1001", meetings[1].MeetId);
        }

        [Fact]
        public async Task CanIssueCertificate_Validation_RequiresNationalCodeAndPhone()
        {
            using var db = CreateInMemoryContext(nameof(CanIssueCertificate_Validation_RequiresNationalCodeAndPhone));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 1);

            var result = await service.GetParticipantsAsync(superAdmin, page: 1, count: 10);

            // User 12 has both NationalCode and PhoneNumber -> CanIssueCertificate is true
            var item12 = result.Items.First(i => i.Id == 12);
            Assert.True(item12.CanIssueCertificate);

            // User 13 has NationalCode but empty PhoneNumber -> CanIssueCertificate is false
            var item13 = result.Items.First(i => i.Id == 13);
            Assert.False(item13.CanIssueCertificate);

            // User 14 has PhoneNumber but empty NationalCode -> CanIssueCertificate is false
            var item14 = result.Items.First(i => i.Id == 14);
            Assert.False(item14.CanIssueCertificate);
        }

        [Fact]
        public async Task CanUserAccessParticipantsPageAsync_OwnerAndAdminAllowed_OthersDenied()
        {
            using var db = CreateInMemoryContext(nameof(CanUserAccessParticipantsPageAsync_OwnerAndAdminAllowed_OthersDenied));
            SeedSampleData(db);

            var service = new AdminParticipantService(db);
            var superAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 1);
            var roomOwner = await db.UserTBLs.FirstAsync(u => u.ID == 2);
            var roomAdmin = await db.UserTBLs.FirstAsync(u => u.ID == 3);
            var userNoRooms = await db.UserTBLs.FirstAsync(u => u.ID == 4);

            Assert.True(await service.CanUserAccessParticipantsPageAsync(superAdmin));
            Assert.True(await service.CanUserAccessParticipantsPageAsync(roomOwner));
            Assert.True(await service.CanUserAccessParticipantsPageAsync(roomAdmin));
            Assert.False(await service.CanUserAccessParticipantsPageAsync(userNoRooms));
        }
    }
}
