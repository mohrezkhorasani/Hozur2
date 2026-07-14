using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class UserFileTBL : MainEntity
    {
        public static RoleTBL Empty = new RoleTBL();
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Title { get; set; } = "";
        public string URL { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string MimeType { get; set; }
        public UserTBL UserTBL { get; set; }
        public long IDUserTBL{ get; set; }

    }
}
