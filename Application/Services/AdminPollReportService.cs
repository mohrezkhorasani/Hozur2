using Application.DTO;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface IAdminPollReportService
    {
        bool CanUserViewGlobalPollReports(UserTBL? user);
        Task<GlobalPollReportResponseDTO> GetGlobalPollReportAsync(UserTBL? user, GlobalPollReportFilterDTO? filter = null);
    }

    /// <summary>
    /// Service for generating comprehensive reports for global poll questions (IDMeetingTBL == null).
    /// </summary>
    public class AdminPollReportService : IAdminPollReportService
    {
        private readonly IDBContext _db;

        public AdminPollReportService(IDBContext db)
        {
            _db = db;
        }

        public bool CanUserViewGlobalPollReports(UserTBL? user)
        {
            if (user == null) return false;
            return user.Type == MainEntity_Types.USER_ADMINSTRATOR ||
                   user.VerifyUserPermissions("manage_admins") ||
                   user.VerifyUserPermissions("view_poll_reports") ||
                   user.VerifyUserPermissions("edit_poll");
        }

        public async Task<GlobalPollReportResponseDTO> GetGlobalPollReportAsync(UserTBL? user, GlobalPollReportFilterDTO? filter = null)
        {
            if (!CanUserViewGlobalPollReports(user))
            {
                return new GlobalPollReportResponseDTO
                {
                    Status = 403,
                    Message = "دسترسی ندارید"
                };
            }

            filter ??= new GlobalPollReportFilterDTO();

            // 1. Load all global questions (IDMeetingTBL == null) along with their answer options
            var quizQuery = _db.MeetPollQuizTBLs.AsNoTracking()
                .Where(q => q.IDMeetingTBL == null);

            if (!filter.IncludeInactive)
            {
                quizQuery = quizQuery.Where(q => q.Status == MainEntity_Status.LOG_NORMAL);
            }

            var allGlobalQuizzes = await quizQuery
                .OrderByDescending(q => q.ID)
                .Select(q => new QuizProjection
                {
                    ID = q.ID,
                    Question = q.Question ?? "",
                    Type = q.Type,
                    Status = q.Status,
                    DateInsert = q.DateInsert,
                    IsForAll = q.IsForAll,
                    Options = q.MeetPollQuizAnsTBLs
                        .Where(a => a.Status == MainEntity_Status.Default || a.Status == MainEntity_Status.LOG_NORMAL)
                        .OrderBy(a => a.Score)
                        .ThenBy(a => a.ID)
                        .Select(a => new AnswerOptionProjection
                        {
                            ID = a.ID,
                            Answer = a.Answer ?? "",
                            Score = a.Score
                        })
                        .ToList()
                })
                .ToListAsync();

            var questionOptions = allGlobalQuizzes
                .Select(q => new GlobalPollQuestionOptionDTO
                {
                    ID = q.ID,
                    Question = q.Question,
                    Type = q.Type
                })
                .ToList();

            // 2. Load all valid votes for global questions (IDMeetingTBL == null) via flat projection
            var votesQuery = _db.MeetPollQuizAnsTBLToUserMeetTBLs.AsNoTracking()
                .Where(v => v.MeetPollQuizAnsTBL != null &&
                            v.MeetPollQuizAnsTBL.QuizTBL != null &&
                            v.MeetPollQuizAnsTBL.QuizTBL.IDMeetingTBL == null &&
                            (v.Status == MainEntity_Status.Default || v.Status == MainEntity_Status.LOG_NORMAL));

            if (!filter.IncludeInactive)
            {
                votesQuery = votesQuery.Where(v => v.MeetPollQuizAnsTBL.QuizTBL.Status == MainEntity_Status.LOG_NORMAL);
            }

            var allGlobalVotes = await votesQuery
                .Select(v => new VoteProjection
                {
                    QuizID = v.MeetPollQuizAnsTBL.IDQuizTBL,
                    QuizType = v.MeetPollQuizAnsTBL.QuizTBL.Type,
                    AnswerID = v.IDMeetPollQuizAnsTBL,
                    AnswerScore = v.MeetPollQuizAnsTBL.Score,
                    Content = v.Content,
                    DateInsert = v.DateInsert,
                    UserMeetID = v.IDUserTBLToMeetingTBL ?? 0,
                    UUID = v.UserTBLToMeetingTBL != null ? (v.UserTBLToMeetingTBL.UUID ?? "") : "",
                    NickName = v.UserTBLToMeetingTBL != null ? (v.UserTBLToMeetingTBL.UserNickName ?? "") : "",
                    Phone = v.UserTBLToMeetingTBL != null ? (v.UserTBLToMeetingTBL.Phone ?? "") : "",
                    MeetID = v.UserTBLToMeetingTBL != null ? (v.UserTBLToMeetingTBL.MeetID ?? "") : "",
                    MeetingTitle = v.UserTBLToMeetingTBL != null && v.UserTBLToMeetingTBL.MeetingTBL != null
                        ? (v.UserTBLToMeetingTBL.MeetingTBL.Title ?? "")
                        : "",
                    MeetingJalaliDate = v.UserTBLToMeetingTBL != null && v.UserTBLToMeetingTBL.MeetingTBL != null
                        ? (v.UserTBLToMeetingTBL.MeetingTBL.JalaliDate ?? "")
                        : "",
                    MeetingDate = v.UserTBLToMeetingTBL != null && v.UserTBLToMeetingTBL.MeetingTBL != null
                        ? (DateTime?)v.UserTBLToMeetingTBL.MeetingTBL.DateInsert
                        : null,
                    RoomFriendlyID = v.UserTBLToMeetingTBL != null && v.UserTBLToMeetingTBL.MeetingTBL != null
                        ? (v.UserTBLToMeetingTBL.MeetingTBL.RoomFriendlyID ?? "")
                        : "",
                    RoomTitle = v.UserTBLToMeetingTBL != null &&
                                v.UserTBLToMeetingTBL.MeetingTBL != null &&
                                v.UserTBLToMeetingTBL.MeetingTBL.RoomTBL != null
                        ? (v.UserTBLToMeetingTBL.MeetingTBL.RoomTBL.Title ?? "")
                        : ""
                })
                .ToListAsync();

            var roomOptions = allGlobalVotes
                .Where(v => !string.IsNullOrWhiteSpace(v.RoomFriendlyID))
                .GroupBy(v => v.RoomFriendlyID)
                .Select(g => new GlobalPollRoomOptionDTO
                {
                    FriendlyID = g.Key,
                    Title = g.Select(x => x.RoomTitle).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? g.Key
                })
                .OrderBy(r => r.Title)
                .ToList();

            // 3. Apply filters
            var filteredVotes = ApplyVoteFilters(allGlobalVotes, filter);
            var filteredQuizzes = ApplyQuizFilters(allGlobalQuizzes, filteredVotes, filter);

            var filteredQuizIds = filteredQuizzes.Select(q => q.ID).ToHashSet();
            var scopedVotes = filteredVotes.Where(v => filteredQuizIds.Contains(v.QuizID)).ToList();

            // 4. Build detailed question reports
            var votesByQuiz = scopedVotes.GroupBy(v => v.QuizID).ToDictionary(g => g.Key, g => g.ToList());
            var questionReports = filteredQuizzes
                .Select(q => BuildQuestionReport(q, votesByQuiz.GetValueOrDefault(q.ID) ?? new List<VoteProjection>()))
                .ToList();

            // 5. Build cross-meeting & cross-room breakdowns and summary
            var meetingsBreakdown = BuildMeetingsBreakdown(scopedVotes);
            var roomsBreakdown = BuildRoomsBreakdown(scopedVotes);
            var summary = BuildSummary(questionReports, scopedVotes, meetingsBreakdown.Count, roomsBreakdown.Count);

            return new GlobalPollReportResponseDTO
            {
                Status = 200,
                Message = "ok",
                Summary = summary,
                Questions = questionReports,
                MeetingsBreakdown = meetingsBreakdown,
                RoomsBreakdown = roomsBreakdown,
                FilterOptions = new GlobalPollFilterOptionsDTO
                {
                    Questions = questionOptions,
                    Rooms = roomOptions
                }
            };
        }

        private static List<VoteProjection> ApplyVoteFilters(List<VoteProjection> votes, GlobalPollReportFilterDTO filter)
        {
            IEnumerable<VoteProjection> query = votes;

            if (filter.QID.HasValue && filter.QID.Value > 0)
            {
                query = query.Where(v => v.QuizID == filter.QID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.RoomFriendlyId))
            {
                var targetRoom = filter.RoomFriendlyId.Trim();
                query = query.Where(v => string.Equals(v.RoomFriendlyID, targetRoom, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.MeetId))
            {
                var targetMeet = filter.MeetId.Trim();
                query = query.Where(v => string.Equals(v.MeetID, targetMeet, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(v => v.DateInsert >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(v => v.DateInsert <= to);
            }

            return query.ToList();
        }

        private static List<QuizProjection> ApplyQuizFilters(
            List<QuizProjection> quizzes,
            List<VoteProjection> filteredVotes,
            GlobalPollReportFilterDTO filter)
        {
            IEnumerable<QuizProjection> query = quizzes;

            if (filter.QID.HasValue && filter.QID.Value > 0)
            {
                query = query.Where(q => q.ID == filter.QID.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                var matchingQuizIdsFromContent = filteredVotes
                    .Where(v => !string.IsNullOrWhiteSpace(v.Content) &&
                                v.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Select(v => v.QuizID)
                    .ToHashSet();

                query = query.Where(q =>
                    q.Question.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    q.Options.Any(o => o.Answer.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    matchingQuizIdsFromContent.Contains(q.ID));
            }

            return query.ToList();
        }

        private static GlobalPollQuestionReportDTO BuildQuestionReport(QuizProjection quiz, List<VoteProjection> quizVotes)
        {
            var totalVotes = quizVotes.Count;
            var votesByOption = quizVotes.GroupBy(v => v.AnswerID).ToDictionary(g => g.Key, g => g.ToList());
            var answersReport = new List<GlobalPollAnswerReportDTO>();

            foreach (var option in quiz.Options)
            {
                var optionVotes = votesByOption.GetValueOrDefault(option.ID) ?? new List<VoteProjection>();
                var count = optionVotes.Count;
                double percentage = (quiz.Type == MainEntity_Types.QUIZ_OPTIONAL || quiz.Type == MainEntity_Types.QUIZ_OPTIONAL_NOSCORE) && totalVotes > 0
                    ? Math.Round((double)count / totalVotes * 100, 2)
                    : 0;

                answersReport.Add(new GlobalPollAnswerReportDTO
                {
                    ID = option.ID,
                    Answer = option.Answer,
                    Score = option.Score,
                    Count = count,
                    Percentage = percentage,
                    UserAnswers = optionVotes
                        .OrderByDescending(v => v.DateInsert)
                        .Select(v => new GlobalPollUserAnswerDetailDTO
                        {
                            UserMeetID = v.UserMeetID,
                            NickName = v.NickName,
                            Phone = v.Phone,
                            MeetID = v.MeetID,
                            MeetingTitle = v.MeetingTitle,
                            MeetingJalaliDate = v.MeetingJalaliDate,
                            RoomFriendlyID = v.RoomFriendlyID,
                            RoomTitle = v.RoomTitle,
                            Content = v.Content,
                            DateInsert = v.DateInsert
                        })
                        .ToList()
                });
            }

            double? overallAverageScore = null;
            if (quiz.Type == MainEntity_Types.QUIZ_OPTIONAL)
            {
                overallAverageScore = quizVotes.Any()
                    ? Math.Round(quizVotes.Average(v => (double)v.AnswerScore), 2)
                    : 0;
            }

            var uniqueRespondents = CountUniqueRespondents(quizVotes);
            var uniqueMeetingsCount = quizVotes
                .Where(v => !string.IsNullOrWhiteSpace(v.MeetID))
                .Select(v => v.MeetID)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return new GlobalPollQuestionReportDTO
            {
                ID = quiz.ID,
                Question = quiz.Question,
                Type = quiz.Type,
                Status = quiz.Status,
                DateInsert = quiz.DateInsert,
                IsForAll = quiz.IsForAll,
                TotalVotes = totalVotes,
                UniqueRespondents = uniqueRespondents,
                UniqueMeetingsCount = uniqueMeetingsCount,
                OverallAverageScore = overallAverageScore,
                AnswersReport = answersReport
            };
        }

        private static List<GlobalPollMeetingBreakdownDTO> BuildMeetingsBreakdown(List<VoteProjection> votes)
        {
            return votes
                .Where(v => !string.IsNullOrWhiteSpace(v.MeetID))
                .GroupBy(v => v.MeetID, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var items = g.ToList();
                    var scoredItems = items.Where(x => x.QuizType == MainEntity_Types.QUIZ_OPTIONAL).ToList();
                    var first = items.First();

                    return new GlobalPollMeetingBreakdownDTO
                    {
                        MeetID = first.MeetID,
                        MeetingTitle = string.IsNullOrWhiteSpace(first.MeetingTitle) ? first.MeetID : first.MeetingTitle,
                        JalaliDate = first.MeetingJalaliDate,
                        MeetingDate = first.MeetingDate,
                        RoomFriendlyID = first.RoomFriendlyID,
                        RoomTitle = string.IsNullOrWhiteSpace(first.RoomTitle) ? first.RoomFriendlyID : first.RoomTitle,
                        VotesCount = items.Count,
                        UniqueRespondents = CountUniqueRespondents(items),
                        AverageScore = scoredItems.Any()
                            ? Math.Round(scoredItems.Average(x => (double)x.AnswerScore), 2)
                            : null
                    };
                })
                .OrderByDescending(m => m.MeetingDate ?? DateTime.MinValue)
                .ThenByDescending(m => m.VotesCount)
                .ToList();
        }

        private static List<GlobalPollRoomBreakdownDTO> BuildRoomsBreakdown(List<VoteProjection> votes)
        {
            return votes
                .Where(v => !string.IsNullOrWhiteSpace(v.RoomFriendlyID))
                .GroupBy(v => v.RoomFriendlyID, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var items = g.ToList();
                    var scoredItems = items.Where(x => x.QuizType == MainEntity_Types.QUIZ_OPTIONAL).ToList();
                    var first = items.First();

                    return new GlobalPollRoomBreakdownDTO
                    {
                        RoomFriendlyID = first.RoomFriendlyID,
                        RoomTitle = string.IsNullOrWhiteSpace(first.RoomTitle) ? first.RoomFriendlyID : first.RoomTitle,
                        MeetingsCount = items.Select(x => x.MeetID).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                        VotesCount = items.Count,
                        UniqueRespondents = CountUniqueRespondents(items),
                        AverageScore = scoredItems.Any()
                            ? Math.Round(scoredItems.Average(x => (double)x.AnswerScore), 2)
                            : null
                    };
                })
                .OrderByDescending(r => r.VotesCount)
                .ToList();
        }

        private static GlobalPollSummaryDTO BuildSummary(
            List<GlobalPollQuestionReportDTO> questions,
            List<VoteProjection> votes,
            int uniqueMeetingsCount,
            int uniqueRoomsCount)
        {
            var scoredVotes = votes.Where(v => v.QuizType == MainEntity_Types.QUIZ_OPTIONAL).ToList();
            return new GlobalPollSummaryDTO
            {
                TotalQuestions = questions.Count,
                OptionalScoredCount = questions.Count(q => q.Type == MainEntity_Types.QUIZ_OPTIONAL),
                OptionalNoScoreCount = questions.Count(q => q.Type == MainEntity_Types.QUIZ_OPTIONAL_NOSCORE),
                TextInputCount = questions.Count(q => q.Type == MainEntity_Types.QUIZ_INPUT),
                TotalVotes = votes.Count,
                UniqueRespondents = CountUniqueRespondents(votes),
                UniqueMeetingsCount = uniqueMeetingsCount,
                UniqueRoomsCount = uniqueRoomsCount,
                OverallAverageScore = scoredVotes.Any()
                    ? Math.Round(scoredVotes.Average(v => (double)v.AnswerScore), 2)
                    : 0
            };
        }

        private static int CountUniqueRespondents(IEnumerable<VoteProjection> votes)
        {
            return votes
                .Select(v => !string.IsNullOrWhiteSpace(v.UUID) ? $"u:{v.UUID}" : $"um:{v.UserMeetID}")
                .Distinct()
                .Count();
        }

        private class QuizProjection
        {
            public long ID { get; set; }
            public string Question { get; set; } = "";
            public MainEntity_Types Type { get; set; }
            public MainEntity_Status Status { get; set; }
            public DateTime DateInsert { get; set; }
            public bool IsForAll { get; set; }
            public List<AnswerOptionProjection> Options { get; set; } = new();
        }

        private class AnswerOptionProjection
        {
            public long ID { get; set; }
            public string Answer { get; set; } = "";
            public int Score { get; set; }
        }

        private class VoteProjection
        {
            public long QuizID { get; set; }
            public MainEntity_Types QuizType { get; set; }
            public long AnswerID { get; set; }
            public int AnswerScore { get; set; }
            public string? Content { get; set; }
            public DateTime DateInsert { get; set; }
            public long UserMeetID { get; set; }
            public string UUID { get; set; } = "";
            public string NickName { get; set; } = "";
            public string Phone { get; set; } = "";
            public string MeetID { get; set; } = "";
            public string MeetingTitle { get; set; } = "";
            public string MeetingJalaliDate { get; set; } = "";
            public DateTime? MeetingDate { get; set; }
            public string RoomFriendlyID { get; set; } = "";
            public string RoomTitle { get; set; } = "";
        }
    }
}
