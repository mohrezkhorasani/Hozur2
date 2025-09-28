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


        public async Task NewUser(UserTBL user)
        {
            if (user.ID != 0) return;

        }

        public async Task<bool> CheckUserExist(UserTBL user)
        {
            return await _db.UserTBLs.AnyAsync(x =>
                x.ID == user.ID ||
                (x.UUID == user.UUID || (x.UUID == null && user.UUID == null)) ||
                (x.Email == user.Email || (x.Email == null && user.Email == null)) ||
                (x.PhoneNumber == user.PhoneNumber || (x.PhoneNumber == null && user.PhoneNumber == null)) ||
                (x.NationalCode == user.NationalCode || (x.NationalCode == null && user.NationalCode == null))
                );

        }

    }
}
