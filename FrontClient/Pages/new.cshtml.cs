using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application.General;
using Application.Interface.IInfrastructure;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presistence.Entities;
using System.Text.Json;

namespace FrontClient.Pages
{

    public class RoomsModel : PageModel
    {
        private readonly DBContext _db;
        private readonly IBBBGetMeetings _bbb;

        public RoomsModel(DBContext db, IBBBGetMeetings bbb)
        {
            _db = db;
            _bbb = bbb;
        }

        public List<RoomTBL> Rooms { get; set; }
        private HashSet<long> LoadingStart { get; set; } = new HashSet<long>();
        private HashSet<long> CopiedRooms { get; set; } = new HashSet<long>();

        public async Task OnGetAsync()
        {
            Rooms = _db.RoomTBLs.Where(e => e.Type == MainEntity_Types.ROOM_NORMAL).ToList();
        }


        public class RoomRequest
        {
            public long RoomId { get; set; }
        }

        public async Task<IActionResult> OnPostStartRoom(long roomId)
        {
            var room = _db.RoomTBLs.FirstOrDefault(r => r.ID == roomId);
            if (room == null) return NotFound();

            if (string.IsNullOrEmpty(room.BBB_MeetID))
                room.BBB_MeetID = "hekmateislamic";

            var domain = "http://vc2.hekmateislami.com";

            // ساخت جلسه
            var t = await _bbb.CreateMeetingAsync(room.BBB_MeetID, room.Title, "r75-spv-fwr-g2t");

            // گرفتن join URL
            var join = await _bbb.GetJoinUrlAsync(room.BBB_MeetID, "مجمع عالی حکمت اسلامی", true, Statics.BBB_MODERATOR_PW);

            // redirect به join
            return Redirect(join);
        }

        public bool IsLoading(long roomId) => LoadingStart.Contains(roomId);
        public bool IsCopied(long roomId) => CopiedRooms.Contains(roomId);
    }

}
