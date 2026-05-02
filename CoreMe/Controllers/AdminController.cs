using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IUsecase;
using AutoMapper;
using CoreMe.Presenter;
using CoreMe.VM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Presenter;
using System.Runtime.InteropServices.Marshalling;

namespace CoreMe.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
    [EnableCors("AllowHekmateSubdomains")]
    [Route("admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<TaskController> _logger;
        private readonly IWebHostEnvironment _hosting;
        public IAdminGetUserListUseCase _IAdminGetUserListUseCase { get; }
        public ApiPresenter<AdminGetUserListResponse> _AdminGetUserListResponse { get; }
        public IAdminGetUserRoomListUseCase _IAdminGetUserRoomListUseCase { get; }
        public ApiPresenter<AdminGetUserRoomListResponse> _AdminGetUserRoomListResponse { get; }
        public IAdminCreateRoomUseCase _IAdminCreateRoomUseCase { get; }
        public ApiPresenter<AdminCreateRoomResponse> _AdminCreateRoomResponse { get; }

        public IAdminChangeRoleUseCase _IAdminChangeRoleUseCase { get; }
        public ApiPresenter<AdminChangeRoleResponse> _AdminChangeRoleResponse { get; }
      public IChangeAvatarOfRoomUseCase _IChangeAvatarOfRoomUseCase { get; }
        public ApiPresenter<ChangeAvatarOfRoomResponse> _ChangeAvatarOfRoomResponse { get; }
        public IInsertSlideShowUseCase _IInsertSlideShowUseCase { get; }
        public ApiPresenter<InsertSlideShowResponse> _InsertSlideShowResponse { get; }
        public IEditSlideShowUseCase _IEditSlideShowUseCase { get; }
        public ApiPresenter<EditSlideShowResponse> _EditSlideShowResponse { get; }

        public AdminController(IMapper mapper, ILogger<TaskController> logger, IWebHostEnvironment hosting, IAdminGetUserListUseCase iAdminGetUserListUseCase, ApiPresenter<AdminGetUserListResponse> adminGetUserListResponse, IAdminGetUserRoomListUseCase iAdminGetUserRoomListUseCase, ApiPresenter<AdminGetUserRoomListResponse> adminGetUserRoomListResponse, IAdminCreateRoomUseCase iAdminCreateRoomUseCase, ApiPresenter<AdminCreateRoomResponse> adminCreateRoomResponse, IAdminChangeRoleUseCase iAdminChangeRoleUseCase, ApiPresenter<AdminChangeRoleResponse> adminChangeRoleResponse, IChangeAvatarOfRoomUseCase iChangeAvatarOfRoomUseCase, ApiPresenter<ChangeAvatarOfRoomResponse> changeAvatarOfRoomResponse, IInsertSlideShowUseCase iInsertSlideShowUseCase, ApiPresenter<InsertSlideShowResponse> insertSlideShowResponse, IEditSlideShowUseCase iEditSlideShowUseCase, ApiPresenter<EditSlideShowResponse> editSlideShowResponse)
        {
            _mapper = mapper;
            _logger = logger;
            _hosting = hosting;
            _IAdminGetUserListUseCase = iAdminGetUserListUseCase;
            _AdminGetUserListResponse = adminGetUserListResponse;
            _IAdminGetUserRoomListUseCase = iAdminGetUserRoomListUseCase;
            _AdminGetUserRoomListResponse = adminGetUserRoomListResponse;
            _IAdminCreateRoomUseCase = iAdminCreateRoomUseCase;
            _AdminCreateRoomResponse = adminCreateRoomResponse;
            _IAdminChangeRoleUseCase = iAdminChangeRoleUseCase;
            _AdminChangeRoleResponse = adminChangeRoleResponse;
            _IChangeAvatarOfRoomUseCase = iChangeAvatarOfRoomUseCase;
            _ChangeAvatarOfRoomResponse = changeAvatarOfRoomResponse;
            _IInsertSlideShowUseCase = iInsertSlideShowUseCase;
            _InsertSlideShowResponse = insertSlideShowResponse;
            _IEditSlideShowUseCase = iEditSlideShowUseCase;
            _EditSlideShowResponse = editSlideShowResponse;
        }

        [HttpGet("/user/AdminGetUserList")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminGetUserList(int page, int count, OrderBy o, string? q)
        {
            var req = new AdminGetUserListRequest
            {
                GeneralHeaders = HttpContext.GetGeneralHeaders(),
                Count = count,
                Page = page,
                OrderBy = o,
                Query = q
            };
            await _IAdminGetUserListUseCase.handleAsync(req, _AdminGetUserListResponse);
            return _AdminGetUserListResponse.jsonContentResult;
        }
        [HttpGet("/user/AdminGetUserRoomList")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminGetUserRoomList(int page, int count, OrderBy o, long uid, string? q)
        {
            await _IAdminGetUserRoomListUseCase.handleAsync(new AdminGetUserRoomListRequest
            {
                GeneralHeaders = HttpContext.GetGeneralHeaders(),
                Count = count,
                Page = page,
                OrderBy = o,
                OwnerID = uid,
                Query = q
            }, _AdminGetUserRoomListResponse);
            return _AdminGetUserRoomListResponse.jsonContentResult;
        }
        [HttpPost("/user/AdminCreateRoom")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminCreateRoom(AdminCreateRoomVM vm)
        {
            AdminCreateRoomRequest req = _mapper.Map<AdminCreateRoomRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IAdminCreateRoomUseCase.handleAsync(req, _AdminCreateRoomResponse);
            return _AdminCreateRoomResponse.jsonContentResult;
        }
        [HttpPost("/user/AdminChangeRole")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> AdminChangeRole(AdminChangeRoleVM vm)
        {
            AdminChangeRoleRequest req = _mapper.Map<AdminChangeRoleRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IAdminChangeRoleUseCase.handleAsync(req, _AdminChangeRoleResponse);
            return _AdminChangeRoleResponse.jsonContentResult;
        }
        [HttpPost("/user/ChangeAvatarOfRoom")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> ChangeAvatarOfRoom(ChangeAvatarOfRoomVM vm)
        {
            ChangeAvatarOfRoomRequest req = _mapper.Map<ChangeAvatarOfRoomRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IChangeAvatarOfRoomUseCase.handleAsync(req, _ChangeAvatarOfRoomResponse);
            return _ChangeAvatarOfRoomResponse.jsonContentResult;
        }

        [HttpPost("/user/InsertSlideShow")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> InsertSlideShow(InsertSlideShowVM vm)
        {
            InsertSlideShowRequest req = _mapper.Map<InsertSlideShowRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IInsertSlideShowUseCase.handleAsync(req, _InsertSlideShowResponse);
            return _InsertSlideShowResponse.jsonContentResult;
        }

        [HttpPost("/user/EditSlideShow")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> EditSlideShow(EditSlideShowVM vm)
        {
            EditSlideShowRequest req = _mapper.Map<EditSlideShowRequest>(vm);
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await _IEditSlideShowUseCase.handleAsync(req, _EditSlideShowResponse);
            return _EditSlideShowResponse.jsonContentResult;
        }










    }


}
