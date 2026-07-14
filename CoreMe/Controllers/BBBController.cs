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
        public IGetRoomsPublicUseCase _IGetRoomsPublicUseCase { get; }
        public ApiPresenter<GetRoomsPublicResponse> _GetRoomsPublicResponse { get; }
        public IGetLivesOfProfileUseCase _IGetLivesOfProfileUseCase { get; }
        public ApiPresenter<GetLivesOfProfileResponse> _GetLivesOfProfileResponse { get; }
        public IGetLivesUseCase _IGetLivesUseCase { get; }
        public ApiPresenter<GetLivesResponse> _GetLivesResponse { get; }
        public IGetLiveWithIDUseCase _IGetLiveWithIDUseCase { get; }
        public ApiPresenter<GetLiveWithIDResponse> _GetLiveWithIDResponse { get; }
        public IGetMembersOfMeetUseCase _IGetMembersOfMeetUseCase { get; }
        public ApiPresenter<GetMembersOfMeetResponse> _GetMembersOfMeetResponse { get; }
        public IGetRoomCalendarActivityUseCase _IGetRoomCalendarActivityUseCase { get; }
        public ApiPresenter<GetRoomCalendarActivityResponse> _GetRoomCalendarActivityResponse { get; }
        public IGetLivesPublicUseCase _IGetLivesPublicUseCase { get; }
        public ApiPresenter<GetLivesPublicResponse> _GetLivesPublicResponse { get; }
        public IStartNewMeetUseCase _IStartNewMeetUseCase { get; }
        public ApiPresenter<StartNewMeetResponse> _StartNewMeetResponse { get; }
        public IGetMeetingURLUseCase _IGetMeetingURLUseCase { get; }
        public ApiPresenter<GetMeetingURLResponse> _GetMeetingURLResponse { get; }
        public IGetRoomByFriendlyIDUseCase _IGetRoomByFriendlyIDUseCase { get; }
        public ApiPresenter<GetRoomByFriendlyIDResponse> _GetRoomByFriendlyIDResponse { get; }
        public INewRoomUseCase _INewRoomUseCase { get; }
        public ApiPresenter<NewRoomResponse> _NewRoomResponse { get; }
        public IGenerateCertificateUseCase _IGenerateCertificateUseCase { get; }
        public ApiPresenter<GenerateCertificateResponse> _GenerateCertificateResponse { get; }
        public IGuarantCertificateUseCase _IGuarantCertificateUseCase { get; }
        public ApiPresenter<GuarantCertificateResponse> _GuarantCertificateResponse { get; }
        public IEndTheMeetingUseCase _IEndTheMeetingUseCase { get; }
        public ApiPresenter<EndTheMeetingResponse> _EndTheMeetingResponse { get; }
        public IEndTheMeetingWithAPIUseCase _IEndTheMeetingWithAPIUseCase { get; }
        public ApiPresenter<EndTheMeetingWithAPIResponse> _EndTheMeetingWithAPIResponse { get; }
        public INewQuestionUseCase _INewQuestionUseCase { get; }
        public ApiPresenter<NewQuestionResponse> _NewQuestionResponse { get; }
        public INewPlanRoomUseCase _INewPlanRoomUseCase { get; }
        public ApiPresenter<NewPlanRoomResponse> _NewPlanRoomResponse { get; }
        public IGetQuestionOfMeetUseCase _IGetQuestionOfMeetUseCase { get; }
        public ApiPresenter<GetQuestionOfMeetResponse> _GetQuestionOfMeetResponse { get; }
        public IQuestionAnsweredUseCase _IQuestionAnsweredUseCase { get; }
        public ApiPresenter<QuestionAnsweredResponse> _QuestionAnsweredResponse { get; }
        public ISignInUserPanelUseCase _ISignInUserPanelUseCase { get; }
        public ApiPresenter<SignInUserPanelResponse> _SignInUserPanelResponse { get; }
        public ILoginUserPanelUseCase _ILoginUserPanelUseCase { get; }
        public ApiPresenter<LoginUserPanelResponse> _LoginUserPanelResponse { get; }
        public IVerifyTokenUseCase _IVerifyTokenUseCase { get; }
        public ApiPresenter<VerifyTokenResponse> _VerifyTokenResponse { get; }
        public IGetSlideShowUseCase _IGetSlideShowUseCase { get; }
        public ApiPresenter<GetSlideShowResponse> _GetSlideShowResponse { get; }
        public IContactusUseCase _IContactusUseCase { get; }
        public ApiPresenter<ContactusResponse> _ContactusResponse { get; }
        public IGetProfileInfoUseCase _IGetProfileInfoUseCase { get; }
        public ApiPresenter<GetProfileInfoResponse> _GetProfileInfoResponse { get; }
        public IFollowProfileUseCase _IFollowProfileUseCase { get; }
        public ApiPresenter<FollowProfileResponse> _FollowProfileResponse { get; }
        public IGetReservationUseCase _IGetReservationUseCase { get; }
        public ApiPresenter<GetReservationResponse> _GetReservationResponse { get; }
        public INewReservationUseCase _INewReservationUseCase { get; }
        public ApiPresenter<NewReservationResponse> _NewReservationResponse { get; }
        public IGetHekmatShobeUseCase _IGetHekmatShobeUseCase { get; }
        public ApiPresenter<GetHekmatShobeResponse> _GetHekmatShobeResponse { get; }
        public IGetPlanRoomUseCase _IGetPlanRoomUseCase { get; }
        public ApiPresenter<GetPlanRoomResponse> _GetPlanRoomResponse { get; }
        public IGetPlanRoomPublicUseCase _IGetPlanRoomPublicUseCase { get; }
        public ApiPresenter<GetPlanRoomPublicResponse> _GetPlanRoomPublicResponse { get; }
        public IGetLatestMeetingsUseCase _IGetLatestMeetingsUseCase { get; }
        public ApiPresenter<GetLatestMeetingsResponse> _GetLatestMeetingsResponse { get; }
         public IGetPollListUseCase _IGetPollListUseCase { get; }
        public ApiPresenter<GetPollListResponse> _GetPollListResponse { get; }
       public IGetFileStorageUseCase _IGetFileStorageUseCase { get; }
        public ApiPresenter<GetFileStorageResponse> _GetFileStorageResponse { get; }
      public IGetSingleFileUseCase _IGetSingleFileUseCase { get; }
        public ApiPresenter<GetSingleFileResponse> _GetSingleFileResponse { get; }
        public IAnswerPollListUseCase _IAnswerPollListUseCase { get; }
        public ApiPresenter<AnswerPollListResponse> _AnswerPollListResponse { get; }
        public IUpdatePresentMinToCertOfMeetUseCase _IUpdatePresentMinToCertOfMeetUseCase { get; }
        public ApiPresenter<UpdatePresentMinToCertOfMeetResponse> _UpdatePresentMinToCertOfMeetResponse { get; }
        public IUploadFileStorageUseCase _IUploadFileStorageUseCase { get; }
        public ApiPresenter<UploadFileStorageResponse> _UploadFileStorageResponse { get; }
        public IDeletePlanRoomUseCase _IDeletePlanRoomUseCase { get; }
        public ApiPresenter<DeletePlanRoomResponse> _DeletePlanRoomResponse { get; }

        public BBBController(ILogger<BBBController> logger, IMapper mapper, INewUserUseCase iNewUserUseCase, ApiPresenter<NewUserResponse> newUserResponse, INewMeetingUseCase iNewMeetingUseCase, ApiPresenter<NewMeetingResponse> newMeetingResponse, IUpdateMeetingUseCase iUpdateMeetingUseCase, ApiPresenter<UpdateMeetingResponse> updateMeetingResponse, INewUserMettingsUseCase iNewUserMettingsUseCase, ApiPresenter<NewUserMettingsResponse> newUserMettingsResponse, INewUserMeetingLogUseCase iNewUserMeetingLogUseCase, ApiPresenter<NewUserMeetingLogResponse> newUserMeetingLogResponse, INewUserMeetingLogFromModeratorUseCase iNewUserMeetingLogFromModeratorUseCase, ApiPresenter<NewUserMeetingLogFromModeratorResponse> newUserMeetingLogFromModeratorResponse, IGetTeacherListUseCase iGetTeacherListUseCase, ApiPresenter<GetTeacherListResponse> getTeacherListResponse, IGetRoomsUseCase iGetRoomsUseCase, ApiPresenter<GetRoomsResponse> getRoomsResponse, IGetRoomsPublicUseCase iGetRoomsPublicUseCase, ApiPresenter<GetRoomsPublicResponse> getRoomsPublicResponse, IGetLivesOfProfileUseCase iGetLivesOfProfileUseCase, ApiPresenter<GetLivesOfProfileResponse> getLivesOfProfileResponse, IGetLivesUseCase iGetLivesUseCase, ApiPresenter<GetLivesResponse> getLivesResponse, IGetLiveWithIDUseCase iGetLiveWithIDUseCase, ApiPresenter<GetLiveWithIDResponse> getLiveWithIDResponse, IGetMembersOfMeetUseCase iGetMembersOfMeetUseCase, ApiPresenter<GetMembersOfMeetResponse> getMembersOfMeetResponse, IGetRoomCalendarActivityUseCase iGetRoomCalendarActivityUseCase, ApiPresenter<GetRoomCalendarActivityResponse> getRoomCalendarActivityResponse, IGetLivesPublicUseCase iGetLivesPublicUseCase, ApiPresenter<GetLivesPublicResponse> getLivesPublicResponse, IStartNewMeetUseCase iStartNewMeetUseCase, ApiPresenter<StartNewMeetResponse> startNewMeetResponse, IGetMeetingURLUseCase iGetMeetingURLUseCase, ApiPresenter<GetMeetingURLResponse> getMeetingURLResponse, IGetRoomByFriendlyIDUseCase iGetRoomByFriendlyIDUseCase, ApiPresenter<GetRoomByFriendlyIDResponse> getRoomByFriendlyIDResponse, INewRoomUseCase iNewRoomUseCase, ApiPresenter<NewRoomResponse> newRoomResponse, IGenerateCertificateUseCase iGenerateCertificateUseCase, ApiPresenter<GenerateCertificateResponse> generateCertificateResponse, IGuarantCertificateUseCase iGuarantCertificateUseCase, ApiPresenter<GuarantCertificateResponse> guarantCertificateResponse, IEndTheMeetingUseCase iEndTheMeetingUseCase, ApiPresenter<EndTheMeetingResponse> endTheMeetingResponse, IEndTheMeetingWithAPIUseCase iEndTheMeetingWithAPIUseCase, ApiPresenter<EndTheMeetingWithAPIResponse> endTheMeetingWithAPIResponse, INewQuestionUseCase iNewQuestionUseCase, ApiPresenter<NewQuestionResponse> newQuestionResponse, INewPlanRoomUseCase iNewPlanRoomUseCase, ApiPresenter<NewPlanRoomResponse> newPlanRoomResponse, IGetQuestionOfMeetUseCase iGetQuestionOfMeetUseCase, ApiPresenter<GetQuestionOfMeetResponse> getQuestionOfMeetResponse, IQuestionAnsweredUseCase iQuestionAnsweredUseCase, ApiPresenter<QuestionAnsweredResponse> questionAnsweredResponse, ISignInUserPanelUseCase iSignInUserPanelUseCase, ApiPresenter<SignInUserPanelResponse> signInUserPanelResponse, ILoginUserPanelUseCase iLoginUserPanelUseCase, ApiPresenter<LoginUserPanelResponse> loginUserPanelResponse, IVerifyTokenUseCase iVerifyTokenUseCase, ApiPresenter<VerifyTokenResponse> verifyTokenResponse, IGetSlideShowUseCase iGetSlideShowUseCase, ApiPresenter<GetSlideShowResponse> getSlideShowResponse, IContactusUseCase iContactusUseCase, ApiPresenter<ContactusResponse> contactusResponse, IGetProfileInfoUseCase iGetProfileInfoUseCase, ApiPresenter<GetProfileInfoResponse> getProfileInfoResponse, IFollowProfileUseCase iFollowProfileUseCase, ApiPresenter<FollowProfileResponse> followProfileResponse, IGetReservationUseCase iGetReservationUseCase, ApiPresenter<GetReservationResponse> getReservationResponse, INewReservationUseCase iNewReservationUseCase, ApiPresenter<NewReservationResponse> newReservationResponse, IGetHekmatShobeUseCase iGetHekmatShobeUseCase, ApiPresenter<GetHekmatShobeResponse> getHekmatShobeResponse, IGetPlanRoomUseCase iGetPlanRoomUseCase, ApiPresenter<GetPlanRoomResponse> getPlanRoomResponse, IGetPlanRoomPublicUseCase iGetPlanRoomPublicUseCase, ApiPresenter<GetPlanRoomPublicResponse> getPlanRoomPublicResponse, IGetLatestMeetingsUseCase iGetLatestMeetingsUseCase, ApiPresenter<GetLatestMeetingsResponse> getLatestMeetingsResponse, IGetPollListUseCase iGetPollListUseCase, ApiPresenter<GetPollListResponse> getPollListResponse, IGetFileStorageUseCase iGetFileStorageUseCase, ApiPresenter<GetFileStorageResponse> getFileStorageResponse, IGetSingleFileUseCase iGetSingleFileUseCase, ApiPresenter<GetSingleFileResponse> getSingleFileResponse, IAnswerPollListUseCase iAnswerPollListUseCase, ApiPresenter<AnswerPollListResponse> answerPollListResponse, IUpdatePresentMinToCertOfMeetUseCase iUpdatePresentMinToCertOfMeetUseCase, ApiPresenter<UpdatePresentMinToCertOfMeetResponse> updatePresentMinToCertOfMeetResponse, IUploadFileStorageUseCase iUploadFileStorageUseCase, ApiPresenter<UploadFileStorageResponse> uploadFileStorageResponse, IDeletePlanRoomUseCase iDeletePlanRoomUseCase, ApiPresenter<DeletePlanRoomResponse> deletePlanRoomResponse)
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
            _IGetRoomsPublicUseCase = iGetRoomsPublicUseCase;
            _GetRoomsPublicResponse = getRoomsPublicResponse;
            _IGetLivesOfProfileUseCase = iGetLivesOfProfileUseCase;
            _GetLivesOfProfileResponse = getLivesOfProfileResponse;
            _IGetLivesUseCase = iGetLivesUseCase;
            _GetLivesResponse = getLivesResponse;
            _IGetLiveWithIDUseCase = iGetLiveWithIDUseCase;
            _GetLiveWithIDResponse = getLiveWithIDResponse;
            _IGetMembersOfMeetUseCase = iGetMembersOfMeetUseCase;
            _GetMembersOfMeetResponse = getMembersOfMeetResponse;
            _IGetRoomCalendarActivityUseCase = iGetRoomCalendarActivityUseCase;
            _GetRoomCalendarActivityResponse = getRoomCalendarActivityResponse;
            _IGetLivesPublicUseCase = iGetLivesPublicUseCase;
            _GetLivesPublicResponse = getLivesPublicResponse;
            _IStartNewMeetUseCase = iStartNewMeetUseCase;
            _StartNewMeetResponse = startNewMeetResponse;
            _IGetMeetingURLUseCase = iGetMeetingURLUseCase;
            _GetMeetingURLResponse = getMeetingURLResponse;
            _IGetRoomByFriendlyIDUseCase = iGetRoomByFriendlyIDUseCase;
            _GetRoomByFriendlyIDResponse = getRoomByFriendlyIDResponse;
            _INewRoomUseCase = iNewRoomUseCase;
            _NewRoomResponse = newRoomResponse;
            _IGenerateCertificateUseCase = iGenerateCertificateUseCase;
            _GenerateCertificateResponse = generateCertificateResponse;
            _IGuarantCertificateUseCase = iGuarantCertificateUseCase;
            _GuarantCertificateResponse = guarantCertificateResponse;
            _IEndTheMeetingUseCase = iEndTheMeetingUseCase;
            _EndTheMeetingResponse = endTheMeetingResponse;
            _IEndTheMeetingWithAPIUseCase = iEndTheMeetingWithAPIUseCase;
            _EndTheMeetingWithAPIResponse = endTheMeetingWithAPIResponse;
            _INewQuestionUseCase = iNewQuestionUseCase;
            _NewQuestionResponse = newQuestionResponse;
            _INewPlanRoomUseCase = iNewPlanRoomUseCase;
            _NewPlanRoomResponse = newPlanRoomResponse;
            _IGetQuestionOfMeetUseCase = iGetQuestionOfMeetUseCase;
            _GetQuestionOfMeetResponse = getQuestionOfMeetResponse;
            _IQuestionAnsweredUseCase = iQuestionAnsweredUseCase;
            _QuestionAnsweredResponse = questionAnsweredResponse;
            _ISignInUserPanelUseCase = iSignInUserPanelUseCase;
            _SignInUserPanelResponse = signInUserPanelResponse;
            _ILoginUserPanelUseCase = iLoginUserPanelUseCase;
            _LoginUserPanelResponse = loginUserPanelResponse;
            _IVerifyTokenUseCase = iVerifyTokenUseCase;
            _VerifyTokenResponse = verifyTokenResponse;
            _IGetSlideShowUseCase = iGetSlideShowUseCase;
            _GetSlideShowResponse = getSlideShowResponse;
            _IContactusUseCase = iContactusUseCase;
            _ContactusResponse = contactusResponse;
            _IGetProfileInfoUseCase = iGetProfileInfoUseCase;
            _GetProfileInfoResponse = getProfileInfoResponse;
            _IFollowProfileUseCase = iFollowProfileUseCase;
            _FollowProfileResponse = followProfileResponse;
            _IGetReservationUseCase = iGetReservationUseCase;
            _GetReservationResponse = getReservationResponse;
            _INewReservationUseCase = iNewReservationUseCase;
            _NewReservationResponse = newReservationResponse;
            _IGetHekmatShobeUseCase = iGetHekmatShobeUseCase;
            _GetHekmatShobeResponse = getHekmatShobeResponse;
            _IGetPlanRoomUseCase = iGetPlanRoomUseCase;
            _GetPlanRoomResponse = getPlanRoomResponse;
            _IGetPlanRoomPublicUseCase = iGetPlanRoomPublicUseCase;
            _GetPlanRoomPublicResponse = getPlanRoomPublicResponse;
            _IGetLatestMeetingsUseCase = iGetLatestMeetingsUseCase;
            _GetLatestMeetingsResponse = getLatestMeetingsResponse;
            _IGetPollListUseCase = iGetPollListUseCase;
            _GetPollListResponse = getPollListResponse;
            _IGetFileStorageUseCase = iGetFileStorageUseCase;
            _GetFileStorageResponse = getFileStorageResponse;
            _IGetSingleFileUseCase = iGetSingleFileUseCase;
            _GetSingleFileResponse = getSingleFileResponse;
            _IAnswerPollListUseCase = iAnswerPollListUseCase;
            _AnswerPollListResponse = answerPollListResponse;
            _IUpdatePresentMinToCertOfMeetUseCase = iUpdatePresentMinToCertOfMeetUseCase;
            _UpdatePresentMinToCertOfMeetResponse = updatePresentMinToCertOfMeetResponse;
            _IUploadFileStorageUseCase = iUploadFileStorageUseCase;
            _UploadFileStorageResponse = uploadFileStorageResponse;
            _IDeletePlanRoomUseCase = iDeletePlanRoomUseCase;
            _DeletePlanRoomResponse = deletePlanRoomResponse;
        }

        [HttpGet("/user/GetSlideShow")]
        public async Task<IActionResult> GetSlideShow(string? page)
        {
            //GetSlideShowRequest req = _mapper.Map<GetSlideShowRequest>();
            //req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            var head = HttpContext.GetGeneralHeaders();
            await _IGetSlideShowUseCase.handleAsync(new GetSlideShowRequest { GeneralHeaders = head, Page = page }, _GetSlideShowResponse);
            return _GetSlideShowResponse.jsonContentResult;
        }
        [HttpGet("/user/GetProfileInfo")]
        public async Task<IActionResult> GetProfileInfo([FromQuery] string username)
        {
            //GetProfileInfoRequest req = _mapper.Map<GetProfileInfoRequest>();
            //req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            var head = HttpContext.GetGeneralHeaders();
            await _IGetProfileInfoUseCase.handleAsync(new GetProfileInfoRequest { GeneralHeaders = head, Username = username }, _GetProfileInfoResponse);
            return _GetProfileInfoResponse.jsonContentResult;
        }

        [HttpPost("/user/FollowProfile")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> FollowProfile([FromQuery] string username)
        {
            //FollowProfileRequest req = _mapper.Map<FollowProfileRequest>();
            //req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            var head = HttpContext.GetGeneralHeaders();
            await _IFollowProfileUseCase.handleAsync(new FollowProfileRequest { GeneralHeaders = head, Username = username }, _FollowProfileResponse);
            return _FollowProfileResponse.jsonContentResult;
        }

        [HttpGet("/user/GetReservation/{pid}")]
        public async Task<IActionResult> GetReservation([FromRoute] long pid)
        {
            //GetReservationRequest req = _mapper.Map<GetReservationRequest>();
            //req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            var head = HttpContext.GetGeneralHeaders();
            await _IGetReservationUseCase.handleAsync(new GetReservationRequest { GeneralHeaders = head, PlanID = pid }, _GetReservationResponse);
            return _GetReservationResponse.jsonContentResult;
        }

        [HttpPost("/user/NewReservation/{pid}/{nick}")]
        public async Task<IActionResult> NewReservation([FromRoute] long pid, [FromRoute] string nick, [FromQuery]string phone, [FromQuery] string na)
        {
            //NewReservationRequest req = _mapper.Map<NewReservationRequest>();
            //req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            var head = HttpContext.GetGeneralHeaders();
            await _INewReservationUseCase.handleAsync(new NewReservationRequest { GeneralHeaders = head, Name = nick, PlanID = pid, Phone=phone,NationalCode=na }, _NewReservationResponse);
            return _NewReservationResponse.jsonContentResult;
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
                GeneralHeaders = HttpContext.GetGeneralHeaders(),
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
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IUpdateMeetingUseCase.handleAsync(req, _UpdateMeetingResponse);
            return _UpdateMeetingResponse.jsonContentResult;
        }

        [HttpPost("/user/NewMeeting")]
        // [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewMeeting(NewMeetingViewModel vm)
        {
            Statics.Log("NewMeetingContr", JsonConvert.SerializeObject(vm));
            NewMeetingRequest req = _mapper.Map<NewMeetingRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _INewMeetingUseCase.handleAsync(req, _NewMeetingResponse);
            return _NewMeetingResponse.jsonContentResult;
        }

        [HttpPost("/user/NewUserMettings")]
        // [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUserMettings(NewUserMettingsViewModel vm)
        {
            NewUserMettingsRequest req = _mapper.Map<NewUserMettingsRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _INewUserMettingsUseCase.handleAsync(req, _NewUserMettingsResponse);
            return _NewUserMettingsResponse.jsonContentResult;
        }
        [HttpGet("/user/GetListRoomIcon")]
        public async Task<IActionResult> GetListRoomIcon([FromQuery] string path = null)
        {
            // Base directory – you can also read this from configuration
            string baseDirectory = @"C:\hekmatFiles\img\rooms\icons";

            // If a relative subfolder is provided (optional), combine it with base directory
            string targetDirectory = baseDirectory;
            if (!string.IsNullOrEmpty(path))
            {
                // Prevent path traversal attacks
                string combined = Path.GetFullPath(Statics.CombinePath(baseDirectory, path));
                if (!combined.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid path.");

                targetDirectory = combined;
            }

            if (!Directory.Exists(targetDirectory))
                return NotFound($"Directory not found: {targetDirectory}");

            // Allowed image extensions (you can add more)
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg"
            };

            var result = new Dictionary<string, List<object>>();

            // Get all subdirectories inside the target directory
            var directories = Directory.GetDirectories(baseDirectory);
            var dir = directories.First(e => e.Contains(path));
            string folderName = Path.GetFileName(dir);
            var files = Directory.GetFiles(dir)
                .Where(file => allowedExtensions.Contains(Path.GetExtension(file)))
                .Select(file => new
                {
                    fileName = Path.GetFileName(file),
                    path = folderName   // or relative path inside icons if needed
                })
                .ToList<object>();

            result[folderName] = files;

            return Ok(result);
        }
        // [HttpPost("/user/NewTeacher")]
        // [EnableCors("AllowHekmateSubdomains")]
        // public async Task<IActionResult> NewTeacher(NewTeacherViewModel vm)
        // {
        //     NewTeacherRequest req = _mapper.Map<NewTeacherRequest>(vm);
        //     req.GeneralHeaders = HttpContext.GetGeneralHeaders();
        //     await _INewTeacherUseCase.handleAsync(req, _NewTeacherResponse);
        //     return _NewTeacherResponse.jsonContentResult;
        // }


        [HttpGet("/user/GetTeacherList")]
        public async Task<IActionResult> GetTeacherList([FromQuery] string q = "")
        {
            Statics.Log("GetTeacherList", string.Join("\n", Request.Headers));
            Statics.Log("GetTeacherList", string.Join("\n", Request.Headers.Select(e => e.Key).ToList()));
            GetTeacherListRequest req = new GetTeacherListRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetTeacherListUseCase.handleAsync(req, _GetTeacherListResponse);
            return _GetTeacherListResponse.jsonContentResult;
        }
        [HttpGet("/user/GetHekmatShobe")]
        public async Task<IActionResult> GetHekmatShobe([FromQuery] string q = "")
        {
            Statics.Log("GetHekmatShobe", string.Join("\n", Request.Headers));
            Statics.Log("GetHekmatShobe", string.Join("\n", Request.Headers.Select(e => e.Key).ToList()));
            GetHekmatShobeRequest req = new GetHekmatShobeRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetHekmatShobeUseCase.handleAsync(req, _GetHekmatShobeResponse);
            return _GetHekmatShobeResponse.jsonContentResult;
        }
        [HttpGet("/user/GetRoomByFriendlyID")]
        public async Task<IActionResult> GetRoomByFriendlyID([FromQuery] string q = "")
        {
            GetRoomByFriendlyIDRequest req = new GetRoomByFriendlyIDRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetRoomByFriendlyIDUseCase.handleAsync(req, _GetRoomByFriendlyIDResponse);
            return _GetRoomByFriendlyIDResponse.jsonContentResult;
        }
        [HttpGet("/user/GetRooms")]
        //[Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetRooms([FromQuery] string q = "", [FromQuery] string uname = "")
        {
            GetRoomsRequest req = new GetRoomsRequest
            {
                Query = q,
                UserName = uname
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetRoomsUseCase.handleAsync(req, _GetRoomsResponse);
            return _GetRoomsResponse.jsonContentResult;
        }
        [HttpGet("/user/GetRoomsPublic")]
        public async Task<IActionResult> GetRoomsPublic([FromQuery] string q = "", [FromQuery] string username = null)
        {
            GetRoomsPublicRequest req = new GetRoomsPublicRequest
            {
                Query = q,
                UserName = username
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetRoomsPublicUseCase.handleAsync(req, _GetRoomsPublicResponse);
            return _GetRoomsPublicResponse.jsonContentResult;
        }

        [HttpGet("/user/GetLivesPublic")]
        public async Task<IActionResult> GetLivesPublic([FromQuery] string q = "")
        {
            GetLivesPublicRequest req = new GetLivesPublicRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetLivesPublicUseCase.handleAsync(req, _GetLivesPublicResponse);
            return _GetLivesPublicResponse.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpGet("/user/GetLives")]
        public async Task<IActionResult> GetLives([FromQuery] string q = "")
        {
            GetLivesRequest req = new GetLivesRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetLivesUseCase.handleAsync(req, _GetLivesResponse);
            return _GetLivesResponse.jsonContentResult;
        }
        [HttpGet("/user/GetLivesOfProfile")]
        public async Task<IActionResult> GetLivesOfProfile([FromQuery] string q = "")
        {
            GetLivesOfProfileRequest req = new GetLivesOfProfileRequest
            {
                Query = q
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetLivesOfProfileUseCase.handleAsync(req, _GetLivesOfProfileResponse);
            return _GetLivesOfProfileResponse.jsonContentResult;
        }
        [HttpGet("/user/GetLiveWithID")]
        public async Task<IActionResult> GetLiveWithID([FromQuery] string idRoom = "")
        {
            GetLiveWithIDRequest req = new GetLiveWithIDRequest
            {
                ID = idRoom
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetLiveWithIDUseCase.handleAsync(req, _GetLiveWithIDResponse);
            return _GetLiveWithIDResponse.jsonContentResult;
        }
        [HttpGet("/user/GetMembersOfMeet")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetMembersOfMeet([FromQuery] string idMeet = "")
        {
            GetMembersOfMeetRequest req = new GetMembersOfMeetRequest
            {
                ID = idMeet
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetMembersOfMeetUseCase.handleAsync(req, _GetMembersOfMeetResponse);
            return _GetMembersOfMeetResponse.jsonContentResult;
        }
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpGet("/user/GetRoomCalendarActivity")]
        public async Task<IActionResult> GetRoomCalendarActivity([FromQuery] string friendlyID = "", [FromQuery] string month = "", [FromQuery] int year=1405)
        {
            GetRoomCalendarActivityRequest req = new GetRoomCalendarActivityRequest
            {
                FriendlyID = friendlyID,
                Month=month,
                Year = year
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetRoomCalendarActivityUseCase.handleAsync(req, _GetRoomCalendarActivityResponse);
            return _GetRoomCalendarActivityResponse.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpPost("/user/StartNewMeet/{IDRoom}")]
        public async Task<IActionResult> StartNewMeet(StartMeetingViewModel vm)
        {
            StartNewMeetRequest req = _mapper.Map<StartNewMeetRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IStartNewMeetUseCase.handleAsync(req, _StartNewMeetResponse);
            return _StartNewMeetResponse.jsonContentResult;
        }

        [HttpGet("/user/GetMeetingURL")]
        public async Task<IActionResult> GetMeetingURL(string nick, string meetid, bool modir, string pass, string nationalCode = null, string phoneNumber = null)
        {   
            GetMeetingURLRequest req = new GetMeetingURLRequest
            {
                FullName = nick,
                MeetiID = meetid,
                AsModerator = modir,
                Password = pass,
                NationalCode = nationalCode,
                PhoneNumber = phoneNumber
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetMeetingURLUseCase.handleAsync(req, _GetMeetingURLResponse);
            return _GetMeetingURLResponse.jsonContentResult;
        }


        [HttpPost("/user/NewRoom")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewRoom(NewRoomViewModel vm)
        {
            NewRoomRequest req = _mapper.Map<NewRoomRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _INewRoomUseCase.handleAsync(req, _NewRoomResponse);
            return _NewRoomResponse.jsonContentResult;
        }


        [HttpPost("/user/GenerateCertificate")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> GenerateCertificate(GenerateCertificateViewModel vm)
        {
            GenerateCertificateRequest req = _mapper.Map<GenerateCertificateRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGenerateCertificateUseCase.handleAsync(req, _GenerateCertificateResponse);
            return _GenerateCertificateResponse.jsonContentResult;
        }
        [HttpPost("/user/GuarantCertificate")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> GuarantCertificate(GuarantCertificateViewModel vm)
        {
            GuarantCertificateRequest req = _mapper.Map<GuarantCertificateRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            Statics.Log("GuarantCertificate", "GeneralHeaders22222" + JsonConvert.SerializeObject(req.GeneralHeaders));
            Statics.Log("GuarantCertificate", "GeneralHeaders22222" + (req.GeneralHeaders.JWTMeetingToken));

            await _IGuarantCertificateUseCase.handleAsync(req, _GuarantCertificateResponse);
            return _GuarantCertificateResponse.jsonContentResult;
        }
        [HttpPost("/user/EndTheMeeting")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> EndTheMeeting(EndTheMeetingViewModel vm)
        {
            EndTheMeetingRequest req = _mapper.Map<EndTheMeetingRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            //Statics.Log("EndTheMeeting", "GeneralHeaders22222" + JsonConvert.SerializeObject(req.GeneralHeaders));
            //Statics.Log("EndTheMeeting", "GeneralHeaders22222" + (req.GeneralHeaders.JWTMeetingToken));

            await _IEndTheMeetingUseCase.handleAsync(req, _EndTheMeetingResponse);
            return _EndTheMeetingResponse.jsonContentResult;
        }
        [HttpPost("/user/EndTheMeetingWithAPI")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> EndTheMeetingWithAPI(EndTheMeetingWithAPIViewModel vm)
        {
            EndTheMeetingWithAPIRequest req = _mapper.Map<EndTheMeetingWithAPIRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            //Statics.Log("EndTheMeetingWithAPI", "GeneralHeaders22222" + JsonConvert.SerializeObject(req.GeneralHeaders));
            //Statics.Log("EndTheMeetingWithAPI", "GeneralHeaders22222" + (req.GeneralHeaders.JWTMeetingToken));

            await _IEndTheMeetingWithAPIUseCase.handleAsync(req, _EndTheMeetingWithAPIResponse);
            return _EndTheMeetingWithAPIResponse.jsonContentResult;
        }
        [HttpPost("/user/NewQuestion")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewQuestion(NewQuestionViewModel vm)
        {
            NewQuestionRequest req = _mapper.Map<NewQuestionRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            Statics.Log("NewQuestion", "GeneralHeaders22222" + JsonConvert.SerializeObject(req.GeneralHeaders));
            Statics.Log("NewQuestion", "GeneralHeaders22222" + (req.GeneralHeaders.JWTMeetingToken));

            await _INewQuestionUseCase.handleAsync(req, _NewQuestionResponse);
            return _NewQuestionResponse.jsonContentResult;
        }
        [HttpGet("/user/GetQuestionOfMeet")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> GetQuestionOfMeet()
        {
            GetQuestionOfMeetRequest req = new GetQuestionOfMeetRequest { };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            Statics.Log("GetQuestionOfMeet", "GeneralHeaders22222" + JsonConvert.SerializeObject(req.GeneralHeaders));
            Statics.Log("GetQuestionOfMeet", "GeneralHeaders22222" + (req.GeneralHeaders.JWTMeetingToken));

            await _IGetQuestionOfMeetUseCase.handleAsync(req, _GetQuestionOfMeetResponse);
            return _GetQuestionOfMeetResponse.jsonContentResult;
        }
        [HttpPost("/user/QuestionAnswered/{id}")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> QuestionAnswered([FromRoute] long id)
        {
            QuestionAnsweredRequest req = new QuestionAnsweredRequest { ID = id };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            Statics.Log("QuestionAnswered", "GeneralHeaders22222" + JsonConvert.SerializeObject(req.GeneralHeaders));
            Statics.Log("QuestionAnswered", "GeneralHeaders22222" + (req.GeneralHeaders.JWTMeetingToken));

            await _IQuestionAnsweredUseCase.handleAsync(req, _QuestionAnsweredResponse);
            return _QuestionAnsweredResponse.jsonContentResult;
        }

        [HttpPost("/user/NewUserMeetingLog")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUserMeetingLog(NewUserMeetingLogViewModel vm)
        {
            NewUserMeetingLogRequest req = _mapper.Map<NewUserMeetingLogRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _INewUserMeetingLogUseCase.handleAsync(req, _NewUserMeetingLogResponse);
            return _NewUserMeetingLogResponse.jsonContentResult;
        }


        [HttpPost("/user/NewUserMeetingLogFromModerator")]
        [EnableCors("AllowHekmateSubdomains")]
        public async Task<IActionResult> NewUserMeetingLogFromModerator(NewUserMeetingLogFromModeratorViewModel vm)
        {
            NewUserMeetingLogFromModeratorRequest req = _mapper.Map<NewUserMeetingLogFromModeratorRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _INewUserMeetingLogFromModeratorUseCase.handleAsync(req, _NewUserMeetingLogFromModeratorResponse);
            return _NewUserMeetingLogFromModeratorResponse.jsonContentResult;
        }

        //[Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpPost("/user/SignInUserPanel")]
        public async Task<IActionResult> SignInUserPanel(SignInUserPanelViewModel vm)
        {
            var headers = HttpContext.GetGeneralHeaders();
            SignInUserPanelRequest req = _mapper.Map<SignInUserPanelRequest>(vm);
            req.GeneralHeaders = headers;
            await _ISignInUserPanelUseCase.handleAsync(req, _SignInUserPanelResponse);
            return _SignInUserPanelResponse.jsonContentResult;
        }



        [HttpPost("/user/LoginUserPanel")]
        public async Task<IActionResult> LoginUserPanel(LoginUserPanelViewModel vm)
        {
            LoginUserPanelRequest req = _mapper.Map<LoginUserPanelRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _ILoginUserPanelUseCase.handleAsync(req, _LoginUserPanelResponse);
            return _LoginUserPanelResponse.jsonContentResult;
        }


        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpPost("/user/VerifyToken")]
        public async Task<IActionResult> VerifyToken()
        {
            //VerifyTokenRequest req = _mapper.Map<VerifyTokenRequest>();
            //req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            var head = HttpContext.GetGeneralHeaders();
            await _IVerifyTokenUseCase.handleAsync(new VerifyTokenRequest { GeneralHeaders = head }, _VerifyTokenResponse);
            return _VerifyTokenResponse.jsonContentResult;
        }

        [HttpPost("/user/NewPlanRoom")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> NewPlanRoom(NewPlanRoomViewModel vm)
        {
            NewPlanRoomRequest req = _mapper.Map<NewPlanRoomRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();

            await _INewPlanRoomUseCase.handleAsync(req, _NewPlanRoomResponse);
            return _NewPlanRoomResponse.jsonContentResult;
        }
    
        [HttpGet("/user/GetPlanRoom")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetPlanRoom([FromQuery] string friendlyID = "")
        {
            Statics.Log("GetPlanRoom", string.Join("\n", Request.Headers));
            Statics.Log("GetPlanRoom", string.Join("\n", Request.Headers.Select(e => e.Key).ToList()));
            GetPlanRoomRequest req = new GetPlanRoomRequest
            {
                FriendlyID = friendlyID
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetPlanRoomUseCase.handleAsync(req, _GetPlanRoomResponse);
            return _GetPlanRoomResponse.jsonContentResult;
        }
        [HttpGet("/user/GetPlanRoomPublic")]
        public async Task<IActionResult> GetPlanRoomPublic()
        {
            GetPlanRoomPublicRequest req = new GetPlanRoomPublicRequest
            {
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetPlanRoomPublicUseCase.handleAsync(req, _GetPlanRoomPublicResponse);
            return _GetPlanRoomPublicResponse.jsonContentResult;
        }
        [HttpGet("/user/GetLatestMeetings")]
        public async Task<IActionResult> GetLatestMeetings()
        {
            GetLatestMeetingsRequest req = new GetLatestMeetingsRequest
            {
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetLatestMeetingsUseCase.handleAsync(req, _GetLatestMeetingsResponse);
            return _GetLatestMeetingsResponse.jsonContentResult;
        }
        [HttpGet("/user/GetPollList")]
        public async Task<IActionResult> GetPollList()
        {
            GetPollListRequest req = new GetPollListRequest
            {
            };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetPollListUseCase.handleAsync(req, _GetPollListResponse);
            return _GetPollListResponse.jsonContentResult;
        }
  
        [HttpPost("/user/AnswerPollList")]
        public async Task<IActionResult> AnswesrPollList(AnswerPollListViewModel vm)
        {
            AnswerPollListRequest req = _mapper.Map<AnswerPollListRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();

            await _IAnswerPollListUseCase.handleAsync(req, _AnswerPollListResponse);
            return _AnswerPollListResponse.jsonContentResult;
        }

        [HttpPost("/user/UpdatePresentMinToCertOfMeet")]
        public async Task<IActionResult> UpdatePresentMinToCertOfMeet(UpdatePresentMinToCertOfMeetViewModel vm)
        {
            UpdatePresentMinToCertOfMeetRequest req = _mapper.Map<UpdatePresentMinToCertOfMeetRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();

            await _IUpdatePresentMinToCertOfMeetUseCase.handleAsync(req, _UpdatePresentMinToCertOfMeetResponse);
            return _UpdatePresentMinToCertOfMeetResponse.jsonContentResult;
        }



        [HttpPost("/File/UploadFileStorage")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [DisableRequestSizeLimit] // غیرفعال کردن محدودیت حجم
        [RequestFormLimits(
        MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024,
        ValueLengthLimit = int.MaxValue,
        MultipartHeadersLengthLimit = int.MaxValue)]

        public async Task<IActionResult> UploadFileStorage(UploadFileStorageViewModel vm)
        {
            UploadFileStorageRequest req = _mapper.Map<UploadFileStorageRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();

            await _IUploadFileStorageUseCase.handleAsync(req, _UploadFileStorageResponse);
            return _UploadFileStorageResponse.jsonContentResult;
        }











        [HttpDelete("/user/DeletePlanRoom/{idplan}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> DeletePlanRoom(long idplan)
        {
            try
            {
                // Validate input
                if ((idplan <= 0))
                {
                    return BadRequest(new { error = "idplan is required" });
                }

                Statics.Log("DeletePlanRoom", string.Join("\n", Request.Headers));
                Statics.Log("DeletePlanRoom", string.Join("\n", Request.Headers.Select(e => e.Key).ToList()));

                DeletePlanRoomRequest req = new DeletePlanRoomRequest
                {
                    IDPlan = idplan
                };
                req.GeneralHeaders = HttpContext.GetGeneralHeaders();

                await _IDeletePlanRoomUseCase.handleAsync(req, _DeletePlanRoomResponse);

                return _DeletePlanRoomResponse.jsonContentResult;
            }
            catch (Exception ex)
            {
                Statics.Log("DeletePlanRoom Error", ex.Message);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        [HttpGet("/File/GetFileStorage")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetFileStorage([FromQuery] GetFileStorageVM vm)
        {
            GetFileStorageRequest req = _mapper.Map<GetFileStorageRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetFileStorageUseCase.handleAsync(req, _GetFileStorageResponse);
            return _GetFileStorageResponse.jsonContentResult;
        }
        [HttpGet("/File/GetSingleFile")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetSingleFile([FromQuery] GetSingleFileVM vm)
        {
            GetSingleFileRequest req = _mapper.Map<GetSingleFileRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IGetSingleFileUseCase.handleAsync(req, _GetSingleFileResponse);
            return _GetSingleFileResponse.jsonContentResult;
        }




    }


}