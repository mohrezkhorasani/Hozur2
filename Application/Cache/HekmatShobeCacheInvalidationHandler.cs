using Application.DTO.Models;
using Application.General;
using Application.Interfaces;
using CacheHandler.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Cache.Handlers
{
    // Handlers/HekmatShobeCacheInvalidationHandler.cs


    public class HekmatShobeCacheInvalidationHandler :
        INotificationHandler<UserUpdatedEvent>,
        INotificationHandler<RoomUpdatedEvent>,
        INotificationHandler<MeetingUpdatedEvent>
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDBContext _db;
        private readonly ILogger<HekmatShobeCacheInvalidationHandler> _logger;
        private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1); // فقط یک ترد مجاز

        public HekmatShobeCacheInvalidationHandler(IMemoryCache memoryCache, IDBContext db, ILogger<HekmatShobeCacheInvalidationHandler> logger)
        {
            _memoryCache = memoryCache;
            this._db = db;
            _logger = logger;
        }

        private const string CACHE_KEY = "HekmatShobeList";

        // Handle User updates
        public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var user = await _db.UserTBLs.FindAsync(notification.UserId);
            if (!user.IsHekmatShobe)
                return;
            _memoryCache.Remove(CACHE_KEY);
            _logger.LogInformation($"Cache '{CACHE_KEY}' invalidated due to User {notification.Action}: UserId={notification.UserId}");
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                await DoCache();
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        // Handle Room updates
        public async Task Handle(RoomUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var room = await _db.RoomTBLs.FindAsync(notification.RoomId);
            var user = await _db.UserTBLs.FindAsync(room.IDOwnerTBL);
            if (!user.IsHekmatShobe)
                return;
            _memoryCache.Remove(CACHE_KEY);
            _logger.LogInformation($"Cache '{CACHE_KEY}' invalidated due to Room {notification.Action}: RoomId={notification.RoomId}");
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                await DoCache();
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        // Handle Meeting updates
        public async Task Handle(MeetingUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var meeting = await _db.MeetingTBLs.FindAsync(notification.MeetingId);
            var room = await _db.RoomTBLs.Where(e => e.FriendlyID == meeting.RoomFriendlyID).FirstOrDefaultAsync();
            var user = await _db.UserTBLs.FindAsync(room.IDOwnerTBL);
            if (!user.IsHekmatShobe)
                return;
            _memoryCache.Remove(CACHE_KEY);
            _logger.LogInformation($"Cache '{CACHE_KEY}' invalidated due to Meeting {notification.Action}: MeetingId={notification.MeetingId}");
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                await DoCache();
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public async Task DoCache()
        {
            List<MiniShobeJSON> cachedData = null;

            var list = _db.UserTBLs.Where(e => e.IsHekmatShobe)
              .Include(e => e.RoomTBLs)
              .ThenInclude(e => e.MeetingTBLs)
              .ThenInclude(e => e.UserTBLToMeetingTBLs)
              .ToList();

            cachedData = list.Select(e => new MiniShobeJSON
            {
                Avatar = Statics.CombinePath(Statics.URLFiles, e.AvatarPath),
                Fullname = e.GetName(),
                Room = e.RoomTBLs.Select(r => new MiniRoomJSON
                {
                    Title = r.Title,
                    Count = r.MeetingTBLs.Count(d => d.UserTBLToMeetingTBLs.Count > 1),
                    RoomID = r.FriendlyID,
                    CountPartis = r.MeetingTBLs.Sum(f => f.UserTBLToMeetingTBLs.Count),
                    Avatar = Statics.CombinePath(Statics.URLFiles, r.Avatar)
                }).ToList(),
                ID = e.ID
            }).ToList();

            // Configure cache options
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))  // Reset expiration if accessed
                .SetAbsoluteExpiration(TimeSpan.FromDays(1))     // Absolute expiration time
                .SetPriority(CacheItemPriority.High)            // Cache priority
                .SetSize(20);                                      // Size for memory pressure management

            // Add data to cache
            _memoryCache.Set(CACHE_KEY, cachedData, cacheOptions);
        }
    }
}
