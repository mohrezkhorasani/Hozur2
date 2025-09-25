using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDBContext
    {

        public DbSet<ConfigTBL> ConfigTBLs { get; set; }
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

    }
}
