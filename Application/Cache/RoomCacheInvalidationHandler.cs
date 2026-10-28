using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application.DTO.Models;
using Application.General;
using Application.Interfaces;
using CacheHandler.Events;
using Domain.Entity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Cache.Handlers
{
    public class RoomCacheInvalidationHandler : INotificationHandler<RoomUpdatedEvent>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDBContext _db;
        private readonly ILogger<RoomCacheInvalidationHandler> _logger;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public RoomCacheInvalidationHandler(IMemoryCache memoryCache, IDBContext db, ILogger<RoomCacheInvalidationHandler> logger)
        {
            _memoryCache = memoryCache;
            _db = db;
            _logger = logger;
        }

        public async Task Handle(RoomUpdatedEvent notification, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                var room = await _db.RoomTBLs.Include(r => r.OwnerTBL).FirstOrDefaultAsync(r => r.ID == notification.RoomId, cancellationToken);
                if (room == null) return;

                _logger.LogInformation($"[RoomCacheInvalidationHandler] Room {notification.Action} detected for RoomId={room.ID}, Owner={room.IDOwnerTBL}. Invalidating group & room caches...");

                // 1. Remove all keys starting with GetAllRooms_, GetRoomsPublic_, GetRooms_User_
                var allKeys = GetAllKeysFromMemoryCache();
                foreach (var k in allKeys)
                {
                    string keyStr = k?.ToString() ?? "";
                    if (keyStr.StartsWith("GetAllRooms_") ||
                        keyStr.StartsWith("GetRoomsPublic_") ||
                        keyStr.StartsWith($"GetRooms_User_{room.IDOwnerTBL}_") ||
                        keyStr == "PlanRoomPublic")
                    {
                        _memoryCache.Remove(k);
                        _logger.LogInformation($"[RoomCacheInvalidationHandler] Cleared cache key '{keyStr}'");
                    }
                }

                // 2. If user is Hekmat Shobe, invalidate and rebuild HekmatShobeList
                if (room.OwnerTBL != null && room.OwnerTBL.IsHekmatShobe)
                {
                    _memoryCache.Remove("HekmatShobeList");
                    await RebuildHekmatShobeCache();
                }

                _logger.LogInformation($"[RoomCacheInvalidationHandler] Room/Group cache invalidation completed successfully for RoomId={room.ID}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[RoomCacheInvalidationHandler] Error invalidating room caches for RoomId={notification.RoomId}");
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task RebuildHekmatShobeCache()
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

        private List<object> GetAllKeysFromMemoryCache()
        {
            var keys = new List<object>();
            if (_memoryCache == null) return keys;

            try
            {
                var dictionaries = new List<IDictionary>();
                var coherentStateField = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (coherentStateField != null)
                {
                    var coherentState = coherentStateField.GetValue(_memoryCache);
                    if (coherentState != null)
                    {
                        var csType = coherentState.GetType();
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
                        keys.Add(de.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read MemoryCache keys");
            }

            return keys;
        }
    }
}
