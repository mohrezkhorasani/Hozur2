using Application.DTO;
using Application.Interface.IUsecase;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using NajmBridge.Cofigurations;
using NajmBridge.Presenter;
using NajmBridge.vm;

namespace NajmBridge.Controllers
{
    [RequireApiToken]
    [ApiController]
    [Route("[controller]")]
    public class MeetController : ControllerBase
    {
        private readonly IMapper _mapper;
        public IGetRoomsUseCase _IGetRoomsUseCase { get; }
        public ApiPresenter<GetRoomsResponse> _GetRoomsResponse { get; }
        public IGetMeetingFromRoomIDUseCase _IGetMeetingFromRoomIDUseCase { get; }
        public ApiPresenter<GetMeetingFromRoomIDResponse> _GetMeetingFromRoomIDResponse { get; }
        public IGetMeetInfoUseCase _IGetMeetInfoUseCase { get; }
        public ApiPresenter<GetMeetInfoResponse> _GetMeetInfoResponse { get; }

       public IGetMembersOfMeetUseCase _IGetMembersOfMeetUseCase { get; }
        public ApiPresenter<GetMembersOfMeetResponse> _GetMembersOfMeetResponse { get; }
       public IGetMembersOfMeetOnlyCertificatedUseCase _IGetMembersOfMeetOnlyCertificatedUseCase { get; }
        public ApiPresenter<GetMembersOfMeetOnlyCertificatedResponse> _GetMembersOfMeetOnlyCertificatedResponse { get; }
       public IGenerateCertificateWithUserMeetIDUseCase _IGenerateCertificateWithUserMeetIDUseCase { get; }
        public ApiPresenter<GenerateCertificateWithUserMeetIDResponse> _GenerateCertificateWithUserMeetIDResponse { get; }
       public ISearchUserUseCase _ISearchUserUseCase { get; }
        public ApiPresenter<SearchUserResponse> _SearchUserResponse { get; }
       public IGetExcellMembersUseCase _IGetExcellMembersUseCase { get; }
        public ApiPresenter<GetExcellMembersResponse> _GetExcellMembersResponse { get; }
       public IStartNewMeetUseCase _IStartNewMeetUseCase { get; }
        public ApiPresenter<StartNewMeetResponse> _StartNewMeetResponse { get; }

         public IGetMemberByIDUseCase _IGetMemberByIDUseCase { get; }
        public ApiPresenter<GetMemberByIDResponse> _GetMemberByIDResponse { get; }

        public MeetController(IMapper mapper, IGetRoomsUseCase iGetRoomsUseCase, ApiPresenter<GetRoomsResponse> getRoomsResponse, IGetMeetingFromRoomIDUseCase iGetMeetingFromRoomIDUseCase, ApiPresenter<GetMeetingFromRoomIDResponse> getMeetingFromRoomIDResponse, IGetMeetInfoUseCase iGetMeetInfoUseCase, ApiPresenter<GetMeetInfoResponse> getMeetInfoResponse, IGetMembersOfMeetUseCase iGetMembersOfMeetUseCase, ApiPresenter<GetMembersOfMeetResponse> getMembersOfMeetResponse, IGetMembersOfMeetOnlyCertificatedUseCase iGetMembersOfMeetOnlyCertificatedUseCase, ApiPresenter<GetMembersOfMeetOnlyCertificatedResponse> getMembersOfMeetOnlyCertificatedResponse, IGenerateCertificateWithUserMeetIDUseCase iGenerateCertificateWithUserMeetIDUseCase, ApiPresenter<GenerateCertificateWithUserMeetIDResponse> generateCertificateWithUserMeetIDResponse, ISearchUserUseCase iSearchUserUseCase, ApiPresenter<SearchUserResponse> searchUserResponse, IGetExcellMembersUseCase iGetExcellMembersUseCase, ApiPresenter<GetExcellMembersResponse> getExcellMembersResponse, IStartNewMeetUseCase iStartNewMeetUseCase, ApiPresenter<StartNewMeetResponse> startNewMeetResponse, IGetMemberByIDUseCase iGetMemberByIDUseCase, ApiPresenter<GetMemberByIDResponse> getMemberByIDResponse)
        {
            _mapper = mapper;
            _IGetRoomsUseCase = iGetRoomsUseCase;
            _GetRoomsResponse = getRoomsResponse;
            _IGetMeetingFromRoomIDUseCase = iGetMeetingFromRoomIDUseCase;
            _GetMeetingFromRoomIDResponse = getMeetingFromRoomIDResponse;
            _IGetMeetInfoUseCase = iGetMeetInfoUseCase;
            _GetMeetInfoResponse = getMeetInfoResponse;
            _IGetMembersOfMeetUseCase = iGetMembersOfMeetUseCase;
            _GetMembersOfMeetResponse = getMembersOfMeetResponse;
            _IGetMembersOfMeetOnlyCertificatedUseCase = iGetMembersOfMeetOnlyCertificatedUseCase;
            _GetMembersOfMeetOnlyCertificatedResponse = getMembersOfMeetOnlyCertificatedResponse;
            _IGenerateCertificateWithUserMeetIDUseCase = iGenerateCertificateWithUserMeetIDUseCase;
            _GenerateCertificateWithUserMeetIDResponse = generateCertificateWithUserMeetIDResponse;
            _ISearchUserUseCase = iSearchUserUseCase;
            _SearchUserResponse = searchUserResponse;
            _IGetExcellMembersUseCase = iGetExcellMembersUseCase;
            _GetExcellMembersResponse = getExcellMembersResponse;
            _IStartNewMeetUseCase = iStartNewMeetUseCase;
            _StartNewMeetResponse = startNewMeetResponse;
            _IGetMemberByIDUseCase = iGetMemberByIDUseCase;
            _GetMemberByIDResponse = getMemberByIDResponse;
        }


        /// <summary>
        /// دریافت گروه ها
        /// </summary>
        /// <param name="q">کوئری برای جستجوی گروه در صورت نیاز</param>
        /// <returns>
        /// خروجی یک لیستی از گروه هاست
        /// </returns>
        [HttpGet("/GetRooms")]
        public async Task<IActionResult> GetRooms([FromQuery] string? q="")
        {
            if (string.IsNullOrEmpty(q))
                q = "";
            GetRoomsRequest req = new GetRoomsRequest
            {
                Query = q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetRoomsUseCase.handleAsync(req, _GetRoomsResponse);
            return _GetRoomsResponse.jsonContentResult;
        }
        /// <summary>
        /// دریافت لیست جلسات
        /// </summary>
        /// <param name="roomid">شناسه گروه از ای پی آی بالا</param>
        /// <param name="countPerPage">تعداد بازگشتی به ازای هر صفحه</param>
        /// <param name="page">شماره صفحه (شروع از صفر )</param>
        /// <returns></returns>
        [HttpGet("{roomid}/GetMeetings/{page}")]
        public async Task<IActionResult> GetMeetings([FromRoute(Name ="roomid")] string roomid= "r75-spv-fwr-g2t", [FromQuery] int countPerPage=20, [FromRoute(Name = "page")] int page=0) 
        {
            GetMeetingFromRoomIDRequest req = new GetMeetingFromRoomIDRequest
            {
                ID = roomid,
                Count=countPerPage,
                Page= page,
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetMeetingFromRoomIDUseCase.handleAsync(req, _GetMeetingFromRoomIDResponse);
            return _GetMeetingFromRoomIDResponse.jsonContentResult;
        }
        /// <summary>
        /// دریافت اطلاعات یک جلسه
        /// </summary>
        /// <param name="meetid">شناسه جلسه (2 نوع شناسه داریم یکی عددیست یکی رشته است، فرق ندارد کدام ارسال شود</param>
        /// <returns></returns>
        [HttpGet("{meetid}/GetMeetingsInfo")]
        public async Task<IActionResult> GetMeetingsInfo([FromRoute] string meetid= "e19fd1c396774ff96556be2302421e58291acea0-1764603132359")
        {
            GetMeetInfoRequest req = new GetMeetInfoRequest
            {
                ID = meetid
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetMeetInfoUseCase.handleAsync(req, _GetMeetInfoResponse);
            return _GetMeetInfoResponse.jsonContentResult;
        }

        /// <summary>
        /// دریافت لیست شرکت کنندگان در یک جلسه
        /// </summary>
        /// <param name="meetid">شناسه جلسه</param>
        /// <returns></returns>
        [HttpGet("{meetid}/GetMeetingsInfo/Members")]
        public async Task<IActionResult> GetMeetingsInfoMembers([FromRoute] string meetid= "e19fd1c396774ff96556be2302421e58291acea0-1764603132359")
        {
            GetMembersOfMeetRequest req = new GetMembersOfMeetRequest
            {
                ID = meetid
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetMembersOfMeetUseCase.handleAsync(req, _GetMembersOfMeetResponse);
            return _GetMembersOfMeetResponse.jsonContentResult;
        }
        /// <summary>
        /// دریافت لیست شرکت کنندگانی که گواهینامه دریافت کرده اند
        /// </summary>
        /// <param name="meetid">شناسه جلسه</param>
        /// <returns></returns>
        [HttpGet("{meetid}/GetMeetingsInfo/Members/Certificate/GetOnlyCertificateds")]
        public async Task<IActionResult> GetMeetingsInfoMembersGetOnlyCertificated([FromRoute] string meetid= "e19fd1c396774ff96556be2302421e58291acea0-1764603132359")
        {
            GetMembersOfMeetOnlyCertificatedRequest req = new GetMembersOfMeetOnlyCertificatedRequest
            {
                ID = meetid
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetMembersOfMeetOnlyCertificatedUseCase.handleAsync(req, _GetMembersOfMeetOnlyCertificatedResponse);
            return _GetMembersOfMeetOnlyCertificatedResponse.jsonContentResult;
        }
        /// <summary>
        /// تولید گواهینامه برای یک فرد در جلسه
        /// </summary>
        /// <param name="userid">شناسه فرد در جلسه</param>
        /// <param name="nationalcode">کد ملی</param>
        /// <param name="isMale">جنسیت: در صورت مقدار True مرد در نظر گرفته میشود</param>
        /// <returns></returns>
        [HttpGet("/Certificate/GenerateCertificate")]
        public async Task<IActionResult> GetMeetingsInfoMembersGenerateCertificate([FromQuery] long userid= 18649883,  [FromQuery] string nationalcode= "0000001104", [FromQuery] bool isMale=true)
        {
            GenerateCertificateWithUserMeetIDRequest req = new GenerateCertificateWithUserMeetIDRequest
            {
                ID = userid,
                NationalCode=nationalcode,
                IsMale= isMale,
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGenerateCertificateWithUserMeetIDUseCase.handleAsync(req, _GenerateCertificateWithUserMeetIDResponse);
            return _GenerateCertificateWithUserMeetIDResponse.jsonContentResult;
        }
        /// <summary>
        /// جستجوی یک شرکت کننده در یک جلسه
        /// </summary>
        /// <param name="meetid">شناسه جلسه</param>
        /// <param name="q">کوئری جستجو که میتواند نام کاربر باشد</param>
        /// <param name="day">روز تاریخ جلسه که میتواند نال باشد</param>
        /// <param name="year">سال تاریخ جلسه هم میتوان نال باشد</param>
        /// <param name="month">ماه تاریخ جلسه به صورت رشته مثل آبان، خرداد، مهر و.. که میتواند نال باشد</param>
        /// <returns></returns>
        [HttpGet("{meetid}/GetMeetingsInfo/Members/Certificate/SearchUser")]
        public async Task<IActionResult> GetMeetingsInfoMembersSearch([FromRoute] string meetid, [FromQuery] string? q, [FromQuery] int? day=10 ,[FromQuery] int? year =1404,[FromQuery]string? month="آذر")
        {
            SearchUserRequest req = new SearchUserRequest
            {
                Query = string.IsNullOrEmpty(q)?"":q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _ISearchUserUseCase.handleAsync(req, _SearchUserResponse);
            return _SearchUserResponse.jsonContentResult;
        }
        /// <summary>
        /// جستجوی یک شرکت کننده به صورت کلی. اینجا کوئری متفاوت با بالاست
        /// </summary>
        /// <param name="q">کوئری جستجو که در اینجا میتواند نام کاربر، نام جلسه، عنوان جلسه، عنوان گروه و یا نقش کاربر باشد</param>
        /// <param name="day">روز تاریخ جلسه که میتواند نال باشد</param>
        /// <param name="year">سال تاریخ جلسه هم میتوان نال باشد</param>
        /// <param name="month">ماه تاریخ جلسه به صورت رشته مثل آبان، خرداد، مهر و.. که میتواند نال باشد</param>
        /// <returns></returns>
        [HttpGet("/Members/Certificate/SearchUser")]
        public async Task<IActionResult> GetMembersSearch([FromQuery] string? q, [FromQuery] int? day=10, [FromQuery] int? year = 1404, [FromQuery] string? month = "آذر")
        {
            SearchUserRequest req = new SearchUserRequest
            {
                Query = string.IsNullOrEmpty(q) ? "" : q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _ISearchUserUseCase.handleAsync(req, _SearchUserResponse);
            return _SearchUserResponse.jsonContentResult;
        }


        /// <summary>
        /// خروجی اکسل لیست شرکت کنندگان در یک جلسه
        /// </summary>
        /// <param name="meetid">شناسه جلسه</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("{meetid}/GetMeetingsInfo/Members/ExportExcell")]
        public async Task<IActionResult> GetMeetingsInfoMembersExportExcell([FromRoute] string meetid)
        {
            GetExcellMembersRequest req = new GetExcellMembersRequest
            {
                MeetID=meetid
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetExcellMembersUseCase.handleAsync(req, _GetExcellMembersResponse);
            return _GetExcellMembersResponse.jsonContentResult;
        }

        /// <summary>
        /// خروجی اکسل لیست شرکت کنندگان در یک جلسه
        /// </summary>
        /// <param name="meetid">شناسه جلسه</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("{IDRoom}/Meet")]
        public async Task<IActionResult> StartNewMeeting(StartMeetingViewModel vm)
        {
            StartNewMeetRequest req = _mapper.Map<StartNewMeetRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IStartNewMeetUseCase.handleAsync(req, _StartNewMeetResponse);
            return _StartNewMeetResponse.jsonContentResult;
        }



    }
}
