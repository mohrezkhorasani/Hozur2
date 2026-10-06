using Application.DTO;
using Application.General;
using Application.Interface.IUsecase;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CoreMe.Presenter;
using CoreMe.VM;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Presenter;
using Presistence.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public IBackupDBUseCase _IBackupDBUseCase { get; }
        public ApiPresenter<BackupDBResponse> _BackupDBResponse { get; }
        private readonly DBContext _db;
        private readonly IPaymentPDFGenerator pdf;
        private readonly IWebHostEnvironment _environment;

        public TaskController(ILogger<TaskController> logger, IWebHostEnvironment hosting, ICronJobCheckLiveMeetingTaskUseCase iCronJobCheckLiveMeetingTaskUseCase, ApiPresenter<CronJobCheckLiveMeetingTaskResponse> cronJobCheckLiveMeetingTaskResponse, IBackupDBUseCase iBackupDBUseCase, ApiPresenter<BackupDBResponse> backupDBResponse, DBContext db, IPaymentPDFGenerator pdf, IWebHostEnvironment environment)
        {
            _logger = logger;
            _hosting = hosting;
            _ICronJobCheckLiveMeetingTaskUseCase = iCronJobCheckLiveMeetingTaskUseCase;
            _CronJobCheckLiveMeetingTaskResponse = cronJobCheckLiveMeetingTaskResponse;
            _IBackupDBUseCase = iBackupDBUseCase;
            _BackupDBResponse = backupDBResponse;
            _db = db;
            this.pdf = pdf;
            _environment = environment;
        }

        [HttpGet("/user/CronJobCheckLiveMeetingTask")]
        public async Task<IActionResult> CronJobCheckLiveMeetingTask()
        {
            return Ok("");
            await _ICronJobCheckLiveMeetingTaskUseCase.handleAsync(new CronJobCheckLiveMeetingTaskRequest(), _CronJobCheckLiveMeetingTaskResponse);
            return _CronJobCheckLiveMeetingTaskResponse.jsonContentResult;
        }
        [HttpPost("/user/BackupDB")]
        public async Task<IActionResult> BackupDB()
        {
            await _IBackupDBUseCase.handleAsync(new BackupDBRequest(), _BackupDBResponse);
            return _BackupDBResponse.jsonContentResult;
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

        [HttpPost("addNewPayment")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]

        public IActionResult AddNewPayment(AddNewPaymentVM vm)
        {
            var h = HttpContext.GetGeneralHeaders();
            var p = new Domain.Entity.PaymentTBL
            {
                amount = vm.amount,
                date = GetDateWithZiro(vm.date),
                documentNumber = vm.documentNumber,
                bankName = vm.bankName,
                paymentType = vm.paymentType,
                netAmount = vm.netAmount,
                FriendlyID = Statics.GenerateSecretKey(64).Replace("\\", "]").Replace("/", "["),
                source = vm.source,
                subject = vm.subject,
                tax = vm.taxPercent,
                to = vm.to,
            };
            string pdfpath = "";
            if (h.UserTBL.VerifyUserPermissions("pay_request"))
            {

                _db.PaymentTBLs.Add(p);
                _db.SaveChanges();
                pdfpath = pdf.CreatePaymentPDF(
                  pdfPath: "pdf",
                  fontPath: $"{Path.Combine(_environment.WebRootPath, "fonts/Iranian-Sans.ttf")}",
                  Name: $"{p.FriendlyID}.pdf",
                  amount: p.amount,
                  tax: (decimal)p.tax,
                  to: p.to,
                  subject: p.subject,
                  source: p.source,
                  documentNumber: p.documentNumber,
                  dateInsert: p.date,
                  friendlyID: p.FriendlyID,
                  bankName: p.bankName,
                  paymentType: p.paymentType
              );

            }
            return Ok(new
            {
                status = 200,
                message = "ok",
                data = p,
                path = pdfpath,
            });
        }
        [SwaggerIgnore]
        public string GetDateWithZiro(string date)
        {
            var l = date.Split("/");
            var d1 = "";
            var d2 = "";
            var d3 = "";
            int c = 0;
            foreach (var i in l)
            {
                if (c == 2)
                {
                    if (i.Length < 2)
                        d3 = $"0{i}";
                    else
                        d3 = i;
                }
                if (c == 1)
                {
                    if (i.Length < 2)
                        d2 = $"0{i}";
                    else
                        d2 = i;

                }
                if (c == 0)
                {
                    d1 = i;

                }

                c++;
            }
            return $"{d1}/{d2}/{d3}";

        }
        [HttpPost("ChangeStatusOfPayment")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]

        public IActionResult ChangeStatusOfPayment(ChangeStatusOfPaymentVM vm)
        {
            var h = HttpContext.GetGeneralHeaders();
            if (h.UserTBL.VerifyUserPermissions("pay_request"))
            {
                var c = _db.PaymentTBLs.FirstOrDefault(e => e.ID == vm.ID);
                if (c == null)
                    return Ok(new
                    {
                        status = 404,
                        message = "not found"
                    });
                c.Status = vm.Status;
                _db.Update(c);
                _db.SaveChanges();
            }
            return Ok(new
            {
                status = 200,
                message = "ok"
            });
        }
        [HttpPost("GetPaymentList")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]

        public IActionResult GetPaymentList()
        {
            List<PaymentTBL> outpot = new List<PaymentTBL>();
            var h = HttpContext.GetGeneralHeaders();
            if (h.UserTBL.VerifyUserPermissions("pay_request"))
            {
                outpot = _db.PaymentTBLs.OrderByDescending(e => e.ID).ToList();
            }
            return Ok(new
            {
                status = 200,
                message = "ok",
                data = outpot
            });
        }
        [HttpGet("GetPDFOfPayment")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]

        public IActionResult GetPDFOfPayment(long id)
        {
            List<PaymentTBL> outpot = new List<PaymentTBL>();
            var h = HttpContext.GetGeneralHeaders();
            string pdfpath = "";
            if (h.UserTBL.VerifyUserPermissions("pay_request"))
            {
                var p = _db.PaymentTBLs.FirstOrDefault(e => e.ID == id);
                pdfpath = pdf.CreatePaymentPDF(
                  pdfPath: $"pdf",
                  fontPath: $"{Path.Combine(_environment.WebRootPath, "fonts/Iranian-Sans.ttf")}",
                  Name: $"{p.FriendlyID+(DateTime.Now.Ticks)}.pdf",
                  amount: p.amount,
                  tax: (decimal)p.tax,
                  to: p.to,
                  subject: p.subject,
                  source: p.source,
                  documentNumber: p.documentNumber,
                  dateInsert: p.date,
                  friendlyID: p.FriendlyID,
                  bankName: p.bankName,
                  paymentType: p.paymentType
              );

            }
            return Ok(new
            {
                status = 200,
                message = "ok",
                path = pdfpath
            });
        }

        [HttpPost("EditPayment")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public IActionResult EditPayment(EditPaymentVM vm)
        {
            var h = HttpContext.GetGeneralHeaders();
            if (h.UserTBL.VerifyUserPermissions("pay_request"))
            {
                var item = _db.PaymentTBLs.FirstOrDefault(e => e.ID == vm.ID);
                if (item == null)
                {
                    return Ok(new { status = 404, message = "not found" });
                }

                if (vm.amount.HasValue) item.amount = vm.amount.Value;
                if (vm.subject != null) item.subject = vm.subject;
                if (vm.source != null) item.source = vm.source;
                if (vm.date != null) item.date = GetDateWithZiro(vm.date);
                if (vm.taxPercent.HasValue) item.tax = vm.taxPercent.Value;
                
                if (vm.netAmount.HasValue) {
                    item.netAmount = vm.netAmount.Value;
                } else if (vm.amount.HasValue || vm.taxPercent.HasValue) {
                    item.netAmount = item.amount - (long)((decimal)item.amount * (decimal)item.tax / 100m);
                }

                if (vm.documentNumber != null) item.documentNumber = vm.documentNumber;
                if (vm.bankName != null) item.bankName = vm.bankName;
                if (vm.paymentType != null) item.paymentType = vm.paymentType;
                if (vm.to != null) item.to = vm.to;

                _db.PaymentTBLs.Update(item);
                _db.SaveChanges();

                return Ok(new
                {
                    status = 200,
                    message = "ok",
                    data = item
                });
            }

            return Ok(new { status = 403, message = "permission denied" });
        }
    }

    public class EditPaymentVM
    {
        public long ID { get; set; }
        public long? amount { get; set; }
        public string? subject { get; set; }
        public string? source { get; set; }
        public string? date { get; set; }
        public float? taxPercent { get; set; }
        public long? netAmount { get; set; }
        public string? documentNumber { get; set; }
        public string? bankName { get; set; }
        public string? paymentType { get; set; }
        public string? to { get; set; }
    }

    public class ChangeStatusOfPaymentVM
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
    }

    public class AddNewPaymentVM
    {
        public long amount { get; set; }
        public string subject { get; set; }
        public string source { get; set; }
        public string date { get; set; }
        public float taxPercent { get; set; }
        public long netAmount { get; set; }
        public string documentNumber { get; set; }
        public string? bankName { get; set; }
        public string? paymentType { get; set; }
        public string to { get; set; }
    }


}











    public class ChangeStatusOfPaymentVM
    {
        public long ID { get; set; }
        public MainEntity_Status Status { get; set; }
    }

    public class AddNewPaymentVM
    {
        public long amount { get; set; }
        public string subject { get; set; }
        public string source { get; set; }
        public string date { get; set; }
        public float taxPercent { get; set; }
        public long netAmount { get; set; }
        public string documentNumber { get; set; }
        public string? bankName { get; set; }
        public string? paymentType { get; set; }
        public string to { get; set; }
    }

