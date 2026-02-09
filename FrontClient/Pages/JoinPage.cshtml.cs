using Application.DTO;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presenter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application.Interface.IInfrastructure;
using Presistence.Entities;
using Domain.Entity;

namespace FrontClient.Pages
{

    public class JoinPageModel : PageModel
    {
        private readonly DBContext _db;
        private readonly IBBBGetMeetings _bbb;

        public JoinPageModel(DBContext db, IBBBGetMeetings bbb)
        {
            _db = db;
            _bbb = bbb;
        }

        [BindProperty(SupportsGet = true)]
        public string Mid { get; set; }

        [BindProperty]
        public string DisplayName { get; set; }

        [BindProperty]
        public string AdminPass { get; set; }

        public string MeetName { get; set; }
        public RoomTBL Room { get; set; }
        public bool Loading { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrWhiteSpace(Mid))
            {
                TempData["Error"] = "شناسه جلسه معتبر نیست";
                return RedirectToPage("/Index");
            }

            Room = _db.RoomTBLs.FirstOrDefault(r => r.FriendlyID == Mid);
            if (Room == null)
            {
                TempData["Error"] = "جلسه پیدا نشد";
                return RedirectToPage("/Index");
            }

            MeetName = Room.Title;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(DisplayName))
            { 
                ModelState.AddModelError(string.Empty, "نام نمایشی الزامی است");
                Room = _db.RoomTBLs.FirstOrDefault(r => r.FriendlyID == Mid);
                MeetName = Room?.Title ?? "جلسه بدون نام";
                return Page();
            }

            Loading = true;

            Room = _db.RoomTBLs.FirstOrDefault(r => r.FriendlyID == Mid);

            if (Room == null)
            {
                TempData["Error"] = "جلسه پیدا نشد";
                return RedirectToPage("/Index");
            }

            try
            {
                bool isAdmin = !string.IsNullOrWhiteSpace(AdminPass);
                string joinUrl = isAdmin
                    ? await _bbb.GetJoinUrlAsync(Room.BBB_MeetID, DisplayName, true, AdminPass)
                    : await _bbb.GetJoinUrlAsync(Room.BBB_MeetID, DisplayName, false, Statics.BBB_ATTEND_PW);

                return Redirect(joinUrl); // ریدایرکت به join
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "خطا در اتصال به جلسه");
                return Page();
            }
            finally
            {
                Loading = false;
            }
        }
    }

}
