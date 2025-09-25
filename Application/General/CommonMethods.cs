using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
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
        


    }
}
