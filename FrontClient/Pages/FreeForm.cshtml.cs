using Application.DTO;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presenter;
using System.Web;

namespace FrontClient.Pages
{
    public class FreeFormModel : PageModel
    {
        public class StartMeetingRequest
        {
            public string Title { get; set; }
        }
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
        public IBBBGetMeetings bbb { get; }
        public bool LoadingStartMeet = false;
        public string link = null;
        public List<RoomTBL> Rooms =null;
        public IDBContext _db { get; }
        public FreeFormModel(IDBContext db,INewUserUseCase iNewUserUseCase, ApiPresenter<NewUserResponse> newUserResponse, INewMeetingUseCase iNewMeetingUseCase, ApiPresenter<NewMeetingResponse> newMeetingResponse, IUpdateMeetingUseCase iUpdateMeetingUseCase, ApiPresenter<UpdateMeetingResponse> updateMeetingResponse, INewUserMettingsUseCase iNewUserMettingsUseCase, ApiPresenter<NewUserMettingsResponse> newUserMettingsResponse, INewUserMeetingLogUseCase iNewUserMeetingLogUseCase, ApiPresenter<NewUserMeetingLogResponse> newUserMeetingLogResponse, INewUserMeetingLogFromModeratorUseCase iNewUserMeetingLogFromModeratorUseCase, ApiPresenter<NewUserMeetingLogFromModeratorResponse> newUserMeetingLogFromModeratorResponse, IGetTeacherListUseCase iGetTeacherListUseCase, ApiPresenter<GetTeacherListResponse> getTeacherListResponse, IGetRoomListUseCase iGetRoomListUseCase, ApiPresenter<GetRoomListResponse> getRoomListResponse, IGetLivesUseCase iGetLivesUseCase, ApiPresenter<GetLivesResponse> getLivesResponse, IStartNewMeetUseCase iStartNewMeetUseCase, ApiPresenter<StartNewMeetResponse> startNewMeetResponse, IGetMeetingURLUseCase iGetMeetingURLUseCase, ApiPresenter<GetMeetingURLResponse> getMeetingURLResponse, IGetRoomByFriendlyIDUseCase iGetRoomByFriendlyIDUseCase, ApiPresenter<GetRoomByFriendlyIDResponse> getRoomByFriendlyIDResponse, INewRoomUseCase iNewRoomUseCase, ApiPresenter<NewRoomResponse> newRoomResponse, IGenerateCertificateUseCase iGenerateCertificateUseCase, ApiPresenter<GenerateCertificateResponse> generateCertificateResponse, IGuarantCertificateUseCase iGuarantCertificateUseCase, ApiPresenter<GuarantCertificateResponse> guarantCertificateResponse, IEndTheMeetingUseCase iEndTheMeetingUseCase, ApiPresenter<EndTheMeetingResponse> endTheMeetingResponse, INewQuestionUseCase iNewQuestionUseCase, ApiPresenter<NewQuestionResponse> newQuestionResponse, IGetQuestionOfMeetUseCase iGetQuestionOfMeetUseCase, ApiPresenter<GetQuestionOfMeetResponse> getQuestionOfMeetResponse, IQuestionAnsweredUseCase iQuestionAnsweredUseCase, ApiPresenter<QuestionAnsweredResponse> questionAnsweredResponse, IBBBGetMeetings bbb)
        {
            _db = db;
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
            _GetRoomListResponse = getRoomListResponse;
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
            this.bbb = bbb;
        }

        public async Task<IActionResult> OnPostStartMeeting(string Title)
        {
            LoadingStartMeet = true;
            string random = Statics.GenerateSecretKey(16);
            random = "hekmateislami";
            var Domain = $"{Request.Scheme}://{Request.Host}";


            var t = await bbb.CreateMeetingAsync("hekmateislami", Title, "r75-spv-fwr-g2t");
            var join = await bbb.GetJoinUrlAsync("hekmateislami", "مهمان", false, Statics.BBB_ATTEND_PW);
            
            //Console.WriteLine(join);
            //link = join.Replace("&", "%26");
            return new JsonResult (new
            {
                url= $"{Domain}/Joinpage?mid={random}&admin=1",
                join= $"{join}"
            });
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
            var r = _db.RoomTBLs.Where(e=>e.Type==MainEntity_Types.ROOM_NORMAL).ToList();
            this.Rooms = r;
        }   
    }
}
