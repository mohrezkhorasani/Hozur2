using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using Application.DTO;
using Application.DTO.Models;
using Application.General;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AdminCacheService : IAdminCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDBContext _db;
        private readonly ILogger<AdminCacheService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public AdminCacheService(IMemoryCache memoryCache, IDBContext db, ILogger<AdminCacheService> logger)
        {
            _memoryCache = memoryCache;
            _db = db;
            _logger = logger;
        }

        private void EnsureSuperAdmin(UserTBL user)
        {
            if (user == null)
                throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

            bool isSuperAdmin = user.Type == MainEntity_Types.USER_ADMINSTRATOR ||
                               user.VerifyUserPermissions("manage_admins");

            if (!isSuperAdmin)
                throw new UnauthorizedAccessException("دسترسی به مدیریت کش فقط برای مدیر کل امکان‌پذیر است.");
        }

        public Task<CacheManagementResultDTO> GetCacheDetailsAsync(UserTBL user)
        {
            EnsureSuperAdmin(user);

            var result = new CacheManagementResultDTO();
            var rawEntries = GetAllEntriesFromMemoryCache();

            long totalEstimatedCacheSizeBytes = 0;

            foreach (var item in rawEntries)
            {
                var entryDto = new CacheEntryItemDTO
                {
                    Key = item.Key?.ToString() ?? "",
                    Value = item.Value,
                    TypeName = item.Value?.GetType().Name ?? "null",
                    AbsoluteExpiration = item.AbsoluteExpiration,
                    SlidingExpiration = item.SlidingExpiration,
                    Priority = item.Priority.ToString(),
                    CanReCache = IsRegisteredKeyForReCache(item.Key?.ToString() ?? "")
                };

                // Serialize preview
                try
                {
                    if (item.Value != null)
                    {
                        var json = JsonSerializer.Serialize(item.Value, _jsonOptions);
                        entryDto.ValueJson = json;
                        entryDto.EstimatedSizeBytes = System.Text.Encoding.UTF8.GetByteCount(json);
                    }
                    else
                    {
                        entryDto.ValueJson = "null";
                        entryDto.EstimatedSizeBytes = 0;
                    }
                }
                catch (Exception ex)
                {
                    entryDto.ValueJson = $"{{\"error\": \"Serialization failed: {ex.Message}\"}}";
                    entryDto.EstimatedSizeBytes = 128;
                }

                entryDto.FormattedSize = FormatBytes(entryDto.EstimatedSizeBytes);
                entryDto.MemoryAddress = GetMemoryAddress(item.Value);
                totalEstimatedCacheSizeBytes += entryDto.EstimatedSizeBytes;

                result.Entries.Add(entryDto);
            }

            // Stats
            long totalProcessMemory = GC.GetTotalMemory(false);
            result.Stats = new CacheMemoryStatsDTO
            {
                TotalAllocatedMemoryBytes = totalProcessMemory,
                FormattedTotalMemory = FormatBytes(totalProcessMemory),
                TotalCacheEntriesCount = result.Entries.Count,
                TotalEstimatedCacheSizeBytes = totalEstimatedCacheSizeBytes,
                FormattedTotalCacheSize = FormatBytes(totalEstimatedCacheSizeBytes),
                GcGen0Collections = GC.CollectionCount(0),
                GcGen1Collections = GC.CollectionCount(1),
                GcGen2Collections = GC.CollectionCount(2)
            };

            return Task.FromResult(result);
        }

        public Task<bool> ClearKeyAsync(string key, UserTBL user)
        {
            EnsureSuperAdmin(user);
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult(false);

            _memoryCache.Remove(key);
            _logger.LogInformation($"[CacheManagement] Cache key '{key}' cleared by user {user.Username}");
            return Task.FromResult(true);
        }

        public Task<int> ClearKeysAsync(List<string> keys, UserTBL user)
        {
            EnsureSuperAdmin(user);
            if (keys == null || keys.Count == 0) return Task.FromResult(0);

            int count = 0;
            foreach (var key in keys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _memoryCache.Remove(key);
                    count++;
                }
            }

            _logger.LogInformation($"[CacheManagement] {count} cache keys cleared by user {user.Username}");
            return Task.FromResult(count);
        }

        public Task<int> ClearAllAsync(UserTBL user)
        {
            EnsureSuperAdmin(user);

            var keys = GetAllKeysFromMemoryCache();
            int count = 0;
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
                count++;
            }

            _logger.LogInformation($"[CacheManagement] All {count} cache keys cleared by user {user.Username}");
            return Task.FromResult(count);
        }

        public async Task<bool> ReCacheKeyAsync(string key, UserTBL user)
        {
            EnsureSuperAdmin(user);
            if (string.IsNullOrWhiteSpace(key)) return false;

            return await ExecuteReCacheForKey(key);
        }

        public async Task<int> ReCacheKeysAsync(List<string> keys, UserTBL user)
        {
            EnsureSuperAdmin(user);
            if (keys == null || keys.Count == 0) return 0;

            int count = 0;
            foreach (var key in keys)
            {
                if (await ExecuteReCacheForKey(key))
                {
                    count++;
                }
            }
            return count;
        }

        public async Task<int> ReCacheAllAsync(UserTBL user)
        {
            EnsureSuperAdmin(user);

            var standardKeys = new List<string>
            {
                "PlanRoomPublic",
                "LatestMeetings",
                "HekmatShobeList",
                "GetAllRooms_null_null",
                "GetRoomsPublic_null_null"
            };

            // Also include any currently cached keys that support recache
            var currentKeys = GetAllKeysFromMemoryCache().Select(k => k.ToString() ?? "");
            foreach (var k in currentKeys)
            {
                if (IsRegisteredKeyForReCache(k) && !standardKeys.Contains(k))
                {
                    standardKeys.Add(k);
                }
            }

            int count = 0;
            foreach (var key in standardKeys)
            {
                if (await ExecuteReCacheForKey(key))
                {
                    count++;
                }
            }

            _logger.LogInformation($"[CacheManagement] {count} caches re-warmed by user {user.Username}");
            return count;
        }

        private async Task<bool> ExecuteReCacheForKey(string key)
        {
            try
            {
                if (key == "PlanRoomPublic")
                {
                    await ReCachePlanRoomPublic();
                    return true;
                }
                if (key == "LatestMeetings")
                {
                    await ReCacheLatestMeetings();
                    return true;
                }
                if (key == "HekmatShobeList")
                {
                    await ReCacheHekmatShobeList();
                    return true;
                }
                if (key.StartsWith("HekmatShobeList") && key.Length > "HekmatShobeList".Length)
                {
                    string friendlyId = key.Substring("HekmatShobeList".Length);
                    await ReCacheRoomPlan(friendlyId);
                    return true;
                }
                if (key.StartsWith("GetAllRooms_"))
                {
                    await ReCacheGetAllRooms(key);
                    return true;
                }
                if (key.StartsWith("GetRoomsPublic_"))
                {
                    await ReCacheGetRoomsPublic(key);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to re-cache key: {key}");
            }
            return false;
        }

        private bool IsRegisteredKeyForReCache(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return key == "PlanRoomPublic" ||
                   key == "LatestMeetings" ||
                   key == "HekmatShobeList" ||
                   key.StartsWith("HekmatShobeList") ||
                   key.StartsWith("GetAllRooms_") ||
                   key.StartsWith("GetRoomsPublic_");
        }

        private async Task ReCachePlanRoomPublic()
        {
            var list = await _db.RoomTBLPlanTBLs
                .Where(e => e.StartDate.HasValue && e.StartDate.Value > DateTime.Now && e.RoomTBL.IsPublicRoom
                    && (e.Status == MainEntity_Status.Enabled || e.Status == MainEntity_Status.MEETING_STARTED_LATER))
                .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                .ToListAsync();

            var cachedData = list.OrderBy(e => e.StartDate).Select(e => new MiniPlanJSON
            {
                Id = e.ID,
                StartAt = e.StartDate,
                Teacher = e.Teachers,
                Title = e.Title,
                RoomJSON = new MiniRoomJSON
                {
                    RoomID = e.RoomTBL.FriendlyID,
                    IsSubscribed = e.RoomTBL.IsSubscribed,
                    Title = e.RoomTBL.Title,
                    Owner = new MiniUserJSON
                    {
                        Fullname = e.RoomTBL.OwnerTBL?.GetName() ?? "",
                        Username = e.RoomTBL.OwnerTBL?.Username ?? ""
                    }
                }
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal);

            _memoryCache.Set("PlanRoomPublic", cachedData, cacheOptions);
        }

        private async Task ReCacheLatestMeetings()
        {
            var list = await _db.RoomTBLPlanTBLs
                .Where(e => e.StartDate.HasValue && !string.IsNullOrEmpty(e.Title) && !string.IsNullOrEmpty(e.Teachers)
                    && e.StartDate.Value <= DateTime.Now && e.RoomTBL.IsPublicRoom)
                .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                .ToListAsync();

            var cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
            {
                Id = e.ID,
                StartAt = e.StartDate,
                Teacher = e.Teachers,
                Title = e.Title,
                RoomJSON = new MiniRoomJSON
                {
                    RoomID = e.RoomTBL.FriendlyID,
                    IsSubscribed = e.RoomTBL.IsSubscribed,
                    Title = e.RoomTBL.Title,
                    Avatar = e.RoomTBL.Avatar,
                    Owner = new MiniUserJSON
                    {
                        Fullname = e.RoomTBL.OwnerTBL?.GetName() ?? "",
                        Username = e.RoomTBL.OwnerTBL?.Username ?? "",
                    }
                }
            }).Take(15).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(2))
                .SetAbsoluteExpiration(TimeSpan.FromDays(1))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set("LatestMeetings", cachedData, cacheOptions);
        }

        private async Task ReCacheHekmatShobeList()
        {
            var users = await _db.UserTBLs
                .Where(e => e.IsHekmatShobe)
                .Select(e => new { e.ID, e.AvatarPath, e.Firstname, e.LastName })
                .ToListAsync();

            var userIds = users.Select(u => u.ID).ToList();
            var rooms = await _db.RoomTBLs
                .Where(r => userIds.Contains(r.IDOwnerTBL))
                .Select(r => new
                {
                    r.IDOwnerTBL,
                    r.Title,
                    r.FriendlyID,
                    r.IsSubscribed,
                    r.Avatar,
                    MeetingCount = r.MeetingTBLs.Count(),
                    ParticipantCount = r.MeetingTBLs.Sum(m => m.UserTBLToMeetingTBLs.Count)
                })
                .ToListAsync();

            var cachedData = users.Select(u => new MiniShobeJSON
            {
                Avatar = Statics.CombinePath(Statics.URLFiles, u.AvatarPath),
                Fullname = $"{u.Firstname} {u.LastName}".Trim(),
                ID = u.ID,
                Room = rooms.Where(r => r.IDOwnerTBL == u.ID)
                    .Select(r => new MiniRoomJSON
                    {
                        Title = r.Title,
                        RoomID = r.FriendlyID,
                        IsSubscribed = r.IsSubscribed,
                        Avatar = Statics.CombinePath(Statics.URLFiles, r.Avatar),
                        Count = r.MeetingCount,
                        CountPartis = r.ParticipantCount
                    }).ToList()
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2))
                .SetPriority(CacheItemPriority.Normal);

            _memoryCache.Set("HekmatShobeList", cachedData, cacheOptions);
        }

        private async Task ReCacheRoomPlan(string friendlyId)
        {
            var room = await _db.RoomTBLs.FirstOrDefaultAsync(e => e.FriendlyID == friendlyId);
            if (room == null) return;

            var list = await _db.RoomTBLPlanTBLs
                .Where(e => e.IDRoomTBL == room.ID && (e.Status == MainEntity_Status.Enabled || e.Status == MainEntity_Status.MEETING_STARTED_LATER))
                .Include(e => e.RoomTBL)
                .ToListAsync();

            var cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
            {
                Id = e.ID,
                StartAt = e.StartDate,
                Teacher = e.Teachers,
                Dabir = e.Dabir,
                Title = e.Title,
                RoomJSON = new MiniRoomJSON { RoomID = e.RoomTBL.FriendlyID, IsSubscribed = e.RoomTBL.IsSubscribed }
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetPriority(CacheItemPriority.Normal);

            _memoryCache.Set($"HekmatShobeList{friendlyId}", cachedData, cacheOptions);
        }

        private async Task ReCacheGetAllRooms(string cacheKey)
        {
            var rooms = await _db.RoomTBLs
                .Include(e => e.OwnerTBL)
                .Include(e => e.RoomTBLPlanTBLs)
                .Include(e => e.MeetingTBLs)
                .ToListAsync();

            var result = rooms.Select(room => new MiniRoomJSON
            {
                RoomID = room.FriendlyID,
                IsSubscribed = room.IsSubscribed,
                Title = room.Title,
                Created = room.DateInsert,
                Avatar = room.Avatar,
                LastMeet = room.MeetingTBLs.OrderByDescending(m => m.DateInsert).FirstOrDefault()?.DateInsert ?? room.DateInsert,
                Owner = new MiniUserJSON
                {
                    UID = room.OwnerTBL?.UUID,
                    Fullname = room.OwnerTBL?.GetName() ?? "",
                    Username = room.OwnerTBL?.Username ?? ""
                }
            }).ToList();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(1))
                .SetAbsoluteExpiration(TimeSpan.FromDays(2))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(cacheKey, result, cacheEntryOptions);
        }

        private async Task ReCacheGetRoomsPublic(string cacheKey)
        {
            var rooms = await _db.RoomTBLs
                .Where(e => e.IsPublicRoom)
                .Include(e => e.OwnerTBL)
                .Include(e => e.RoomTBLPlanTBLs)
                .Include(e => e.MeetingTBLs)
                .ToListAsync();

            var result = rooms.Select(room => new MiniRoomJSON
            {
                RoomID = room.FriendlyID,
                IsSubscribed = room.IsSubscribed,
                Title = room.Title,
                Created = room.DateInsert,
                Avatar = room.Avatar,
                LastMeet = room.MeetingTBLs.OrderByDescending(m => m.DateInsert).FirstOrDefault()?.DateInsert ?? room.DateInsert,
                Owner = new MiniUserJSON
                {
                    UID = room.OwnerTBL?.UUID,
                    Fullname = room.OwnerTBL?.GetName() ?? "",
                    Username = room.OwnerTBL?.Username ?? ""
                }
            }).ToList();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(1))
                .SetAbsoluteExpiration(TimeSpan.FromDays(2))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(cacheKey, result, cacheEntryOptions);
        }

        #region Reflection Helpers for MemoryCache
        private class RawCacheEntry
        {
            public object Key { get; set; } = string.Empty;
            public object? Value { get; set; }
            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
        }

        private List<RawCacheEntry> GetAllEntriesFromMemoryCache()
        {
            var list = new List<RawCacheEntry>();
            if (_memoryCache == null) return list;

            try
            {
                var dictionaries = new List<IDictionary>();

                // .NET 7/8/9/10: _coherentState
                var coherentStateField = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (coherentStateField != null)
                {
                    var coherentState = coherentStateField.GetValue(_memoryCache);
                    if (coherentState != null)
                    {
                        var csType = coherentState.GetType();

                        // In .NET 9 & 10: split into _stringEntries and _nonStringEntries
                        var stringEntriesField = csType.GetField("_stringEntries", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (stringEntriesField?.GetValue(coherentState) is IDictionary sDict)
                        {
                            dictionaries.Add(sDict);
                        }

                        var nonStringEntriesField = csType.GetField("_nonStringEntries", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (nonStringEntriesField?.GetValue(coherentState) is IDictionary nsDict)
                        {
                            dictionaries.Add(nsDict);
                        }

                        // In .NET 7 & 8: _entries
                        var entriesField = csType.GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (entriesField?.GetValue(coherentState) is IDictionary eDict && !dictionaries.Contains(eDict))
                        {
                            dictionaries.Add(eDict);
                        }
                    }
                }

                if (dictionaries.Count == 0)
                {
                    var directEntriesField = typeof(MemoryCache).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (directEntriesField?.GetValue(_memoryCache) is IDictionary directDict)
                    {
                        dictionaries.Add(directDict);
                    }
                }

                foreach (var dict in dictionaries)
                {
                    foreach (DictionaryEntry de in dict)
                    {
                        var key = de.Key;
                        var rawCacheEntry = de.Value;
                        if (rawCacheEntry == null) continue;

                        var item = new RawCacheEntry { Key = key };

                        // CacheEntry properties: Value, AbsoluteExpiration, SlidingExpiration, Priority
                        var entryType = rawCacheEntry.GetType();
                        var valProp = entryType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                        item.Value = valProp?.GetValue(rawCacheEntry);

                        var absProp = entryType.GetProperty("AbsoluteExpiration", BindingFlags.Public | BindingFlags.Instance)
                                      ?? entryType.GetProperty("AbsoluteExpirationRelativeToNow", BindingFlags.Public | BindingFlags.Instance);
                        item.AbsoluteExpiration = absProp?.GetValue(rawCacheEntry) as DateTimeOffset?;

                        var slidingProp = entryType.GetProperty("SlidingExpiration", BindingFlags.Public | BindingFlags.Instance);
                        item.SlidingExpiration = slidingProp?.GetValue(rawCacheEntry) as TimeSpan?;

                        var prioProp = entryType.GetProperty("Priority", BindingFlags.Public | BindingFlags.Instance);
                        if (prioProp?.GetValue(rawCacheEntry) is CacheItemPriority prio)
                        {
                            item.Priority = prio;
                        }

                        list.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to inspect MemoryCache entries via reflection");
            }

            return list;
        }

        private List<object> GetAllKeysFromMemoryCache()
        {
            return GetAllEntriesFromMemoryCache().Select(e => e.Key).ToList();
        }

        private static string GetMemoryAddress(object? obj)
        {
            if (obj == null) return "0x00000000";
            try
            {
                GCHandle handle = GCHandle.Alloc(obj, GCHandleType.WeakTrackResurrection);
                IntPtr ptr = GCHandle.ToIntPtr(handle);
                handle.Free();
                return $"0x{ptr.ToString("X16")}";
            }
            catch
            {
                return $"0x{RuntimeHelpers.GetHashCode(obj):X8}";
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{(bytes / (1024.0 * 1024.0)):F2} MB";
            return $"{(bytes / (1024.0 * 1024.0 * 1024.0)):F2} GB";
        }
        #endregion
    }
}
