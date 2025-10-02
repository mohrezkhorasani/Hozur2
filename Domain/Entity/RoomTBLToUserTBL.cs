using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class RoomTBLToUserTBL : MainEntity
    {
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }//USER TYPE
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public UserTBL ParticipatorTBL { get; set; }
        public string UUIDParticipatorTBL{ get; set; }
        public RoomTBL RoomTBL { get; set; }
        public string FriendlyIDRoomTBL{ get; set; }
    }
}
