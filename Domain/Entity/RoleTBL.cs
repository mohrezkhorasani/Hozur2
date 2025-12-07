using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class RoleTBL : MainEntity
    {
        public static RoleTBL Empty = new RoleTBL();
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Name { get; set; } = "";
        public ICollection<AccessLevelTBLToRoleTBL> AccessLevelTBLToRoleTBLs { get; set; }
        public ICollection<UserTBLToRoleTBL> UserTBLToRoleTBLs { get; set; }
    }
}
