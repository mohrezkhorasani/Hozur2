using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Application.DTO.Models
{
    public class JWTTokenJSON
    {
        public string Token { get; set; }
    }
    public class MeetJSON
    {
        public string MeetID { get; set; }
        public JWTTokenJSON JWT { get; set; }
        public string Title { get; set; }
        public string JalaliDate { get; set; }
        public string StartedTime { get; set; }
        public string Status { get; set; } //live - end
        public string JoinUrl { get; set; } // لینک ورود
        public int CountPartis { get; set; } = 0;
        public MiniRoomJSON Room { get; set; }
        public List<MiniUserJSON> Teacher { get; set; }
        public MiniUserJSON Creator { get; set; }

    }
    public class PollReportJSON
    {
        public long ID { get; set; }
        public string Question { get; set; }
        public MainEntity_Types Type { get; set; }
        public List<AnswerReportJSON> AnswersReport { get; set; }
        public double? OverallAverageScore { get; set; } // Only for QUIZ_OPTIONAL type
    }

    public class AnswerReportJSON
    {
        public long ID { get; set; }
        public string Answer { get; set; }
        public int Score { get; set; }
        public int Count { get; set; }              // Number of users who selected this answer
        public double? Percentage { get; set; }      // Percentage of users who selected this answer
        public double? AverageScore { get; set; }    // Average score for this answer (if scored)
        public List<UserAnswerDetailJSON> UserAnswers { get; set; }
    }

    public class UserAnswerDetailJSON
    {
        public long UserMeetID { get; set; }
        public string? Content { get; set; }         // For INPUT type, contains the user's answer text
        public DateTime DateInsert { get; set; }
        public string NickName { get; set; } = "";
    }
    public class MiniUserToMeetJSON
    {
        public long ID { get; set; }
        public long UserMeetID { get; set; }
        public string Fullname { get; set; }
        public bool? IsVIPMember { get; set; }
        public bool? IsRegistered { get; set; }
    }
    public class MiniUserJSON
    {
        public long ID { get; set; }
        public string UID { get; set; }
        public string Fullname { get; set; }
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool? IsValid { get; set; }
        public MainEntity_Status? Status { get; set; }
        public bool? IsHekmatShobe { get; set; }
        public bool? IsVIPMember { get; set; }
        public string? AvatarPath { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
    public class MicroUserJSON
    {
        public long ID { get; set; }
        public string Fullname { get; set; }
    }
    public class MiniShobeJSON
    {
        public long ID { get; set; }
        public string Fullname { get; set; }
        public string Avatar { get; set; }
        public List<MiniRoomJSON> Room { get; set; }
    }
    public class MiniReservationJSON
    {
        public long ID { get; set; }
        public string NickName { get; set; }
    }
    public class PollJSON
    {
        public long ID { get; set; }
        public string Question { get; set; }
        public MainEntity_Status Status { get; set; } = MainEntity_Status.LOG_NORMAL;
        public List<PoolAnsJSON> Answers { get; set; }
        public MainEntity_Types Type { get; set; }
    }
    public class PoolAnsJSON
    {
        public long ID { get; set; }
        public string Content { get; set; }
    }
    public class MiniLogJSON
    {
        public long ID { get; set; }
        public string Content { get; set; }
        public MicroUserJSON User { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string IP { get; set; }
        public DateTime Date { get; set; }
    }
    public class MiniPlanJSON
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Teacher { get; set; }
        public string Dabir { get; set; }
        public DateTime? StartAt { get; set; }
        public MiniRoomJSON RoomJSON { get; set; }
    }
    public class RevokedTokenJSON
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime RevokedAt { get; set; }
        public string UserId { get; set; }
    }
    public class MicroRoomJSON
    {
        public string Title { get; set; }
        public string BBBMeetID { get; set; }
        public string FriendlyID { get; set; }
        public MiniUserJSON Owner { get; set; }
    }
    public class MiniMeet
    {
        public long ID { get; set; }
        public string MeetID { get; set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string Date { get; set; }
        public DateTime EndDateTime { get; set; }
        public int PartiCount { get; set; }
        public MicroRoomJSON Room { get; set; }
    }
    public class MiniRoomJSON
    {
        public string RoomID { get; set; }
        public string Title { get; set; }
        public bool IsAdmin { get; set; }
        public string BBB_MeetID { get; set; }
        public string LatestTitle { get; set; }
        public string? Avatar { get; set; }
        public MiniUserJSON Owner { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastMeet { get; set; }
        public DateTime? NextMeet { get; set; }
        public MiniPlanJSON? NextPlan { get; set; }
        public List<string> NextMeetTeacher { get; set; } = [];
        public int Count { get; set; }
        public int CountTeacher { get; set; }
        public int CountPartis { get; set; }
        public List<MiniPlanJSON> PlanJSONs { get; set; } = [];
    }
    public class MiniCalendarActivity
    {
        public int Month { get; set; } //farvardin is 0
        public int Year { get; set; } //Jalali = 1405
        public List<ActivityItem> Activities { get; set; }
    }
    public class ActivityItem
    {
        public int Day { get; set; }
        public int Count { get; set; }
        public List<MeetJSON> MeetJSONs { get; set; }
    }
    public class MiniSlideShowJSON
    {
        public string Href { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int Position { get; set; }
        public long ID { get; set; }

    }
    public class MiniProfileJSON
    {
        public bool IsFollowing { get; set; }
        public int FollowersCount { get; set; }
        public string Avatar { get; set; }
        public string Bio { get; set; }
        public string Name { get; set; }
        public long ID { get; set; }

    }
    [XmlRoot("response")]
    public class BBBHookResponse
    {
        [XmlElement("returncode")]
        public string ReturnCode { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("messageKey")]
        public string MessageKey { get; set; }

        [XmlElement("hookID")]
        public string HookID { get; set; }

        [XmlElement("callbackURL")]
        public string CallbackURL { get; set; }
    }

    public class BBBHookInfo
    {
        public string HookID { get; set; }
        public string CallbackURL { get; set; }
        public string MeetingID { get; set; }
        public bool Permanent { get; set; }
        public bool RawData { get; set; }
        public List<string> EventIDs { get; set; } = new List<string>();
    }


    public class AttendeeInfo
    {
        public string UserID { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string IsPresenter { get; set; }
        public string IsListeningOnly { get; set; }
        public string HasJoinedVoice { get; set; }
        public string HasVideo { get; set; }
        public string ClientType { get; set; }
        public string NationalCode { get; set; }
        public string Phone { get; set; }
        public string UserID5 { get; set; }
        public string Vc2AuthNorm { get; set; }
    }

}
