using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entity
{
    public class UserRevokeTokenTBL : MainEntity
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string Token { get; set; }
        [Required]
        [MaxLength(64)]  // طول ثابت برای SHA256 در Base64
        [Column(TypeName = "nvarchar(64)")]
        public string TokenHash { get; set; }

        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public UserTBL UserTBL { get; set; }
        public long IDUserTBL { get; set; }
    }
}
