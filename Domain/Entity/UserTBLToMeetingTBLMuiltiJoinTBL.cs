using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity
{
    public class UserTBLToMeetingTBLMuiltiJoinTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string UserMeetID{ get; set; }
        public string IPList { get; set; } 
        public UserTBLToMeetingTBL UserTBLToMeetingTBL { get; set; }
        public long IDUserTBLToMeetingTBL { get; set; }

    }
}
