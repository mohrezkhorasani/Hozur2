using Application.DTO;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CoreMe.VM
{
  
    public class AdminCreateRoomVM
    {
        [DefaultValue("گروه تستی")]
        public string Title { get; set; }
        [DefaultValue(21360)]
        public long UserID { get; set; }
        [DefaultValue(false)]
        public bool IsPublic { get; set; } = false;
        [DefaultValue(false)]
        public bool IsRequiredLogin { get; set; } = false;
        [DefaultValue(false)]
        public bool IsRequiredAccept { get; set; } = false;
        [DefaultValue(false)]
        public bool IsRecordable { get; set; } = false;

    }
    public class AdminChangeRoleVM
    {
        [DefaultValue(MainEntity_Types.USER_GUEST)]
        public MainEntity_Types Role { get; set; }
        [DefaultValue(21360)]
        public long UserId { get; set; }

    }
    public class InsertSlideShowVM
    {
        public int Position { get; set; } //0 = disable
        public string? Title { get; set; } //0 = disable

        public string? Href { get; set; } //0 = disable
        public IFormFile? File { get; set; } //0 = disable
    }
    public class EditSlideShowVM
    {
        public long IDSlideshow { get; set; }
        public int Position { get; set; } //0 = disable
        public string? Href { get; set; } //0 = disable
    }
    public class ChangeAvatarOfRoomVM
    {
        public string RoomID { get; set; }
        public string AvatarPath { get; set; }
    }
    public class GetPollReportVM
    {
        public string MeetID { get; set; }
        public long? QID { get; set; }  
        public string? UserID { get; set; } 
    }
    public class AddPollQuizVM
    {
        public string Question { get; set; }
        public MainEntity_Types Type { get; set; } = MainEntity_Types.QUIZ_OPTIONAL;
        public List<AnsPollRequestJSON> AnsPollRequestJSONs { get; set; }
    }







    #region logical
    public class AddToRoomsVM
    {
        [Required(ErrorMessage = "At least one ID is required")]
        [MinLength(1, ErrorMessage = "At least one ID is required")]
        [DefaultValue(" [\r\n    \"qbg-ihe-ncc-e4q\",\"r75-spv-fwr-g2t\"\r\n  ]")]
        public List<string> FriendlyIDs { get; set; } = new(); // Initialize to avoid null

        [Required(ErrorMessage = "Type is required")]
        [DefaultValue(MainEntity_Types.ROOMTOUSER_ADMIN)]
        [AllowedValues(
            MainEntity_Types.ROOMTOUSER_ADMIN,
            MainEntity_Types.ROOMTOUSER_MEMBER,
            MainEntity_Types.ROOMTOUSER_SIGNEDUP
        )]
        public MainEntity_Types Type { get; set; } // Use string for validation
    }



    public class EditMemberVM
    {
        public long UserMeetID { get; set; }
        public bool IsMember { get; set; }
        public string? AttendanceType { get; set; } // "virtual" or "hozuri"
        public bool? IsHozuri { get; set; }
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public string? NationalCode { get; set; }
    }

    public class AddHozuriMemberVM
    {
        public string Firstname { get; set; } = "";
        public string LastName { get; set; } = "";
        public string NationalCode { get; set; } = "";
    }

    public class AdminGenerateCertificateVM
    {
        public long ID { get; set; }
        public string? NationalCode { get; set; }
        public string? Phone { get; set; }
        public bool? IsMale { get; set; } = true;
        public string? MeetID { get; set; }
        public string? Name { get; set; }
    }

    public class UpdateMeetingMediaVM
    {
        public long MeetingId { get; set; }
        public int? Type { get; set; }
        public string? VoiceUrl { get; set; }
        public int? VoiceType { get; set; }
        public string? VideoUrl { get; set; }
        public int? VideoType { get; set; }
        public string? ReportUrl { get; set; }
        public int? ReportType { get; set; }
        public string? Title { get; set; }
        public string? Explain { get; set; }
    }

    public class SaveMeetingMediaLogVM
    {
        public long MeetingId { get; set; }
        public string? MeetID { get; set; }
        public string Key { get; set; } = string.Empty; // "sound_link", "video_link", "report_link"
        public string? Value { get; set; }
        public int Type { get; set; } = 102; // 102 (MEETLOG_DOWNLOADABLE), 103 (MEETLOG_DOWNLOAD_FORBIDDEN), 1 (MEETLOG_NORMAL)
        public string? Explain { get; set; }
        public long? ID { get; set; }
    }
    #endregion
}