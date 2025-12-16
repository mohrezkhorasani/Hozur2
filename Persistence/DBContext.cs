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
        #region 12/07/2025
        public DbSet<AccessLevelTBL> AccessLevelTBLs { get; set; }
        public DbSet<AccessLevelTBLToRoleTBL> AccessLevelTBLToRoleTBLs { get; set; }
        public DbSet<MeetCommentTBL> MeetCommentTBLs { get; set; }
        public DbSet<MeetQuestionTBL> MeetQuestionTBLs { get; set; }

        #endregion
        #region 12/07/2025  
        public DbSet<RoleTBL> RoleTBLs { get; set; }
        public DbSet<UserTBLToRoleTBL> UserTBLToRoleTBLs { get; set; }
        public DbSet<ConfigTBL> ConfigTBLs { get; set; }
        public DbSet<CertificateTBL> CertificateTBLs { get; set; }
        public DbSet<MeetingTBL> MeetingTBLs { get; set; }
        public DbSet<RoomTBL> RoomTBLs { get; set; }
        public DbSet<UserTBL> UserTBLs { get; set; }
        public DbSet<RoomTBLToUserTBL> RoomTBLToUserTBLs { get; set; }
        public DbSet<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public DbSet<UserTBLToMeetingTBLLogs> UserTBLToMeetingTBLLogs { get; set; }
        #endregion

        public class BloggingContextfactory : IDesignTimeDbContextFactory<DBContext>
        {
            
            public DBContext CreateDbContext(string[] args)
            {
                var optioonbuilder = new DbContextOptionsBuilder<DBContext>();

                optioonbuilder.UseSqlServer(@"Data Source=192.168.0.158;Initial Catalog=eitherir_maj;User ID=hekmatbbbremote;Password=n098*0EWj#JWjK@&;TrustServerCertificate=True", builder =>
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

            #region 12/07/2025
            modelBuilder.ApplyConfiguration(new RoleTBLConfiguration());
            modelBuilder.ApplyConfiguration(new UserTBLToRoleTBLConfiguration());

            modelBuilder.ApplyConfiguration(new AccessLevelTBLConfiguration());
            modelBuilder.ApplyConfiguration(new AccessLevelTBLToRoleTBLConfiguration());
            #endregion
            #region 8/6/2023
            modelBuilder.ApplyConfiguration(new CertificateTBLConfiguration());
            modelBuilder.ApplyConfiguration(new MeetQuestionTBLConfiguration());
            modelBuilder.ApplyConfiguration(new MeetCommentTBLConfiguration());
            modelBuilder.ApplyConfiguration(new MeetingTBLConfiguration());
            modelBuilder.ApplyConfiguration(new RoomTBLConfiguration());
            modelBuilder.ApplyConfiguration(new UserTBLConfiguration());
            modelBuilder.ApplyConfiguration(new UserTBLToMeetingTBLConfiguration());
            modelBuilder.ApplyConfiguration(new ConfigTBLConfiguration());
            modelBuilder.ApplyConfiguration(new UserTBLToMeetingTBLLogsConfiguration());
            modelBuilder.ApplyConfiguration(new RoomTBLToUserTBLConfiguration());
            #endregion
        }



    }
}
