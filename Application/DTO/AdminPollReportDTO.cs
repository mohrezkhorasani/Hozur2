using Domain.Entity;
using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public class GlobalPollReportFilterDTO
    {
        public long? QID { get; set; }
        public string? RoomFriendlyId { get; set; }
        public string? MeetId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }

    public class GlobalPollReportResponseDTO
    {
        public int Status { get; set; } = 200;
        public string Message { get; set; } = "ok";
        public GlobalPollSummaryDTO Summary { get; set; } = new GlobalPollSummaryDTO();
        public List<GlobalPollQuestionReportDTO> Questions { get; set; } = new List<GlobalPollQuestionReportDTO>();
        public List<GlobalPollMeetingBreakdownDTO> MeetingsBreakdown { get; set; } = new List<GlobalPollMeetingBreakdownDTO>();
        public List<GlobalPollRoomBreakdownDTO> RoomsBreakdown { get; set; } = new List<GlobalPollRoomBreakdownDTO>();
        public GlobalPollFilterOptionsDTO FilterOptions { get; set; } = new GlobalPollFilterOptionsDTO();
    }

    public class GlobalPollSummaryDTO
    {
        public int TotalQuestions { get; set; }
        public int OptionalScoredCount { get; set; }
        public int OptionalNoScoreCount { get; set; }
        public int TextInputCount { get; set; }
        public int TotalVotes { get; set; }
        public int UniqueRespondents { get; set; }
        public int UniqueMeetingsCount { get; set; }
        public int UniqueRoomsCount { get; set; }
        public double OverallAverageScore { get; set; }
    }

    public class GlobalPollQuestionReportDTO
    {
        public long ID { get; set; }
        public string Question { get; set; } = "";
        public MainEntity_Types Type { get; set; }
        public MainEntity_Status Status { get; set; }
        public DateTime DateInsert { get; set; }
        public bool IsForAll { get; set; }
        public int TotalVotes { get; set; }
        public int UniqueRespondents { get; set; }
        public int UniqueMeetingsCount { get; set; }
        public double? OverallAverageScore { get; set; }
        public List<GlobalPollAnswerReportDTO> AnswersReport { get; set; } = new List<GlobalPollAnswerReportDTO>();
    }

    public class GlobalPollAnswerReportDTO
    {
        public long ID { get; set; }
        public string Answer { get; set; } = "";
        public int Score { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public List<GlobalPollUserAnswerDetailDTO> UserAnswers { get; set; } = new List<GlobalPollUserAnswerDetailDTO>();
    }

    public class GlobalPollUserAnswerDetailDTO
    {
        public long UserMeetID { get; set; }
        public string NickName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string MeetID { get; set; } = "";
        public string MeetingTitle { get; set; } = "";
        public string MeetingJalaliDate { get; set; } = "";
        public string RoomFriendlyID { get; set; } = "";
        public string RoomTitle { get; set; } = "";
        public string? Content { get; set; }
        public DateTime DateInsert { get; set; }
    }

    public class GlobalPollMeetingBreakdownDTO
    {
        public string MeetID { get; set; } = "";
        public string MeetingTitle { get; set; } = "";
        public string JalaliDate { get; set; } = "";
        public DateTime? MeetingDate { get; set; }
        public string RoomFriendlyID { get; set; } = "";
        public string RoomTitle { get; set; } = "";
        public int VotesCount { get; set; }
        public int UniqueRespondents { get; set; }
        public double? AverageScore { get; set; }
    }

    public class GlobalPollRoomBreakdownDTO
    {
        public string RoomFriendlyID { get; set; } = "";
        public string RoomTitle { get; set; } = "";
        public int MeetingsCount { get; set; }
        public int VotesCount { get; set; }
        public int UniqueRespondents { get; set; }
        public double? AverageScore { get; set; }
    }

    public class GlobalPollFilterOptionsDTO
    {
        public List<GlobalPollQuestionOptionDTO> Questions { get; set; } = new List<GlobalPollQuestionOptionDTO>();
        public List<GlobalPollRoomOptionDTO> Rooms { get; set; } = new List<GlobalPollRoomOptionDTO>();
    }

    public class GlobalPollQuestionOptionDTO
    {
        public long ID { get; set; }
        public string Question { get; set; } = "";
        public MainEntity_Types Type { get; set; }
    }

    public class GlobalPollRoomOptionDTO
    {
        public string FriendlyID { get; set; } = "";
        public string Title { get; set; } = "";
    }

    public class PollQuizActionResponseDTO
    {
        public int Status { get; set; } = 200;
        public string Message { get; set; } = "ok";
        public long PollID { get; set; }
        public int PollStatus { get; set; }
    }

    public class PollStatusUpdateDTO
    {
        public int? Status { get; set; }
    }

    public class PollQuizUpdateDTO
    {
        public string Question { get; set; } = "";
        public MainEntity_Types Type { get; set; } = MainEntity_Types.QUIZ_OPTIONAL;
        public List<AnsPollRequestJSON> AnsPollRequestJSONs { get; set; } = new List<AnsPollRequestJSON>();
    }
}
