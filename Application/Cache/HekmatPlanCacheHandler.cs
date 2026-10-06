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
    // Handlers/HekmatShobeCacheInvalidationHandler.cs


    public class HekmatPlanCacheHandler :
        INotificationHandler<PlanUpdateEvent>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDBContext _db;
        private readonly ILogger<HekmatShobeCacheInvalidationHandler> _logger;
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1); // فقط یک ترد مجاز

        public HekmatPlanCacheHandler(IMemoryCache memoryCache, IDBContext db, ILogger<HekmatShobeCacheInvalidationHandler> logger)
        {
            _memoryCache = memoryCache;
            this._db = db;
            _logger = logger;
        }

        public static string CACHE_KEY = "";
        public static string CACHE_KEY_PUBLIC = "PlanRoomPublic";
        public static string CACHE_KEY_PUBLIC_MEETING = "LatestMeetings";
        public async Task DoCache(PlanUpdateEvent notification)
        {
            List<MiniPlanJSON> cachedData = null;
            var room = _db.RoomTBLs.Where(e => e.FriendlyID == notification.FriendlyID).First();

            var list = _db.RoomTBLPlanTBLs.Where(e => e.IDRoomTBL == room.ID
            && (e.Status == MainEntity_Status.Enabled || e.Status == MainEntity_Status.MEETING_STARTED_LATER)

            )
                .ToList();

            cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
            {
                Id = e.ID,
                StartAt = e.StartDate,
                Teacher = e.Teachers,
                Title = e.Title
            }).ToList();

            // Configure cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))  // Reset expiration if accessed
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))     // Absolute expiration time
                .SetPriority(CacheItemPriority.Normal)            // Cache priority
                .SetSize(1);                                      // Size for memory pressure management


            // Add data to cache
            _memoryCache.Set(CACHE_KEY, cachedData, cacheOptions);

        }
        public async Task DoCache()
        {
            List<MiniPlanJSON> cachedData = null;

            var list = _db.RoomTBLPlanTBLs.Where(e => e.StartDate.HasValue && e.StartDate.Value > DateTime.Now && e.RoomTBL.IsPublicRoom
            && (e.Status == MainEntity_Status.Enabled || e.Status == MainEntity_Status.MEETING_STARTED_LATER)

            ).Include(e => e.RoomTBL).ThenInclude(e => e.RoomTBLPlanTBLs)
                .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                .ToList();

            cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
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
                        Fullname = e.RoomTBL.OwnerTBL.GetName(),
                        Username = e.RoomTBL.OwnerTBL.Username
                    }
                }
            }).ToList();

            // Configure cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(48))  // Reset expiration if accessed
                .SetAbsoluteExpiration(TimeSpan.FromDays(15))     // Absolute expiration time
                .SetPriority(CacheItemPriority.High)            // Cache priority
                .SetSize(1);                                      // Size for memory pressure management


            // Add data to cache
            _memoryCache.Set(CACHE_KEY_PUBLIC, cachedData, cacheOptions);

        }
        public async Task DoCacheMeeting()
        {
            List<MiniPlanJSON> cachedData = null;

            var list = _db.RoomTBLPlanTBLs.Where(e => e.StartDate.HasValue && !string.IsNullOrEmpty(e.Title) && !string.IsNullOrEmpty(e.Teachers) &&
            e.StartDate.Value <= DateTime.Now && e.RoomTBL.IsPublicRoom
            && (e.Status == MainEntity_Status.Enabled || e.Status == MainEntity_Status.MEETING_STARTED_LATER)

            ).Include(e => e.RoomTBL).ThenInclude(e => e.RoomTBLPlanTBLs).Include(e=>e.RoomTBL).ThenInclude(e=>e.OwnerTBL)
                .ToList();

            cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
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
                        Fullname = e.RoomTBL.OwnerTBL.GetName(),
                        Username = e.RoomTBL.OwnerTBL.Username
                    }
                }
            }).ToList();

            // Configure cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(2))  // Reset expiration if accessed
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1))     // Absolute expiration time
                .SetPriority(CacheItemPriority.High)            // Cache priority
                .SetSize(1);                                      // Size for memory pressure management


            // Add data to cache
            _memoryCache.Set(CACHE_KEY_PUBLIC_MEETING, cachedData, cacheOptions);

        }

        public async Task Handle(PlanUpdateEvent notification, CancellationToken cancellationToken)
        {
            string cacheKey = @$"HekmatShobeList{notification.FriendlyID}";
            CACHE_KEY = cacheKey;
            _memoryCache.Remove(CACHE_KEY);
            _memoryCache.Remove(CACHE_KEY_PUBLIC);
            _memoryCache.Remove(CACHE_KEY_PUBLIC_MEETING);
            _logger.LogInformation($"Cache '{CACHE_KEY}' invalidated due to User {notification.Action}: UserId={notification.FriendlyID}");
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                await DoCache(notification);
                await DoCache();
                await DoCacheMeeting();
            }
            finally
            {
                _cacheLock.Release();
            }

        }
    }
}
