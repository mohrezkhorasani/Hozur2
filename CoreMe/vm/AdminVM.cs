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
}