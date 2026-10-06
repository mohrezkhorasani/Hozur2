using Application.DTO;
using Application.DTO.Generals;
using Application.Interfaces;
using DocumentFormat.OpenXml.Drawing.Charts;
using Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
            if (!string.IsNullOrEmpty(m.CertificateTBL?.NationalCode))
            {
                return m.CertificateTBL.NationalCode;
            }
            if (!string.IsNullOrEmpty(m.NationalCode))
            {
                return m.NationalCode;
            }
            if (!string.IsNullOrEmpty(m.UserTBL?.NationalCode))
                return m.UserTBL.NationalCode;
            return null;
        }
        public bool CheckCertificate(UserTBLToMeetingTBL e)
        {
            if (e.CertificateTBL == null)
                return false;
            if (e.CertificateTBL.FilePath != null)
                return true;
            return false;
        }
        public object GetCertificate(UserTBLToMeetingTBL e)
        {
            if (e.CertificateTBL == null)
                return null;
            if (e.CertificateTBL.FilePath != null)
                return new
                {
                    e.CertificateTBL.FilePath,
                    e.CertificateTBL.FileURL,
                    e.CertificateTBL.Validation,
                    e.CertificateTBL.NationalCode,
                    e.CertificateTBL.IsMale,
                    e.CertificateTBL.Title,
                    e.CertificateTBL.ID,
                    e.CertificateTBL.IDUserTBLToMeetingTBL,
                };
            return null;
        }
        public object GetMultiJoins(UserTBLToMeetingTBL e)
        {
            if (e.UserTBLToMeetingTBLMuiltiJoinTBLs == null)
                return null;
            return e.UserTBLToMeetingTBLMuiltiJoinTBLs.Select(q => new
            {
                q.IDUserTBLToMeetingTBL,
                q.UserMeetID,
                q.IPList,
                q.ID
            }).ToList();
        }

        public long GetTimerStayUserInMeet(UserTBLToMeetingTBL m,bool isMeetingEnded=true)
        {
            if (m.TimeSpendSeconds != 0 && isMeetingEnded)
                return m.TimeSpendSeconds;
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
//                net accounts / lockoutduration:< minutes >
//مثال :
//net accounts / lockoutduration:40
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
            m.TimeSpendSeconds = seconds;
            _db.Update(m);
            _db.SaveChanges();
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
            .FirstOrDefault(e => e.MeetID == q.MeetID && e.UUID == q.UUID);
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
        public bool VerifyAdminAccessOfRoom(RoomTBL room, UserTBL user)
        {
            if (room.IDOwnerTBL == user.ID) return true;
            if (room.RoomTBLToUserTBLs.Any(e => e.UUIDParticipatorTBL == user.UUID && e.Type == MainEntity_Types.ROOMTOUSER_ADMIN)) return true;
            return false;
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
            try
            {

                if (await CheckUserMeetingExist(tbl))
                {
                    var r = await GetUserMeetWithoutID(tbl);
                    if (r.Status != MainEntity_Status.USERMEET_JOINED)
                    {
                        r.Status = MainEntity_Status.USERMEET_JOINED;
                        r.DateUpdate = DateTime.UtcNow;
                        r.TokenJWT = tbl.TokenJWT;
                        r.Explain = tbl.Explain;
                        r.Type = tbl.Type;
                        r.Role = tbl.Role;
                        r.IsMember = tbl.IsMember;
                        r.UserMeetID = tbl.UserMeetID;
                        r.Phone = tbl.Phone;
                        r.NationalCode = tbl.NationalCode;
                        r.IPList.Add(tbl.IPList[0]);
                        r.UserNickName = tbl.UserNickName;
                        if (!r.IsCertificated)
                        {
                            r.IsCertificated = tbl.IsCertificated;
                        }
                        if (tbl.TimeSpendSeconds > r.TimeSpendSeconds)
                        {
                            r.TimeSpendSeconds = tbl.TimeSpendSeconds;
                        }
                        _db.Update(r);
                        _db.SaveChanges();

                    }
                    return await GetUserMeetWithoutID(tbl);
                }
                _db.UserTBLToMeetingTBLs.Add(tbl);
                _db.SaveChanges();
                return tbl;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
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
                (user.MeetID != null && x.MeetID == user.MeetID && user.UUID == x.UUID)
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

        public async Task<int> SyncWithNajm(MeetingTBL? meet = null, UserTBL? user = null)
        {
            if (meet == null) return 400;
            return await SyncWithNajm(meet.MeetID);
        }

        public async Task<int> SyncWithNajm(string meetId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(meetId))
                {
                    Statics.Log("SyncWithNajm_Error", "meetId is null or empty");
                    return 400;
                }

                MeetingTBL? meet = null;
                if (long.TryParse(meetId, out long numericId))
                {
                    meet = await _db.MeetingTBLs
                        .Include(m => m.RoomTBL)
                            .ThenInclude(r => r.OwnerTBL)
                        .Include(m => m.Teacher)
                        .Include(m => m.Creator)
                        .Include(m => m.RoomTBLPlanTBL)
                        .Include(m => m.UserTBLToMeetingTBLs)
                            .ThenInclude(um => um.UserTBL)
                        .FirstOrDefaultAsync(m => m.ID == numericId || m.MeetID == meetId);
                }
                else
                {
                    meet = await _db.MeetingTBLs
                        .Include(m => m.RoomTBL)
                            .ThenInclude(r => r.OwnerTBL)
                        .Include(m => m.Teacher)
                        .Include(m => m.Creator)
                        .Include(m => m.RoomTBLPlanTBL)
                        .Include(m => m.UserTBLToMeetingTBLs)
                            .ThenInclude(um => um.UserTBL)
                        .FirstOrDefaultAsync(m => m.MeetID == meetId);
                }

                if (meet == null)
                {
                    Statics.Log("SyncWithNajm_Error", $"Meeting '{meetId}' not found in DB.");
                    return 404;
                }

                if (meet.RoomTBL == null && !string.IsNullOrEmpty(meet.RoomFriendlyID))
                {
                    meet.RoomTBL = await _db.RoomTBLs
                        .Include(r => r.OwnerTBL)
                        .FirstOrDefaultAsync(r => r.FriendlyID == meet.RoomFriendlyID);
                }

                var room = meet.RoomTBL;
                if (room == null || string.IsNullOrWhiteSpace(room.NajmID))
                {
                    Statics.Log("SyncWithNajm", $"Room NajmID is null or empty for meeting '{meet.MeetID}'. Skipping Najm sync as required.");
                    return 200;
                }

                string groupId = room.NajmID.Trim();
                if (groupId.Length > 1)
                {
                    groupId = groupId.Substring(0, 1);
                }

                if (string.IsNullOrWhiteSpace(groupId))
                {
                    Statics.Log("SyncWithNajm", $"Room NajmID character is empty for meeting '{meet.MeetID}'. Skipping Najm sync.");
                    return 200;
                }

                // Generate 120-minute token before request
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                string? token = null;
                try
                {
                    var tokenResp = await httpClient.GetAsync("http://192.168.0.4:1940/api/token/generate?minutes=120");
                    if (tokenResp.IsSuccessStatusCode)
                    {
                        var tokenJson = await tokenResp.Content.ReadAsStringAsync();
                        using var tokenDoc = System.Text.Json.JsonDocument.Parse(tokenJson);
                        if (tokenDoc.RootElement.TryGetProperty("token", out var tokProp))
                        {
                            token = tokProp.GetString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Statics.Log("SyncWithNajm_Error", $"Error requesting token from Najm: {ex.Message}");
                    return 500;
                }

                if (string.IsNullOrEmpty(token))
                {
                    Statics.Log("SyncWithNajm_Error", "Failed to retrieve valid token from Najm API.");
                    return 500;
                }

                // Date and Times in Iran timezone (UTC+3:30)
                DateTime startUtc = meet.DateInsert != default ? meet.DateInsert : DateTime.UtcNow;
                if (startUtc.Kind != DateTimeKind.Utc)
                    startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);

                DateTime endUtc = meet.EndDate ?? DateTime.UtcNow;
                if (endUtc.Kind != DateTimeKind.Utc)
                    endUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);

                TimeZoneInfo iranZone;
                try
                {
                    iranZone = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
                }
                catch
                {
                    try
                    {
                        iranZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
                    }
                    catch
                    {
                        iranZone = TimeZoneInfo.CreateCustomTimeZone("Iran Standard Time", TimeSpan.FromMinutes(210), "Iran Standard Time", "Iran Standard Time");
                    }
                }

                DateTime localStart = TimeZoneInfo.ConvertTimeFromUtc(startUtc, iranZone);
                DateTime localEnd = TimeZoneInfo.ConvertTimeFromUtc(endUtc, iranZone);

                var pc = new PersianCalendar();
                string dateStr = $"{pc.GetYear(localStart):0000}/{pc.GetMonth(localStart):00}/{pc.GetDayOfMonth(localStart):00}";
                string startTimeStr = $"{localStart.Hour:00}:{localStart.Minute:00}";
                string endTimeStr = $"{localEnd.Hour:00}:{localEnd.Minute:00}";

                string title = !string.IsNullOrWhiteSpace(meet.Title) ? meet.Title.Trim() : (!string.IsNullOrWhiteSpace(room.Title) ? room.Title.Trim() : "جلسه");

                // Dastor: combination of meeting title and explain/agenda
                string dastor = "";
                if (!string.IsNullOrWhiteSpace(meet.Explain))
                {
                    dastor = $"موضوع جلسه: {title}\nدستور جلسه: {meet.Explain.Trim()}";
                }
                else
                {
                    dastor = $"موضوع جلسه: {title}";
                }

                var allUserMeets = meet.UserTBLToMeetingTBLs != null ? meet.UserTBLToMeetingTBLs.ToList() : new List<UserTBLToMeetingTBL>();

                // Boss: NajmIDBoss from table roomTBL
                UserTBL? bossUser = null;
                if (!string.IsNullOrWhiteSpace(room.NajmIDBoss))
                {
                    string bossNajmIdLookup = room.NajmIDBoss.Trim();
                    bossUser = await _db.UserTBLs.FirstOrDefaultAsync(u => u.NajmID == bossNajmIdLookup);
                }
                if (bossUser == null)
                {
                    bossUser = room.OwnerTBL;
                    if (bossUser == null && room.IDOwnerTBL > 0)
                    {
                        bossUser = await _db.UserTBLs.FirstOrDefaultAsync(u => u.ID == room.IDOwnerTBL);
                    }
                }

                var bossUserMeet = allUserMeets.FirstOrDefault(um => bossUser != null && (um.UUID == bossUser.UUID || (um.UserTBL != null && um.UserTBL.ID == bossUser.ID)));

                string bossNajmId = !string.IsNullOrWhiteSpace(room.NajmIDBoss)
                    ? room.NajmIDBoss.Trim()
                    : (!string.IsNullOrWhiteSpace(bossUser?.NajmID) ? bossUser.NajmID.Trim() : "0");

                string bossBbbId = bossUserMeet != null ? bossUserMeet.ID.ToString() : "0";
                string bossNatCode = !string.IsNullOrWhiteSpace(bossUser?.NationalCode) ? bossUser.NationalCode.Trim() : "";

                var bossObj = new
                {
                    firstName = bossUser?.Firstname ?? "",
                    lastName = bossUser?.LastName ?? "",
                    najmId = bossNajmId,
                    bbbId = bossBbbId,
                    nationalCode = bossNatCode
                };

                // Presenter: first teacher
                var teacherMeets = allUserMeets
                    .Where(um => um.Type == MainEntity_Types.USER_TEACHER ||
                                 (um.Role != null && (um.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) || um.Role.Contains("استاد"))) ||
                                 (meet.IDTeacher.HasValue && um.UserTBL != null && um.UserTBL.ID == meet.IDTeacher.Value))
                    .ToList();

                UserTBLToMeetingTBL? primaryTeacherMeet = teacherMeets.FirstOrDefault();
                UserTBL? primaryTeacherUser = primaryTeacherMeet?.UserTBL;
                if (primaryTeacherUser == null && meet.Teacher != null)
                {
                    primaryTeacherUser = meet.Teacher;
                }
                else if (primaryTeacherUser == null && meet.IDTeacher.HasValue)
                {
                    primaryTeacherUser = await _db.UserTBLs.FirstOrDefaultAsync(u => u.ID == meet.IDTeacher.Value);
                }

                string presenterNajmId = !string.IsNullOrWhiteSpace(primaryTeacherUser?.NajmID) ? primaryTeacherUser.NajmID.Trim() : "0";
                string presenterBbbId = primaryTeacherMeet != null ? primaryTeacherMeet.ID.ToString() : "0";
                string presenterNatCode = !string.IsNullOrWhiteSpace(primaryTeacherUser?.NationalCode) ? primaryTeacherUser.NationalCode.Trim() : "";

                var presenterObj = new
                {
                    firstName = primaryTeacherUser?.Firstname ?? "",
                    lastName = primaryTeacherUser?.LastName ?? "",
                    najmId = presenterNajmId,
                    bbbId = presenterBbbId,
                    nationalCode = presenterNatCode
                };

                // Dabir: meeting dabir
                var dabirMeet = allUserMeets.FirstOrDefault(um => um.Type == MainEntity_Types.USER_DABIR ||
                    (um.Role != null && (um.Role.Equals("Dabir", StringComparison.OrdinalIgnoreCase) || um.Role.Contains("دبیر"))));

                UserTBL? dabirUser = dabirMeet?.UserTBL;
                if (dabirUser == null && meet.RoomTBLPlanTBL != null && !string.IsNullOrWhiteSpace(meet.RoomTBLPlanTBL.Dabir))
                {
                    string dabirPlanName = meet.RoomTBLPlanTBL.Dabir.Trim();
                    dabirUser = await _db.UserTBLs.FirstOrDefaultAsync(u => (u.Firstname + " " + u.LastName).Trim() == dabirPlanName);
                }

                string dabirNajmId = !string.IsNullOrWhiteSpace(dabirUser?.NajmID) ? dabirUser.NajmID.Trim() : "0";
                string dabirBbbId = dabirMeet != null ? dabirMeet.ID.ToString() : "0";
                string dabirNatCode = !string.IsNullOrWhiteSpace(dabirUser?.NationalCode) ? dabirUser.NationalCode.Trim() : "";

                var dabirObj = new
                {
                    firstName = dabirUser?.Firstname ?? "",
                    lastName = dabirUser?.LastName ?? "",
                    najmId = dabirNajmId,
                    bbbId = dabirBbbId,
                    nationalCode = dabirNatCode
                };

                // ListHozour: send all usertblmeeting except teachers and dabir
                // If user najmid is null in table userTBL send 0 for field najmId, if nationalCode is null send empty, for bbbID send id of userTBLMeeting
                var teacherUserIds = new HashSet<long>();
                if (meet.IDTeacher.HasValue) teacherUserIds.Add(meet.IDTeacher.Value);
                if (meet.Teacher != null) teacherUserIds.Add(meet.Teacher.ID);
                if (primaryTeacherUser != null && primaryTeacherUser.ID > 0) teacherUserIds.Add(primaryTeacherUser.ID);

                var dabirUserIds = new HashSet<long>();
                if (dabirUser != null && dabirUser.ID > 0) dabirUserIds.Add(dabirUser.ID);

                var listHozourMeets = allUserMeets.Where(um =>
                    !um.IsRemoved &&
                    um.Type != MainEntity_Types.USER_TEACHER &&
                    !(um.Role != null && (um.Role.Equals("Teacher", StringComparison.OrdinalIgnoreCase) || um.Role.Contains("استاد"))) &&
                    !(um.UserTBL != null && teacherUserIds.Contains(um.UserTBL.ID)) &&
                    um.Type != MainEntity_Types.USER_DABIR &&
                    !(um.Role != null && (um.Role.Equals("Dabir", StringComparison.OrdinalIgnoreCase) || um.Role.Contains("دبیر"))) &&
                    !(um.UserTBL != null && dabirUserIds.Contains(um.UserTBL.ID))
                ).ToList();

                var listHozour = new List<object>();
                foreach (var um in listHozourMeets)
                {
                    var u = um.UserTBL;

                    string fn = u?.Firstname ?? "";
                    string ln = u?.LastName ?? "";
                    if (string.IsNullOrWhiteSpace(fn) && string.IsNullOrWhiteSpace(ln) && !string.IsNullOrWhiteSpace(um.UserNickName))
                    {
                        var parts = um.UserNickName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                        fn = parts.Length > 0 ? parts[0] : "";
                        ln = parts.Length > 1 ? parts[1] : "";
                    }

                    string userNajmId = !string.IsNullOrWhiteSpace(u?.NajmID) ? u.NajmID.Trim() : "0";
                    string natCode = !string.IsNullOrWhiteSpace(u?.NationalCode)
                        ? u.NationalCode.Trim()
                        : (!string.IsNullOrWhiteSpace(um.NationalCode) ? um.NationalCode.Trim() : "");
                    string bbbId = um.ID.ToString();

                    listHozour.Add(new
                    {
                        firstName = fn,
                        lastName = ln,
                        najmId = userNajmId,
                        bbbId = bbbId,
                        nationalCode = natCode
                    });
                }

                var payload = new
                {
                    groupid = groupId,
                    Title = title,
                    date = dateStr,
                    startTime = startTimeStr,
                    endTime = endTimeStr,
                    meetingID = meet.ID,
                    Dastor = dastor,
                    dabir = dabirObj,
                    presenter = presenterObj,
                    boss = bossObj,
                    listHozour = listHozour
                };

                string jsonPayload = JsonConvert.SerializeObject(payload);
                Statics.Log("SyncWithNajm", $"Sending to Najm /groups/addmeeting: {jsonPayload}");

                using var req = new HttpRequestMessage(HttpMethod.Post, "http://192.168.0.4:1940/api/groups/addmeeting");
                req.Headers.Add("X-Api-Token", token);
                req.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var resp = await httpClient.SendAsync(req);
                int statusCode = (int)resp.StatusCode;
                var respContent = await resp.Content.ReadAsStringAsync();

                Statics.Log("SyncWithNajm", $"Najm /groups/addmeeting response ({statusCode}): {respContent}");

                if (resp.IsSuccessStatusCode)
                {
                    try
                    {
                        using var respDoc = System.Text.Json.JsonDocument.Parse(respContent);
                        string? returnedNajmId = null;
                        if (respDoc.RootElement.TryGetProperty("id", out var idProp))
                        {
                            returnedNajmId = idProp.ToString();
                        }
                        else if (respDoc.RootElement.TryGetProperty("meeting", out var mProp) && mProp.TryGetProperty("ID", out var midProp))
                        {
                            returnedNajmId = midProp.ToString();
                        }

                        if (!string.IsNullOrEmpty(returnedNajmId))
                        {
                            meet.NajmID = returnedNajmId;
                            _db.MeetingTBLs.Update(meet);
                            await _db.SaveChangesAsync();
                            Statics.Log("SyncWithNajm", $"Updated meeting '{meet.MeetID}' NajmID to '{meet.NajmID}'");
                        }
                    }
                    catch (Exception ex)
                    {
                        Statics.Log("SyncWithNajm_Error", $"Error parsing Najm response or updating NajmID: {ex.Message}");
                    }
                }

                return statusCode;
            }
            catch (Exception ex)
            {
                Statics.Log("SyncWithNajm_Error", ex.ToString());
                return 500;
            }
        }

        #region Room Metadata Helpers
        public string? GetRoomMetadata(long roomId, string key)
        {
            var meta = _db.RoomMetaTBLs.FirstOrDefault(m => m.IDRoomTBL == roomId && m.Key == key && m.Status == MainEntity_Status.Enabled);
            return meta?.Value;
        }

        public string? GetRoomMetadata(string friendlyId, string key)
        {
            var room = _db.RoomTBLs.FirstOrDefault(r => r.FriendlyID == friendlyId);
            if (room == null) return null;
            return GetRoomMetadata(room.ID, key);
        }

        public async Task<bool> SetRoomMetadataAsync(long roomId, string key, string? value, MainEntity_Types dataType = MainEntity_Types.META_STRING)
        {
            var meta = await _db.RoomMetaTBLs.FirstOrDefaultAsync(m => m.IDRoomTBL == roomId && m.Key == key);
            if (meta == null)
            {
                _db.RoomMetaTBLs.Add(new RoomMetaTBL
                {
                    IDRoomTBL = roomId,
                    Key = key,
                    Value = value,
                    DataType = dataType,
                    Status = MainEntity_Status.Enabled,
                    DateInsert = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                meta.Value = value;
                meta.DataType = dataType;
                meta.UpdatedAt = DateTime.UtcNow;
                meta.Status = MainEntity_Status.Enabled;
                _db.RoomMetaTBLs.Update(meta);
            }
            await _db.SaveChangesAsync();
            return true;
        }
        #endregion

    }
}
