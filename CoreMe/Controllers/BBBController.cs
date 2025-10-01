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
        public async Task<IActionResult> NewUser(UserViewModel vm)
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
        [HttpPost("/user/NewMeeting")]
        [EnableCors("AllowOnlyVC")]
        public async Task<IActionResult> NewMeeting(NewUserViewModel vm)
        {
            NewMeetingRequest req = _mapper.Map<NewMeetingRequest>(vm);
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _INewMeetingUseCase.handleAsync(req, _NewMeetingResponse);
            return _NewMeetingResponse.jsonContentResult;
        }

    }
}