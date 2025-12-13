using Application.DTO;
using Application.Interface.IUsecase;
using Microsoft.AspNetCore.Mvc;
using NajmBridge.Presenter;

namespace NajmBridge.Controllers
{
    public class MemberController : Controller
    {
        public IGetMemberByIDUseCase _IGetMemberByIDUseCase { get; }
        public ApiPresenter<GetMemberByIDResponse> _GetMemberByIDResponse { get; }
        public IGetAllMeetingMemberUseCase _IGetAllMeetingMemberUseCase { get; }
        public ApiPresenter<GetAllMeetingMemberResponse> _GetAllMeetingMemberResponse { get; }
        public IGetAllCertificateOfMemberUseCase _IGetAllCertificateOfMemberUseCase { get; }
        public ApiPresenter<GetAllCertificateOfMemberResponse> _GetAllCertificateOfMemberResponse { get; }

        public MemberController(IGetMemberByIDUseCase iGetMemberByIDUseCase, ApiPresenter<GetMemberByIDResponse> getMemberByIDResponse, IGetAllMeetingMemberUseCase iGetAllMeetingMemberUseCase, ApiPresenter<GetAllMeetingMemberResponse> getAllMeetingMemberResponse, IGetAllCertificateOfMemberUseCase iGetAllCertificateOfMemberUseCase, ApiPresenter<GetAllCertificateOfMemberResponse> getAllCertificateOfMemberResponse)
        {
            _IGetMemberByIDUseCase = iGetMemberByIDUseCase;
            _GetMemberByIDResponse = getMemberByIDResponse;
            _IGetAllMeetingMemberUseCase = iGetAllMeetingMemberUseCase;
            _GetAllMeetingMemberResponse = getAllMeetingMemberResponse;
            _IGetAllCertificateOfMemberUseCase = iGetAllCertificateOfMemberUseCase;
            _GetAllCertificateOfMemberResponse = getAllCertificateOfMemberResponse;
        }



        /// <summary>
        /// دریافت اطلاعات کامل یک شرکت کننده مشخص با شناسه شرکت کننده در جلسه
        /// </summary>
        /// <param name="idMember">شناسه شرکت کننده در جلسه (long)</param>
        /// <returns></returns>
        [HttpGet("/Members/{idMember}")]
        public async Task<IActionResult> GetMemberByID([FromRoute] long idMember= 18649892)
        {
            GetMemberByIDRequest req = new GetMemberByIDRequest
            {
                ID = idMember
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetMemberByIDUseCase.handleAsync(req, _GetMemberByIDResponse);
            return _GetMemberByIDResponse.jsonContentResult;
        }
        /// <summary>
        /// دریافت لیست تمام جلسات یک شرکت کننده
        /// </summary>
        /// <param name="idMember">شناسه شرکت کننده اینجا به صورت رشته ارسال شود</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("/Members/{idMember}/GetMeetings")]
        public async Task<IActionResult> GetMemberMeeting([FromRoute] string idMember= "18649983", [FromQuery] int countPerPage = 20, [FromRoute(Name = "page")] int page = 0)
        {
            GetAllMeetingMemberRequest req = new GetAllMeetingMemberRequest
            {
                ID = idMember,
                Count= countPerPage,
                Page=page
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetAllMeetingMemberUseCase.handleAsync(req, _GetAllMeetingMemberResponse);
            return _GetAllMeetingMemberResponse.jsonContentResult;
        }
        /// <summary>
        /// دریافت لیست تمامی گواهی نامه های دریافت شده شرکت کننده
        /// </summary>
        /// <param name="idMember">شناسه شرکت کننده اینجا به صورت رشته ارسال شود</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpGet("/Members/{idMember}/GetCertificates")]
        public async Task<IActionResult> GetMemberCertificates([FromRoute] string idMember= "18649983", [FromQuery] int countPerPage = 20, [FromRoute(Name = "page")] int page = 0)
        {
            GetAllCertificateOfMemberRequest req = new GetAllCertificateOfMemberRequest
            {
                ID = idMember,
                Count = countPerPage,
                Page = page
            };
            req.GeneralHeaders = GeneralHeader.GetGeneralHeaders(Request);
            await _IGetAllCertificateOfMemberUseCase.handleAsync(req, _GetAllCertificateOfMemberResponse);
            return _GetAllCertificateOfMemberResponse.jsonContentResult;

        }
    }
}
