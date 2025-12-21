using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace NajmBridge.vm
{
    public class MeetVM
    {

    }

    public class StartMeetingViewModel
    {
        [DefaultValue("jtb-a8e-ydl-05b")]
        [FromRoute]
        public string IDRoom { get; set; }
        public bool RequiredToEnterName { get; set; } = false;
        public string? MeetTitle { get; set; }
        public List<string> Teachers { get; set; } = new List<string> { "استاد تستی", "استاد تستی 2" };

    }
}
