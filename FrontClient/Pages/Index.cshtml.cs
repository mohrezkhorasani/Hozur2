using Application.DTO;
using Application.Interface.IUsecase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Presenter;

namespace FrontClient.Pages
{
    public class IndexModel : PageModel
    {
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
        public IGetRoomListUseCase _IGetRoomListUseCase { get; }
        public ApiPresenter<GetRoomListResponse> _GetRoomListResponse { get; }
        public IGetLivesUseCase _IGetLivesUseCase { get; }
        public ApiPresenter<GetLivesResponse> _GetLivesResponse { get; }
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

        public INewQuestionUseCase _INewQuestionUseCase { get; }
        public ApiPresenter<NewQuestionResponse> _NewQuestionResponse { get; }

        public IGetQuestionOfMeetUseCase _IGetQuestionOfMeetUseCase { get; }
        public ApiPresenter<GetQuestionOfMeetResponse> _GetQuestionOfMeetResponse { get; }
        public IQuestionAnsweredUseCase _IQuestionAnsweredUseCase { get; }
        public ApiPresenter<QuestionAnsweredResponse> _QuestionAnsweredResponse { get; }
        public bool LoadingStartMeet = false;
        public string link = null;
        public IndexModel(INewUserUseCase iNewUserUseCase, ApiPresenter<NewUserResponse> newUserResponse, INewMeetingUseCase iNewMeetingUseCase, ApiPresenter<NewMeetingResponse> newMeetingResponse, IUpdateMeetingUseCase iUpdateMeetingUseCase, ApiPresenter<UpdateMeetingResponse> updateMeetingResponse, INewUserMettingsUseCase iNewUserMettingsUseCase, ApiPresenter<NewUserMettingsResponse> newUserMettingsResponse, INewUserMeetingLogUseCase iNewUserMeetingLogUseCase, ApiPresenter<NewUserMeetingLogResponse> newUserMeetingLogResponse, INewUserMeetingLogFromModeratorUseCase iNewUserMeetingLogFromModeratorUseCase, ApiPresenter<NewUserMeetingLogFromModeratorResponse> newUserMeetingLogFromModeratorResponse, IGetTeacherListUseCase iGetTeacherListUseCase, ApiPresenter<GetTeacherListResponse> getTeacherListResponse, IGetRoomListUseCase iGetRoomListUseCase, ApiPresenter<GetRoomListResponse> GetRoomListResponse, IGetLivesUseCase iGetLivesUseCase, ApiPresenter<GetLivesResponse> getLivesResponse, IStartNewMeetUseCase iStartNewMeetUseCase, ApiPresenter<StartNewMeetResponse> startNewMeetResponse, IGetMeetingURLUseCase iGetMeetingURLUseCase, ApiPresenter<GetMeetingURLResponse> getMeetingURLResponse, IGetRoomByFriendlyIDUseCase iGetRoomByFriendlyIDUseCase, ApiPresenter<GetRoomByFriendlyIDResponse> getRoomByFriendlyIDResponse, INewRoomUseCase iNewRoomUseCase, ApiPresenter<NewRoomResponse> newRoomResponse, IGenerateCertificateUseCase iGenerateCertificateUseCase, ApiPresenter<GenerateCertificateResponse> generateCertificateResponse, IGuarantCertificateUseCase iGuarantCertificateUseCase, ApiPresenter<GuarantCertificateResponse> guarantCertificateResponse, IEndTheMeetingUseCase iEndTheMeetingUseCase, ApiPresenter<EndTheMeetingResponse> endTheMeetingResponse, INewQuestionUseCase iNewQuestionUseCase, ApiPresenter<NewQuestionResponse> newQuestionResponse, IGetQuestionOfMeetUseCase iGetQuestionOfMeetUseCase, ApiPresenter<GetQuestionOfMeetResponse> getQuestionOfMeetResponse, IQuestionAnsweredUseCase iQuestionAnsweredUseCase, ApiPresenter<QuestionAnsweredResponse> questionAnsweredResponse)
        {
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
            _IGetRoomListUseCase = iGetRoomListUseCase;
            _GetRoomListResponse = GetRoomListResponse;
            _IGetLivesUseCase = iGetLivesUseCase;
            _GetLivesResponse = getLivesResponse;
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
            _INewQuestionUseCase = iNewQuestionUseCase;
            _NewQuestionResponse = newQuestionResponse;
            _IGetQuestionOfMeetUseCase = iGetQuestionOfMeetUseCase;
            _GetQuestionOfMeetResponse = getQuestionOfMeetResponse;
            _IQuestionAnsweredUseCase = iQuestionAnsweredUseCase;
            _QuestionAnsweredResponse = questionAnsweredResponse;
        }
        public async Task<IActionResult> OnPostStartMeeting(string title,string RoomID)
        {
            LoadingStartMeet = true;

            await _IStartNewMeetUseCase.handleAsync(new StartNewMeetRequest
            {
                IDRoom = RoomID,
                MeetTitle=title,
                Teachers = new List<string>(),
                RequiredToEnterName=true,
                
            }, _StartNewMeetResponse);
            
            string join = ((StartNewMeetResponse)_StartNewMeetResponse.jsonContentResult.obj).RedirectURL;
            Console.WriteLine(join);
            link = join;
            
            return Redirect(link);
        }

        public async Task<IActionResult> StartMeeting(string RoomID, string Title)
        {
            // Your logic here

            // Example:
            // await _service.StartMeetingAsync(RoomID, Title);
            return null;
        }   
        public async Task OnGet()
        {
            if (Request.Cookies["bbbToken"] == null)
            {
                Response.Redirect("/Login");
            }
            await _IGetRoomListUseCase.handleAsync(new GetRoomListRequest
            {
                q=""
            }, _GetRoomListResponse);
            
        }
    }
}
