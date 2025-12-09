using Application.DTO;
using Application.Interface.IUsecase;
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

        public MeetController(IGetRoomsUseCase iGetRoomsUseCase, ApiPresenter<GetRoomsResponse> getRoomsResponse)
        {
            _IGetRoomsUseCase = iGetRoomsUseCase;
            _GetRoomsResponse = getRoomsResponse;
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
        public async Task<IActionResult> GetMeetings([FromRoute(Name ="roomid")] string roomid, [FromQuery] int countPerPage, [FromRoute(Name = "page")] int page) 
        {
            throw new NotImplementedException();
        }

        [HttpGet("{meetid}/GetMeetingsInfo")]
        public async Task<IActionResult> GetMeetingsInfo([FromRoute] string meetid)
        {
            throw new NotImplementedException();
            //start time
            //end time
            //onvan
            //teachers
            //count of hozar
            //count of certificates
        }

        [HttpGet("{meetid}/GetMeetingsInfo/Members")]
        public async Task<IActionResult> GetMeetingsInfoMembers([FromRoute] string meetid)
        {
            throw new NotImplementedException();
            //list of members
            //each
            //name
            //nationalCode
            //phone
            //email
            //userToMeetID
            //time-stay
            //CanTakeCertificate
            //HasTakenCertificate
        }
        [HttpGet("{meetid}/GetMeetingsInfo/Members/Certificate/GetOnlyCertificateds")]
        public async Task<IActionResult> GetMeetingsInfoMembersGetOnlyCertificated([FromRoute] string meetid)
        {
            throw new NotImplementedException();
            //list of user certificateds
        }
        [HttpGet("{meetid}/GetMeetingsInfo/Members/Certificate/GenerateCertificate")]
        public async Task<IActionResult> GetMeetingsInfoMembersGenerateCertificate([FromRoute] string meetid, [FromQuery] string userToMeetID)
        {
            throw new NotImplementedException();
            //pdf Link To download
        }
        [HttpPost("{meetid}/GetMeetingsInfo/Members/Certificate/SearchUser")]
        public async Task<IActionResult> GetMeetingsInfoMembersSearch([FromRoute] string meetid,[FromQuery] string q)
        {
            throw new NotImplementedException();
            //can be anything- name, userToMeetID, email, phone,...
        }
        [HttpGet("{meetid}/GetMeetingsInfo/Members/ExportExcell")]
        public async Task<IActionResult> GetMeetingsInfoMembersExportExcell([FromRoute] string meetid)
        {
            throw new NotImplementedException();
            //Excell Link To download
        }



    }
}
