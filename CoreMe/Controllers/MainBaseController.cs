using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IUsecase;
using AutoMapper;
using CoreMe.VM;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Presenter;

namespace CoreMe.Controllers
{
    [ApiController]
    [Route("task")]
    public class MainBaseController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly IWebHostEnvironment _hosting;
        private GeneralHeaders _generalHeaders;
        private readonly IGeneralHeadersService _headersService;

        protected MainBaseController(IGeneralHeadersService headersService)
        {
            _headersService = headersService;
        }

        protected async Task<GeneralHeaders> GetHeadersAsync()
        {
            if (_generalHeaders == null)
            {
                _generalHeaders = await _headersService.GetHeadersAsync(HttpContext);
            }
            return _generalHeaders;
        }




    }














}
