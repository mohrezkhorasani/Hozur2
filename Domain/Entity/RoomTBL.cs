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
        public int Status { get; set; }
        public int Type { get; set; }
        public string Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Title { get; set; }
    }
}
