using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDBContext
    {
        #region 12/07/2025
        public DbSet<AccessLevelTBL> AccessLevelTBLs { get; set; }
        public DbSet<AccessLevelTBLToRoleTBL> AccessLevelTBLToRoleTBLs { get; set; }
        #endregion
        #region 12/07/2025  
        public DbSet<MeetingReservationTBL> MeetingReservationTBLs { get; set; }
        public DbSet<RoleTBL> RoleTBLs { get; set; }
        public DbSet<UserTBLToRoleTBL> UserTBLToRoleTBLs { get; set; }
        public DbSet<ConfigTBL> ConfigTBLs { get; set; }
        public DbSet<CertificateTBL> CertificateTBLs { get; set; }
        public DbSet<MeetingTBL> MeetingTBLs { get; set; }
        public DbSet<RoomTBL> RoomTBLs { get; set; }
        public DbSet<UserTBL> UserTBLs { get; set; }
        public DbSet<MeetCommentTBL> MeetCommentTBLs { get; set; }
        public DbSet<MeetQuestionTBL> MeetQuestionTBLs { get; set; }
        public DbSet<RoomTBLToUserTBL> RoomTBLToUserTBLs { get; set; }
        public DbSet<UserTBLToMeetingTBL> UserTBLToMeetingTBLs { get; set; }
        public DbSet<UserTBLToMeetingTBLLogs> UserTBLToMeetingTBLLogs { get; set; }
        public DbSet<UserRevokeTokenTBL> UserRevokeTokenTBL { get; set; }
        public DbSet<ContactUsTBL> ContactUsTBL { get; set; }
        public DbSet<UserTBLToUserTBL> UserTBLToUserTBLs { get; set; }
        public DbSet<RoomTBLPlanTBL> RoomTBLPlanTBLs { get; set; }
        public DbSet<SlideshowTBL> SlideshowTBL { get; set; }
        public DbSet<MeetPollQuizAnsTBL> MeetPollQuizAnsTBLs { get; set; }
        public DbSet<MeetPollQuizAnsTBLToUserMeetTBL> MeetPollQuizAnsTBLToUserMeetTBLs { get; set; }
        public DbSet<MeetPollQuizTBL> MeetPollQuizTBLs { get; set; }
        #endregion
        public int SaveChanges();
        public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        public ValueTask<object> FindAsync(Type entityType, params object[] keyValues);
        public ValueTask<object> FindAsync(Type entityType, object[] keyValues, CancellationToken cancellationToken);
        public EntityEntry Remove(object entity);

        public void RemoveRange(IEnumerable<object> entities);

        public void RemoveRange(params object[] entities);

        public void UpdateRange(IEnumerable<object> entities);
        public void UpdateRange(params object[] entities);
        public EntityEntry Update(object entity);
        public EntityEntry Entry(object entity);
        public void Dispose();
        public ValueTask DisposeAsync();
        public DatabaseFacade Database { get; }
    }
}
