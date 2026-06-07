using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity
{
    public class MeetingReservationTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public long IDRoomTBLPlanTBL { get; set; }
        public RoomTBLPlanTBL RoomTBLPlanTBL { get; set; }
        public long? IDUserTBL{ get; set; }    
        public string TokenJWT { get; set; }
        public UserTBL UserTBL { get; set; }

        public string UserNickName{ get; set; }
        public string Phone{ get; set; }

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
