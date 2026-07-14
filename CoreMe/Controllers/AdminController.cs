using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IUsecase;
using Application.Usecase;
using AutoMapper;
using CoreMe.Presenter;
using CoreMe.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Presenter;
using System.Runtime.InteropServices.Marshalling;

namespace CoreMe.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
    [EnableCors("AllowHekmateSubdomains")]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<TaskController> _logger;
        private readonly IWebHostEnvironment _hosting;
        public IAdminGetUserListUseCase _IAdminGetUserListUseCase { get; }
        public ApiPresenter<AdminGetUserListResponse> _AdminGetUserListResponse { get; }
        public IAdminGetUserRoomListUseCase _IAdminGetUserRoomListUseCase { get; }
        public ApiPresenter<AdminGetUserRoomListResponse> _AdminGetUserRoomListResponse { get; }
        public IAdminCreateRoomUseCase _IAdminCreateRoomUseCase { get; }
        public ApiPresenter<AdminCreateRoomResponse> _AdminCreateRoomResponse { get; }

        public IAdminChangeRoleUseCase _IAdminChangeRoleUseCase { get; }
        public ApiPresenter<AdminChangeRoleResponse> _AdminChangeRoleResponse { get; }
      public IChangeAvatarOfRoomUseCase _IChangeAvatarOfRoomUseCase { get; }
        public ApiPresenter<ChangeAvatarOfRoomResponse> _ChangeAvatarOfRoomResponse { get; }
        public IInsertSlideShowUseCase _IInsertSlideShowUseCase { get; }
        public ApiPresenter<InsertSlideShowResponse> _InsertSlideShowResponse { get; }
        public IGetAllLivesUseCase _IGetAllLivesUseCase { get; }
        public ApiPresenter<GetAllLivesResponse> _GetAllLivesResponse { get; }
        public IEditSlideShowUseCase _IEditSlideShowUseCase { get; }
        public ApiPresenter<EditSlideShowResponse> _EditSlideShowResponse { get; }
        public IGetAllCertificateOfMemberUseCase _IGetAllCertificateOfMemberUseCase { get; }
        public ApiPresenter<GetAllCertificateOfMemberResponse> _GetAllCertificateOfMemberResponse { get; }
        public IGetMembersOfMeetOnlyCertificatedUseCase _IGetMembersOfMeetOnlyCertificatedUseCase { get; }
        public ApiPresenter<GetMembersOfMeetOnlyCertificatedResponse> _GetMembersOfMeetOnlyCertificatedResponse { get; }
        public IGenerateCertificateWithUserMeetIDUseCase _IGenerateCertificateWithUserMeetIDUseCase { get; }
        public ApiPresenter<GenerateCertificateWithUserMeetIDResponse> _GenerateCertificateWithUserMeetIDResponse { get; }
       public IGetPollListAdminUseCase _IGetPollListAdminUseCase { get; }
        public ApiPresenter<GetPollListAdminResponse> _GetPollListAdminResponse { get; }
        public IAddPollQuizUseCase _IAddPollQuizUseCase { get; }
        public ApiPresenter<AddPollQuizResponse> _AddPollQuizResponse { get; }
        public IGetPollReportUseCase _IGetPollReportUseCase { get; }
        public ApiPresenter<GetPollReportResponse> _GetPollReportResponse { get; }
        public IGetAllRoomsUseCase _IGetAllRoomsUseCase { get; }
        public ApiPresenter<GetAllRoomsResponse> _GetAllRoomsResponse { get; }
        public IGetAllRoomsMiniUseCase _IGetAllRoomsMiniUseCase { get; }
        public ApiPresenter<GetAllRoomsMiniResponse> _GetAllRoomsMiniResponse { get; }
        public IGetAllMeetingUseCase _IGetAllMeetingUseCase { get; }
        public ApiPresenter<GetAllMeetingResponse> _GetAllMeetingResponse { get; }

        
        public AdminController(IMapper mapper, ILogger<TaskController> logger, IWebHostEnvironment hosting, IAdminGetUserListUseCase
            iAdminGetUserListUseCase, ApiPresenter<AdminGetUserListResponse> adminGetUserListResponse, IAdminGetUserRoomListUseCase iAdminGetUserRoomListUseCase,
            ApiPresenter<AdminGetUserRoomListResponse> adminGetUserRoomListResponse, IAdminCreateRoomUseCase iAdminCreateRoomUseCase, ApiPresenter<AdminCreateRoomResponse>
            adminCreateRoomResponse, IAdminChangeRoleUseCase iAdminChangeRoleUseCase, ApiPresenter<AdminChangeRoleResponse> adminChangeRoleResponse, IChangeAvatarOfRoomUseCase 
            iChangeAvatarOfRoomUseCase, ApiPresenter<ChangeAvatarOfRoomResponse> changeAvatarOfRoomResponse, IInsertSlideShowUseCase iInsertSlideShowUseCase,
            ApiPresenter<InsertSlideShowResponse> insertSlideShowResponse, IGetAllLivesUseCase iGetAllLivesUseCase, ApiPresenter<GetAllLivesResponse> getAllLivesResponse,
            IEditSlideShowUseCase iEditSlideShowUseCase, ApiPresenter<EditSlideShowResponse> editSlideShowResponse, IGetAllCertificateOfMemberUseCase iGetAllCertificateOfMemberUseCase,
            ApiPresenter<GetAllCertificateOfMemberResponse> getAllCertificateOfMemberResponse, IGetMembersOfMeetOnlyCertificatedUseCase iGetMembersOfMeetOnlyCertificatedUseCase, 
            ApiPresenter<GetMembersOfMeetOnlyCertificatedResponse> getMembersOfMeetOnlyCertificatedResponse, IGenerateCertificateWithUserMeetIDUseCase iGenerateCertificateWithUserMeetIDUseCase,
            ApiPresenter<GenerateCertificateWithUserMeetIDResponse> generateCertificateWithUserMeetIDResponse, IGetPollListAdminUseCase iGetPollListAdminUseCase, ApiPresenter<GetPollListAdminResponse>
            getPollListAdminResponse, IAddPollQuizUseCase iAddPollQuizUseCase, ApiPresenter<AddPollQuizResponse> addPollQuizResponse, IGetPollReportUseCase iGetPollReportUseCase, 
            ApiPresenter<GetPollReportResponse> getPollReportResponse, IGetAllRoomsUseCase iGetAllRoomsUseCase, ApiPresenter<GetAllRoomsResponse> getAllRoomsResponse,
            IGetAllMeetingUseCase iGetAllMeetingUseCase, ApiPresenter<GetAllMeetingResponse> GetAllMeetingResponse,
            IGetAllRoomsMiniUseCase iGetAllRoomsMiniUseCase, ApiPresenter<GetAllRoomsMiniResponse> getAllRoomsMiniResponse)
        {
            _mapper = mapper;
            _logger = logger;
            _hosting = hosting;
            _IAdminGetUserListUseCase = iAdminGetUserListUseCase;
            _AdminGetUserListResponse = adminGetUserListResponse;
            _IAdminGetUserRoomListUseCase = iAdminGetUserRoomListUseCase;
            _AdminGetUserRoomListResponse = adminGetUserRoomListResponse;
            _IAdminCreateRoomUseCase = iAdminCreateRoomUseCase;
            _AdminCreateRoomResponse = adminCreateRoomResponse;
            _IAdminChangeRoleUseCase = iAdminChangeRoleUseCase;
            _AdminChangeRoleResponse = adminChangeRoleResponse;
            _IChangeAvatarOfRoomUseCase = iChangeAvatarOfRoomUseCase;
            _ChangeAvatarOfRoomResponse = changeAvatarOfRoomResponse;
            _IInsertSlideShowUseCase = iInsertSlideShowUseCase;
            _InsertSlideShowResponse = insertSlideShowResponse;
            _IGetAllLivesUseCase = iGetAllLivesUseCase;
            _GetAllLivesResponse = getAllLivesResponse;
            _IEditSlideShowUseCase = iEditSlideShowUseCase;
            _EditSlideShowResponse = editSlideShowResponse;
            _IGetAllCertificateOfMemberUseCase = iGetAllCertificateOfMemberUseCase;
            _GetAllCertificateOfMemberResponse = getAllCertificateOfMemberResponse;
            _IGetMembersOfMeetOnlyCertificatedUseCase = iGetMembersOfMeetOnlyCertificatedUseCase;
            _GetMembersOfMeetOnlyCertificatedResponse = getMembersOfMeetOnlyCertificatedResponse;
            _IGenerateCertificateWithUserMeetIDUseCase = iGenerateCertificateWithUserMeetIDUseCase;
            _GenerateCertificateWithUserMeetIDResponse = generateCertificateWithUserMeetIDResponse;
            _IGetPollListAdminUseCase = iGetPollListAdminUseCase;
            _GetPollListAdminResponse = getPollListAdminResponse;
            _IAddPollQuizUseCase = iAddPollQuizUseCase;
            _AddPollQuizResponse = addPollQuizResponse;
            _IGetPollReportUseCase = iGetPollReportUseCase;
            _GetPollReportResponse = getPollReportResponse;
            _IGetAllRoomsUseCase = iGetAllRoomsUseCase;
            _GetAllRoomsResponse = getAllRoomsResponse;
            _IGetAllRoomsMiniUseCase = iGetAllRoomsMiniUseCase;
            _GetAllRoomsMiniResponse = getAllRoomsMiniResponse;
            _IGetAllMeetingUseCase = iGetAllMeetingUseCase;
            _GetAllMeetingResponse = GetAllMeetingResponse;
        }

        [HttpGet("/user/AdminGetUserList")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminGetUserList(int page, int count, OrderBy o, string? q)
        {
            var req = new AdminGetUserListRequest
            {
                GeneralHeaders = HttpContext.GetGeneralHeaders(),
                Count = count,
                Page = page,
                OrderBy = o,
                Query = q
            };
            await _IAdminGetUserListUseCase.handleAsync(req, _AdminGetUserListResponse);
            return _AdminGetUserListResponse.jsonContentResult;
        }
        [HttpGet("/user/AdminGetUserRoomList")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminGetUserRoomList(int page, int count, OrderBy o, long uid, string? q)
        {
            await _IAdminGetUserRoomListUseCase.handleAsync(new AdminGetUserRoomListRequest
            {
                GeneralHeaders = HttpContext.GetGeneralHeaders(),
                Count = count,
                Page = page,
                OrderBy = o,
                OwnerID = uid,
                Query = q
            }, _AdminGetUserRoomListResponse);
            return _AdminGetUserRoomListResponse.jsonContentResult;
        }
        [HttpPost("/user/AdminCreateRoom")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminCreateRoom(AdminCreateRoomVM vm)
        {
            AdminCreateRoomRequest req = _mapper.Map<AdminCreateRoomRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IAdminCreateRoomUseCase.handleAsync(req, _AdminCreateRoomResponse);
            return _AdminCreateRoomResponse.jsonContentResult;
        }
        [HttpPost("/user/AdminChangeRole")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminChangeRole(AdminChangeRoleVM vm)
        {
            AdminChangeRoleRequest req = _mapper.Map<AdminChangeRoleRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IAdminChangeRoleUseCase.handleAsync(req, _AdminChangeRoleResponse);
            return _AdminChangeRoleResponse.jsonContentResult;
        }
        [HttpPost("/user/ChangeAvatarOfRoom")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> ChangeAvatarOfRoom(ChangeAvatarOfRoomVM vm)
        {
            ChangeAvatarOfRoomRequest req = _mapper.Map<ChangeAvatarOfRoomRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IChangeAvatarOfRoomUseCase.handleAsync(req, _ChangeAvatarOfRoomResponse);
            return _ChangeAvatarOfRoomResponse.jsonContentResult;
        }

        [HttpPost("/user/InsertSlideShow")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> InsertSlideShow(InsertSlideShowVM vm)
        {
            InsertSlideShowRequest req = _mapper.Map<InsertSlideShowRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IInsertSlideShowUseCase.handleAsync(req, _InsertSlideShowResponse);
            return _InsertSlideShowResponse.jsonContentResult;
        }

        [HttpPost("/user/EditSlideShow")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> EditSlideShow(EditSlideShowVM vm)
        {
            EditSlideShowRequest req = _mapper.Map<EditSlideShowRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IEditSlideShowUseCase.handleAsync(req, _EditSlideShowResponse);
            return _EditSlideShowResponse.jsonContentResult;
        }

        [HttpGet("/room/GetAllCertificateOfMember/{idroom}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetAllCertificateOfMember([FromRoute] string idroom)
        {
            GetAllCertificateOfMemberRequest req = new GetAllCertificateOfMemberRequest
            {
                ID=idroom,
                Page=0,
                Count=5000
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetAllCertificateOfMemberUseCase.handleAsync(req, _GetAllCertificateOfMemberResponse);
            return _GetAllCertificateOfMemberResponse.jsonContentResult;
        }

        [HttpGet("/room/GetMembersOfMeetOnlyCertificated/{id}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetMembersOfMeetOnlyCertificated([FromRoute] string id)
        {
            GetMembersOfMeetOnlyCertificatedRequest req = new GetMembersOfMeetOnlyCertificatedRequest
            {
                ID= id,
                Page=0,
                Count=5000
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetMembersOfMeetOnlyCertificatedUseCase.handleAsync(req, _GetMembersOfMeetOnlyCertificatedResponse);
            return _GetMembersOfMeetOnlyCertificatedResponse.jsonContentResult;
        }
        [HttpGet("/room/GenerateCertificateWithUserMeetID/{umid}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GenerateCertificateWithUserMeetID([FromRoute] long umid)
        {
            GenerateCertificateWithUserMeetIDRequest req = new GenerateCertificateWithUserMeetIDRequest
            {
                ID= umid,
                Page=0,
                Count=5000
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGenerateCertificateWithUserMeetIDUseCase.handleAsync(req, _GenerateCertificateWithUserMeetIDResponse);
            return _GenerateCertificateWithUserMeetIDResponse.jsonContentResult;
        }
        [HttpGet("poll/GetPollListAdmin/")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetPollListAdmin()
        {
            GetPollListAdminRequest req = new GetPollListAdminRequest
            {
                Page=0,
                Count=5000
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetPollListAdminUseCase.handleAsync(req, _GetPollListAdminResponse);
            return _GetPollListAdminResponse.jsonContentResult;
        }
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpGet("/admin/GetLives")]
        public async Task<IActionResult> GetLives([FromQuery] string q = "")
        {
            GetAllLivesRequest req = new GetAllLivesRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetAllLivesUseCase.handleAsync(req, _GetAllLivesResponse);
            return _GetAllLivesResponse.jsonContentResult;
        }
        [HttpGet("/admin/GetAllRooms")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetAllRooms([FromQuery] string q = "", [FromQuery] string username = null)
        {
            GetAllRoomsRequest req = new GetAllRoomsRequest
            {
                Query = q,
                UserName = username
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetAllRoomsUseCase.handleAsync(req, _GetAllRoomsResponse);
            return _GetAllRoomsResponse.jsonContentResult;
        }
        [HttpGet("/admin/GetAllRoomsMini")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetAllRoomsMini([FromQuery] string q = "", [FromQuery] string username = null)
        {
            GetAllRoomsMiniRequest req = new GetAllRoomsMiniRequest
            {
                Query = q,
                UserName = username
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetAllRoomsMiniUseCase.handleAsync(req, _GetAllRoomsMiniResponse);
            return _GetAllRoomsMiniResponse.jsonContentResult;
        }


        [HttpPost("poll/AddPollQuiz")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AddPollQuiz(AddPollQuizVM vm)
        {
            AddPollQuizRequest req = new AddPollQuizRequest
            {
                Question= vm.Question,
                AnsPollRequestJSONs=vm.AnsPollRequestJSONs,
                Type=vm.Type,
                Page=0,
                Count=5000
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IAddPollQuizUseCase.handleAsync(req, _AddPollQuizResponse);
            return _AddPollQuizResponse.jsonContentResult;
        }


        [HttpGet("poll/GetPollReport")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetPollReport([FromQuery]GetPollReportVM vm)
        {
            GetPollReportRequest req = _mapper.Map<GetPollReportRequest>(vm);

            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetPollReportUseCase.handleAsync(req, _GetPollReportResponse);
            return _GetPollReportResponse.jsonContentResult;
        }


        [HttpGet("/admin/GetAllMeeting")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetAllMeeting([FromQuery] string q = "", [FromQuery] string username = null)
        {
            GetAllMeetingRequest req = new GetAllMeetingRequest
            {
                Query = q,
                UserName = username
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetAllMeetingUseCase.handleAsync(req, _GetAllMeetingResponse);
            return _GetAllMeetingResponse.jsonContentResult;
        }









    }


}
