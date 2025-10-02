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
        public IGetTeacherListUseCase _IGetTeacherListUseCase { get; }
        public ApiPresenter<GetTeacherListResponse> _GetTeacherListResponse { get; }

        public INewRoomUseCase _INewRoomUseCase { get; }
        public ApiPresenter<NewRoomResponse> _NewRoomResponse { get; }


        public BBBController(
            ILogger<BBBController> logger,
            IMapper mapper,
            INewUserUseCase newUserUseCase,
            ApiPresenter<NewUserResponse> newUserResponse,
            INewMeetingUseCase newMeetingUseCase,
            ApiPresenter<NewMeetingResponse> newMeetingResponse,
            IUpdateMeetingUseCase updateMeetingUseCase,
            ApiPresenter<UpdateMeetingResponse> updateMeetingResponse,
            INewUserMettingsUseCase newUserMettingsUseCase,
            ApiPresenter<NewUserMettingsResponse> newUserMettingsResponse,
            INewUserMeetingLogUseCase newUserMeetingLogUseCase,
            ApiPresenter<NewUserMeetingLogResponse> newUserMeetingLogResponse,
            IGetTeacherListUseCase getTeacherListUseCase,
            ApiPresenter<GetTeacherListResponse> getTeacherListResponse,
             INewRoomUseCase NewRoomUseCase,
            ApiPresenter<NewRoomResponse> NewRoomResponse
         )
        {
            _logger = logger;
            _mapper = mapper;
            _INewUserUseCase = newUserUseCase;
            _NewUserResponse = newUserResponse;
            _INewMeetingUseCase = newMeetingUseCase;
            _NewMeetingResponse = newMeetingResponse;
            _IUpdateMeetingUseCase = updateMeetingUseCase;
            _UpdateMeetingResponse = updateMeetingResponse;
            _INewUserMettingsUseCase = newUserMettingsUseCase;
            _NewUserMettingsResponse = newUserMettingsResponse;
            _INewUserMeetingLogUseCase = newUserMeetingLogUseCase;
            _NewUserMeetingLogResponse = newUserMeetingLogResponse;
            _IGetTeacherListUseCase = getTeacherListUseCase;
            _NewRoomResponse = NewRoomResponse;
            _INewRoomUseCase = NewRoomUseCase;
            _NewRoomResponse = NewRoomResponse;
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
        [EnableCors("AllowOnlyVC")]
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
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> UpdateMeeting(UpdateMeetingViewModel vm)
        {
            UpdateMeetingRequest req = _mapper.Map<UpdateMeetingRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IUpdateMeetingUseCase.handleAsync(req, _UpdateMeetingResponse);
            return _UpdateMeetingResponse.jsonContentResult;
        }

        [HttpPost("/user/NewMeeting")]
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> NewMeeting(NewMeetingViewModel vm)
        {
            NewMeetingRequest req = _mapper.Map<NewMeetingRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewMeetingUseCase.handleAsync(req, _NewMeetingResponse);
            return _NewMeetingResponse.jsonContentResult;
        }


        [HttpPost("/user/NewUserMettings")]
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> NewUserMettings(NewUserMettingsViewModel vm)
        {
            NewUserMettingsRequest req = _mapper.Map<NewUserMettingsRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewUserMettingsUseCase.handleAsync(req, _NewUserMettingsResponse);
            return _NewUserMettingsResponse.jsonContentResult;
        }

        // [HttpPost("/user/NewTeacher")]
        // [EnableCors("AllowOnlyVC")]
        // public async Task<IActionResult> NewTeacher(NewTeacherViewModel vm)
        // {
        //     NewTeacherRequest req = _mapper.Map<NewTeacherRequest>(vm);
        //     req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
        //     await _INewTeacherUseCase.handleAsync(req, _NewTeacherResponse);
        //     return _NewTeacherResponse.jsonContentResult;
        // }


        [HttpGet("/user/GetTeacherList")]
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> GetTeacherList([FromQuery] string q)
        {
            GetTeacherListRequest req = new GetTeacherListRequest
            {
                Query = q
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetTeacherListUseCase.handleAsync(req, _GetTeacherListResponse);
            return _GetTeacherListResponse.jsonContentResult;
        }


        [HttpPost("/user/NewRoom")]
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> NewRoom(NewRoomViewModel vm)
        {
            NewRoomRequest req = _mapper.Map<NewRoomRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewRoomUseCase.handleAsync(req, _NewRoomResponse);
            return _NewRoomResponse.jsonContentResult;
        }

        [HttpPost("/user/NewUserMeetingLog")]
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> NewUserMeetingLog(NewUserMeetingLogViewModel vm)
        {
            NewUserMeetingLogRequest req = _mapper.Map<NewUserMeetingLogRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewUserMeetingLogUseCase.handleAsync(req, _NewUserMeetingLogResponse);
            return _NewUserMeetingLogResponse.jsonContentResult;
        }














    }
}