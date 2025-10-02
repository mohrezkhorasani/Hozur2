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
        public ICollection<MeetingTBL> MeetingTBLs { get; set; }
        public ICollection<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public ICollection<RoomTBLToUserTBL> RoomTBLToUserTBLs { get; set; }
    }
}
