using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class CertificateTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Title { get; set; }
        public long IDUserTBLToMeetingTBL { get; set;   }
        public UserTBLToMeetingTBL UserTBLToMeetingTBL { get; set; }
        public string? FilePath { get; set; }
        public string? FileURL { get; set; }
        public bool Validation { get; set; }=false;
        public bool? IsMale { get; set; }
        public string? NationalCode { get; set; }
    }
}
