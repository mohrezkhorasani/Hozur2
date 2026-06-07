using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class MeetPollQuizAnsTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Answer{ get; set; }
        public int Score{ get; set; }
        public long IDQuizTBL { get; set; }
        public MeetPollQuizTBL QuizTBL { get; set; }
        public ICollection<MeetPollQuizAnsTBLToUserMeetTBL> MeetPollQuizAnsTBLToUserMeetTBLs { get; set; }
    }
}
