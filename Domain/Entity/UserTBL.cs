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
        public static List<MainEntity_Types> TYPE_ALLOWED_QUESTION_ACTION = new List<MainEntity_Types>
        {
            MainEntity_Types.USER_ADMINSTRATOR,
            MainEntity_Types.USER_ADMIN,
            MainEntity_Types.USER_CREATOR,
            MainEntity_Types.USER_TEACHER,
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
        public bool IsHekmatShobe { get; set; } = false;
        public bool IsMemberVIP { get; set; } = false;
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
        public DateTime? OTPExpire { get; set; } = null;
        public byte[] OTPHash { get; set; } = Array.Empty<byte>();
        public byte[] OTPSalt { get; set; } = Array.Empty<byte>();
        public List<string> ExtId { get; set; } = new List<string>();
        public List<string> IPList { get; set; } = new List<string>();
        public string? Email { get; set; }
        public string? AvatarPath { get; set; }
        public DateTime? LastUpdate { get; set; }
        public ICollection<MeetingTBL> TeacherMeetings { get; set; }
        public ICollection<MeetingTBL> CreatedMeetings { get; set; }
        public ICollection<MeetingReservationTBL> MeetingReservationTBLs { get; set; }
        public ICollection<RoomTBL> RoomTBLs { get; set; }
        public ICollection<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public ICollection<RoomTBLToUserTBL> RoomTBLToUserTBLs { get; set; }
        public ICollection<UserRevokeTokenTBL> UserRevokeTokenTBLs { get; set; }
        public ICollection<UserTBLToUserTBL> Followings { get; set; }
        public ICollection<UserTBLToUserTBL> Followers { get; set; }
        public ICollection<UserFileTBL> UserFileTBLs { get; set; }
        public ICollection<UserTBLLogs> UserTBLLogs { get; set; }
        public string NajmID { get; set; } = "";

        public string GetName()
        {
            return $@"{Firstname} {LastName}";
        }
        public List<string> GetUserPermissions()
        {
            var permissions = new List<string>();
            UserTBL user = this;
            // اضافه کردن پرمیشن‌ها بر اساس نقش کاربر
            switch (user.Type)
            {
                case MainEntity_Types.USER_ADMINSTRATOR:
                    permissions.AddRange(new[] { "manage_admins", "manage_bbb_hooks", });
                    goto case MainEntity_Types.USER_ADMIN;
                case MainEntity_Types.USER_ADMIN:
                    permissions.AddRange(new[] { "manage_users", "view_reports", "create_rooms_for_others", "edit_poll", "see_relatedrooms", "add_relatedrooms" });
                    goto case MainEntity_Types.USER_ADMIN2;
                case MainEntity_Types.USER_ADMIN2:
                    permissions.AddRange(new[] { "pay_request" });
                    goto case MainEntity_Types.USER_MODERATOR;
                case MainEntity_Types.USER_MODERATOR:
                    permissions.AddRange(new[] { "create_rooms", "view_poll_reports" });
                    goto case MainEntity_Types.USER_CREATOR;
                case MainEntity_Types.USER_CREATOR:
                    permissions.AddRange(new[] { "create_meeting", "edit_meeting", "delete_meeting", "ftp", "edit_meeting_member" });
                    goto case MainEntity_Types.USER_TEACHER;
                case MainEntity_Types.USER_TEACHER:
                case MainEntity_Types.USER_VIEWER:
                    permissions.AddRange(new[] { "follow_user", "block_user" });
                    break;
                case MainEntity_Types.USER_GUEST:
                default:
                    permissions.AddRange(new[] { "join_meeting", "view_meeting" });
                    break;
            }

            return permissions;
        }

        public bool VerifyUserPermissions(params string[] permissions)
        {
            var userPermissions = GetUserPermissions();
            return permissions.All(p => userPermissions.Contains(p));
        }

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
        public static string MapTypeToRole(MainEntity_Types type)
        {
            switch (type)
            {
                case MainEntity_Types.USER_GUEST:
                    return "GUEST";
                case MainEntity_Types.USER_MODERATOR:
                    return "MODERATOR";
                case MainEntity_Types.USER_ADMINSTRATOR:
                    return "ADMINISTRATOR";
                case MainEntity_Types.USER_ADMIN:
                    return "ADMIN";
                case MainEntity_Types.USER_USER:
                    return "USER";
                case MainEntity_Types.USER_CREATOR:
                    return "CREATOR";
                case MainEntity_Types.USER_VIEWER:
                    return "VIEWER";
                case MainEntity_Types.USER_TEACHER:
                    return "TEACHER";
                case MainEntity_Types.USER_UNKNOWN:
                    return "UNKNOWN";
                default:
                    return "NORMAL";
            }
        }
        public string GetRole()
        {
            switch (Type)
            {
                case MainEntity_Types.USER_GUEST:
                    return "GUEST";
                case MainEntity_Types.USER_MODERATOR:
                    return "MODERATOR";
                case MainEntity_Types.USER_ADMINSTRATOR:
                    return "ADMINISTRATOR";
                case MainEntity_Types.USER_ADMIN:
                    return "ADMIN";
                case MainEntity_Types.USER_USER:
                    return "USER";
                case MainEntity_Types.USER_CREATOR:
                    return "CREATOR";
                case MainEntity_Types.USER_VIEWER:
                    return "VIEWER";
                case MainEntity_Types.USER_TEACHER:
                    return "TEACHER";
                case MainEntity_Types.USER_UNKNOWN:
                    return "UNKNOWN";
                default:
                    return "NORMAL";
            }
        }

        public bool AddIP(string q)
        {
            if (IPList.Any(e => e == q)) return false;
            IPList.Add(q);
            return true;
        }
        public bool AddGuid(Guid q)
        {
            if (AllowedGUID.Any(e => e == q)) return false;
            AllowedGUID.Add(q);
            return true;
        }
    }
}
