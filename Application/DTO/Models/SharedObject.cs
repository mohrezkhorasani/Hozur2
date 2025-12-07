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
        public string MeetID{ get; set; }
        public JWTTokenJSON JWT { get; set; }
        public string Title { get; set; }
        public string JalaliDate { get; set; }
        public string StartedTime{ get; set; }
        public string Status { get; set; } //live - end
        public MiniRoomJSON Room { get; set; }
        public List<MiniUserJSON> Teacher { get; set; }
        public MiniUserJSON Creator { get; set; }

    }
    public class MiniUserJSON
    {
        public long ID { get; set; }
        public string UID { get; set; }
        public string Fullname { get; set; }
    }
    public class MiniRoomJSON
    {
        public string RoomID { get; set; }
        public string Title { get; set; }
        public DateTime Created{ get; set; }
        public DateTime LastMeet{ get; set; }
        public int Count{ get; set; }
        public int CountTeacher { get; set; }
        public int CountPartis { get; set; }
    }
}
