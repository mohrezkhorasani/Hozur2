using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entity
{
    [Index(nameof(UUID), nameof(MeetID), IsUnique = true)]
    public class UserTBLToMeetingTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string MeetID { get; set; }
        public MeetingTBL MeetingTBL { get; set; }
        public string UUID { get; set; }
        public UserTBL UserTBL { get; set; }
        public bool IsCertificated { get; set; } = false;
        public long TimeSpendSeconds { get; set; } = 0; // update after meet closed!
        public string? CertificateFileURL { get; set; }
        public CertificateTBL CertificateTBL { get; set; }
        public List<string> IPList { get; set; } = new List<string>();
        public ICollection<CertificateTBL> CertificateTBLs { get; set; }

    }
}
