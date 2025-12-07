using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class AccessLevelTBLToRoleTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public DateTime DateUpdate { get; set; }
        public AccessLevelTBL AccessLevelTBL { get; set; }
        public RoleTBL RoleTBL { get; set; }
        public long IDRoleTBL { get; set; }
        public long IDAccessLevelTBL { get; set; }
    }
}
