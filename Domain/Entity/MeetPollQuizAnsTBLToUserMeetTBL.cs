using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class MeetPollQuizAnsTBLToUserMeetTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public UserTBLToMeetingTBL UserTBLToMeetingTBL { get; set; }
        public long? IDUserTBLToMeetingTBL { get; set; }
        public MeetPollQuizAnsTBL MeetPollQuizAnsTBL { get; set; }
        public long IDMeetPollQuizAnsTBL { get; set; }
        public string? Content { get; set; } //if it is type is Input not OPTIONAL
    }
}
