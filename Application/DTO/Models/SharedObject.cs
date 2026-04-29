using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public MiniRoomJSON Room { get; set; }
        public List<MiniUserJSON> Teacher { get; set; }
        public MiniUserJSON Creator { get; set; }

    }
    public class MiniUserJSON
    {
        public long ID { get; set; }
        public string UID { get; set; }
        public string Fullname { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool? IsValid { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
    public class MiniShobeJSON
    {
        public long ID { get; set; }
        public string Fullname { get; set; }
        public string Avatar { get; set; }
        public List<MiniRoomJSON> Room { get; set; }
    }

    public class MiniPlanJSON
    {
        public long Id { get; set; }
        public string Title{ get; set; }
        public string Teacher{ get; set; }
        public DateTime? StartAt{ get; set; }
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
    public class MiniRoomJSON
    {
        public string RoomID { get; set; }  
        public string Title { get; set; }

        public string BBB_MeetID { get; set; }
        public string LatestTitle { get; set; }
        public string? Avatar { get; set; }
        public MiniUserJSON Owner { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastMeet { get; set; }
        public DateTime? NextMeet { get; set; }
        public List<string> NextMeetTeacher { get; set; } = [];
        public int Count { get; set; }
        public int CountTeacher { get; set; }
        public int CountPartis { get; set; }
    }
    public class MiniSlideShowJSON
    {
        public string Href { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int Position{ get; set; }
        public long ID { get; set; }

    }
    public class MiniProfileJSON
    {
        public bool IsFollowing { get; set; }
        public int FollowersCount { get; set; }
        public string Avatar{ get; set; }
        public string Bio { get; set; }
        public string Name { get; set; }
        public long ID { get; set; }

    }

}
