using Application.DTO;
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
    public class TaskController : ControllerBase
    {
        private readonly ILogger<TaskController> _logger;
        private readonly IWebHostEnvironment _hosting;
        public ICronJobCheckLiveMeetingTaskUseCase _ICronJobCheckLiveMeetingTaskUseCase { get; }
        public ApiPresenter<CronJobCheckLiveMeetingTaskResponse> _CronJobCheckLiveMeetingTaskResponse { get; }

        public TaskController(ILogger<TaskController> logger, IWebHostEnvironment hosting, ICronJobCheckLiveMeetingTaskUseCase iCronJobCheckLiveMeetingTaskUseCase, ApiPresenter<CronJobCheckLiveMeetingTaskResponse> cronJobCheckLiveMeetingTaskResponse)
        {
            _logger = logger;
            _hosting = hosting;
            _ICronJobCheckLiveMeetingTaskUseCase = iCronJobCheckLiveMeetingTaskUseCase;
            _CronJobCheckLiveMeetingTaskResponse = cronJobCheckLiveMeetingTaskResponse;
        }

        [HttpPost("/user/CronJobCheckLiveMeetingTask")]
        public async Task<IActionResult> CronJobCheckLiveMeetingTask()
        {
            await _ICronJobCheckLiveMeetingTaskUseCase.handleAsync(new CronJobCheckLiveMeetingTaskRequest(), _CronJobCheckLiveMeetingTaskResponse);
            return _CronJobCheckLiveMeetingTaskResponse.jsonContentResult;
        }


        [HttpGet("cleanup-certificates")]
        public IActionResult CleanupCertificates()
        {
            // مسیر فولدر
            var folderPath = Path.Combine(_hosting.WebRootPath, "pdf", "certificates");

            if (!Directory.Exists(folderPath))
            {
                return NotFound("Folder does not exist.");
            }

            var files = Directory.GetFiles(folderPath);
            int deletedCount = 0;

            foreach (var file in files)
            {
                try
                {
                    var creationTime = System.IO.File.GetCreationTimeUtc(file);
                    if (creationTime < DateTime.UtcNow.AddHours(-24))
                    {
                        System.IO.File.Delete(file);
                        deletedCount++;
                    }
                }
                catch
                {
                    // اگر فایلی قابل حذف نبود، رد می‌کنیم
                }
            }

            return Ok($"{deletedCount} file(s) deleted.");
        }
    }














}
