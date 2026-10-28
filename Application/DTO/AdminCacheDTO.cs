using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public class CacheEntryItemDTO
    {
        public string Key { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string ValueJson { get; set; } = string.Empty;
        public long EstimatedSizeBytes { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public string MemoryAddress { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
        public bool CanReCache { get; set; }
    }

    public class CacheMemoryStatsDTO
    {
        public long TotalAllocatedMemoryBytes { get; set; }
        public string FormattedTotalMemory { get; set; } = string.Empty;
        public int TotalCacheEntriesCount { get; set; }
        public long TotalEstimatedCacheSizeBytes { get; set; }
        public string FormattedTotalCacheSize { get; set; } = string.Empty;
        public int GcGen0Collections { get; set; }
        public int GcGen1Collections { get; set; }
        public int GcGen2Collections { get; set; }
    }

    public class CacheManagementResultDTO
    {
        public CacheMemoryStatsDTO Stats { get; set; } = new CacheMemoryStatsDTO();
        public List<CacheEntryItemDTO> Entries { get; set; } = new List<CacheEntryItemDTO>();
    }

    public class ClearCacheRequestDTO
    {
        public List<string>? Keys { get; set; }
    }

    public class ReCacheRequestDTO
    {
        public List<string>? Keys { get; set; }
    }
}
