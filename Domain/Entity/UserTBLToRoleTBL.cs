using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class UserTBLToRoleTBL : MainEntity
    {
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public long IDUserTBL { get; set; }
        public UserTBL User { get; set; }

        public long IDRoleTBL { get; set; }
        public RoleTBL Role { get; set; }
    }
}
