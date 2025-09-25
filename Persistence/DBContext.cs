using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore.Design;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading;
using Domain.Entity;
using System.ComponentModel.Design;
using Microsoft.Extensions.Options;

namespace Presistence.Entities
{
    public class DBContext : DbContext, IDBContext
    {
        public DbSet<ConfigTBL> ConfigTBLs { get; set; }//454

        public class BloggingContextfactory : IDesignTimeDbContextFactory<DBContext>
        {
            public DBContext CreateDbContext(string[] args)
            {
                var optioonbuilder = new DbContextOptionsBuilder<DBContext>();

                optioonbuilder.UseSqlServer(@"Data Source=201.414.111.22;Initial Catalog=dbname;User ID=user;Password=pass;TrustServerCertificate=True", builder =>
                {
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });



                return new DBContext(optioonbuilder.Options);
            }
        }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            
            #region 8/6/2023
            modelBuilder.ApplyConfiguration(new ConfigTBLConfiguration());
            #endregion
        }



    }
}
