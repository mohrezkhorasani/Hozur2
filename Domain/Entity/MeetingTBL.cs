using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class MeetingTBL : MainEntity
    {
        public long ID { get; set; }
        public string MeetID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public DateTime? EndDate { get; set; }
        public string? JalaliDate { get; set; }
        public string? Title { get; set; }
        public string RoomFriendlyID { get; set; }
        public RoomTBL RoomTBL { get; set; }
        public UserTBL Teacher { get; set; }
        public long? IDTeacher { get; set; }
        public UserTBL Creator { get; set; }
        public long IDCreator { get; set; }
        public int PresentMinutesToCert { get; set; } = 0;
        public string? JWT { get; set; }
        public bool RequiredToEnterName { get; set; } = false;
        public bool IsRecorded { get; set; } = false;
        public ICollection<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public ICollection<MeetPollQuizTBL> MeetPollQuizTBLs { get; set; }
        public ICollection<MeetingLogTBL> MeetingLogTBLs { get; set; }

        public RoomTBLPlanTBL RoomTBLPlanTBL { get; set; }
        public long? IDRoomTBLPlanTBL { get; set; }
        public static string GetStatusString(MainEntity_Status status)
        {
            switch (status)
            {
                case MainEntity_Status.MEETING_END: return "خاتمه یافته";
                case MainEntity_Status.MEETING_LIVE: return "زنده";
                case MainEntity_Status.MEETING_STARTED_LATER: return "در حال شروع";
                default: return "نامشخص";
            }
        }

        public bool IsMeetingRunning()
        {
            return Status == MainEntity_Status.MEETING_STARTED_LATER ||
                Status == MainEntity_Status.MEETING_LIVE
                
                
                ;
        }
    }
}
