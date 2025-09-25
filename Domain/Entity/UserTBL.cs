using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Domain.Entity
{
    public class UserTBL : MainEntity
    {
        public long ID { get; set; }
        public List<Guid> Guids { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Title { get; set; }
        //passwords
    }
}
