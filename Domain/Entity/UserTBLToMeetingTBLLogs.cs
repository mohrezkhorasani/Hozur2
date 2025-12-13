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
            "Question",
            "Chat",
            "Review",
            "React",
        };
        [NotMapped]
        public List<string> Keys = new List<string>
        {
            "JOIN",
            "LEFT",
            "ACTIVITY"
        };
        [NotMapped]
        public static Dictionary<string, string> KEYS_TO_PERSION = new Dictionary<string, string>
        {
            { "JOIN","وارد جلسه شد"},
            { "LEFT","از جلسه خارج شد"},
            { "ACTIVITY","فعالیت خود را آغاز کرد"},
            { "RaisedHand","دست خود را بالا برد"},
            { "OpenedMicrophone","میکروفون خود را باز کرد"},
            { "ClosedMicrophone","میکروفون خود را بست"},
            { "OpenSharedFile","یک فایل به اشتراک گذاشت"},
            { "OpenedScreenShare","صفحه نمایش خود را به اشتراک گذاشت"},
            { "ClosedScreenShare","اشتراک گذاری صفحه نمایش خود را غیر فعال کرد"},
            { "CloseSharedFile","اشتراک گذاری فایل را غیر فعال کرد"},
            { "Question","یک سوال جدید پرسید"},
            { "Chat","یک پیام جدید ارسال کرد"},
            { "Review","یک باز خورد جدید ثبت کرد"},
            { "React","یک ریکشن جدید ارسال کرد"},

            { "key",""},
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
        public string UserMeetID{ get; set; }
    }
}
