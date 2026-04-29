using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class RoomTBLPlanTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public RoomTBL RoomTBL { get; set; }
        public long IDRoomTBL { get; set; }
        public string? Title{ get; set; }
        public string? Teachers{ get; set; }
        public DateTime? StartDate { get; set; }
    }
}
