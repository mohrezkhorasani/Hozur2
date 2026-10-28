using System;
using System.Collections.Generic;

namespace Application.DTO
{
    public class AdminParticipantItemDTO
    {
        public long Id { get; set; }
        public string Uuid { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Firstname { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Username { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string NationalCode { get; set; } = "";
        public string AvatarPath { get; set; } = "";
        public int Type { get; set; }
        public string RoleTitle { get; set; } = "";
        public int AttendedMeetingsCount { get; set; }
        public bool CanIssueCertificate { get; set; }
    }

    public class AdminParticipantsResultDTO
    {
        public List<AdminParticipantItemDTO> Items { get; set; } = new List<AdminParticipantItemDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ParticipantAttendedMeetingDTO
    {
        public long MeetingId { get; set; }
        public string MeetId { get; set; } = "";
        public string Title { get; set; } = "";
        public string RoomFriendlyId { get; set; } = "";
        public string RoomTitle { get; set; } = "";
        public string JalaliDate { get; set; } = "";
        public DateTime DateInsert { get; set; }
        public long UserMeetId { get; set; }
        public bool IsCertificated { get; set; }
        public string? CertificateFileUrl { get; set; }
        public long TimeSpendSeconds { get; set; }
    }
}
