using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class RoomTBL : MainEntity
    {
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Title { get; set; }
        public UserTBL OwnerTBL { get; set; }
        public long IDOwnerTBL{ get; set; }
        public string? TTR_URL{ get; set; }
        public string? BBB_MeetID{ get; set; }
        public string? ModeratorCode { get; set; }
        public string? Avatar { get; set; }
        public string? ViewerCode { get; set; }
        //قبل از پیوستن به جلسه نیاز به تأیید مدیر باشد
        public bool RequiredAdminAcceptToView { get; set; } = false;
        //کاربران قبل از ورود به جلسه، نیازمند ورود به سیستم باشند
        public bool RequiredLoginToView { get; set; } = false;
        //اجازه دهید اتاق ضبط شود
        public bool RecordRoom { get; set; } = true;
        public bool IsPublicRoom { get; set; } = false;
        public string? PresentationPath { get; set; }
        public ICollection<MeetingTBL> MeetingTBLs { get; set; }
        public ICollection<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public ICollection<RoomTBLToUserTBL> RoomTBLToUserTBLs { get; set; }
        public ICollection<RoomTBLPlanTBL> RoomTBLPlanTBLs { get; set; }
    }
}
