using Application.DTO;
using Application.Interface.IUsecase;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using NajmBridge.Cofigurations;
using NajmBridge.Presenter;

namespace NajmBridge.Controllers
{
    [RequireApiToken]
    [ApiController]
    [Route("[controller]")]
    public class MeetController : ControllerBase
    {
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

        public MeetController(IGetRoomsUseCase iGetRoomsUseCase, ApiPresenter<GetRoomsResponse> getRoomsResponse, IGetMeetingFromRoomIDUseCase iGetMeetingFromRoomIDUseCase, ApiPresenter<GetMeetingFromRoomIDResponse> getMeetingFromRoomIDResponse, IGetMeetInfoUseCase iGetMeetInfoUseCase, ApiPresenter<GetMeetInfoResponse> getMeetInfoResponse, IGetMembersOfMeetUseCase iGetMembersOfMeetUseCase, ApiPresenter<GetMembersOfMeetResponse> getMembersOfMeetResponse, IGetMembersOfMeetOnlyCertificatedUseCase iGetMembersOfMeetOnlyCertificatedUseCase, ApiPresenter<GetMembersOfMeetOnlyCertificatedResponse> getMembersOfMeetOnlyCertificatedResponse, IGenerateCertificateWithUserMeetIDUseCase iGenerateCertificateWithUserMeetIDUseCase, ApiPresenter<GenerateCertificateWithUserMeetIDResponse> generateCertificateWithUserMeetIDResponse)
        {
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
        }

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

        [HttpGet("/Certificate/GenerateCertificate")]
        public async Task<IActionResult> GetMeetingsInfoMembersGenerateCertificate([FromQuery] long userid,  [FromQuery] string nationalcode, [FromQuery] bool isMale)
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

        [HttpPost("{meetid}/GetMeetingsInfo/Members/Certificate/SearchUser")]
        public async Task<IActionResult> GetMeetingsInfoMembersSearch([FromRoute] string meetid, [FromQuery] string q)
        {
            throw new NotImplementedException();
            //can be anything- name, userToMeetID, email, phone,...
        }

        [HttpGet("{meetid}/GetMeetingsInfo/Members/{idMember}")]
        public async Task<IActionResult> GetMeetingsInfoMemberByID([FromRoute] string meetid, [FromRoute] long idMember)
        {
            throw new NotImplementedException();
            //info of user
        }
     


        
        [HttpGet("{meetid}/GetMeetingsInfo/Members/ExportExcell")]
        public async Task<IActionResult> GetMeetingsInfoMembersExportExcell([FromRoute] string meetid)
        {
            throw new NotImplementedException();
            //Excell Link To download
        }



    }
}
