using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class MeetCommentTBL : MainEntity
    {
        public long ID { get; set; }
        public string MeetID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Content{ get; set; }
        public UserTBLToMeetingTBL UserTBLToMeeting { get; set; }
        public long IDUserTBLToMeeting { get; set; }
        public int Score { get; set; }//1-5
    }
}
