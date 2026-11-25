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
    public class AdminPollReportServiceTests
    {
        private DBContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new DBContext(options);
        }

        private (UserTBL admin, UserTBL viewer) SeedPollData(DBContext db)
        {
            var admin = new UserTBL
            {
                ID = 1,
                UUID = "uuid-admin",
                Username = "admin1",
                Firstname = "مدیر",
                LastName = "سیستم",
                Type = MainEntity_Types.USER_ADMIN,
                Status = MainEntity_Status.Succedd
            };

            var viewer = new UserTBL
            {
                ID = 2,
                UUID = "uuid-viewer",
                Username = "viewer1",
                Firstname = "کاربر",
                LastName = "عادی",
                Type = MainEntity_Types.USER_VIEWER,
                Status = MainEntity_Status.Succedd
            };

            db.UserTBLs.AddRange(admin, viewer);

            var room1 = new RoomTBL
            {
                ID = 1,
                FriendlyID = "room-alpha",
                Title = "اتاق الف",
                IDOwnerTBL = 1,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            };
            var room2 = new RoomTBL
            {
                ID = 2,
                FriendlyID = "room-beta",
                Title = "اتاق ب",
                IDOwnerTBL = 1,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            };
            db.RoomTBLs.AddRange(room1, room2);

            var meet1 = new MeetingTBL
            {
                ID = 10,
                MeetID = "meet-10",
                Title = "جلسه اول",
                JalaliDate = "1405/09/01",
                RoomFriendlyID = "room-alpha",
                RoomTBL = room1,
                IDCreator = 1,
                Status = MainEntity_Status.MEETING_END,
                Type = MainEntity_Types.Normal,
                DateInsert = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc)
            };

            var meet2 = new MeetingTBL
            {
                ID = 20,
                MeetID = "meet-20",
                Title = "جلسه دوم",
                JalaliDate = "1405/09/02",
                RoomFriendlyID = "room-beta",
                RoomTBL = room2,
                IDCreator = 1,
                Status = MainEntity_Status.MEETING_END,
                Type = MainEntity_Types.Normal,
                DateInsert = new DateTime(2026, 11, 22, 10, 0, 0, DateTimeKind.Utc)
            };
            db.MeetingTBLs.AddRange(meet1, meet2);

            var um1 = new UserTBLToMeetingTBL
            {
                ID = 100,
                MeetID = "meet-10",
                MeetingTBL = meet1,
                UUID = "uuid-u1",
                TokenJWT = "t1",
                UserNickName = "علی رضایی",
                UserMeetID = "u1",
                Phone = "09120000001",
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.USER_VIEWER,
                DateInsert = DateTime.UtcNow
            };
            var um2 = new UserTBLToMeetingTBL
            {
                ID = 101,
                MeetID = "meet-20",
                MeetingTBL = meet2,
                UUID = "uuid-u2",
                TokenJWT = "t2",
                UserNickName = "مریم احمدی",
                UserMeetID = "u2",
                Phone = "09120000002",
                Status = MainEntity_Status.LOG_NORMAL,
                Type = MainEntity_Types.USER_VIEWER,
                DateInsert = DateTime.UtcNow
            };
            db.UserTBLToMeetingTBLs.AddRange(um1, um2);

            // Global Question 1: Scored Optional (IDMeetingTBL == null)
            var qGlobalScored = new MeetPollQuizTBL
            {
                ID = 1001,
                Question = "کیفیت برگزاری جلسه چطور بود؟",
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                IDMeetingTBL = null,
                IsForAll = true,
                DateInsert = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            // Global Question 2: Free-Text Input (IDMeetingTBL == null)
            var qGlobalText = new MeetPollQuizTBL
            {
                ID = 1002,
                Question = "پیشنهاد شما چیست؟",
                Type = MainEntity_Types.QUIZ_INPUT,
                Status = MainEntity_Status.LOG_NORMAL,
                IDMeetingTBL = null,
                IsForAll = true,
                DateInsert = new DateTime(2026, 11, 2, 0, 0, 0, DateTimeKind.Utc)
            };

            // Global Question 3: Unanswered global question (IDMeetingTBL == null)
            var qGlobalUnanswered = new MeetPollQuizTBL
            {
                ID = 1003,
                Question = "سوال بدون پاسخ عمومی",
                Type = MainEntity_Types.QUIZ_OPTIONAL_NOSCORE,
                Status = MainEntity_Status.LOG_NORMAL,
                IDMeetingTBL = null,
                IsForAll = true,
                DateInsert = new DateTime(2026, 11, 3, 0, 0, 0, DateTimeKind.Utc)
            };

            // Meeting-specific Question (IDMeetingTBL != null -> MUST BE EXCLUDED!)
            var qMeetingSpecific = new MeetPollQuizTBL
            {
                ID = 1004,
                Question = "سوال اختصاصی جلسه ۱۰",
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                IDMeetingTBL = 10,
                IsForAll = false,
                DateInsert = new DateTime(2026, 11, 4, 0, 0, 0, DateTimeKind.Utc)
            };

            db.MeetPollQuizTBLs.AddRange(qGlobalScored, qGlobalText, qGlobalUnanswered, qMeetingSpecific);

            var ans1 = new MeetPollQuizAnsTBL
            {
                ID = 2001,
                IDQuizTBL = 1001,
                QuizTBL = qGlobalScored,
                Answer = "عالی",
                Score = 100,
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            };
            var ans2 = new MeetPollQuizAnsTBL
            {
                ID = 2002,
                IDQuizTBL = 1001,
                QuizTBL = qGlobalScored,
                Answer = "متوسط",
                Score = 50,
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            };
            var ansText = new MeetPollQuizAnsTBL
            {
                ID = 2003,
                IDQuizTBL = 1002,
                QuizTBL = qGlobalText,
                Answer = "متن آزاد",
                Score = 0,
                Type = MainEntity_Types.QUIZ_INPUT,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            };
            var ansMeeting = new MeetPollQuizAnsTBL
            {
                ID = 2004,
                IDQuizTBL = 1004,
                QuizTBL = qMeetingSpecific,
                Answer = "خوب",
                Score = 80,
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                Status = MainEntity_Status.LOG_NORMAL,
                DateInsert = DateTime.UtcNow
            };

            db.MeetPollQuizAnsTBLs.AddRange(ans1, ans2, ansText, ansMeeting);

            db.MeetPollQuizAnsTBLToUserMeetTBLs.AddRange(
                new MeetPollQuizAnsTBLToUserMeetTBL
                {
                    ID = 3001,
                    IDMeetPollQuizAnsTBL = 2001,
                    MeetPollQuizAnsTBL = ans1,
                    IDUserTBLToMeetingTBL = 100,
                    UserTBLToMeetingTBL = um1,
                    Status = MainEntity_Status.LOG_NORMAL,
                    Type = MainEntity_Types.QUIZ_OPTIONAL,
                    DateInsert = new DateTime(2026, 11, 20, 11, 0, 0, DateTimeKind.Utc)
                },
                new MeetPollQuizAnsTBLToUserMeetTBL
                {
                    ID = 3002,
                    IDMeetPollQuizAnsTBL = 2002,
                    MeetPollQuizAnsTBL = ans2,
                    IDUserTBLToMeetingTBL = 101,
                    UserTBLToMeetingTBL = um2,
                    Status = MainEntity_Status.LOG_NORMAL,
                    Type = MainEntity_Types.QUIZ_OPTIONAL,
                    DateInsert = new DateTime(2026, 11, 22, 11, 0, 0, DateTimeKind.Utc)
                },
                new MeetPollQuizAnsTBLToUserMeetTBL
                {
                    ID = 3003,
                    IDMeetPollQuizAnsTBL = 2003,
                    MeetPollQuizAnsTBL = ansText,
                    IDUserTBLToMeetingTBL = 100,
                    UserTBLToMeetingTBL = um1,
                    Content = "صدای جلسه بسیار شفاف بود",
                    Status = MainEntity_Status.LOG_NORMAL,
                    Type = MainEntity_Types.QUIZ_INPUT,
                    DateInsert = new DateTime(2026, 11, 20, 11, 5, 0, DateTimeKind.Utc)
                },
                // Vote on meeting-specific question - must NOT appear in global report
                new MeetPollQuizAnsTBLToUserMeetTBL
                {
                    ID = 3004,
                    IDMeetPollQuizAnsTBL = 2004,
                    MeetPollQuizAnsTBL = ansMeeting,
                    IDUserTBLToMeetingTBL = 100,
                    UserTBLToMeetingTBL = um1,
                    Status = MainEntity_Status.LOG_NORMAL,
                    Type = MainEntity_Types.QUIZ_OPTIONAL,
                    DateInsert = new DateTime(2026, 11, 20, 11, 10, 0, DateTimeKind.Utc)
                }
            );

            db.SaveChanges();
            return (admin, viewer);
        }

        [Fact]
        public async Task GetGlobalPollReport_UnauthorizedUser_Returns403()
        {
            using var db = CreateInMemoryContext(nameof(GetGlobalPollReport_UnauthorizedUser_Returns403));
            var (_, viewer) = SeedPollData(db);
            var service = new AdminPollReportService(db);

            var res = await service.GetGlobalPollReportAsync(viewer);

            Assert.Equal(403, res.Status);
        }

        [Fact]
        public async Task GetGlobalPollReport_ExcludesMeetingSpecificQuestions_AndAggregatesCorrectly()
        {
            using var db = CreateInMemoryContext(nameof(GetGlobalPollReport_ExcludesMeetingSpecificQuestions_AndAggregatesCorrectly));
            var (admin, _) = SeedPollData(db);
            var service = new AdminPollReportService(db);

            var res = await service.GetGlobalPollReportAsync(admin);

            Assert.Equal(200, res.Status);
            // Only the 3 questions with IDMeetingTBL == null should be returned
            Assert.Equal(3, res.Questions.Count);
            Assert.DoesNotContain(res.Questions, q => q.ID == 1004);
            Assert.Contains(res.Questions, q => q.ID == 1003 && q.TotalVotes == 0);

            // Summary metrics
            Assert.Equal(3, res.Summary.TotalQuestions);
            Assert.Equal(3, res.Summary.TotalVotes);
            Assert.Equal(2, res.Summary.UniqueRespondents);
            Assert.Equal(2, res.Summary.UniqueMeetingsCount);
            Assert.Equal(2, res.Summary.UniqueRoomsCount);
            Assert.Equal(75.0, res.Summary.OverallAverageScore); // (100 + 50) / 2

            // Scored question breakdown
            var q1 = res.Questions.First(q => q.ID == 1001);
            Assert.Equal(2, q1.TotalVotes);
            Assert.Equal(75.0, q1.OverallAverageScore);
            Assert.Equal(50.0, q1.AnswersReport.First(a => a.ID == 2001).Percentage);
            Assert.Equal(50.0, q1.AnswersReport.First(a => a.ID == 2002).Percentage);

            // Text question breakdown
            var q2 = res.Questions.First(q => q.ID == 1002);
            Assert.Equal(1, q2.TotalVotes);
            Assert.Equal("صدای جلسه بسیار شفاف بود", q2.AnswersReport[0].UserAnswers[0].Content);

            // Meeting & Room breakdowns
            Assert.Equal(2, res.MeetingsBreakdown.Count);
            Assert.Equal(2, res.RoomsBreakdown.Count);
        }

        [Fact]
        public async Task GetGlobalPollReport_FiltersByRoomAndSearchCorrectly()
        {
            using var db = CreateInMemoryContext(nameof(GetGlobalPollReport_FiltersByRoomAndSearchCorrectly));
            var (admin, _) = SeedPollData(db);
            var service = new AdminPollReportService(db);

            var resByRoom = await service.GetGlobalPollReportAsync(admin, new GlobalPollReportFilterDTO
            {
                RoomFriendlyId = "room-alpha"
            });

            Assert.Equal(2, resByRoom.Summary.TotalVotes);
            Assert.Equal(100.0, resByRoom.Summary.OverallAverageScore);
            Assert.Single(resByRoom.MeetingsBreakdown);
            Assert.Equal("meet-10", resByRoom.MeetingsBreakdown[0].MeetID);
            // FilterOptions should remain complete
            Assert.Equal(2, resByRoom.FilterOptions.Rooms.Count);
            Assert.Equal(3, resByRoom.FilterOptions.Questions.Count);

            var resBySearch = await service.GetGlobalPollReportAsync(admin, new GlobalPollReportFilterDTO
            {
                Search = "شفاف"
            });
            Assert.Single(resBySearch.Questions);
            Assert.Equal(1002, resBySearch.Questions[0].ID);
        }

        [Fact]
        public async Task TogglePollStatus_TogglesBetweenActiveAndDisabled_AndChecksPermission()
        {
            using var db = CreateInMemoryContext(nameof(TogglePollStatus_TogglesBetweenActiveAndDisabled_AndChecksPermission));
            var (admin, viewer) = SeedPollData(db);
            var service = new AdminPollReportService(db);

            // Unauthorized viewer gets 403
            var forbidden = await service.TogglePollStatusAsync(viewer, 1001);
            Assert.Equal(403, forbidden.Status);

            // Admin toggles active (300) -> disabled (-100)
            var disabledRes = await service.TogglePollStatusAsync(admin, 1001);
            Assert.Equal(200, disabledRes.Status);
            Assert.Equal((int)MainEntity_Status.Disable, disabledRes.PollStatus);

            // Toggle again -> active (300)
            var activeRes = await service.TogglePollStatusAsync(admin, 1001);
            Assert.Equal(200, activeRes.Status);
            Assert.Equal((int)MainEntity_Status.LOG_NORMAL, activeRes.PollStatus);

            // Missing poll -> 404
            var notFound = await service.TogglePollStatusAsync(admin, 999999);
            Assert.Equal(404, notFound.Status);
        }

        [Fact]
        public async Task UpdateAndDeletePollQuiz_UpdatesFieldsAndSoftDeletes()
        {
            using var db = CreateInMemoryContext(nameof(UpdateAndDeletePollQuiz_UpdatesFieldsAndSoftDeletes));
            var (admin, _) = SeedPollData(db);
            var service = new AdminPollReportService(db);

            var updateRes = await service.UpdatePollQuizAsync(admin, 1001, new PollQuizUpdateDTO
            {
                Question = "سوال ویرایش‌شده؟",
                Type = MainEntity_Types.QUIZ_OPTIONAL,
                AnsPollRequestJSONs = new List<AnsPollRequestJSON>
                {
                    new AnsPollRequestJSON { Content = "خیلی خوب", Score = 95 },
                    new AnsPollRequestJSON { Content = "ضعیف", Score = 20 }
                }
            });
            Assert.Equal(200, updateRes.Status);
            var updatedPoll = await db.MeetPollQuizTBLs.Include(p => p.MeetPollQuizAnsTBLs).FirstAsync(p => p.ID == 1001);
            Assert.Equal("سوال ویرایش‌شده؟", updatedPoll.Question);

            var delRes = await service.DeletePollQuizAsync(admin, 1001);
            Assert.Equal(200, delRes.Status);
            Assert.Equal((int)MainEntity_Status.LOG_REMOVED, delRes.PollStatus);
        }

        [Fact]
        public async Task GetTeacherList_WithIncludeDabir_ReturnsTeachersAndDabirs()
        {
            using var db = CreateInMemoryContext(nameof(GetTeacherList_WithIncludeDabir_ReturnsTeachersAndDabirs));
            db.UserTBLs.AddRange(
                new UserTBL
                {
                    ID = 50,
                    UUID = "t-1",
                    Username = "t1",
                    Firstname = "حسن",
                    LastName = "علوی",
                    Type = MainEntity_Types.USER_TEACHER,
                    Status = MainEntity_Status.USER_ACTIVE
                },
                new UserTBL
                {
                    ID = 51,
                    UUID = "d-1",
                    Username = "d1",
                    Firstname = "مهدی",
                    LastName = "نوری",
                    Type = MainEntity_Types.USER_DABIR,
                    Status = MainEntity_Status.USER_ACTIVE
                }
            );
            await db.SaveChangesAsync();

            var useCase = new Application.Usecase.GetTeacherListUseCase(db, null!);

            Application.DTO.GetTeacherListResponse? onlyTeachersRes = null;
            var outPort1 = new TestOutputPort<Application.DTO.Generals.GenericResponse<Application.DTO.GetTeacherListResponse>>(r => onlyTeachersRes = r.data);
            await useCase.handleAsync(new Application.DTO.GetTeacherListRequest { Query = "", IncludeDabir = false }, outPort1);

            Assert.NotNull(onlyTeachersRes);
            Assert.Single(onlyTeachersRes!.Data);
            Assert.Equal("حسن علوی", onlyTeachersRes.Data[0].Fullname);

            Application.DTO.GetTeacherListResponse? withDabirRes = null;
            var outPort2 = new TestOutputPort<Application.DTO.Generals.GenericResponse<Application.DTO.GetTeacherListResponse>>(r => withDabirRes = r.data);
            await useCase.handleAsync(new Application.DTO.GetTeacherListRequest { Query = "", IncludeDabir = true }, outPort2);

            Assert.NotNull(withDabirRes);
            Assert.Equal(2, withDabirRes!.Data.Count);
            Assert.Contains(withDabirRes.Data, x => x.Fullname == "حسن علوی" && x.Role == "استاد");
            Assert.Contains(withDabirRes.Data, x => x.Fullname == "مهدی نوری" && x.Role == "دبیر");
        }

        private class TestOutputPort<T> : Application.Interface.IUsecase.IOutPutPort<T>
        {
            private readonly Action<T> _onHandle;
            public TestOutputPort(Action<T> onHandle) => _onHandle = onHandle;
            public void Handle(T response) => _onHandle(response);
        }
    }
}
