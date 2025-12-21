using Application.DTO;
using Application.Interface.IUsecase;
using Microsoft.AspNetCore.Mvc;
using Presenter;

namespace CoreMe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HandleNavigationController : ControllerBase
    {
        public IGetRedirectURLUseCase _IGetRedirectURLUseCase { get; }
        public ApiPresenter<GetRedirectURLResponse> _GetRedirectURLResponse { get; }

        public HandleNavigationController(IGetRedirectURLUseCase iGetRedirectURLUseCase, ApiPresenter<GetRedirectURLResponse> getRedirectURLResponse)
        {
            _IGetRedirectURLUseCase = iGetRedirectURLUseCase;
            _GetRedirectURLResponse = getRedirectURLResponse;
        }

        [HttpGet("/room/{roomID}")]
        public async Task<IActionResult> RedirectExample([FromRoute] string roomID)
        {
            await _IGetRedirectURLUseCase.handleAsync(new GetRedirectURLRequest { RoomID=roomID}, _GetRedirectURLResponse);
            //return .jsonContentResult;

            return Redirect((_GetRedirectURLResponse.jsonContentResult.obj as GetRedirectURLResponse).Link);
        }
    }
}