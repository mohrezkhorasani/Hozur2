using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class MeetingLogTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public MeetingTBL MeetingTBL { get; set; }
        public long IDMeetingTBL{ get; set; }
        public string? Key { get; set;  }
        public string? Value { get; set;  }
    }
}
    