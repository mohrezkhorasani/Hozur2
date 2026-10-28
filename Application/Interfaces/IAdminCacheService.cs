using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTO;
using Domain.Entity;

namespace Application.Interfaces
{
    public interface IAdminCacheService
    {
        Task<CacheManagementResultDTO> GetCacheDetailsAsync(UserTBL user);
        Task<bool> ClearKeyAsync(string key, UserTBL user);
        Task<int> ClearKeysAsync(List<string> keys, UserTBL user);
        Task<int> ClearAllAsync(UserTBL user);
        Task<bool> ReCacheKeyAsync(string key, UserTBL user);
        Task<int> ReCacheKeysAsync(List<string> keys, UserTBL user);
        Task<int> ReCacheAllAsync(UserTBL user);
    }
}
