using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTO;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IAdminParticipantService
    {
        bool IsSuperAdmin(UserTBL user);
        Task<bool> CanUserAccessParticipantsPageAsync(UserTBL user);
        Task<AdminParticipantsResultDTO> GetParticipantsAsync(UserTBL user, int page = 1, int count = 10, string q = "");
        Task<List<ParticipantAttendedMeetingDTO>> GetParticipantMeetingsAsync(UserTBL user, long participantUserId, string q = "");
    }

    public class AdminParticipantService : IAdminParticipantService
    {
        private readonly IDBContext _db;

        public AdminParticipantService(IDBContext db)
        {
            _db = db;
        }

        public bool IsSuperAdmin(UserTBL user)
        {
            if (user == null) return false;
            return user.Type == MainEntity_Types.USER_ADMINSTRATOR ||
                   user.VerifyUserPermissions("manage_admins");
        }

        public async Task<bool> CanUserAccessParticipantsPageAsync(UserTBL user)
        {
            if (user == null) return false;
            if (IsSuperAdmin(user)) return true;

            var permissions = user.GetUserPermissions();
            if (permissions.Contains("create_rooms") ||
                user.Type == MainEntity_Types.USER_MODERATOR ||
                user.Type == MainEntity_Types.USER_ADMIN)
            {
                return true;
            }

            // Check if user owns any room
            if (await _db.RoomTBLs.AnyAsync(r => r.IDOwnerTBL == user.ID))
                return true;

            // Check if user is room admin in any room
            if (await _db.RoomTBLToUserTBLs.AnyAsync(ru =>
                (ru.UUIDParticipatorTBL == user.UUID || (ru.ParticipatorTBL != null && ru.ParticipatorTBL.ID == user.ID)) &&
                ru.Type == MainEntity_Types.ROOMTOUSER_ADMIN &&
                ru.Status != MainEntity_Status.Deactive &&
                ru.Status != MainEntity_Status.Delete))
            {
                return true;
            }

            return false;
        }

        private IQueryable<string> GetAccessibleRoomFriendlyIds(UserTBL user)
        {
            if (IsSuperAdmin(user))
            {
                return _db.RoomTBLs.Select(r => r.FriendlyID);
            }

            return _db.RoomTBLs
                .Where(r => r.IDOwnerTBL == user.ID ||
                    _db.RoomTBLToUserTBLs.Any(ru =>
                        (ru.FriendlyID == r.FriendlyID || ru.FriendlyIDRoomTBL == r.FriendlyID) &&
                        (ru.UUIDParticipatorTBL == user.UUID || (ru.ParticipatorTBL != null && ru.ParticipatorTBL.ID == user.ID)) &&
                        ru.Type == MainEntity_Types.ROOMTOUSER_ADMIN &&
                        ru.Status != MainEntity_Status.Deactive &&
                        ru.Status != MainEntity_Status.Delete
                    )
                )
                .Select(r => r.FriendlyID);
        }

        private IQueryable<MeetingTBL> GetAccessibleMeetingsQuery(UserTBL user)
        {
            if (IsSuperAdmin(user))
            {
                return _db.MeetingTBLs.AsQueryable();
            }

            var roomFids = GetAccessibleRoomFriendlyIds(user);
            return _db.MeetingTBLs.Where(m =>
                roomFids.Contains(m.RoomFriendlyID) ||
                (m.RoomTBL != null && m.RoomTBL.IDOwnerTBL == user.ID)
            );
        }

        public async Task<AdminParticipantsResultDTO> GetParticipantsAsync(UserTBL user, int page = 1, int count = 10, string q = "")
        {
            if (page < 1) page = 1;
            if (count < 1) count = 10;

            // Non-superadmin with no rooms should get an empty result
            if (!IsSuperAdmin(user))
            {
                bool hasAccess = await CanUserAccessParticipantsPageAsync(user);
                if (!hasAccess)
                {
                    return new AdminParticipantsResultDTO
                    {
                        Items = new List<AdminParticipantItemDTO>(),
                        TotalCount = 0,
                        Page = page,
                        PageSize = count,
                        TotalPages = 0
                    };
                }
            }

            var accessibleMeetings = GetAccessibleMeetingsQuery(user);
            var accessibleRoomFids = GetAccessibleRoomFriendlyIds(user);

            // Filter users who participated in accessible meetings OR are registered members in accessible rooms
            var usersQuery = _db.UserTBLs.Where(u =>
                _db.UserTBLToMeetingTBLs.Any(um => um.UUID == u.UUID && accessibleMeetings.Any(m => m.MeetID == um.MeetID)) ||
                _db.RoomTBLToUserTBLs.Any(ru => ru.UUIDParticipatorTBL == u.UUID &&
                                                accessibleRoomFids.Contains(ru.FriendlyIDRoomTBL ?? ru.FriendlyID) &&
                                                ru.Type != MainEntity_Types.ROOMTOUSER_ADMIN &&
                                                ru.Status != MainEntity_Status.Delete &&
                                                ru.Status != MainEntity_Status.Deactive)
            );

            // Search filter
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.Firstname != null && u.Firstname.ToLower().Contains(q)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(q)) ||
                    (u.Username != null && u.Username.ToLower().Contains(q)) ||
                    (u.NationalCode != null && u.NationalCode.Contains(q)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(q)) ||
                    (u.Email != null && u.Email.ToLower().Contains(q)) ||
                    u.ID.ToString().Contains(q)
                );
            }

            int totalCount = await usersQuery.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)count);
            if (totalPages == 0 && totalCount > 0) totalPages = 1;

            var usersPage = await usersQuery
                .OrderByDescending(u => u.ID)
                .Skip((page - 1) * count)
                .Take(count)
                .ToListAsync();

            var pageUuids = usersPage.Select(u => u.UUID).Where(uuid => !string.IsNullOrEmpty(uuid)).ToList();

            // Calculate meeting counts for these users within accessible meetings
            var meetingCounts = await _db.UserTBLToMeetingTBLs
                .Where(um => pageUuids.Contains(um.UUID) && accessibleMeetings.Any(m => m.MeetID == um.MeetID))
                .GroupBy(um => um.UUID)
                .Select(g => new { UUID = g.Key, Count = g.Select(x => x.MeetID).Distinct().Count() })
                .ToDictionaryAsync(x => x.UUID, x => x.Count);

            var items = usersPage.Select(u => new AdminParticipantItemDTO
            {
                Id = u.ID,
                Uuid = u.UUID ?? "",
                FullName = $"{u.Firstname} {u.LastName}".Trim(),
                Firstname = u.Firstname ?? "",
                LastName = u.LastName ?? "",
                Username = u.Username ?? "",
                PhoneNumber = u.PhoneNumber ?? "",
                Email = u.Email ?? "",
                NationalCode = u.NationalCode ?? "",
                AvatarPath = u.AvatarPath ?? "",
                Type = (int)u.Type,
                RoleTitle = GetRoleTitle(u.Type),
                AttendedMeetingsCount = (u.UUID != null && meetingCounts.TryGetValue(u.UUID, out var c)) ? c : 0,
                CanIssueCertificate = !string.IsNullOrWhiteSpace(u.NationalCode) && !string.IsNullOrWhiteSpace(u.PhoneNumber)
            }).ToList();

            return new AdminParticipantsResultDTO
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = count,
                TotalPages = totalPages
            };
        }

        public async Task<List<ParticipantAttendedMeetingDTO>> GetParticipantMeetingsAsync(UserTBL user, long participantUserId, string q = "")
        {
            var participant = await _db.UserTBLs.FirstOrDefaultAsync(u => u.ID == participantUserId);
            if (participant == null || string.IsNullOrEmpty(participant.UUID))
                return new List<ParticipantAttendedMeetingDTO>();

            var accessibleMeetings = GetAccessibleMeetingsQuery(user);

            var attendancesQuery = _db.UserTBLToMeetingTBLs
                .Include(um => um.MeetingTBL)
                    .ThenInclude(m => m.RoomTBL)
                .Where(um => um.UUID == participant.UUID && accessibleMeetings.Any(m => m.MeetID == um.MeetID));

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                attendancesQuery = attendancesQuery.Where(um =>
                    (um.MeetingTBL != null && um.MeetingTBL.Title != null && um.MeetingTBL.Title.ToLower().Contains(q)) ||
                    (um.MeetID != null && um.MeetID.ToLower().Contains(q)) ||
                    (um.MeetingTBL != null && um.MeetingTBL.RoomFriendlyID != null && um.MeetingTBL.RoomFriendlyID.ToLower().Contains(q)) ||
                    (um.MeetingTBL != null && um.MeetingTBL.RoomTBL != null && um.MeetingTBL.RoomTBL.Title != null && um.MeetingTBL.RoomTBL.Title.ToLower().Contains(q))
                );
            }

            var attendances = await attendancesQuery
                .OrderByDescending(um => um.MeetingTBL != null ? um.MeetingTBL.DateInsert : um.DateInsert)
                .ToListAsync();

            return attendances.Select(um => new ParticipantAttendedMeetingDTO
            {
                MeetingId = um.MeetingTBL?.ID ?? 0,
                MeetId = um.MeetID ?? "",
                Title = um.MeetingTBL?.Title ?? "بدون عنوان",
                RoomFriendlyId = um.MeetingTBL?.RoomFriendlyID ?? "",
                RoomTitle = um.MeetingTBL?.RoomTBL?.Title ?? "",
                JalaliDate = um.MeetingTBL?.JalaliDate ?? "",
                DateInsert = um.MeetingTBL?.DateInsert ?? um.DateInsert,
                UserMeetId = um.ID,
                IsCertificated = um.IsCertificated,
                CertificateFileUrl = um.CertificateFileURL,
                TimeSpendSeconds = um.TimeSpendSeconds
            }).ToList();
        }

        private static string GetRoleTitle(MainEntity_Types type)
        {
            return type switch
            {
                MainEntity_Types.USER_ADMINSTRATOR => "مدیر کل",
                MainEntity_Types.USER_ADMIN => "مدیر",
                MainEntity_Types.USER_MODERATOR => "ناظر",
                MainEntity_Types.USER_TEACHER => "استاد",
                MainEntity_Types.USER_CREATOR => "سازنده",
                MainEntity_Types.USER_GUEST => "مهمان",
                MainEntity_Types.USER_VIEWER => "شرکت‌کننده",
                MainEntity_Types.USER_USER => "کاربر عادی",
                _ => "کاربر"
            };
        }
    }
}
