using Application.DTO;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Presistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CoreMe.Tests
{
    public class MockOutputPort<T> : IOutPutPort<T>
    {
        public T Result { get; private set; }
        public void Handle(T response)
        {
            Result = response;
        }
    }

    public class MeetingAttendeesTests
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
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "TestDb" }
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();
        }

        [Fact]
        public async Task GetMembersOfMeet_ShouldReturnCorrectTags_ForAttendees()
        {
            // Arrange
            using var db = CreateInMemoryContext();
            var config = CreateConfiguration();

            var adminUser = new UserTBL
            {
                ID = 1,
                UUID = "admin-uuid",
                Type = MainEntity_Types.USER_ADMINSTRATOR,
                Username = "superadmin",
                Firstname = "Super",
                LastName = "Admin",
                DateInsert = DateTime.UtcNow
            };
            db.UserTBLs.Add(adminUser);

            var teacherUser = new UserTBL
            {
                ID = 2,
                UUID = "teacher-uuid",
                Type = MainEntity_Types.USER_TEACHER,
                Username = "ostad",
                Firstname = "دکتر",
                LastName = "احمدی",
                Email = "teacher@hekmat.ir",
                IsMemberVIP = false,
                DateInsert = DateTime.UtcNow
            };
            db.UserTBLs.Add(teacherUser);

            var vipUser = new UserTBL
            {
                ID = 3,
                UUID = "vip-uuid",
                Type = MainEntity_Types.USER_VIEWER,
                Username = "vipuser",
                Firstname = "علی",
                LastName = "رضایی",
                Email = "ali@hekmat.ir",
                IsMemberVIP = true,
                DateInsert = DateTime.UtcNow
            };
            db.UserTBLs.Add(vipUser);

            var guestUser = new UserTBL
            {
                ID = 4,
                UUID = "guest-uuid",
                Type = MainEntity_Types.USER_GUEST,
                Username = "guest",
                Firstname = "محمد",
                LastName = "حسینی",
                Email = null, // No email / no formal account
                IsMemberVIP = false,
                DateInsert = DateTime.UtcNow
            };
            db.UserTBLs.Add(guestUser);

            var meet = new MeetingTBL
            {
                ID = 100,
                MeetID = "meet-test-100",
                Title = "نشست تخصصی مجمع عالی حکمت",
                Type = MainEntity_Types.MEETING_BBB,
                Status = MainEntity_Status.MEETING_END,
                DateInsert = DateTime.UtcNow.AddHours(-2),
                Teacher = teacherUser,
                IDTeacher = teacherUser.ID
            };
            db.MeetingTBLs.Add(meet);

            // Add attendees
            // 1. Teacher attendee
            var attTeacher = new UserTBLToMeetingTBL
            {
                ID = 10,
                MeetID = meet.MeetID,
                MeetingTBL = meet,
                UserTBL = teacherUser,
                Type = MainEntity_Types.USER_TEACHER,
                Role = "Teacher",
                UserNickName = "Teacher",
                DateInsert = DateTime.UtcNow.AddHours(-2),
                DateUpdate = DateTime.UtcNow,
                IsCertificated = false,
                UserMeetID = "um-teacher"
            };
            db.UserTBLToMeetingTBLs.Add(attTeacher);

            // 2. VIP attendee with certificate
            var attVip = new UserTBLToMeetingTBL
            {
                ID = 20,
                MeetID = meet.MeetID,
                MeetingTBL = meet,
                UserTBL = vipUser,
                Type = MainEntity_Types.USER_VIEWER,
                Role = "VIEWER",
                UserNickName = "علی رضایی",
                DateInsert = DateTime.UtcNow.AddHours(-1.8),
                DateUpdate = DateTime.UtcNow,
                IsCertificated = true,
                CertificateFileURL = "https://vc.hekmateislami.com/cert/certif-20.pdf",
                UserMeetID = "um-vip"
            };
            db.UserTBLToMeetingTBLs.Add(attVip);

            // 3. Guest attendee (no email, not VIP, no cert)
            var attGuest = new UserTBLToMeetingTBL
            {
                ID = 30,
                MeetID = meet.MeetID,
                MeetingTBL = meet,
                UserTBL = guestUser,
                Type = MainEntity_Types.USER_GUEST,
                Role = "VIEWER",
                UserNickName = "محمد حسینی",
                DateInsert = DateTime.UtcNow.AddHours(-1.5),
                DateUpdate = DateTime.UtcNow,
                IsCertificated = false,
                UserMeetID = "um-guest"
            };
            db.UserTBLToMeetingTBLs.Add(attGuest);

            await db.SaveChangesAsync();

            var usecase = new GetMembersOfMeetUseCase(db, config);
            var output = new MockOutputPort<GenericResponse<GetMembersOfMeetResponse>>();

            var request = new GetMembersOfMeetRequest
            {
                ID = meet.MeetID,
                GeneralHeaders = new GeneralHeaders
                {
                    UserTBL = adminUser
                }
            };

            // Act
            await usecase.handleAsync(request, output);

            // Assert
            Assert.NotNull(output.Result);
            Assert.Equal(200, output.Result.Value.Status);
            Assert.NotNull(output.Result.Value.Members);
            Assert.Equal(3, output.Result.Value.Members.Count);

            // Check Teacher Attendee tags
            var teacherResult = output.Result.Value.Members.FirstOrDefault(m => m.ID == attTeacher.ID);
            Assert.NotNull(teacherResult);
            Assert.True(teacherResult.IsTeacher);
            Assert.False(teacherResult.IsMemberVIP);
            Assert.True(teacherResult.HasAccount);
            Assert.False(teacherResult.HasTakenCertificate);

            // Check VIP Attendee tags
            var vipResult = output.Result.Value.Members.FirstOrDefault(m => m.ID == attVip.ID);
            Assert.NotNull(vipResult);
            Assert.False(vipResult.IsTeacher);
            Assert.True(vipResult.IsMemberVIP);
            Assert.True(vipResult.HasAccount);
            Assert.True(vipResult.HasTakenCertificate);
            Assert.Equal("https://vc.hekmateislami.com/cert/certif-20.pdf", vipResult.CertificateURL);

            // Check Guest Attendee tags
            var guestResult = output.Result.Value.Members.FirstOrDefault(m => m.ID == attGuest.ID);
            Assert.NotNull(guestResult);
            Assert.False(guestResult.IsTeacher);
            Assert.False(guestResult.IsMemberVIP);
            Assert.False(guestResult.HasAccount);
            Assert.False(guestResult.HasTakenCertificate);
        }
    }
}
