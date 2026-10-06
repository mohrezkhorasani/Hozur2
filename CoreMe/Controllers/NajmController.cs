using Application.DTO;
using Application.General;
using Application.Interface.IUsecase;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CoreMe.Presenter;
using CoreMe.VM;
using Domain.Entity;
using Infrastructure.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Presenter;
using Presistence.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace CoreMe.Controllers
{
    [ApiController]
    [Route("task")]
    public class NajmController : ControllerBase
    {
        private readonly ILogger<NajmController> _logger;
        private readonly IWebHostEnvironment _hosting;
        public ICronJobCheckLiveMeetingTaskUseCase _ICronJobCheckLiveMeetingTaskUseCase { get; }
        public ApiPresenter<CronJobCheckLiveMeetingTaskResponse> _CronJobCheckLiveMeetingTaskResponse { get; }
        public IBackupDBUseCase _IBackupDBUseCase { get; }
        public ApiPresenter<BackupDBResponse> _BackupDBResponse { get; }
        private readonly DBContext _db;
        private readonly IPaymentPDFGenerator pdf;
        private readonly IWebHostEnvironment _environment;
        private readonly CommonMethods _common;
        public NajmController(ILogger<NajmController> logger, IConfiguration configuration, IWebHostEnvironment hosting, ICronJobCheckLiveMeetingTaskUseCase iCronJobCheckLiveMeetingTaskUseCase, ApiPresenter<CronJobCheckLiveMeetingTaskResponse> cronJobCheckLiveMeetingTaskResponse, IBackupDBUseCase iBackupDBUseCase, ApiPresenter<BackupDBResponse> backupDBResponse, DBContext db, IPaymentPDFGenerator pdf, IWebHostEnvironment environment)
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
            _common = new CommonMethods(configuration, _db, hosting);
        }

        [HttpGet("GetAttendsOfMeet")]
        public IActionResult GetAttendsOfMeet(string meetID)
        {
            if (!_db.MeetingTBLs.Any(e => e.MeetID == meetID))
            {
                return NotFound();
            }
            if (!_db.UserTBLToMeetingTBLs.Any(e => e.MeetID == meetID))
            {
                return NotFound();
            }
            var meet = _db.MeetingTBLs.Where(e => e.MeetID == meetID)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.MeetingTBL)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLLogs)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.CertificateTBL)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLMuiltiJoinTBLs)
                .FirstOrDefault();
            var attends = meet.UserTBLToMeetingTBLs.Select(e => new
            {
                userMeetID = e.ID,
                userID = e.UserTBL.ID,
                userUUID = e.UserTBL.UUID,
                isMember = e.IsMember,
                timeToStay = _common.GetTimerStayUserInMeet(e),
                isRemoved = e.IsRemoved,
                e.NationalCode,
                e.Phone,
                hasCertificate = _common.CheckCertificate(e),
                certificate = _common.GetCertificate(e),
                multiJoinID = _common.GetMultiJoins(e),
                logs = e.UserTBLToMeetingTBLLogs.Select(r => new
                {
                    ket = r.Key,
                    value = r.Value,
                    r.UserMeetID,
                    date = r.DateInsert.ToString("yyyy/MM/dd"),
                    time = r.DateInsert.ToString("HH:mm:ss zzz")

                }).ToList(),
                e.Role,
            });
            return Ok(new
            {
                //meetings = meetings,
                status = 200,
                message = "ok"
            });
        }
        [HttpGet("GetMeetingByID")]
        public IActionResult GetMeetingByID(string meetID)
        {
            if (!_db.MeetingTBLs.Any(e => e.MeetID == meetID))
            {
                return NotFound();
            }
            var meet = _db.MeetingTBLs.Where(e => e.MeetID == meetID)
                .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                .Include(e => e.RoomTBLPlanTBL)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                .Include(e => e.MeetPollQuizTBLs).ThenInclude(e => e.MeetPollQuizAnsTBLs).ThenInclude(e => e.MeetPollQuizAnsTBLToUserMeetTBLs)
                ;

            var meetData = meet.Select(e => new
            {
                e.MeetID,
                end = e.EndDate,
                start = e.DateInsert,
                e.Title,
                countPartis = e.UserTBLToMeetingTBLs.Count(r => r.IsTypeOfNormalUser()),
                e.Status,
                room = new
                {
                    e.RoomTBL.Title,
                    e.RoomTBL.ID,
                    e.RoomTBL.FriendlyID,
                    e.RoomTBL.IsPublicRoom,
                    e.RoomTBL.IsPrivateRoom,
                    e.RoomTBL.IsSuperPrivateRoom,
                    owner = new
                    {
                        e.RoomTBL.OwnerTBL.ID,
                        name = e.RoomTBL.OwnerTBL.GetName(),
                        e.RoomTBL.OwnerTBL.Username,
                        e.RoomTBL.OwnerTBL.Email,
                        role = e.RoomTBL.OwnerTBL.GetRole(),
                    }
                },
                teachers = e.UserTBLToMeetingTBLs.Any(r => r.Type == MainEntity_Types.USER_TEACHER) ? e.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).Select(r => new
                {
                    userIDMeet = r.ID,
                    name = r.UserTBL.GetName(),
                    userID = r.UserTBL.ID,
                }).ToList() : null,
                dabir = e.UserTBLToMeetingTBLs.Any(r => r.Type == MainEntity_Types.USER_DABIR) ? e.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_DABIR).Select(r => new
                {
                    userIDMeet = r.ID,
                    name = r.UserTBL.GetName(),
                    userID = r.UserTBL.ID,
                }).ToList() : null,
                minMinutesTimeToCertificate = e.PresentMinutesToCert,
                e.JWT,
                e.JalaliDate,
                e.IsRecorded,
                e.RequiredToEnterName,
                poll = e.MeetPollQuizTBLs.Select(r => new
                {
                    r.Question,
                    answers = r.MeetPollQuizAnsTBLs.Select(a => new
                    {
                        a.Answer,
                        a.ID,
                        a.Score,
                        result = a.MeetPollQuizAnsTBLToUserMeetTBLs.Select(ma => new
                        {
                            ma.IDUserTBLToMeetingTBL,
                            ma.IDMeetPollQuizAnsTBL,
                            ma.ID,
                            ma.Content,
                        })
                    })
                }).ToList()


            }).ToList();

            return Ok(new
            {
                data = meetData,
                status = 200,
                message = "ok"
            });
        }
        [HttpGet("GetLatestMeeting")]
        public IActionResult GetLatestMeeting(
            [FromQuery][DefaultValue(25)] int count = 25,
            [FromQuery][DefaultValue(1)] int page = 1,
            [FromQuery][DefaultValue("")] string q = "",
            [FromQuery][DefaultValue(0)] long idroom = 0,
            [FromQuery][DefaultValue("1405-خرداد-09")] string datejalali = "1405-خرداد-09",
            [FromQuery][DefaultValue("2025-11-15")] DateOnly? fromDate = null,
            [FromQuery][DefaultValue("2027-12-20")] DateOnly? toDate = null
            ) //datejalali= 1405-تیر-28 
              // 1405-تیر-28
        {
            page = page - 1;
            if (page < 0) page = 0;
            var meetings = _db.MeetingTBLs.Where(e => (e.Title.Contains(q) || e.RoomTBL.Title.Contains(q)) &&
            (idroom == 0 || idroom == e.RoomTBL.ID) &&
            (e.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL) &&
            (datejalali == null || e.JalaliDate == datejalali) &&
            (!fromDate.HasValue || e.DateInsert >= fromDate.Value.ToDateTime(TimeOnly.MinValue)) &&
            (!toDate.HasValue || e.DateInsert <= toDate.Value.ToDateTime(TimeOnly.MinValue))

            ).OrderByDescending(e => e.ID).Skip(page * count).Take(count)
                .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                .Include(e => e.RoomTBLPlanTBL)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                .Select(e => new
                {
                    e.MeetID,
                    end = e.EndDate,
                    start = e.DateInsert,
                    e.Title,
                    countPartis = e.UserTBLToMeetingTBLs.Count(r => r.Type != MainEntity_Types.USER_TEACHER && r.Type != MainEntity_Types.USER_DABIR && r.Type != MainEntity_Types.USER_ADMINSTRATOR && r.Type != MainEntity_Types.USER_MODERATOR),
                    e.Status,
                    room = new
                    {
                        e.RoomTBL.Title,
                        e.RoomTBL.ID,
                        e.RoomTBL.FriendlyID,
                        e.RoomTBL.IsPublicRoom,
                        e.RoomTBL.IsPrivateRoom,
                        e.RoomTBL.IsSuperPrivateRoom,
                        owner = new
                        {
                            e.RoomTBL.OwnerTBL.ID,
                            name = e.RoomTBL.OwnerTBL.GetName(),
                            e.RoomTBL.OwnerTBL.Username,
                            e.RoomTBL.OwnerTBL.Email,
                            role = e.RoomTBL.OwnerTBL.GetRole(),
                        }
                    },
                    teachers = e.UserTBLToMeetingTBLs.Any(r => r.Type == MainEntity_Types.USER_TEACHER) ? e.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).Select(r => new
                    {
                        userIDMeet = r.ID,
                        name = r.UserTBL.GetName(),
                        userID = r.UserTBL.ID,
                    }).ToList() : null,
                    dabir = e.UserTBLToMeetingTBLs.Any(r => r.Type == MainEntity_Types.USER_DABIR) ? e.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_DABIR).Select(r => new
                    {
                        userIDMeet = r.ID,
                        name = r.UserTBL.GetName(),
                        userID = r.UserTBL.ID,
                    }).ToList() : null


                }).ToList();



            return Ok(new
            {
                meetings = meetings,
                status = 200,
                message = "ok"
            });
        }
        //[SwaggerIgnore]
        [HttpGet("insertUsers")]
        public async Task<IActionResult> insertUsers()
        {
            //string filePath = Path.Combine(_hosting.ContentRootPath, "MajmaMemberVIP.json");

            //var phones = _db.UserTBLs.Where(e => !string.IsNullOrEmpty(e.PhoneNumber) && e.IsMemberVIP).ToList();
            //foreach (var p in phones)
            //{
            //    var phon = p.PhoneNumber;
            //    var validate = PhoneValidator.Verify(phon);
            //    if (validate.IsValid)
            //    {
            //        p.PhoneNumber = validate.NormalizedPhoneNumber;
            //        _db.Update(p);
            //    }
            //}
            //_db.SaveChanges();
            //return Ok();
            string filePath = Path.Combine(_hosting.ContentRootPath, "majmaMemberVIPFromNajm.json");
            if (!System.IO.File.Exists(filePath))
                return NotFound("فایل پیدا نشد");
            string text = await System.IO.File.ReadAllTextAsync(filePath);
            var members = System.Text.Json.JsonSerializer.Deserialize<List<MemberVip>>(text);
            string respond = "";
            foreach (var i in members)
            {
                if (string.IsNullOrEmpty(i.phone1))
                    i.phone1 = null;
                if (string.IsNullOrEmpty(i.email1))
                    i.email1 = null;
                int c = 0;
                if (i.phone1 == null && i.phone2 == null)
                    continue;
                if (i.phone1 == null)
                    i.phone1 = "0";
                if (i.phone2 == null)
                    i.phone2 = "0";

                string email = i.email1 ?? i.email2;
                var validate = PhoneValidator.Verify(i.phone1);
                var validate2 = PhoneValidator.Verify(i.phone2);
                if (!validate2.IsValid && !validate.IsValid)
                    continue;
                i.phone1 = validate.NormalizedPhoneNumber;
                i.phone2 = validate2.NormalizedPhoneNumber;
                if (_db.UserTBLs.Any(r => (r.PhoneNumber == i.phone1 || r.PhoneNumber == i.phone2)))
                {
                    Console.WriteLine(++c + "", "already added ", i.lname);
                    var user = await _db.UserTBLs.FirstOrDefaultAsync(r => (r.PhoneNumber == i.phone1 || r.PhoneNumber == i.phone2));
                    if (_db.UserTBLs.Any(e => e.NationalCode == i.nationalCode))
                    {
                        var ru = _db.UserTBLs.First(e => e.NationalCode == i.nationalCode);
                        respond += $"{ru.Firstname} {ru.LastName} id-{ru.ID}\n";
                    }
                    else
                    {
                        user.NationalCode = i.nationalCode;
                    }
                    user.NajmID = i.id;
                    user.LastUpdate = DateTime.UtcNow;
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        if (!_db.UserTBLs.Any(e => e.Email == email))
                        {
                            user.Email = email;
                        }
                        else
                        {
                            respond += $"email({email}) existed";
                        }
                    }
                    _db.SaveChanges();

                    continue;

                }
                respond += $"{i.phone1} - {i.phone2} - {i.id} \n";
                Console.WriteLine(i.phone1, i.phone2, i.id);
                continue;
                _db.UserTBLs.Add(new UserTBL
                {
                    Type = MainEntity_Types.USER_USER,
                    IsMemberVIP = true,
                    Firstname = i.fname,
                    LastName = i.lname,
                    PhoneNumber = string.IsNullOrEmpty(i.phone1) ? null : i.phone1,
                    Email = string.IsNullOrEmpty(i.email1) || i.email1.Contains("ندارد")
                    || i.email1.Contains("ایمیل") ? null : i.email1,
                    Status = MainEntity_Status.USER_ACTIVE,
                    UUID = IdGenerator.GenerateId(2),
                    IPList = new List<string>(),
                    ExtId = new List<string>(),
                    IsHekmatShobe = false,
                    AllowedGUID = new List<Guid>()
                });
                _db.SaveChanges();
            }


            return Ok(respond);
        }

    }
    public class MemberVip
    {
        public string id { get; set; }
        public string fname { get; set; }
        public string lname { get; set; }
        public string phone1 { get; set; }
        public string phone2 { get; set; }
        public string nationalCode { get; set; }
        public string email1 { get; set; }
        public string email2 { get; set; }
        // سایر فیلدها
    }
}
