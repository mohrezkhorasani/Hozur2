using Application.General;
using Application.DTO;
using Application.DTO.Generals;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Services;
using Application.Usecase;
using CacheHandler.Events;
using Domain.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Presistence.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CoreMe.Tests
{
    public class MockMediator : IMediator
    {
        public List<object> PublishedEvents { get; } = new List<object>();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(notification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            PublishedEvents.Add(notification);
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => Task.FromResult(default(TResponse)!);
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object?>(null);
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => Task.CompletedTask;
    }

    public class BotPlanAndEditTests
    {
        private DBContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new DBContext(options);
        }

        private IConfiguration CreateConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"Bot:Secret", "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"},
                {"URLFiles", "https://files.hekmat.ir"}
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public async Task BotNewPlanRoom_WithValidSecret_CreatesPlan_AndMatchesTeacher()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            // Seed owner & room
            var admin = new UserTBL
            {
                ID = 1,
                UUID = "admin-uuid",
                Username = "admin",
                Type = MainEntity_Types.USER_ADMINSTRATOR
            };
            db.UserTBLs.Add(admin);

            var room = new RoomTBL
            {
                ID = 10,
                FriendlyID = "jtb-a8e-ydl-05b",
                Title = "اتاق مجمع عالی",
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true,
                IDOwnerTBL = admin.ID
            };
            db.RoomTBLs.Add(room);

            // Seed a registered teacher
            var teacher = new UserTBL
            {
                ID = 1087,
                UUID = "teacher-uuid",
                Username = "dr_hosseini",
                Firstname = "دکتر علی",
                LastName = "حسینی",
                Type = MainEntity_Types.USER_TEACHER
            };
            db.UserTBLs.Add(teacher);
            await db.SaveChangesAsync();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var request = new BotNewPlanRoomRequest
            {
                FriendlyID = "jtb-a8e-ydl-05b",
                Title = "نشست تخصصی فلسفه و کلام",
                Teachers = "حجت الاسلام والمسلمین آقای دکتر علی حسینی(سردبیر)",
                Dabir = "مهندس رضایی(دبیر نشست)",
                Date = "1405/08/13",
                Time = "10:30",
                Secret = "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"
            };

            await usecase.handleAsync(request, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);

            // Check database
            var savedPlan = await db.RoomTBLPlanTBLs.FirstOrDefaultAsync();
            Assert.NotNull(savedPlan);
            Assert.Equal("نشست تخصصی فلسفه و کلام", savedPlan.Title);
            Assert.Equal("دکتر علی حسینی(سردبیر)", savedPlan.Teachers);
            Assert.Equal("مهندس رضایی(دبیر نشست)", savedPlan.Dabir);
            Assert.Equal(MainEntity_Status.Enabled, savedPlan.Status);
            Assert.True(mediator.PublishedEvents.Any(e => e is PlanUpdateEvent));
            Assert.True(mediator.PublishedEvents.Any(e => e is RoomUpdatedEvent));
        }

        [Fact]
        public async Task BotNewPlanRoom_WithInvalidSecret_ReturnsUnauthorized()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var request = new BotNewPlanRoomRequest
            {
                Title = "نشست تستی",
                Date = "1405/08/13",
                Time = "10:30",
                Secret = "wrong-secret"
            };

            await usecase.handleAsync(request, output);

            Assert.NotNull(output.Result);
            Assert.Equal(401, output.Result.data.Status);
        }

        [Fact]
        public async Task UpdatePlanRoom_UpdatesExistingPlan()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();

            var admin = new UserTBL
            {
                ID = 1,
                UUID = "admin-uuid",
                Username = "admin",
                Type = MainEntity_Types.USER_ADMINSTRATOR
            };
            db.UserTBLs.Add(admin);

            var room = new RoomTBL
            {
                ID = 10,
                FriendlyID = "jtb-a8e-ydl-05b",
                Title = "اتاق مجمع عالی",
                Status = MainEntity_Status.Enabled,
                IDOwnerTBL = admin.ID
            };
            db.RoomTBLs.Add(room);

            var plan = new RoomTBLPlanTBL
            {
                ID = 55,
                IDRoomTBL = room.ID,
                RoomTBL = room,
                Title = "عنوان قبلی",
                Teachers = "استاد قبلی",
                Status = MainEntity_Status.Enabled,
                StartDate = DateTime.UtcNow
            };
            db.RoomTBLPlanTBLs.Add(plan);
            await db.SaveChangesAsync();

            var usecase = new UpdatePlanRoomUseCase(db, config, mediator);
            var output = new MockOutputPort<GenericResponse<UpdatePlanRoomResponse>>();

            var request = new UpdatePlanRoomRequest
            {
                IDPlan = 55,
                Title = "عنوان ویرایش شده جدید",
                Teachers = "دکتر جدید(استاد مدعو)",
                Date = "1405/09/20",
                Time = "14:00",
                GeneralHeaders = new Application.DTO.Generals.GeneralHeaders
                {
                    UserTBL = admin
                }
            };

            await usecase.handleAsync(request, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);

            var updatedPlan = await db.RoomTBLPlanTBLs.FindAsync((long)55);
            Assert.NotNull(updatedPlan);
            Assert.Equal("عنوان ویرایش شده جدید", updatedPlan.Title);
            Assert.Contains("دکتر جدید", updatedPlan.Teachers);
        }

        [Fact]
        public async Task BotNewPlanRoom_17Aban_1830_StoresUtc_AndGetsTehranTime()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var room = new RoomTBL
            {
                ID = 20,
                FriendlyID = "test-room-17aban",
                Title = "اتاق تست آبان",
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true
            };
            db.RoomTBLs.Add(room);
            await db.SaveChangesAsync();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var request = new BotNewPlanRoomRequest
            {
                FriendlyID = "test-room-17aban",
                Title = "نشست ۱۷ آبان",
                Date = "1405/08/17",
                Time = "18:30",
                Secret = "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"
            };

            await usecase.handleAsync(request, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);

            var plan = await db.RoomTBLPlanTBLs.FirstAsync(p => p.IDRoomTBL == room.ID);
            // Server must store UTC:
            // 1405/08/17 in Persian calendar is 2026-11-08 Gregorian.
            // 18:30 Tehran time (UTC+03:30) is 15:00 UTC.
            Assert.True(plan.StartDate.HasValue);
            Assert.Equal(2026, plan.StartDate.Value.Year);
            Assert.Equal(11, plan.StartDate.Value.Month);
            Assert.Equal(8, plan.StartDate.Value.Day);
            Assert.Equal(15, plan.StartDate.Value.Hour);
            Assert.Equal(0, plan.StartDate.Value.Minute);

            // Moment of GET: formatted strings in Tehran, DateTime in UTC
            var mapped = PlanTimeHelper.MapToMiniPlanJSON(plan);
            Assert.Equal("1405/08/17", mapped.Date);
            Assert.Equal("18:30", mapped.Time);
            Assert.Equal("1405/08/17 18:30", mapped.StartAtTehran);
            Assert.NotNull(mapped.StartAt);
            Assert.Equal(15, mapped.StartAt.Value.Hour);
            Assert.Equal(0, mapped.StartAt.Value.Minute);
            Assert.Equal(15, mapped.StartAtUtc.Value.Hour);
        }

        [Fact]
        public async Task BotNewPlanRoom_WhenDuplicateUpcomingMeeting_Returns409Conflict_AndDoesNotUpdate()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var room = new RoomTBL
            {
                ID = 20,
                FriendlyID = "room-dup-test",
                Title = "اتاق تست تکراری",
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true
            };
            db.RoomTBLs.Add(room);

            var (dateSuccess, utcDt, _) = TeacherNameMatcher.ParseJalaliDateTime("1405/09/20", "14:00");
            Assert.True(dateSuccess);

            var initialPlan = new RoomTBLPlanTBL
            {
                IDRoomTBL = room.ID,
                Title = "نشست تخصصی هوش مصنوعی",
                Teachers = "دکتر حسینی",
                Dabir = "مهندس رضایی",
                StartDate = utcDt,
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_WITH_CERT,
                Explain = "ثبت اولیه"
            };
            db.RoomTBLPlanTBLs.Add(initialPlan);
            await db.SaveChangesAsync();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var duplicateRequest = new BotNewPlanRoomRequest
            {
                FriendlyID = "room-dup-test",
                Title = "نشست تخصصی هوش مصنوعی",
                Teachers = "دکتر حسینی",
                Dabir = "مهندس رضایی",
                Date = "1405/09/20",
                Time = "14:00",
                Secret = "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"
            };

            await usecase.handleAsync(duplicateRequest, output);

            Assert.NotNull(output.Result);
            Assert.Equal(409, output.Result.data.Status);
            Assert.Contains("تکراری", output.Result.data.Message);

            var plans = await db.RoomTBLPlanTBLs.ToListAsync();
            Assert.Single(plans);
            Assert.Equal("ثبت اولیه", plans[0].Explain);
        }

        [Fact]
        public async Task BotNewPlanRoom_WhenUpcomingMeetingExists_WithChangedDetails_UpdatesRecord_AndSetsExplainWithTimestamp()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var room = new RoomTBL
            {
                ID = 21,
                FriendlyID = "room-update-test",
                Title = "اتاق تست به‌روزرسانی",
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true
            };
            db.RoomTBLs.Add(room);

            var (dateSuccess, initialUtc, _) = TeacherNameMatcher.ParseJalaliDateTime("1405/09/20", "14:00");
            Assert.True(dateSuccess);

            var initialPlan = new RoomTBLPlanTBL
            {
                IDRoomTBL = room.ID,
                Title = "نشست فلسفه تطبیقی",
                Teachers = "دکتر حسینی",
                Dabir = "مهندس رضایی",
                StartDate = initialUtc,
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_WITH_CERT,
                Explain = null
            };
            db.RoomTBLPlanTBLs.Add(initialPlan);
            await db.SaveChangesAsync();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var updateRequest = new BotNewPlanRoomRequest
            {
                FriendlyID = "room-update-test",
                Title = "نشست فلسفه تطبیقی",
                Teachers = "دکتر حسینی، دکتر رضوانی",
                Dabir = "دکتر علوی",
                Date = "1405/09/21",
                Time = "16:30",
                Secret = "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"
            };

            await usecase.handleAsync(updateRequest, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);
            Assert.Contains("به‌روزرسانی", output.Result.data.Message);

            var plans = await db.RoomTBLPlanTBLs.ToListAsync();
            Assert.Single(plans);
            var updatedPlan = plans[0];
            Assert.Equal(initialPlan.ID, updatedPlan.ID);
            Assert.Contains("دکتر رضوانی", updatedPlan.Teachers);
            Assert.Equal("دکتر علوی", updatedPlan.Dabir);

            Assert.NotNull(updatedPlan.Explain);
            Assert.Contains("به‌روزرسانی", updatedPlan.Explain);
            Assert.Contains("UTC:", updatedPlan.Explain);

            Assert.True(mediator.PublishedEvents.Any(e => e is PlanUpdateEvent));
            Assert.True(mediator.PublishedEvents.Any(e => e is RoomUpdatedEvent));
        }

        [Fact]
        public async Task BotNewPlanRoom_WhenPastMeetingExists_WithSameTitle_CreatesNewRecord()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var room = new RoomTBL
            {
                ID = 22,
                FriendlyID = "room-past-test",
                Title = "اتاق نشست‌های دوره‌ای",
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true
            };
            db.RoomTBLs.Add(room);

            var pastPlan = new RoomTBLPlanTBL
            {
                IDRoomTBL = room.ID,
                Title = "نشست دوره‌ای اخلاق",
                Teachers = "استاد حسینی",
                StartDate = DateTime.UtcNow.AddDays(-10),
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_WITH_CERT
            };
            db.RoomTBLPlanTBLs.Add(pastPlan);
            await db.SaveChangesAsync();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var newRequest = new BotNewPlanRoomRequest
            {
                FriendlyID = "room-past-test",
                Title = "نشست دوره‌ای اخلاق",
                Teachers = "استاد حسینی",
                Date = "1405/09/25",
                Time = "10:00",
                Secret = "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"
            };

            await usecase.handleAsync(newRequest, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);
            Assert.Contains("تنظیم شد", output.Result.data.Message);

            var plans = await db.RoomTBLPlanTBLs.ToListAsync();
            Assert.Equal(2, plans.Count);
        }

        [Fact]
        public async Task GetUpcomingPlansAsync_ExcludesDeletedPlans_WithTypeOrStatusMinus400()
        {
            var db = CreateInMemoryContext();
            var fakeBbb = new FakeBBBGetMeetings();

            var adminUser = new UserTBL
            {
                ID = 21360,
                Username = "mohrez",
                Type = MainEntity_Types.USER_ADMINSTRATOR,
                UUID = "uuid-mohrez"
            };
            db.UserTBLs.Add(adminUser);

            var room = new RoomTBL
            {
                ID = 14,
                FriendlyID = "ibf-phb-0yp-whu",
                Title = "نشست علمی قم",
                Type = MainEntity_Types.ROOM_NORMAL,
                Status = MainEntity_Status.Enabled,
                IDOwnerTBL = 21319
            };
            db.RoomTBLs.Add(room);

            // Active valid plan
            var validPlan = new RoomTBLPlanTBL
            {
                ID = 150211,
                IDRoomTBL = 14,
                Title = "فلسفه نبوت",
                Teachers = "محمدرضا مصطفی پور",
                StartDate = DateTime.UtcNow.AddDays(1),
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_WITH_CERT
            };

            // Deleted plan by status -400
            var deletedByStatusPlan = new RoomTBLPlanTBL
            {
                ID = 150212,
                IDRoomTBL = 14,
                Title = "پلن حذف شده با استاتوس -۴۰۰",
                Teachers = "استاد تست",
                StartDate = DateTime.UtcNow.AddDays(2),
                Status = MainEntity_Status.Delete, // -400
                Type = MainEntity_Types.PLAN_WITH_CERT
            };

            // Deleted plan by type -400
            var deletedByTypePlan = new RoomTBLPlanTBL
            {
                ID = 150213,
                IDRoomTBL = 14,
                Title = "پلن حذف شده با تایپ -۴۰۰",
                Teachers = "استاد تست ۲",
                StartDate = DateTime.UtcNow.AddDays(3),
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_DELETED // -400
            };

            db.RoomTBLPlanTBLs.AddRange(validPlan, deletedByStatusPlan, deletedByTypePlan);
            await db.SaveChangesAsync();

            var service = new AdminMeetingService(db, fakeBbb);
            var result = await service.GetUpcomingPlansAsync(adminUser);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(150211, result[0].PlanId);
            Assert.DoesNotContain(result, p => p.PlanId == 150212);
            Assert.DoesNotContain(result, p => p.PlanId == 150213);
        }

        [Fact]
        public async Task BotNewPlanRoom_DoesNotMatchDeletedPlan_EvenIfSameTitle()
        {
            var db = CreateInMemoryContext();
            var mediator = new MockMediator();
            var config = CreateConfiguration();
            var fakeBbb = new FakeBBBGetMeetings();

            var room = new RoomTBL
            {
                ID = 14,
                FriendlyID = "ibf-phb-0yp-whu",
                Title = "نشست علمی قم",
                Type = MainEntity_Types.ROOM_NORMAL,
                Status = MainEntity_Status.Enabled,
                IDOwnerTBL = 21319
            };
            db.RoomTBLs.Add(room);

            // Existing deleted plan with the same title
            var deletedPlan = new RoomTBLPlanTBL
            {
                ID = 150214,
                IDRoomTBL = 14,
                Title = "فلسفه نبوت از منظر علامه طباطبائی",
                Teachers = "محمدرضا مصطفی پور",
                StartDate = DateTime.UtcNow.AddDays(1),
                Status = MainEntity_Status.Delete, // -400
                Type = MainEntity_Types.PLAN_WITH_CERT
            };
            db.RoomTBLPlanTBLs.Add(deletedPlan);
            await db.SaveChangesAsync();

            var usecase = new BotNewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<BotNewPlanRoomResponse>>();

            var req = new BotNewPlanRoomRequest
            {
                FriendlyID = "ibf-phb-0yp-whu",
                Title = "فلسفه نبوت از منظر علامه طباطبائی",
                Teachers = "محمدرضا مصطفی پور",
                Date = "1405/08/26",
                Time = "19:30",
                Secret = "xXxxXxXXxXXxXxTokenSecret@1239988mfaMgfo"
            };

            await usecase.handleAsync(req, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);
            // Must have inserted a new plan with message "تنظیم شد", not updated the deleted one
            Assert.Contains("تنظیم شد", output.Result.data.Message);

            var activePlans = await db.RoomTBLPlanTBLs.Where(p => (int)p.Status != -400).ToListAsync();
            Assert.Single(activePlans);
            Assert.NotEqual(150214, activePlans[0].ID);
        }

        [Fact]
        public async Task NewPlanRoom_WhenDuplicateUpcomingMeeting_Returns409Conflict_AndDoesNotUpdate()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var user = new UserTBL
            {
                ID = 50,
                UUID = "user-test-uuid",
                Username = "adminuser",
                Type = MainEntity_Types.USER_ADMIN,
                Status = MainEntity_Status.Enabled
            };
            db.UserTBLs.Add(user);

            var room = new RoomTBL
            {
                ID = 30,
                FriendlyID = "room-user-dup-test",
                Title = "اتاق تست کاربر",
                IDOwnerTBL = user.ID,
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true
            };
            db.RoomTBLs.Add(room);

            var futureDate = DateTime.UtcNow.AddDays(5);
            var initialPlan = new RoomTBLPlanTBL
            {
                IDRoomTBL = room.ID,
                Title = "فلسفه دین",
                Teachers = "دکتر مصطفی پور",
                Dabir = "دبیر نشست",
                StartDate = futureDate,
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_WITH_CERT,
                Explain = "ثبت اولیه"
            };
            db.RoomTBLPlanTBLs.Add(initialPlan);
            await db.SaveChangesAsync();

            var usecase = new NewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<NewPlanRoomResponse>>();

            var duplicateRequest = new NewPlanRoomRequest
            {
                FriendlyID = "room-user-dup-test",
                Title = "فلسفه دین",
                Teacher = "دکتر مصطفی پور",
                Dabir = "دبیر نشست",
                StartDate = futureDate,
                GeneralHeaders = new Application.DTO.Generals.GeneralHeaders
                {
                    UserTBL = user
                }
            };

            await usecase.handleAsync(duplicateRequest, output);

            Assert.NotNull(output.Result);
            Assert.Equal(409, output.Result.data.Status);
            Assert.Contains("تکراری", output.Result.data.Message);

            var plans = await db.RoomTBLPlanTBLs.ToListAsync();
            Assert.Single(plans);
            Assert.Equal("ثبت اولیه", plans[0].Explain);
        }

        [Fact]
        public async Task NewPlanRoom_WhenUpcomingMeetingExists_WithChangedDetails_UpdatesRecord_AndSetsExplainWithTimestamp()
        {
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();
            var mediator = new MockMediator();
            var fakeBbb = new FakeBBBGetMeetings();

            var user = new UserTBL
            {
                ID = 51,
                UUID = "user-test-uuid-2",
                Username = "adminuser2",
                Type = MainEntity_Types.USER_ADMIN,
                Status = MainEntity_Status.Enabled
            };
            db.UserTBLs.Add(user);

            var room = new RoomTBL
            {
                ID = 31,
                FriendlyID = "room-user-update-test",
                Title = "اتاق تست کاربر ۲",
                IDOwnerTBL = user.ID,
                Status = MainEntity_Status.Enabled,
                IsPublicRoom = true
            };
            db.RoomTBLs.Add(room);

            var initialDate = DateTime.UtcNow.AddDays(5);
            var initialPlan = new RoomTBLPlanTBL
            {
                IDRoomTBL = room.ID,
                Title = "کلام اسلامی",
                Teachers = "دکتر رضایی",
                Dabir = "مهندس محمدی",
                StartDate = initialDate,
                Status = MainEntity_Status.Enabled,
                Type = MainEntity_Types.PLAN_WITH_CERT,
                Explain = "ثبت اولیه"
            };
            db.RoomTBLPlanTBLs.Add(initialPlan);
            await db.SaveChangesAsync();

            var usecase = new NewPlanRoomUseCase(db, config, fakeBbb, mediator);
            var output = new MockOutputPort<GenericResponse<NewPlanRoomResponse>>();

            var updatedDate = DateTime.UtcNow.AddDays(6);
            var updateRequest = new NewPlanRoomRequest
            {
                FriendlyID = "room-user-update-test",
                Title = "کلام اسلامی",
                Teacher = "دکتر رضایی",
                Dabir = "مهندس محمدی (دبیر علمی)",
                StartDate = updatedDate,
                GeneralHeaders = new Application.DTO.Generals.GeneralHeaders
                {
                    UserTBL = user
                }
            };

            await usecase.handleAsync(updateRequest, output);

            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.data.Status);
            Assert.Contains("به‌روزرسانی", output.Result.data.Message);

            var plans = await db.RoomTBLPlanTBLs.ToListAsync();
            Assert.Single(plans);
            Assert.Equal(initialPlan.ID, plans[0].ID);
            Assert.Equal(updatedDate, plans[0].StartDate);
            Assert.Contains("به‌روزرسانی در تاریخ", plans[0].Explain);
            Assert.Contains("UTC:", plans[0].Explain);
        }
    }
}
