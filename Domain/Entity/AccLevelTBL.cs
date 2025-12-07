using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class AccessLevelTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Route { get; set; }
        public ICollection<AccessLevelTBLToRoleTBL> AccessLevelTBLToRoleTBLs { get; set; }

    }
}
