using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Cache.Handlers;
using Application.DTO;
using Application.Interfaces;
using Application.Services;
using CacheHandler.Events;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Presistence.Entities;
using Xunit;

namespace CoreMe.Tests
{
    public class AdminCacheServiceTests
    {
        private DBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new DBContext(options);
        }

        private IMemoryCache CreateMemoryCache()
        {
            return new MemoryCache(new MemoryCacheOptions());
        }

        private UserTBL CreateSuperAdminUser()
        {
            return new UserTBL
            {
                ID = 1,
                UUID = "uuid-superadmin",
                Username = "admin_super",
                Firstname = "مدیر",
                LastName = "کل",
                Type = MainEntity_Types.USER_ADMINSTRATOR,
                Status = MainEntity_Status.Succedd
            };
        }

        private UserTBL CreateNormalUser()
        {
            return new UserTBL
            {
                ID = 2,
                UUID = "uuid-normal",
                Username = "normal_user",
                Firstname = "کاربر",
                LastName = "عادی",
                Type = MainEntity_Types.Normal,
                Status = MainEntity_Status.Succedd
            };
        }

        private void SeedData(DBContext db)
        {
            var owner = new UserTBL
            {
                ID = 10,
                UUID = "uuid-owner-10",
                Username = "owner_hekmat",
                Firstname = "مدیر",
                LastName = "شعبه",
                Type = MainEntity_Types.Normal,
                IsHekmatShobe = true,
                Status = MainEntity_Status.Succedd
            };
            db.UserTBLs.Add(owner);

            var room = new RoomTBL
            {
                ID = 100,
                FriendlyID = "room-hekmat-1",
                Title = "اتاق شعبه حکمت",
                IDOwnerTBL = owner.ID,
                IsPublicRoom = true,
                Status = MainEntity_Status.Enabled
            };
            db.RoomTBLs.Add(room);

            var planFuture = new RoomTBLPlanTBL
            {
                ID = 501,
                IDRoomTBL = room.ID,
                Title = "برنامه آینده",
                Teachers = "استاد نمونه",
                StartDate = DateTime.Now.AddDays(2),
                Status = MainEntity_Status.Enabled
            };
            var planPast = new RoomTBLPlanTBL
            {
                ID = 502,
                IDRoomTBL = room.ID,
                Title = "جلسه برگزار شده",
                Teachers = "استاد نمونه",
                StartDate = DateTime.Now.AddDays(-2),
                Status = MainEntity_Status.Enabled
            };
            db.RoomTBLPlanTBLs.AddRange(planFuture, planPast);
            db.SaveChanges();
        }

        [Fact]
        public async Task GetAllCacheDetails_SuperAdmin_ReturnsEntriesAndMemoryStats()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var cache = CreateMemoryCache();
            cache.Set("TestKey1", "Value 1", TimeSpan.FromMinutes(10));
            cache.Set("TestKey2", new { Name = "Obj2", Count = 5 }, TimeSpan.FromMinutes(20));

            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var result = await service.GetCacheDetailsAsync(superAdmin);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Stats);
            Assert.True(result.Stats.TotalAllocatedMemoryBytes > 0);
            Assert.False(string.IsNullOrEmpty(result.Stats.FormattedTotalMemory));
            Assert.True(result.Entries.Count >= 2);
            Assert.Contains(result.Entries, e => e.Key == "TestKey1");
            Assert.Contains(result.Entries, e => e.Key == "TestKey2");
            var entry1 = result.Entries.First(e => e.Key == "TestKey1");
            Assert.False(string.IsNullOrEmpty(entry1.MemoryAddress));
            Assert.True(entry1.EstimatedSizeBytes > 0);
        }

        [Fact]
        public async Task GetAllCacheDetails_NonSuperAdmin_ThrowsUnauthorized()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var cache = CreateMemoryCache();
            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var normalUser = CreateNormalUser();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetCacheDetailsAsync(normalUser));
        }

        [Fact]
        public async Task ClearKey_RemovesSpecificKey()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var cache = CreateMemoryCache();
            cache.Set("KeyA", "DataA");
            cache.Set("KeyB", "DataB");

            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var cleared = await service.ClearKeyAsync("KeyA", superAdmin);

            // Assert
            Assert.True(cleared);
            Assert.False(cache.TryGetValue("KeyA", out _));
            Assert.True(cache.TryGetValue("KeyB", out _));
        }

        [Fact]
        public async Task ClearKeys_RemovesSelectedKeys()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var cache = CreateMemoryCache();
            cache.Set("Key1", "Data1");
            cache.Set("Key2", "Data2");
            cache.Set("Key3", "Data3");

            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var count = await service.ClearKeysAsync(new List<string> { "Key1", "Key2" }, superAdmin);

            // Assert
            Assert.Equal(2, count);
            Assert.False(cache.TryGetValue("Key1", out _));
            Assert.False(cache.TryGetValue("Key2", out _));
            Assert.True(cache.TryGetValue("Key3", out _));
        }

        [Fact]
        public async Task ClearAll_RemovesAllKeys()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            var cache = CreateMemoryCache();
            cache.Set("Key1", "Data1");
            cache.Set("Key2", "Data2");

            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var count = await service.ClearAllAsync(superAdmin);

            // Assert
            Assert.True(count >= 2);
            Assert.False(cache.TryGetValue("Key1", out _));
            Assert.False(cache.TryGetValue("Key2", out _));
        }

        [Fact]
        public async Task ReCacheKey_PlanRoomPublic_RepopulatesCache()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedData(db);
            var cache = CreateMemoryCache();
            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var recached = await service.ReCacheKeyAsync("PlanRoomPublic", superAdmin);

            // Assert
            Assert.True(recached);
            Assert.True(cache.TryGetValue("PlanRoomPublic", out var val));
            Assert.NotNull(val);
        }

        [Fact]
        public async Task ReCacheKey_LatestMeetings_RepopulatesCache()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedData(db);
            var cache = CreateMemoryCache();
            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var recached = await service.ReCacheKeyAsync("LatestMeetings", superAdmin);

            // Assert
            Assert.True(recached);
            Assert.True(cache.TryGetValue("LatestMeetings", out var val));
            Assert.NotNull(val);
        }

        [Fact]
        public async Task ReCacheKey_HekmatShobeList_RepopulatesCache()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedData(db);
            var cache = CreateMemoryCache();
            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var recached = await service.ReCacheKeyAsync("HekmatShobeList", superAdmin);

            // Assert
            Assert.True(recached);
            Assert.True(cache.TryGetValue("HekmatShobeList", out var val));
            Assert.NotNull(val);
        }

        [Fact]
        public async Task ReCacheAll_RepopulatesRegisteredCaches()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedData(db);
            var cache = CreateMemoryCache();
            var service = new AdminCacheService(cache, db, NullLogger<AdminCacheService>.Instance);
            var superAdmin = CreateSuperAdminUser();

            // Act
            var count = await service.ReCacheAllAsync(superAdmin);

            // Assert
            Assert.True(count >= 3);
            Assert.True(cache.TryGetValue("PlanRoomPublic", out _));
            Assert.True(cache.TryGetValue("LatestMeetings", out _));
            Assert.True(cache.TryGetValue("HekmatShobeList", out _));
        }

        [Fact]
        public async Task RoomCacheInvalidationHandler_InvalidatesRoomCachesOnRoomCreated()
        {
            // Arrange
            var db = CreateInMemoryContext(Guid.NewGuid().ToString());
            SeedData(db);
            var cache = CreateMemoryCache();

            // Populate some room caches
            cache.Set("GetAllRooms_null_null", "OldRooms");
            cache.Set("GetRoomsPublic_null_null", "OldPublicRooms");
            cache.Set("GetRooms_User_10_Query_", "OldUserRooms");
            cache.Set("HekmatShobeList", "OldShobe");

            var handler = new RoomCacheInvalidationHandler(cache, db, NullLogger<RoomCacheInvalidationHandler>.Instance);

            // Act
            await handler.Handle(new RoomUpdatedEvent
            {
                RoomId = 100,
                Action = "Create"
            }, CancellationToken.None);

            // Assert
            Assert.False(cache.TryGetValue("GetAllRooms_null_null", out _));
            Assert.False(cache.TryGetValue("GetRoomsPublic_null_null", out _));
            Assert.False(cache.TryGetValue("GetRooms_User_10_Query_", out _));
            // Since owner has IsHekmatShobe = true, HekmatShobeList was invalidated & recached
            Assert.True(cache.TryGetValue("HekmatShobeList", out var newShobe));
            Assert.NotEqual("OldShobe", newShobe);
        }
    }
}
