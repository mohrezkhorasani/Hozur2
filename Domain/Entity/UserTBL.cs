using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


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
        [NotMapped]
        public static List<MainEntity_Types> TYPE_ALLOWED_MEET_CREATE = new List<MainEntity_Types>
        {
            MainEntity_Types.USER_ADMINSTRATOR,
            MainEntity_Types.USER_ADMIN,
            MainEntity_Types.USER_CREATOR,
            MainEntity_Types.USER_MODERATOR,
        };
        [NotMapped]
        public static List<MainEntity_Types> TYPE_ALLOWED_LOG_FOR_OTHERS = new List<MainEntity_Types>
        {
            MainEntity_Types.USER_ADMINSTRATOR,
            MainEntity_Types.USER_ADMIN,
            MainEntity_Types.USER_CREATOR,
            MainEntity_Types.USER_MODERATOR,
            MainEntity_Types.USER_TEACHER,
        };
        [NotMapped]
        public static List<MainEntity_Types> TYPE_ALLOWED_ROOM_CREATE = new List<MainEntity_Types>
        {
            MainEntity_Types.USER_ADMINSTRATOR, 
            MainEntity_Types.USER_ADMIN,
        };
        public MainEntity_Status Status { get; set; }
        public MainEntity_Types Type { get; set; }
        public string? Explain { get; set; }
        public DateTime DateInsert { get; set; }
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public string? UUID { get; set; } //Unique
        public string? NationalCode { get; set; }
        public string? Username { get; set; }
        public string? PhoneNumber { get; set; }
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public List<string> ExtId { get; set; } = new List<string>();
        public List<string> IPList { get; set; } = new List<string>();
        public string? Email { get; set; }
        public ICollection<MeetingTBL> TeacherMeetings { get; set; }
        public ICollection<MeetingTBL> CreatedMeetings { get; set; }
        public ICollection<RoomTBL> RoomTBLs { get; set; }
        public ICollection<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public ICollection<RoomTBLToUserTBL> RoomTBLToUserTBLs { get; set; }



        public static MainEntity_Types MapRoleToType(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return MainEntity_Types.Normal;

            switch (role.Trim().ToUpper())
            {
                case "GUEST":
                    return MainEntity_Types.USER_GUEST;
                case "MODERATOR":
                    return MainEntity_Types.USER_MODERATOR;
                case "ADMINISTRATOR":
                    return MainEntity_Types.USER_ADMINSTRATOR;
                case "ADMIN":
                    return MainEntity_Types.USER_ADMIN;
                case "USER":
                    return MainEntity_Types.USER_USER;
                case "CREATOR":
                    return MainEntity_Types.USER_CREATOR;
                case "VIEWER":
                    return MainEntity_Types.USER_VIEWER;
                case "TEACHER":
                    return MainEntity_Types.USER_TEACHER;
                default:
                    return MainEntity_Types.USER_UNKNOWN;
            }
        }
        public bool AddIP(string q)
        {
            if (IPList.Any(e => e == q)) return false;
            IPList.Add(q);
            return true;
        }
    }
}
