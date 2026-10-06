using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Domain.Entity
{

    public class UserTBLLogs : MainEntity
    {
        [NotMapped]
        public const string KEY_SMS = "sms";
        [NotMapped]
        public const string KEY_CODE = "otp";
        public long ID { get; set; }
 
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
   
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public UserTBL User { get; set; }
        public long IDUser { get; set; }
    }
}
