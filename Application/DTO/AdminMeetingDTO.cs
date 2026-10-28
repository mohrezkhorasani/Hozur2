using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public class AdminMeetingOwnerDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Avatar { get; set; }
    }

    public class AdminMeetingTeacherDTO
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    public class AdminMeetingStatsDTO
    {
        public int TotalMeetings { get; set; }
        public int WithVoiceCount { get; set; }
        public int WithVideoCount { get; set; }
        public int WithReportCount { get; set; }
    }

    public class AdminMeetingItemDTO
    {
        public long Id { get; set; }
        public string MeetID { get; set; } = string.Empty;
        public string? Title { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string StatusTitle { get; set; } = string.Empty;
        public DateTime DateInsert { get; set; }
        public DateTime? EndDate { get; set; }
        public string? JalaliDate { get; set; }
        public bool IsRecorded { get; set; }
        public string? Explain { get; set; }
        public string? RoomFriendlyID { get; set; }
        public string? RoomTitle { get; set; }
        public AdminMeetingOwnerDTO? Owner { get; set; }
        public AdminMeetingTeacherDTO? Teacher { get; set; }
        public int ParticipantsCount { get; set; }

        public string? VoiceUrl { get; set; }
        public int VoiceType { get; set; }
        public long? VoiceLogId { get; set; }
        public string? VoiceExplain { get; set; }

        public string? VideoUrl { get; set; }
        public int VideoType { get; set; }
        public long? VideoLogId { get; set; }
        public string? VideoExplain { get; set; }

        public string? ReportUrl { get; set; }
        public int ReportType { get; set; }
        public long? ReportLogId { get; set; }
        public string? ReportExplain { get; set; }

        public string? BbbMeetID { get; set; }
        public string? RecordingPlaybackUrl { get; set; }
        public string? RecordingRecordId { get; set; }
        public string? RecordingViewPermission { get; set; }
        public bool? RecordingDownloadable { get; set; }
        public bool? RecordingPublished { get; set; }
    }

    public class AdminMeetingsResultDTO
    {
        public List<AdminMeetingItemDTO> Items { get; set; } = new List<AdminMeetingItemDTO>();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int Page { get; set; }
        public int Count { get; set; }
        public AdminMeetingStatsDTO Stats { get; set; } = new AdminMeetingStatsDTO();
    }
}
