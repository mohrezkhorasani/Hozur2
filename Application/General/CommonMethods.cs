using Application.DTO;
using Application.DTO.Generals;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.General
{
    public class CommonMethods
    {
        public IConfiguration _configuration { get; }
        public IDBContext _db { get; }
        public IDBContext _db1 { get; }
        private IWebHostEnvironment _hosting;

        public CommonMethods(IConfiguration configuration, IDBContext db, IWebHostEnvironment hosting)
        {
            _configuration = configuration;
            _db = db;
            _hosting = hosting;
        }

        public CommonMethods(IConfiguration configuration, IDBContext db, IDBContext db1, IWebHostEnvironment hosting)
        {
            _configuration = configuration;
            _db = db;
            _db1 = db1;
            _hosting = hosting;
        }
        public async Task<(UserTBL, MeetingTBL)> Initialize<T>(T message)
        where T : NewUserRequest
        {
            UserTBL user = null;
            if (string.IsNullOrEmpty(message.GeneralHeaders.JWTMeetingUserToken))
            {
                user = new UserTBL
                {
                    UUID = message.UUID,
                    Email = message.Email,
                    Firstname = message.Firstname,
                    NationalCode = message.NationalCode,
                    PhoneNumber = message.PhoneNumber,
                };
                user = await NewUser(user);

            }
            else
            {
                user = await GetUser(message.GeneralHeaders.JWTMeetingUserToken);
                Statics.Log("Initialize", "user is " + (user.UUID));

            }
            if (user.AddIP(message.GeneralHeaders.RequestedIP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            Statics.Log("Initialize", "user is " + (message.GeneralHeaders.JWTMeetingToken));
            var meet = await GetMeeting(message.GeneralHeaders.JWTMeetingToken);
            return (user, meet);
        }
        public async Task<(UserTBL, List<AccessLevelTBL>)> GetUser<T>(T message)
        where T : BaseParentRequest
        {
            UserTBL user = null;
            if (string.IsNullOrEmpty(message.GeneralHeaders.JWTMeetingUserToken))
            {
                return (null, null);
            }
            else
            {
                user = await GetUser(message.GeneralHeaders.JWTMeetingUserToken);
                Statics.Log("Initialize", "user is " + (user.UUID));

            }
            if (user.AddIP(message.GeneralHeaders.RequestedIP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            Statics.Log("Initialize", "user is " + (message.GeneralHeaders.JWTMeetingToken));
            var role = await GetRole(user);
            return (user, role.AccessLevelTBLToRoleTBLs.Select(e => e.AccessLevelTBL).ToList());
        }
        public async Task<RoleTBL> GetRole(UserTBL user)
        {
            if (_db.UserTBLToRoleTBLs.Any(e => e.IDUserTBL == user.ID))
            {
                return _db.RoleTBLs.Where(e => e.UserTBLToRoleTBLs.Any(r => r.IDUserTBL == user.ID))
                    .Include(e => e.AccessLevelTBLToRoleTBLs).ThenInclude(e => e.AccessLevelTBL).First();
            }
            return null;
        }

        public async Task<UserTBL> NewUser(UserTBL user)
        {
            if (await CheckUserExist(user)) return await GetUserWithoutID(user);
            user.AllowedGUID.Add(Guid.NewGuid());

            _db.UserTBLs.Add(user);
            _db.SaveChanges();
            return user;
        }
        public string GetNationalCode(UserTBLToMeetingTBL m)
        {
            if (m.CertificateTBL?.NationalCode != null)
            {
                return m.CertificateTBL.NationalCode;
            }
            if (m.UserTBL?.NationalCode != null)
                return m.UserTBL?.NationalCode;
            return null;
        }

        public long GetTimerStayUserInMeet(UserTBLToMeetingTBL m)
        {
            if (!m.MeetingTBL.EndDate.HasValue || m.MeetingTBL.IsMeetingRunning())
            {
                m.MeetingTBL.EndDate = DateTime.UtcNow;
            }
            long seconds = 0;
            var logs = m.UserTBLToMeetingTBLLogs.OrderBy(e => e.DateInsert);
            DateTime? join = null;
            foreach (var log in logs)
            {
                if (!join.HasValue)
                {
                    if (log.Key == "JOIN")
                    {
                        join = log.DateInsert;
                    }
                }
                else
                {
                    if (log.Key == "LEFT")
                    {
                        seconds += (long)(log.DateInsert - join.Value).TotalSeconds;
                        join = null;
                    }
                }
            }
            if (join.HasValue)
            {
                DateTime end = new[] { m.MeetingTBL.EndDate.Value, m.MeetingTBL.DateInsert.AddHours(4) }.Min();
                seconds += (long)(end - join.Value).TotalSeconds;
            }
            return seconds;
        }
        public long GetTotalUniqueStayTimeInMeet(List<UserTBLToMeetingTBL> userMeetings)
        {
            if (userMeetings == null || userMeetings.Count == 0)
                return 0;

            // جمع‌آوری تمام بازه‌های زمانی حضور هر کاربر
            List<(DateTime Start, DateTime End)> allIntervals = new List<(DateTime, DateTime)>();

            foreach (var userMeeting in userMeetings)
            {
                // تنظیم زمان پایان جلسه
                if (!userMeeting.MeetingTBL.EndDate.HasValue)
                {
                    userMeeting.MeetingTBL.EndDate = DateTime.UtcNow;
                }

                DateTime endOfMeeting = new[]
                {
            userMeeting.MeetingTBL.EndDate.Value,
            userMeeting.MeetingTBL.DateInsert.AddHours(4)
        }.Min();

                // استخراج بازه‌های حضور این کاربر
                var logs = userMeeting.UserTBLToMeetingTBLLogs.OrderBy(e => e.DateInsert);
                DateTime? join = null;

                foreach (var log in logs)
                {
                    if (!join.HasValue)
                    {
                        if (log.Key == "JOIN")
                        {
                            join = log.DateInsert;
                        }
                    }
                    else
                    {
                        if (log.Key == "LEFT")
                        {
                            DateTime start = join.Value;
                            DateTime end = log.DateInsert > endOfMeeting ? endOfMeeting : log.DateInsert;

                            if (start < end)
                            {
                                allIntervals.Add((start, end));
                            }

                            join = null;
                        }
                    }
                }

                // اگر کاربر هنوز در جلسه است
                if (join.HasValue && join.Value < endOfMeeting)
                {
                    allIntervals.Add((join.Value, endOfMeeting));
                }
            }

            // اگر هیچ بازه‌ای وجود نداشت
            if (allIntervals.Count == 0)
                return 0;

            // مرتب‌سازی بازه‌ها بر اساس زمان شروع
            var sortedIntervals = allIntervals.OrderBy(i => i.Start).ToList();

            // ادغام بازه‌های همپوشان (overlapping)
            List<(DateTime Start, DateTime End)> mergedIntervals = new List<(DateTime, DateTime)>();
            DateTime currentStart = sortedIntervals[0].Start;
            DateTime currentEnd = sortedIntervals[0].End;

            for (int i = 1; i < sortedIntervals.Count; i++)
            {
                if (sortedIntervals[i].Start <= currentEnd)
                {
                    // همپوشانی وجود دارد - گسترش بازه فعلی
                    if (sortedIntervals[i].End > currentEnd)
                    {
                        currentEnd = sortedIntervals[i].End;
                    }
                }
                else
                {
                    // هیچ همپوشانی نیست - بازه قبلی را ذخیره کن
                    mergedIntervals.Add((currentStart, currentEnd));
                    currentStart = sortedIntervals[i].Start;
                    currentEnd = sortedIntervals[i].End;
                }
            }
            mergedIntervals.Add((currentStart, currentEnd));

            // محاسبه مجموع زمان‌های ادغام شده
            long totalSeconds = 0;
            foreach (var interval in mergedIntervals)
            {
                totalSeconds += (long)(interval.End - interval.Start).TotalSeconds;
            }

            return totalSeconds;
        }

        public async Task<MeetingTBL> NewMeeting(MeetingTBL tbl)
        {
            if (await CheckMeetingExist(tbl)) return await GetMeetWithoutID(tbl.MeetID);
            Console.WriteLine("inja");
            tbl.JWT = Statics.GenerateJWTMeet(tbl);
            Console.WriteLine("inj2");
            _db.MeetingTBLs.Add(tbl);
            Console.WriteLine("inj3");

            await _db.SaveChangesAsync();
            Console.WriteLine("inj4");

            return tbl;
        }
        public async Task<UserTBL> GetUserWithoutID(UserTBL user)
        {
            if (user.UUID != null)
            {
                return _db.UserTBLs.Where(e => e.UUID == user.UUID).FirstOrDefault();
            }
            if (user.PhoneNumber != null)
            {
                return _db.UserTBLs.Where(e => e.PhoneNumber == user.PhoneNumber).FirstOrDefault();
            }
            if (user.Username != null)
            {
                return _db.UserTBLs.Where(e => e.Username == user.Username).FirstOrDefault();
            }
            if (user.Email != null)
            {
                return _db.UserTBLs.Where(e => e.Email == user.Email).FirstOrDefault();
            }
            if (user.NationalCode != null)
            {
                return _db.UserTBLs.Where(e => e.NationalCode == user.NationalCode).FirstOrDefault();
            }
            return null;
        }
        public async Task<MeetingTBL> GetMeetWithoutID(string q)
        {
            if (!await _db.MeetingTBLs.AnyAsync(e => e.MeetID == q)) return null;
            return _db.MeetingTBLs
            .Where(e => e.MeetID == q).Include(e => e.Creator)
            .Include(e => e.Teacher).Include(e => e.RoomTBL)
            .FirstOrDefault();
        }
        public async Task<UserTBLToMeetingTBL> GetUserMeetWithoutID(UserTBLToMeetingTBL q)
        {
            return _db.UserTBLToMeetingTBLs
            .FirstOrDefault(e => e.MeetID == q.MeetID && e.UUID == q.UUID );
        }

        public async Task<bool> CheckUserGUID(UserTBL user, string q)
        {
            if (user.ID == null) return false;
            if (user.AllowedGUID.Any(e => e.ToString() == q)) return true;
            return false;
        }
        public async Task<(bool, string)> CheckGuaranteedUser(UserTBL user, MeetingTBL meet)
        {
            if (new List<MainEntity_Types>
            { MainEntity_Types.USER_CREATOR, MainEntity_Types.USER_MODERATOR, MainEntity_Types.USER_ADMIN, MainEntity_Types.USER_ADMINSTRATOR
            ,MainEntity_Types.USER_GUEST , MainEntity_Types.USER_VIEWER,}.Contains(user.Type))
            {
                return (true, "خب");
            }
            if ((DateTime.Now - meet.DateInsert).TotalMinutes < 50)
                return (true, "هنوز برای صدور گواهی زوده");
            return (true, "ok");
        }

        public async Task<bool> CheckUserAccessibilityToMeet(UserTBL user, MeetingTBL meet, string action = "")
        {
            if (UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
            {

                return true;
            }
            if (_db.RoomTBLToUserTBLs.Any(e => e.Status == MainEntity_Status.USER_ACTIVE && e.UUIDParticipatorTBL == user.UUID))
            {
                return true;
            }
            if (_db.RoomTBLs.Any(e => e.IDOwnerTBL == user.ID && e.FriendlyID == meet.RoomFriendlyID))
            {
                return true;
            }
            return false;

        }


        public async Task<bool> AddUserExtID(UserTBL user, string q)
        {
            if (user.ID == null) return false;
            if (user.ExtId.Any(e => e == q)) return true;
            user.ExtId.Add(q);
            _db.Update(user);
            _db.SaveChanges();
            return true;
        }

        public async Task<MeetingTBL> GetMeeting(string jwt)
        {
            try
            {
                var claims = JwtHelper.GetAllClaims(jwt);
                Statics.Log("jwt is", jwt);
                Statics.Log("claims", string.Join(",", claims));
                var meetID = claims["guid"];
                var roomID = claims["room"];
                var meet = new MeetingTBL
                {
                    MeetID = meetID,
                    RoomFriendlyID = roomID
                };
                if (await CheckMeetingExist(meet))
                {
                    meet = await GetMeetWithoutID(meet.MeetID);
                    return meet;
                }
            }
            catch (Exception e)
            {
                Statics.Log("exp", e.Message);
            }

            return null;
        }
        public async Task<UserTBL> GetUser(string jwt)
        {
            var claims = JwtHelper.GetAllClaims(jwt);
            string guid = null;
            if (claims.TryGetValue("guid", out var guidValue) && !string.IsNullOrWhiteSpace(guidValue))
            {
                guid = guidValue;
            }
            else
                guid = "";
            var uuid = claims["uuid"];
            if (_db.UserTBLs.Any(e => e.UUID == uuid))
            {
                return await _db.UserTBLs.FirstOrDefaultAsync(e => e.UUID == uuid);
            }
            return await _db.UserTBLs.FirstOrDefaultAsync(e => e.AllowedGUID.Contains(Guid.Parse(guid)));
        }
        public async Task<bool> ValidateUserIds(UserTBL user)
        {
            if (user.UUID != null)
            {
                return true;
            }
            if (user.PhoneNumber != null)
            {
                return true;
            }
            if (user.Username != null)
            {
                return true;
            }
            if (user.Email != null)
            {
                return true;
            }
            if (user.NationalCode != null)
            {
                return true;
            }
            return false;
        }

        public async Task<UserTBLToMeetingTBL> NewUserMeeting(UserTBLToMeetingTBL tbl)
        {
            if (await CheckUserMeetingExist(tbl)) return await GetUserMeetWithoutID(tbl);
            _db.UserTBLToMeetingTBLs.Add(tbl);
            _db.SaveChanges();
            return tbl;
        }
        public async Task<bool> CheckUserExist(UserTBL user)
        {
            return await _db.UserTBLs.AnyAsync(x =>
                x.ID == user.ID ||
                (user.UUID != null && x.UUID == user.UUID) ||
                (user.Email != null && x.Email == user.Email) ||
                (user.PhoneNumber != null && x.PhoneNumber == user.PhoneNumber) ||
                (user.NationalCode != null && x.NationalCode == user.NationalCode)
            );
        }
        public async Task<bool> CheckMeetingExist(MeetingTBL user)
        {
            return await _db.MeetingTBLs.AnyAsync(x =>
                x.ID == user.ID ||
                (user.MeetID != null && x.MeetID == user.MeetID && user.RoomFriendlyID == x.RoomFriendlyID)
            );
        }
        public async Task<bool> CheckUserMeetingExist(UserTBLToMeetingTBL user)
        {
            return await _db.UserTBLToMeetingTBLs.AnyAsync(x =>
                x.ID == user.ID ||
                (user.MeetID != null && x.MeetID == user.MeetID && user.UUID == x.UUID )
            );
        }

        public string GetTimerUserLogs(ICollection<UserTBLToMeetingTBLLogs> userTBLToMeetingTBLLogs)
        {
            if (userTBLToMeetingTBLLogs == null || !userTBLToMeetingTBLLogs.Any(e => e.Key.ToLower() == "join"))
            {
                return "غائب";
            }
            string result = "";
            var list = userTBLToMeetingTBLLogs.Where(e => e.Key.ToLower() == "join").OrderBy(e => e.DateInsert).ToList();
            foreach (var i in list)
            {
                //check for next left
                //loop
                //به این صورت که با تاریخ چک بشه اولین لفت بعدیو بگیره
            }


            return result;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            try
            {


                using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
                {

                    var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    for (int i = 0; i < computedHash.Length; i++)
                    {
                        if (computedHash[i] != passwordHash[i]) return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashedBytes); // طول ثابت 44 کاراکتر
            }
        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

#pragma warning disable CS8603 // Possible null reference return.
        public async Task<string> ReturnPassword(string FriendlyID, bool modir = false)
        {
            try
            {
                var e = await _db.RoomTBLs.Where(e => e.FriendlyID == FriendlyID).FirstOrDefaultAsync();
                var code = modir ? e.ModeratorCode : e.ViewerCode;
                if (string.IsNullOrEmpty(code))
                {
                    code = modir ? Statics.bbb.ModeratorPassword : Statics.bbb.AttendPassword;
                    e.ModeratorCode = Statics.bbb.ModeratorPassword;
                    e.ViewerCode = Statics.bbb.AttendPassword;
                    _db.SaveChanges();
                }
                return code;

            }
            catch (Exception e)
            {
                return "";
            }
        }

#pragma warning restore CS8603 // Possible null reference return.

    }
}
