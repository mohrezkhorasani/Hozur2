using Application.DTO;
using Application.DTO.Generals;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.General
{
    public class CommonMethods
    {
        public IConfiguration _configuration { get; }
        public IDBContext _db { get; }
        public IDBContext _db1 { get; }
        private IHostingEnvironment _hosting;

        public CommonMethods(IConfiguration configuration, IDBContext db, IHostingEnvironment hosting)
        {
            _configuration = configuration;
            _db = db;
            _hosting = hosting;
        }

        public CommonMethods(IConfiguration configuration, IDBContext db, IDBContext db1, IHostingEnvironment hosting)
        {
            _configuration = configuration;
            _db = db;
            _db1 = db1;
            _hosting = hosting;
        }

        public async Task<(UserTBL,MeetingTBL)> Initialize<T>(T message)
        where T:NewUserRequest
        {
            UserTBL user = null;
            if (string.IsNullOrEmpty(message.GeneralHeaders.JWTUserToken))
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
                user = await GetUser(message.GeneralHeaders.JWTUserToken);
            }
            if (user.AddIP(message.GeneralHeaders.RequestedIP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            var meet = await GetMeeting(message.GeneralHeaders.JWTMeetingToken);
            return (user, meet);
        }

        public async Task<UserTBL> NewUser(UserTBL user)
        {
            if (await CheckUserExist(user)) return await GetUserWithoutID(user);
            user.AllowedGUID.Add(Guid.NewGuid());

            _db.UserTBLs.Add(user);
            _db.SaveChanges();
            return user;
        }

        public async Task<MeetingTBL> NewMeeting(MeetingTBL tbl)
        {
            if (await CheckMeetingExist(tbl)) return await GetMeetWithoutID(tbl.MeetID);
            tbl.JWT = Statics.GenerateJWTMeet(tbl);
            _db.MeetingTBLs.Add(tbl);
            _db.SaveChanges();
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
            return _db.MeetingTBLs.FirstOrDefault(e => e.MeetID == q);
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
            var claims = JwtHelper.GetAllClaims(jwt);
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
                (user.MeetID != null && x.MeetID == user.MeetID && user.UUID == x.UUID)
            );
        }

    }
}
