using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace Domain.Entity
{
    [Index(nameof(UUID), IsUnique = true)]
    [Index(nameof(NationalCode), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Username), IsUnique = true)]
    public class UserTBL : MainEntity
    {
        public long ID { get; set; }
        public List<Guid> AllowedGUID { get; set; } = new List<Guid>();
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public string? UUID { get; set; } //Unique
        public string? NationalCode { get; set; }
        public string? Username{ get; set; }
        public string? PhoneNumber { get; set; }
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public List<string> ExtId { get; set; } = new List<string>();
        public List<string> IPList { get; set; } = new List<string>();
        public string? Email { get; set; }
        public ICollection<MeetingTBL> TeacherMeetings { get; set; }
        public ICollection<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }

    }
}
