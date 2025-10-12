using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity
{
    public class UserTBLToMeetingTBLLogs : MainEntity
    {
        [NotMapped]
        public List<string> Values = new List<string>
        {
            "RaisedHand",
            "OpenedMicrophone",
            "ClosedMicrophone",
            "OpenSharedFile",
            "OpenedScreenShare",
            "ClosedScreenShare",
            "CloseSharedFile",
        };
        [NotMapped]
        public List<string> Keys = new List<string>
        {
            "JOIN",
            "LEFT",
            "ACTIVITY"
        };

        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public UserTBLToMeetingTBL UserTBLToMeetingTBL { get; set; }
        public long IDUserTBLToMeetingTBL  { get; set; }
        public string Key{ get; set; }
        public string? Value{ get; set; }
        public string? OS{ get; set; }
    }
}
