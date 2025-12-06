using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IUsecase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SafaretoWeb.ApplicationUsage;

namespace BBB_Dashboard.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IGetTrophiesUseCase _getTrophiesUseCase;
        private readonly IGetRoomStatsUseCase _getRoomStatsUseCase;
        private readonly IGetLivesUseCase _getLivesUseCase;

        public IndexModel(
            IGetTrophiesUseCase getTrophiesUseCase,
            IGetRoomStatsUseCase getRoomStatsUseCase,
            IGetLivesUseCase getLivesUseCase)
        {
            _getTrophiesUseCase = getTrophiesUseCase;
            _getRoomStatsUseCase = getRoomStatsUseCase;
            _getLivesUseCase = getLivesUseCase;
            BG = new() { "1", "2", "3", "4" };
        }

        public GetTrophiesResponse? GetTrophiesResponse { get; set; }
        public GetRoomStatsResponse? GetRoomStatsResponse { get; set; }
        public GetLivesResponse? GetLivesResponse { get; set; }
        public List<SharedOBJ_RoomStats> VisibleRooms { get; set; } = new();
        public List<string> BG { get; }
        public string MoreRoomTitleButton { get; set; } = "نمایش بیشتر";
        public string ChartConfigJson { get; set; } = "{}";

        public async Task OnGetAsync()
        {
            await LoadAllDataAsync();
        }

        public async Task<IActionResult> OnPostLoadMoreAsync()
        {
            await LoadAllDataAsync();

            if (VisibleRooms.Count >= GetRoomStatsResponse!.Rooms.Count)
            {
                VisibleRooms = GetRoomStatsResponse.Rooms.Take(8).ToList();
                MoreRoomTitleButton = "نمایش بیشتر";
            }
            else if (VisibleRooms.Count + 8 >= GetRoomStatsResponse.Rooms.Count)
            {
                VisibleRooms = GetRoomStatsResponse.Rooms;
                MoreRoomTitleButton = "نمایش کمتر";
            }
            else
            {
                VisibleRooms = GetRoomStatsResponse.Rooms.Take(VisibleRooms.Count + 8).ToList();
                MoreRoomTitleButton = "نمایش بیشتر";
            }

            return Page();
        }

        private async Task LoadAllDataAsync()
        {
            // GetTrophies
            var trophiesPresenter = new GenericPresenter<GenericResponse<GetTrophiesResponse>>();
            await _getTrophiesUseCase.handleAsync(new GetTrophiesRequest(), trophiesPresenter);
            GetTrophiesResponse = trophiesPresenter.Response.data;

            // GetRoomStats
            var roomPresenter = new GenericPresenter<GenericResponse<GetRoomStatsResponse>>();
            await _getRoomStatsUseCase.handleAsync(new GetRoomStatsRequest(), roomPresenter);
            GetRoomStatsResponse = roomPresenter.Response.data;
            VisibleRooms = GetRoomStatsResponse.Rooms.Take(8).ToList();

            // GetLives
            var livesPresenter = new GenericPresenter<GenericResponse<GetLivesResponse>>();
            await _getLivesUseCase.handleAsync(new GetLivesRequest(), livesPresenter);
            GetLivesResponse = livesPresenter.Response.data;

            // Chart Data
            GenerateChartConfig();
        }

        private void GenerateChartConfig()
        {
            var data = new[]
            {
                new { Label = "فروردین", Value = 120 },
                new { Label = "اردیبهشت", Value = 190 },
                new { Label = "خرداد", Value = 150 },
                new { Label = "تیر", Value = 280 },
                new { Label = "مرداد", Value = 220 },
                new { Label = "شهریور", Value = 310 }
            };

            var config = new
            {
                type = "bar",
                data = new
                {
                    labels = data.Select(x => x.Label).ToArray(),
                    datasets = new[] { new
                    {
                        label = "تعداد شرکت‌کنندگان",
                        data = data.Select(x => x.Value).ToArray(),
                        backgroundColor = "rgba(54, 162, 235, 0.7)",
                        borderColor = "rgba(54, 162, 235, 1)",
                        borderWidth = 1,
                        borderRadius = 8
                    }}
                },
                options = new { responsive = true, plugins = new { legend = new { display = false } } }
            };

            ChartConfigJson = JsonConvert.SerializeObject(config);
        }
    }
}