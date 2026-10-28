using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public class BBBRecordingItemDTO
    {
        public string RecordId { get; set; } = string.Empty;
        public string MeetingId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Published { get; set; }
        public string State { get; set; } = "published";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string FormattedDuration { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public string PlaybackFormat { get; set; } = "presentation";
        public string? PlaybackUrl { get; set; }
        public string? Thumbnail { get; set; }
        public int ParticipantsCount { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    public class MeetingRecordingSettingsDTO
    {
        public long MeetingId { get; set; }
        public string? RecordId { get; set; }
        /// <summary>
        /// "all" (همه), "members" (اعضا), "admins" (مدیران), "none" (هیچکس)
        /// </summary>
        public string ViewPermission { get; set; } = "all";
        /// <summary>
        /// true (قابل دانلود), false (فقط استریم)
        /// </summary>
        public bool IsDownloadable { get; set; } = false;
        public bool Published { get; set; } = true;
        public string? PlaybackUrl { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class MeetingRecordingsResponseDTO
    {
        public long MeetingId { get; set; }
        public string MeetId { get; set; } = string.Empty;
        public string? RoomFriendlyId { get; set; }
        public string? RoomTitle { get; set; }
        public string? BbbMeetId { get; set; }
        public bool HasRecordings { get; set; }
        public List<BBBRecordingItemDTO> Recordings { get; set; } = new List<BBBRecordingItemDTO>();
        public MeetingRecordingSettingsDTO Settings { get; set; } = new MeetingRecordingSettingsDTO();
    }

    public class UpdateRecordingSettingsRequestDTO
    {
        public long MeetingId { get; set; }
        public string? RecordId { get; set; }
        public string ViewPermission { get; set; } = "all"; // all, members, admins, none
        public bool IsDownloadable { get; set; } = false;
        public bool? Publish { get; set; }
    }

    public class PublishRecordingRequestDTO
    {
        public long MeetingId { get; set; }
        public string RecordId { get; set; } = string.Empty;
        public bool Publish { get; set; } = true;
    }

    public class DeleteRecordingRequestDTO
    {
        public long MeetingId { get; set; }
        public string RecordId { get; set; } = string.Empty;
    }
}
