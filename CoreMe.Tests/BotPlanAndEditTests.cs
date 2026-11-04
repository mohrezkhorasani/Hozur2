using Application.DTO;
using Application.DTO.Generals;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
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
            Assert.Single(mediator.PublishedEvents);
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
    }
}
