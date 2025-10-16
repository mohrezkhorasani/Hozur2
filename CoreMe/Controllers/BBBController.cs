using Application.DTO;
using Application.Interface.IUsecase;
using AutoMapper;
using CoreMe.VM;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Presenter;

namespace CoreMe.Controllers
{
    [ApiController]
    [Route("bbb")]
    public class BBBController : ControllerBase
    {
        private readonly ILogger<BBBController> _logger;
        private readonly IMapper _mapper;

        public INewUserUseCase _INewUserUseCase { get; }
        public ApiPresenter<NewUserResponse> _NewUserResponse { get; }
        public INewMeetingUseCase _INewMeetingUseCase { get; }
        public ApiPresenter<NewMeetingResponse> _NewMeetingResponse { get; }
        public IUpdateMeetingUseCase _IUpdateMeetingUseCase { get; }
        public ApiPresenter<UpdateMeetingResponse> _UpdateMeetingResponse { get; }
        public INewUserMettingsUseCase _INewUserMettingsUseCase { get; }
        public ApiPresenter<NewUserMettingsResponse> _NewUserMettingsResponse { get; }

        public INewUserMeetingLogUseCase _INewUserMeetingLogUseCase { get; }
        public ApiPresenter<NewUserMeetingLogResponse> _NewUserMeetingLogResponse { get; }
      public INewUserMeetingLogFromModeratorUseCase _INewUserMeetingLogFromModeratorUseCase { get; }
        public ApiPresenter<NewUserMeetingLogFromModeratorResponse> _NewUserMeetingLogFromModeratorResponse { get; }
        public IGetTeacherListUseCase _IGetTeacherListUseCase { get; }
        public ApiPresenter<GetTeacherListResponse> _GetTeacherListResponse { get; }
        public IGetRoomsUseCase _IGetRoomsUseCase { get; }
        public ApiPresenter<GetRoomsResponse> _GetRoomsResponse { get; }
        public IGetLivesUseCase _IGetLivesUseCase { get; }
        public ApiPresenter<GetLivesResponse> _GetLivesResponse { get; }
        public IStartMeetingUseCase _IStartMeetingUseCase { get; }
        public ApiPresenter<StartMeetingResponse> _StartMeetingResponse { get; }
        public IGetMeetingURLUseCase _IGetMeetingURLUseCase { get; }
        public ApiPresenter<GetMeetingURLResponse> _GetMeetingURLResponse { get; }
        public IGetRoomByFriendlyIDUseCase _IGetRoomByFriendlyIDUseCase { get; }
        public ApiPresenter<GetRoomByFriendlyIDResponse> _GetRoomByFriendlyIDResponse { get; }

        public INewRoomUseCase _INewRoomUseCase { get; }
        public ApiPresenter<NewRoomResponse> _NewRoomResponse { get; }
        public IGenerateCertificateUseCase _IGenerateCertificateUseCase { get; }
        public ApiPresenter<GenerateCertificateResponse> _GenerateCertificateResponse { get; }

        public BBBController(ILogger<BBBController> logger, IMapper mapper, INewUserUseCase iNewUserUseCase, ApiPresenter<NewUserResponse> newUserResponse, INewMeetingUseCase iNewMeetingUseCase, ApiPresenter<NewMeetingResponse> newMeetingResponse, IUpdateMeetingUseCase iUpdateMeetingUseCase, ApiPresenter<UpdateMeetingResponse> updateMeetingResponse, INewUserMettingsUseCase iNewUserMettingsUseCase, ApiPresenter<NewUserMettingsResponse> newUserMettingsResponse, INewUserMeetingLogUseCase iNewUserMeetingLogUseCase, ApiPresenter<NewUserMeetingLogResponse> newUserMeetingLogResponse, INewUserMeetingLogFromModeratorUseCase iNewUserMeetingLogFromModeratorUseCase, ApiPresenter<NewUserMeetingLogFromModeratorResponse> newUserMeetingLogFromModeratorResponse, IGetTeacherListUseCase iGetTeacherListUseCase, ApiPresenter<GetTeacherListResponse> getTeacherListResponse, IGetRoomsUseCase iGetRoomsUseCase, ApiPresenter<GetRoomsResponse> getRoomsResponse, IGetLivesUseCase iGetLivesUseCase, ApiPresenter<GetLivesResponse> getLivesResponse, IStartMeetingUseCase iStartMeetingUseCase, ApiPresenter<StartMeetingResponse> startMeetingResponse, IGetMeetingURLUseCase iGetMeetingURLUseCase, ApiPresenter<GetMeetingURLResponse> getMeetingURLResponse, IGetRoomByFriendlyIDUseCase iGetRoomByFriendlyIDUseCase, ApiPresenter<GetRoomByFriendlyIDResponse> getRoomByFriendlyIDResponse, INewRoomUseCase iNewRoomUseCase, ApiPresenter<NewRoomResponse> newRoomResponse, IGenerateCertificateUseCase iGenerateCertificateUseCase, ApiPresenter<GenerateCertificateResponse> generateCertificateResponse)
        {
            _logger = logger;
            _mapper = mapper;
            _INewUserUseCase = iNewUserUseCase;
            _NewUserResponse = newUserResponse;
            _INewMeetingUseCase = iNewMeetingUseCase;
            _NewMeetingResponse = newMeetingResponse;
            _IUpdateMeetingUseCase = iUpdateMeetingUseCase;
            _UpdateMeetingResponse = updateMeetingResponse;
            _INewUserMettingsUseCase = iNewUserMettingsUseCase;
            _NewUserMettingsResponse = newUserMettingsResponse;
            _INewUserMeetingLogUseCase = iNewUserMeetingLogUseCase;
            _NewUserMeetingLogResponse = newUserMeetingLogResponse;
            _INewUserMeetingLogFromModeratorUseCase = iNewUserMeetingLogFromModeratorUseCase;
            _NewUserMeetingLogFromModeratorResponse = newUserMeetingLogFromModeratorResponse;
            _IGetTeacherListUseCase = iGetTeacherListUseCase;
            _GetTeacherListResponse = getTeacherListResponse;
            _IGetRoomsUseCase = iGetRoomsUseCase;
            _GetRoomsResponse = getRoomsResponse;
            _IGetLivesUseCase = iGetLivesUseCase;
            _GetLivesResponse = getLivesResponse;
            _IStartMeetingUseCase = iStartMeetingUseCase;
            _StartMeetingResponse = startMeetingResponse;
            _IGetMeetingURLUseCase = iGetMeetingURLUseCase;
            _GetMeetingURLResponse = getMeetingURLResponse;
            _IGetRoomByFriendlyIDUseCase = iGetRoomByFriendlyIDUseCase;
            _GetRoomByFriendlyIDResponse = getRoomByFriendlyIDResponse;
            _INewRoomUseCase = iNewRoomUseCase;
            _NewRoomResponse = newRoomResponse;
            _IGenerateCertificateUseCase = iGenerateCertificateUseCase;
            _GenerateCertificateResponse = generateCertificateResponse;
        }






        // public IGetLanguageTestUseCase _IGetLanguageTestUseCase { get; }
        // public ApiPresenter<GetLanguageTestResponse> _GetLanguageTestResponse { get; }
        // public IGetNotificationsUseCase _IGetNotificationsUseCase { get; }
        // public ApiPresenter<GetNotificationsResponse> _GetNotificationsResponse { get; }
        // public IGetCompletionUseCase _IGetCompletionUseCase { get; }
        // public ApiPresenter<GetCompletionResponse> _GetCompletionResponse { get; }
        // public IGetOtherDocumentSuggestionUseCase _IGetOtherDocumentSuggestionUseCase { get; }
        // public ApiPresenter<GetOtherDocumentSuggestionResponse> _GetOtherDocumentSuggestionResponse { get; }

        /// <summary>
        /// This is new user
        /// </summary>
        /// <returns></returns>
        [HttpPost("/user/NewUser")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUser(NewUserViewModel vm)
        {
            await _INewUserUseCase.handleAsync(new NewUserRequest
            {
                GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request),
                UUID = vm.UUID,
                Firstname = vm.Firstname,
                Lastname = vm.Lastname,
                NationalCode = vm.NationalCode,
                PhoneNumber = vm.PhoneNumber,
                ExtId = vm.ExtId,
                Email = vm.Email,
                AllowedGUID = vm.AllowedGUID,
                Role = vm.Role
            }, _NewUserResponse);
            return _NewUserResponse.jsonContentResult;
        }
        [HttpPost("/user/UpdateMeeting")]
        // [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> UpdateMeeting(UpdateMeetingViewModel vm)
        {
            UpdateMeetingRequest req = _mapper.Map<UpdateMeetingRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IUpdateMeetingUseCase.handleAsync(req, _UpdateMeetingResponse);
            return _UpdateMeetingResponse.jsonContentResult;
        }

        [HttpPost("/user/NewMeeting")]
        // [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewMeeting(NewMeetingViewModel vm)
        {
            NewMeetingRequest req = _mapper.Map<NewMeetingRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewMeetingUseCase.handleAsync(req, _NewMeetingResponse);
            return _NewMeetingResponse.jsonContentResult;
        }


        [HttpPost("/user/NewUserMettings")]
        // [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUserMettings(NewUserMettingsViewModel vm)
        {
            NewUserMettingsRequest req = _mapper.Map<NewUserMettingsRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewUserMettingsUseCase.handleAsync(req, _NewUserMettingsResponse);
            return _NewUserMettingsResponse.jsonContentResult;
        }

        // [HttpPost("/user/NewTeacher")]
        // [EnableCors("AllowHekmateSubdomains")]
        // public async Task<IActionResult> NewTeacher(NewTeacherViewModel vm)
        // {
        //     NewTeacherRequest req = _mapper.Map<NewTeacherRequest>(vm);
        //     req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
        //     await _INewTeacherUseCase.handleAsync(req, _NewTeacherResponse);
        //     return _NewTeacherResponse.jsonContentResult;
        // }


        [HttpGet("/user/GetTeacherList")]
        public async Task<IActionResult> GetTeacherList([FromQuery] string q = "")
        {
            GetTeacherListRequest req = new GetTeacherListRequest
            {
                Query = q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetTeacherListUseCase.handleAsync(req, _GetTeacherListResponse);
            return _GetTeacherListResponse.jsonContentResult;
        }
        [HttpGet("/user/GetRoomByFriendlyID")]
        public async Task<IActionResult> GetRoomByFriendlyID([FromQuery] string q = "")
        {
            GetRoomByFriendlyIDRequest req = new GetRoomByFriendlyIDRequest
            {
                Query = q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetRoomByFriendlyIDUseCase.handleAsync(req, _GetRoomByFriendlyIDResponse);
            return _GetRoomByFriendlyIDResponse.jsonContentResult;
        }
        [HttpGet("/user/GetRooms")]
        public async Task<IActionResult> GetRooms([FromQuery] string q = "")
        {
            GetRoomsRequest req = new GetRoomsRequest
            {
                Query = q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetRoomsUseCase.handleAsync(req, _GetRoomsResponse);
            return _GetRoomsResponse.jsonContentResult;
        }

        [HttpGet("/user/GetLives")]
        public async Task<IActionResult> GetLives([FromQuery] string q = "")
        {
            GetLivesRequest req = new GetLivesRequest
            {
                Query = q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetLivesUseCase.handleAsync(req, _GetLivesResponse);
            return _GetLivesResponse.jsonContentResult;
        }

        [HttpGet("/user/StartMeeting")]
        public async Task<IActionResult> StartMeeting([FromQuery] string ID = "", [FromQuery] string Name = "")
        {
            StartMeetingRequest req = new StartMeetingRequest
            {
                MeetName = Name,
                MeetiID = ID
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IStartMeetingUseCase.handleAsync(req, _StartMeetingResponse);
            return _StartMeetingResponse.jsonContentResult;
        }

        [HttpPost("/user/GetMeetingURL")]
        public async Task<IActionResult> GetMeetingURL(GetMeetingURLViewModel vm)
        {
            GetMeetingURLRequest req = new GetMeetingURLRequest
            {
                FullName = vm.NickName,
                MeetiID = vm.MeetID,
                AsModerator = vm.IsModerator,
                Password = vm.Password
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetMeetingURLUseCase.handleAsync(req, _GetMeetingURLResponse);
            return _GetMeetingURLResponse.jsonContentResult;
        }


        [HttpPost("/user/NewRoom")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewRoom(NewRoomViewModel vm)
        {
            NewRoomRequest req = _mapper.Map<NewRoomRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewRoomUseCase.handleAsync(req, _NewRoomResponse);
            return _NewRoomResponse.jsonContentResult;
        }


        [HttpPost("/user/GenerateCertificate")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> GenerateCertificate(GenerateCertificateViewModel vm)
        {
            GenerateCertificateRequest req = _mapper.Map<GenerateCertificateRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGenerateCertificateUseCase.handleAsync(req, _GenerateCertificateResponse);
            return _GenerateCertificateResponse.jsonContentResult;
        }

        [HttpPost("/user/NewUserMeetingLog")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUserMeetingLog(NewUserMeetingLogViewModel vm)
        {
            NewUserMeetingLogRequest req = _mapper.Map<NewUserMeetingLogRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewUserMeetingLogUseCase.handleAsync(req, _NewUserMeetingLogResponse);
            return _NewUserMeetingLogResponse.jsonContentResult;
        }


        [HttpPost("/user/NewUserMeetingLogFromModerator")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUserMeetingLogFromModerator(NewUserMeetingLogFromModeratorViewModel vm)
        {
            NewUserMeetingLogFromModeratorRequest req = _mapper.Map<NewUserMeetingLogFromModeratorRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewUserMeetingLogFromModeratorUseCase.handleAsync(req, _NewUserMeetingLogFromModeratorResponse);
            return _NewUserMeetingLogFromModeratorResponse.jsonContentResult;
        }














    }
}