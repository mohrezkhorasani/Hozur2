using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity
{
    [Index(nameof(UUID), nameof(MeetID),nameof(Type), IsUnique = true)]
    [Index(nameof(CodeUnique), IsUnique = true)]
    
    public class UserTBLToMeetingTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string MeetID { get; set; }
        public MeetingTBL MeetingTBL { get; set; }
        public string UUID { get; set; }    
        public string TokenJWT { get; set; }
        public UserTBL UserTBL { get; set; }
        public bool IsCertificated { get; set; } = false;
        public long TimeSpendSeconds { get; set; } = 0; // update after meet closed!
        public string? CertificateFileURL { get; set; }
        public string? CodeUnique { get; set; }
        public CertificateTBL CertificateTBL { get; set; }
        public string UserNickName{ get; set; }
        public string UserMeetID{ get; set; }
        public string? Role{ get; set; }
        public string NationalCode { get; set; } = "";
        public string Phone { get; set; } = "";
        public List<string> IPList { get; set; } = new List<string>();
        public ICollection<UserTBLToMeetingTBLLogs> UserTBLToMeetingTBLLogs { get; set; }
        public ICollection<MeetQuestionTBL> MeetQuestionTBLs { get; set; }
        public ICollection<MeetCommentTBL> MeetCommentTBLs { get; set; }
        public ICollection<MeetPollQuizAnsTBLToUserMeetTBL> MeetPollQuizAnsTBLToUserMeetTBLs { get; set; }

        public string ReturnTypeString()
        {
            if (Type == MainEntity_Types.USER_TEACHER)
                return "استاد";
            if (Type == MainEntity_Types.USER_MODERATOR)
                return "مدیر";
            if (Type == MainEntity_Types.USER_GUEST)
                return "مهمان";
            if (Type == MainEntity_Types.USER_ADMINSTRATOR)
                return "ادمینستریتور";
            if (Type == MainEntity_Types.USER_CREATOR)
                return "سازنده";
            if (Type == MainEntity_Types.USER_VIEWER)
                return "شرکت کننده";
            if (Type == MainEntity_Types.USER_ADMIN)
                return "کمک مدیر";
            return "نامشخص";

        }
    }
}
