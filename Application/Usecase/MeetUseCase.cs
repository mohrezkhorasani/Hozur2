using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using CacheHandler.Events;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Application.Usecase
{
    #region NewRoomDashboard
    public class NewRoomDashboardUseCase : INewRoomDashboardUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewRoomDashboardUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewRoomDashboardRequest message,
            IOutPutPort<GenericResponse<NewRoomDashboardResponse>> outputPort)
        {
            var init = await _commonMethods.GetUser<NewRoomDashboardRequest>(message);
            var user = init.Item1;
            var acc = init.Item2;
            if (acc.Any(e => e.Route == message.GeneralHeaders.RequestedPage))
            {
                string id = FriendlyIDGenerator.Generate();
                while (true)
                {
                    if (!_db.RoomTBLs.Any(e => e.FriendlyID == id))
                        break;
                    id = FriendlyIDGenerator.Generate();
                }
                RoomTBL room = new RoomTBL
                {
                    Title = message.GeneralHeaders.RequestedPage,
                    FriendlyID = id,
                    IDOwnerTBL = user.ID,
                    Status = MainEntity_Status.Enabled,
                    Type = MainEntity_Types.ROOM_NORMAL,
                };
                if (message.ID != null)
                {
                    room = await _db.RoomTBLs.FindAsync(message.ID);
                    if (room != null)
                    {
                        room.Title = message.Title;
                        _db.Update(room);
                    }
                    else
                    {
                        _db.SaveChanges();
                        outputPort.Handle(new GenericResponse<NewRoomDashboardResponse>
        (new NewRoomDashboardResponse
        {
            Message = "گروه نیافتیده شد",
            Status = 403,

        }));
                        return;
                    }
                }
                else
                {
                    _db.RoomTBLs.Add(room);
                }


                _db.SaveChanges();
                outputPort.Handle(new GenericResponse<NewRoomDashboardResponse>
(new NewRoomDashboardResponse
{
    Message = "اضافه شد",
    Status = 200,

}));
                return;
            }
            outputPort.Handle(new GenericResponse<NewRoomDashboardResponse>
       (new NewRoomDashboardResponse
       {
           Message = "ok",
           Status = 403,

       }));

        }
    }
    #endregion

    #region GetMeetingFromRoomID
    public class GetMeetingFromRoomIDUseCase : IGetMeetingFromRoomIDUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public GetMeetingFromRoomIDUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetMeetingFromRoomIDRequest message,
            IOutPutPort<GenericResponse<GetMeetingFromRoomIDResponse>> outputPort)
        {
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;
            long id = 0;
            if (long.TryParse(message.ID, out id))
            {

            }
            if (_db.MeetingTBLs.Any(e => e.RoomFriendlyID == message.ID || e.ID == id))
            {
                var room = _db.RoomTBLs.First(e => e.FriendlyID == message.ID || e.ID == id);
                var meets = _db.MeetingTBLs
                    .Where(e => e.RoomFriendlyID == message.ID &&
                        e.Type != MainEntity_Types.MEET_IGNORE &&
                        e.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL
                        )
                    .OrderByDescending(e => e.ID)
                    .Skip(message.Page.Value * message.Count.Value)
                    .Take(message.Count.Value)
                    .Include(e => e.RoomTBL)
                    .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                    .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.CertificateTBL)
                    .Include(e => e.Creator)
                    .Include(e => e.Teacher)
                    .ToList();

                outputPort.Handle(new GenericResponse<GetMeetingFromRoomIDResponse>(
                    new GetMeetingFromRoomIDResponse
                    {
                        Message = "ok",
                        Status = 200,
                        Data = meets.Select(e => new SharedOBJ_Meeting
                        {
                            CountPartis = e.UserTBLToMeetingTBLs?.Count() ?? 0,
                            ID = e.MeetID,
                            EndDate = Statics.GetPersianDate(
                                e.EndDate.HasValue
                                    ? e.EndDate.Value
                                    : e.DateInsert.AddHours(2)
                            ),

                            StartDate = Statics.GetPersianDate(e.DateInsert),

                            Starter = new SharedOBJ_Partis
                            {
                                ID = e.Creator?.ID ?? 0,
                                // اگر سازنده در لیست شرکت‌کننده‌ها نبود یا لیست خالی بود
                                Name = e.UserTBLToMeetingTBLs?
                                       .FirstOrDefault(r => r.UserTBL?.ID == e.Creator?.ID)?.UserNickName
                                       ?? e.Creator?.Username
                                       ?? "نامشخص",

                                Timer = "0"
                            },
                            ShortLink = e.RoomTBL?.TTR_URL,
                            Teachers = e.UserTBLToMeetingTBLs?
                                .Where(x => x.Type == MainEntity_Types.USER_TEACHER && x.UserTBL != null)
                                .Select(r => new SharedOBJ_Partis
                                {
                                    ID = r.ID,
                                    Name = $"{r.UserTBL.Firstname ?? ""} {r.UserTBL.LastName ?? ""}".Trim(),
                                    Timer = "0"
                                })
                                .ToList() ?? new List<SharedOBJ_Partis>() // اگر null بود، لیست خالی برگردون
                        }).ToList()
                    }));
                return;
            }



            outputPort.Handle(new GenericResponse<GetMeetingFromRoomIDResponse>
       (new GetMeetingFromRoomIDResponse
       {
           Message = "ok",
           Status = 403,

       }));

        }
    }
    #endregion

    #region GetMeetInfo
    public class GetMeetInfoUseCase : IGetMeetInfoUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public GetMeetInfoUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetMeetInfoRequest message,
            IOutPutPort<GenericResponse<GetMeetInfoResponse>> outputPort)
        {
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;
            long meetid = 0;
            if (long.TryParse(message.ID, out meetid))
            {

            }
            if (_db.MeetingTBLs.Any(e => e.MeetID == message.ID || e.ID == meetid))
            {
                var m = _db.MeetingTBLs
                    .Where(e => e.MeetID == message.ID || e.ID == meetid)
                    .Include(e => e.RoomTBL)
                    .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                    .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.CertificateTBL)
                    .Include(e => e.Creator)
                    .Include(e => e.Teacher)
                    .First();

                outputPort.Handle(new GenericResponse<GetMeetInfoResponse>(
                    new GetMeetInfoResponse
                    {
                        Message = "ok",
                        Status = 200,
                        Meet = new SharedOBJ_MeetingInfo
                        {
                            ID = m.MeetID,
                            CountCertificateds = m.UserTBLToMeetingTBLs.Count(r => r.CertificateTBL?.FilePath != null),
                            CountPartis = m.UserTBLToMeetingTBLs.Count(e => e.Type != MainEntity_Types.USER_TEACHER),
                            StartDate = Statics.GetPersianDate(m.DateInsert),
                            EndDate = m.EndDate.HasValue ? Statics.GetPersianDate(m.EndDate.Value) : m.Status == MainEntity_Status.MEETING_LIVE || m.Status == MainEntity_Status.MEETING_STARTED_LATER ? "در حال پخش" : "خاتمه یافته",
                            Teachers = m.UserTBLToMeetingTBLs?
                                .Where(x => x.Type == MainEntity_Types.USER_TEACHER && x.UserTBL != null)
                                .Select(r => new SharedOBJ_Partis
                                {
                                    ID = r.ID,
                                    Name = $"{r.UserTBL.Firstname ?? ""} {r.UserTBL.LastName ?? ""}".Trim(),
                                    Timer = "0"
                                })
                                .ToList() ?? new List<SharedOBJ_Partis>(),
                            Title = m.Title,
                            Room = new SharedOBJ_Partis
                            {
                                ID = m.RoomTBL.ID,
                                Name = m.RoomTBL.Title,
                                Timer = ""
                            }
                        },

                    }));
                return;
            }



            outputPort.Handle(new GenericResponse<GetMeetInfoResponse>
       (new GetMeetInfoResponse
       {
           Message = "ok",
           Status = 403,

       }));

        }
    }
    #endregion

    #region GetMembersOfMeet
    public class GetMembersOfMeetUseCase : IGetMembersOfMeetUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public GetMembersOfMeetUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetMembersOfMeetRequest message,
            IOutPutPort<GenericResponse<GetMembersOfMeetResponse>> outputPort)
        {

            if (!_db.MeetingTBLs.Any(e => e.RoomTBL.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.MeetID == message.ID) &&
                               !message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_admins"))
            {
                outputPort.Handle(new GenericResponse<GetMembersOfMeetResponse>(new GetMembersOfMeetResponse
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                }));
                return;
            }

            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;
            long id = 0;
            if (long.TryParse(message.ID, out id))
            {

            }

            if (_db.MeetingTBLs.Any(e => e.MeetID == message.ID || e.ID == id))
            {
                var m = _db.UserTBLToMeetingTBLs
                    .Where(e => e.MeetID == message.ID || e.ID == id)
                    .Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL)
                    .ToList();

                outputPort.Handle(new GenericResponse<GetMembersOfMeetResponse>(
                    new GetMembersOfMeetResponse
                    {
                        Message = "ok",
                        Status = 200,
                        Members = m.Select(e =>
                        {
                            // پیش‌فرض مقادیر
                            string device = "";
                            string browser = "نامشخص";
                            string os = "نامشخص";

                            // استخراج از IPList (یا هر فیلد دیگه‌ای که لاگ‌ها رو داره)
                            if (e.UserTBLToMeetingTBLLogs.Count > 0 && string.IsNullOrEmpty(e.UserTBLToMeetingTBLLogs.First().OS))
                            {
                                var lines = new List<string>();
                                if (!string.IsNullOrEmpty(e.UserTBLToMeetingTBLLogs.First().OS))
                                {
                                    lines = e.UserTBLToMeetingTBLLogs.First().OS
                                      .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(l => l.Trim())
                                      .Where(l => l.Contains("/$$")).ToList();
                                }


                                var firstValidLine = lines.FirstOrDefault(); // یا LastOrDefault() برای آخرین دستگاه

                                if (!string.IsNullOrEmpty(firstValidLine))
                                {
                                    Statics.ParseLine(firstValidLine, out device, out browser, out os);
                                }
                            }

                            return new SharedOBJ_PartisBiggest
                            {
                                ID = e.ID,
                                Name = e.UserNickName != null ? e.UserNickName == "Teacher" ? e.UserTBL.GetName() : e.UserNickName : "نامشخص",
                                Timer = _commonMethods.GetTimerStayUserInMeet(e).ToString(),
                                CanTakeCertificate = e.CertificateTBL != null &&
                                                    e.CertificateTBL.Status == MainEntity_Status.CERT_FULL &&
                                                    e.CertificateTBL.Validation,
                                HasTakenCertificate = (e.CertificateTBL != null && !string.IsNullOrEmpty(e.CertificateTBL.FilePath)) || e.IsCertificated,
                                NationalCode = _commonMethods.GetNationalCode(e) ?? "",
                                Role = e.Role,
                                Email = e.UserTBL?.Email ?? "",
                                Phone = !string.IsNullOrEmpty(e.Phone) ? e.Phone : (e.UserTBL?.PhoneNumber ?? ""),
                                LogCount = e.UserTBLToMeetingTBLLogs.Count(),
                                Gender = e.CertificateTBL == null || !e.CertificateTBL.IsMale.HasValue
                                         ? "تعیین نشده"
                                         : e.CertificateTBL.IsMale.Value ? "آقا" : "خانم",
                                CertificateURL = e.CertificateTBL != null && !string.IsNullOrEmpty(e.CertificateTBL.FilePath)
                                                 ? $"{e.CertificateTBL.FileURL}{e.CertificateTBL.FilePath.Replace('\\', '/')}"
                                                 : (e.CertificateFileURL ?? ""),

                                // <<< اطلاعات دستگاه و مرورگر اضافه شده >>>
                                Device = device,                    // مثلاً gl-006ad8b2-...
                                Browser = browser,                  // مثلاً Firefox 146
                                OS = os                             // مثلاً Windows 10 یا Android 15
                            };
                        }).ToList()
                    }));
                return;
            }

            outputPort.Handle(new GenericResponse<GetMembersOfMeetResponse>
       (new GetMembersOfMeetResponse
       {
           Message = "ok",
           Status = 403,

       }));

        }
    }
    #endregion

    #region GetMembersOfMeetOnlyCertificated
    public class GetMembersOfMeetOnlyCertificatedUseCase : IGetMembersOfMeetOnlyCertificatedUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public GetMembersOfMeetOnlyCertificatedUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetMembersOfMeetOnlyCertificatedRequest message,
            IOutPutPort<GenericResponse<GetMembersOfMeetOnlyCertificatedResponse>> outputPort)
        {
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;
            long id = 0;
            if (long.TryParse(message.ID, out id))
            {

            }
            if (_db.MeetingTBLs.Any(e => e.MeetID == message.ID || e.ID == id))
            {
                var m = await _db.CertificateTBLs
                    .Where(c => c.UserTBLToMeetingTBL.MeetID == message.ID || c.UserTBLToMeetingTBL.ID == id)
                    .Include(c => c.UserTBLToMeetingTBL)
                        .ThenInclude(j => j.UserTBL)
                    .Include(c => c.UserTBLToMeetingTBL)
                        .ThenInclude(j => j.MeetingTBL)
                        .ThenInclude(m => m.RoomTBL)
                        .Include(e => e.UserTBLToMeetingTBL).ThenInclude(e => e.UserTBLToMeetingTBLLogs)
                    .AsSplitQuery() // Optional: recommended when you have multiple collections
                    .ToListAsync();

                outputPort.Handle(new GenericResponse<GetMembersOfMeetOnlyCertificatedResponse>(
                    new GetMembersOfMeetOnlyCertificatedResponse
                    {
                        Message = "ok",
                        Status = 200,
                        Certificates = m.Select(e => new SharedOBJ_Certificate
                        {
                            ID = e.ID,
                            Gender = !e.IsMale.HasValue ? "تعیین نشده" : e.IsMale.Value ? "آقا" : "خانم",
                            Date = e.DateInsert,
                            Meet = new SharedOBJ_Partis
                            {
                                ID = e.UserTBLToMeetingTBL.MeetingTBL.ID,
                                Name = e.UserTBLToMeetingTBL?.MeetingTBL?.Title,
                                HasCert = false,
                                Type = "جلسه",
                                Timer = Statics.GetPrettyDateBySeconds(
                                    (long)(new[] { e.UserTBLToMeetingTBL.MeetingTBL.EndDate.HasValue ? e.UserTBLToMeetingTBL.MeetingTBL.EndDate.Value : e.UserTBLToMeetingTBL.MeetingTBL.DateInsert.AddHours(3), e.UserTBLToMeetingTBL.MeetingTBL.DateInsert.AddHours(4) }.Min() -
                                        e.UserTBLToMeetingTBL.MeetingTBL.DateInsert).TotalSeconds)
                            },
                            Parti = new SharedOBJ_Partis
                            {
                                ID = e.IDUserTBLToMeetingTBL,
                                Name = e.UserTBLToMeetingTBL.UserNickName,
                                HasCert = !string.IsNullOrEmpty(e.FilePath),
                                Type = e.UserTBLToMeetingTBL.ReturnTypeString(),
                                Timer = Statics.GetPrettyDateBySeconds(_commonMethods.GetTimerStayUserInMeet(e.UserTBLToMeetingTBL))
                            },
                            PresentTimeSeconds = _commonMethods.GetTimerStayUserInMeet(e.UserTBLToMeetingTBL),
                            Room = new SharedOBJ_Partis
                            {
                                ID = e.UserTBLToMeetingTBL.MeetingTBL.RoomTBL.ID,
                                Name = e.UserTBLToMeetingTBL.MeetingTBL.RoomTBL.Title,
                                HasCert = false,
                                Type = "گروه",
                                Timer = ""
                            }
                        }).ToList()
                    }));
                return;
            }

            outputPort.Handle(new GenericResponse<GetMembersOfMeetOnlyCertificatedResponse>
       (new GetMembersOfMeetOnlyCertificatedResponse
       {
           Message = "ok",
           Status = 403,

       }));

        }
    }
    #endregion

    #region GenerateCertificateWithUserMeetID
    public class GenerateCertificateWithUserMeetIDUseCase : IGenerateCertificateWithUserMeetIDUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }

        public GenerateCertificateWithUserMeetIDUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader


            )
        {
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GenerateCertificateWithUserMeetIDRequest message,
            IOutPutPort<GenericResponse<GenerateCertificateWithUserMeetIDResponse>> outputPort)
        {
            try
            {
                if (!message.Count.HasValue)
                    message.Count = 50;
                if (!message.Page.HasValue)
                    message.Page = 0;

                UserTBLToMeetingTBL? um = null;
                var cleanNatCode = message.NationalCode?.Trim();
                var cleanPhone = message.Phone?.Trim();

                if (message.ID > 0)
                {
                    um = await _db.UserTBLToMeetingTBLs
                        .Include(e => e.UserTBL)
                        .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                        .FirstOrDefaultAsync(e => e.ID == message.ID);
                }

                if (um == null && !string.IsNullOrEmpty(message.MeetID))
                {
                    um = await _db.UserTBLToMeetingTBLs
                        .Include(e => e.UserTBL)
                        .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                        .Where(e => (e.MeetID == message.MeetID || (e.MeetingTBL != null && e.MeetingTBL.MeetID == message.MeetID)) &&
                            ((!string.IsNullOrEmpty(cleanNatCode) && (e.NationalCode == cleanNatCode || (e.UserTBL != null && e.UserTBL.NationalCode == cleanNatCode))) ||
                             (!string.IsNullOrEmpty(cleanPhone) && (e.Phone == cleanPhone || (e.UserTBL != null && e.UserTBL.PhoneNumber == cleanPhone)))))
                        .OrderByDescending(e => !string.IsNullOrEmpty(e.TokenJWT) && e.TokenJWT != "creating...")
                        .ThenByDescending(e => e.TimeSpendSeconds)
                        .ThenByDescending(e => e.ID)
                        .FirstOrDefaultAsync();

                    if (um == null)
                    {
                        var meet = await _db.MeetingTBLs.Include(m => m.RoomTBL).FirstOrDefaultAsync(m => m.MeetID == message.MeetID);
                        if (meet != null && (!string.IsNullOrEmpty(cleanNatCode) || !string.IsNullOrEmpty(cleanPhone)))
                        {
                            var userEntity = await _db.UserTBLs.FirstOrDefaultAsync(u =>
                                (!string.IsNullOrEmpty(cleanNatCode) && u.NationalCode == cleanNatCode) ||
                                (!string.IsNullOrEmpty(cleanPhone) && u.PhoneNumber == cleanPhone));

                            if (userEntity == null)
                            {
                                userEntity = new UserTBL
                                {
                                    Firstname = !string.IsNullOrWhiteSpace(message.Name) ? message.Name.Trim() : "کاربر",
                                    LastName = "",
                                    NationalCode = cleanNatCode ?? "",
                                    PhoneNumber = cleanPhone ?? "",
                                    Status = MainEntity_Status.USER_ACTIVE,
                                    Type = MainEntity_Types.USER_VIEWER,
                                    UUID = Guid.NewGuid().ToString(),
                                    DateInsert = DateTime.UtcNow
                                };
                                if (userEntity.AllowedGUID == null || !userEntity.AllowedGUID.Any())
                                {
                                    userEntity.AddGuid(Guid.NewGuid());
                                }
                                _db.UserTBLs.Add(userEntity);
                                await _db.SaveChangesAsync();
                            }

                            um = new UserTBLToMeetingTBL
                            {
                                MeetID = meet.MeetID,
                                UUID = userEntity.UUID,
                                UserTBL = userEntity,
                                UserNickName = !string.IsNullOrWhiteSpace(message.Name) ? message.Name.Trim() : userEntity.GetName(),
                                Role = "VIEWER",
                                Status = MainEntity_Status.USERMEET_JOINED,
                                Type = MainEntity_Types.USER_VIEWER,
                                NationalCode = cleanNatCode ?? userEntity.NationalCode,
                                Phone = cleanPhone ?? userEntity.PhoneNumber,
                                DateInsert = DateTime.UtcNow,
                                MeetingTBL = meet
                            };
                            if (userEntity.AllowedGUID == null || !userEntity.AllowedGUID.Any())
                            {
                                userEntity.AddGuid(Guid.NewGuid());
                                _db.UserTBLs.Update(userEntity);
                                await _db.SaveChangesAsync();
                            }
                            um.TokenJWT = Statics.GenerateJWTUser(userEntity, um);
                            _db.UserTBLToMeetingTBLs.Add(um);
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                if (um == null)
                {
                    outputPort.Handle(new GenericResponse<GenerateCertificateWithUserMeetIDResponse>(
                        new GenerateCertificateWithUserMeetIDResponse
                        {
                            Message = "کاربر یا جلسه مورد نظر یافت نشد",
                            Status = 404
                        }));
                    return;
                }

                // Update national code & phone on um and UserTBL if provided
                if (!string.IsNullOrEmpty(cleanNatCode))
                {
                    um.NationalCode = cleanNatCode;
                    if (um.UserTBL != null && string.IsNullOrEmpty(um.UserTBL.NationalCode))
                    {
                        //um.UserTBL.NationalCode = cleanNatCode;
                    }
                }
                else if (string.IsNullOrEmpty(um.NationalCode) && um.UserTBL != null && !string.IsNullOrEmpty(um.UserTBL.NationalCode))
                {
                    um.NationalCode = um.UserTBL.NationalCode;
                }

                if (!string.IsNullOrEmpty(cleanPhone))
                {
                    um.Phone = cleanPhone;
                    if (um.UserTBL != null && string.IsNullOrEmpty(um.UserTBL.PhoneNumber))
                    {
                        //um.UserTBL.PhoneNumber = cleanPhone;
                    }
                }
                else if (string.IsNullOrEmpty(um.Phone) && um.UserTBL != null && !string.IsNullOrEmpty(um.UserTBL.PhoneNumber))
                {
                    um.Phone = um.UserTBL.PhoneNumber;
                }

                if (!string.IsNullOrWhiteSpace(message.Name))
                {
                    um.UserNickName = message.Name.Trim();
                }

                string effectiveNationalCode = !string.IsNullOrEmpty(cleanNatCode) ? cleanNatCode : (!string.IsNullOrEmpty(um.NationalCode) ? um.NationalCode : (um.UserTBL?.NationalCode ?? ""));

                var meetId = um.MeetID ?? um.MeetingTBL?.MeetID ?? message.MeetID;
                var userTblId = um.UserTBL?.ID;

                // Find all existing records for this user in the same meeting (to handle split/duplicate sessions)
                var siblingRows = await _db.UserTBLToMeetingTBLs
                    .Include(e => e.UserTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .Include(e => e.CertificateTBL)
                    .Where(e => (e.MeetID == meetId || (e.MeetingTBL != null && e.MeetingTBL.MeetID == meetId)) &&
                        (e.ID == um.ID ||
                         (!string.IsNullOrEmpty(effectiveNationalCode) && (e.NationalCode == effectiveNationalCode || (e.UserTBL != null && e.UserTBL.NationalCode == effectiveNationalCode))) ||
                         (!string.IsNullOrEmpty(cleanPhone) && (e.Phone == cleanPhone || (e.UserTBL != null && e.UserTBL.PhoneNumber == cleanPhone))) ||
                         (userTblId.HasValue && e.UserTBL != null && e.UserTBL.ID == userTblId.Value) ||
                         (!string.IsNullOrEmpty(um.UUID) && e.UUID == um.UUID)
                        )
                    )
                    .ToListAsync();

                // Look for an existing row in this meeting that already has a valid TokenJWT
                var validTokenRow = siblingRows.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.TokenJWT) && r.TokenJWT != "creating..." && r.TokenJWT.Length > 20);

                if ((string.IsNullOrWhiteSpace(um.TokenJWT) || um.TokenJWT == "creating...") && validTokenRow != null)
                {
                    um.TokenJWT = validTokenRow.TokenJWT;
                    if (um.TimeSpendSeconds == 0 && validTokenRow.TimeSpendSeconds > 0)
                    {
                        um.TimeSpendSeconds = validTokenRow.TimeSpendSeconds;
                    }
                    if (um.UserMeetID == "-" && validTokenRow.UserMeetID != "-")
                    {
                        um.UserMeetID = validTokenRow.UserMeetID;
                    }
                    if (um.UserTBL == null && validTokenRow.UserTBL != null)
                    {
                        um.UserTBL = validTokenRow.UserTBL;
                    }
                }

                // If TokenJWT is still "creating..." or empty, create a real valid JWT now
                if (string.IsNullOrWhiteSpace(um.TokenJWT) || um.TokenJWT == "creating...")
                {
                    var userEntity = um.UserTBL ?? await _db.UserTBLs.FirstOrDefaultAsync(u => u.UUID == um.UUID ||
                        (!string.IsNullOrEmpty(effectiveNationalCode) && u.NationalCode == effectiveNationalCode) ||
                        (!string.IsNullOrEmpty(cleanPhone) && u.PhoneNumber == cleanPhone));

                    if (userEntity != null)
                    {
                        if (userEntity.AllowedGUID == null || !userEntity.AllowedGUID.Any())
                        {
                            userEntity.AddGuid(Guid.NewGuid());
                            _db.UserTBLs.Update(userEntity);
                            await _db.SaveChangesAsync();
                        }
                        um.TokenJWT = Statics.GenerateJWTUser(userEntity, um);
                    }
                }

                string role = "شرکت‌کننده";
                if (um.Role == "MODERATOR")
                {
                    role = "ارائه دهنده";
                }

                var cert = await _db.CertificateTBLs.FirstOrDefaultAsync(e => e.IDUserTBLToMeetingTBL == um.ID);
                if (cert == null)
                {
                    Statics.Log("GenerateCertificateWithUserMeetIDUseCase", $"{um.ID}");
                    cert = new CertificateTBL
                    {
                        Status = MainEntity_Status.CERT_FULL,
                        Type = MainEntity_Types.CERT_NORMAL,
                        Title = "گواهینامه حضور در جلسه",
                        IDUserTBLToMeetingTBL = um.ID,
                        Validation = true,
                        DateInsert = DateTime.UtcNow
                    };
                    _db.CertificateTBLs.Add(cert);
                    await _db.SaveChangesAsync();
                }

                cert.NationalCode = effectiveNationalCode;
                cert.IsMale = message.IsMale;
                cert.Validation = true;
                cert.Status = MainEntity_Status.CERT_FULL;

                var wwwroot = hosting.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fontPath = "IranNastaliq";
                var bgpath = Path.Combine(wwwroot, "bg.png");
                var pdfDir = Path.Combine(wwwroot, "pdf", "certificates");
                if (!Directory.Exists(pdfDir))
                {
                    Directory.CreateDirectory(pdfDir);
                }
                var outputPath = Path.Combine(pdfDir, $@"certif-{um.ID}.pdf");

                string displayName = !string.IsNullOrEmpty(um.UserNickName)
                    ? (um.UserNickName == "Teacher" && um.UserTBL != null ? um.UserTBL.GetName() : um.UserNickName)
                    : (um.UserTBL != null ? um.UserTBL.GetName() : "شرکت‌کننده");

                string roomTitle = um.MeetingTBL?.RoomTBL?.Title ?? "";
                string meetTitle = um.MeetingTBL?.Title ?? "";
                string jalaliDate = Statics.ConvertPersianMonthToNumber(um.MeetingTBL?.JalaliDate ?? "").Replace("-", "/");

                WordService.CreateCertificatePdf(outputPath, fontPath, bgpath, displayName, message.IsMale, effectiveNationalCode,
                    roomTitle, meetTitle, jalaliDate, role, um.ID.ToString());

                cert.FileURL = "https://vc.hekmateislami.com/";
                cert.FilePath = $"cert/certif-{um.ID}.pdf";
                um.IsCertificated = true;
                um.CertificateFileURL = $"{cert.FileURL}{cert.FilePath}";

                _db.CertificateTBLs.Update(cert);
                _db.UserTBLToMeetingTBLs.Update(um);

                // Synchronize all other sibling rows for this user in this meeting
                foreach (var sib in siblingRows)
                {
                    if (sib.ID != um.ID)
                    {
                        sib.IsCertificated = true;
                        sib.CertificateFileURL = um.CertificateFileURL;
                        if (string.IsNullOrWhiteSpace(sib.TokenJWT) || sib.TokenJWT == "creating...")
                        {
                            sib.TokenJWT = um.TokenJWT;
                        }
                        if (string.IsNullOrEmpty(sib.NationalCode) && !string.IsNullOrEmpty(effectiveNationalCode))
                        {
                            sib.NationalCode = effectiveNationalCode;
                        }
                        if (string.IsNullOrEmpty(sib.Phone) && !string.IsNullOrEmpty(cleanPhone))
                        {
                            sib.Phone = cleanPhone;
                        }
                        _db.UserTBLToMeetingTBLs.Update(sib);

                        var sibCert = await _db.CertificateTBLs.FirstOrDefaultAsync(c => c.IDUserTBLToMeetingTBL == sib.ID);
                        if (sibCert == null)
                        {
                            sibCert = new CertificateTBL
                            {
                                Status = MainEntity_Status.CERT_FULL,
                                Type = MainEntity_Types.CERT_NORMAL,
                                Title = "گواهینامه حضور در جلسه",
                                IDUserTBLToMeetingTBL = sib.ID,
                                Validation = true,
                                NationalCode = effectiveNationalCode,
                                IsMale = message.IsMale,
                                FileURL = cert.FileURL,
                                FilePath = cert.FilePath,
                                DateInsert = DateTime.UtcNow
                            };
                            _db.CertificateTBLs.Add(sibCert);
                        }
                        else
                        {
                            sibCert.Status = MainEntity_Status.CERT_FULL;
                            sibCert.Validation = true;
                            sibCert.NationalCode = effectiveNationalCode;
                            sibCert.IsMale = message.IsMale;
                            sibCert.FileURL = cert.FileURL;
                            sibCert.FilePath = cert.FilePath;
                            _db.CertificateTBLs.Update(sibCert);
                        }
                    }
                }
                await _db.SaveChangesAsync();

                Statics.Log("GenerateCertificateWithUserMeetIDUseCase", "Ta upload");
                try
                {
                    scpUploader.UploadFile(outputPath);
                }
                catch (Exception uploadEx)
                {
                    Statics.Log("GenerateCertificateWithUserMeetIDUseCase", "Upload warning: " + uploadEx.Message);
                }

                outputPort.Handle(new GenericResponse<GenerateCertificateWithUserMeetIDResponse>(
                    new GenerateCertificateWithUserMeetIDResponse
                    {
                        Message = "گواهینامه با موفقیت صادر شد",
                        Status = 200,
                        Link = $"{cert.FileURL}{cert.FilePath}"
                    }));
            }
            catch (Exception ex)
            {
                Statics.Log("GenerateCertificateWithUserMeetIDUseCase", "Exception: " + ex.Message + " " + ex.InnerException);
                outputPort.Handle(new GenericResponse<GenerateCertificateWithUserMeetIDResponse>(
                    new GenerateCertificateWithUserMeetIDResponse
                    {
                        Message = "خطا در صدور گواهینامه: " + ex.Message,
                        Status = 500
                    }));
            }
        }
    }
    #endregion

    #region SearchUser
    public class SearchUserUseCase : ISearchUserUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }

        public SearchUserUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader


            )
        {
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(SearchUserRequest message,
            IOutPutPort<GenericResponse<SearchUserResponse>> outputPort)
        {
            string q = message.Query;

            long id = 0;
            List<SharedOBJ_SearchResultModel> r = new List<SharedOBJ_SearchResultModel>();

            if (long.TryParse(q, out id))
            {
                r.AddRange(_db.CertificateTBLs.Where(e => e.IDUserTBLToMeetingTBL.ToString().Contains(q))
                    .Include(e => e.UserTBLToMeetingTBL).ThenInclude(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL).Select(e => new SharedOBJ_SearchResultModel
                    {
                        ID = e.IDUserTBLToMeetingTBL,
                        MeetDate = e.UserTBLToMeetingTBL.MeetingTBL.JalaliDate,
                        MeetGroup = e.UserTBLToMeetingTBL.MeetingTBL.RoomTBL.Title,
                        MeetTitle = e.UserTBLToMeetingTBL.MeetingTBL.Title + "",
                        Name = e.UserTBLToMeetingTBL.UserNickName,
                    }).ToList());
                r.AddRange(_db.UserTBLToMeetingTBLs.Where(e => e.ID.ToString().Contains(q))
                                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL).Select(e => new SharedOBJ_SearchResultModel
                                    {
                                        ID = e.ID,
                                        MeetDate = e.MeetingTBL.JalaliDate,
                                        MeetGroup = e.MeetingTBL.RoomTBL.Title,
                                        MeetTitle = e.MeetingTBL.Title + "",
                                        Name = e.UserNickName,
                                    }).ToList());

            }
            else
            {
                //search in meets
                if (_db.UserTBLToMeetingTBLs.Any(e => e.MeetingTBL.Title.Contains(q) && e.MeetingTBL.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL))
                {
                    r.AddRange(_db.UserTBLToMeetingTBLs.Where(e => e.MeetingTBL.Title.Contains(q) && e.MeetingTBL.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL)
                        .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL).Select(e => new SharedOBJ_SearchResultModel
                        {
                            ID = e.ID,
                            MeetDate = e.MeetingTBL.JalaliDate,
                            MeetGroup = e.MeetingTBL.RoomTBL.Title,
                            MeetTitle = e.MeetingTBL.Title + "",
                            Name = e.UserNickName,
                        })
                        .ToList());
                }

                //search in nicknames
                r.AddRange(_db.UserTBLToMeetingTBLs.Where(e => e.UserNickName.Contains(q) && e.MeetingTBL.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL)
                      .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL).Select(e => new SharedOBJ_SearchResultModel
                      {
                          ID = e.ID,
                          MeetDate = e.MeetingTBL.JalaliDate,
                          MeetGroup = e.MeetingTBL.RoomTBL.Title,
                          MeetTitle = e.MeetingTBL.Title + "",
                          Name = e.UserNickName,
                      })
                      .ToList());
                //search in date
                r.AddRange(_db.UserTBLToMeetingTBLs.Where(e => e.MeetingTBL.JalaliDate.Contains(q) && e.MeetingTBL.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL)
                     .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL).Select(e => new SharedOBJ_SearchResultModel
                     {
                         ID = e.ID,
                         MeetDate = e.MeetingTBL.JalaliDate,
                         MeetGroup = e.MeetingTBL.RoomTBL.Title,
                         MeetTitle = e.MeetingTBL.Title + "",
                         Name = e.UserNickName,
                     })
                     .ToList());

                if (message.Day.HasValue && message.Year.HasValue && !string.IsNullOrEmpty(message.Month))
                {
                    r = r.Where(e => e.MeetDate.Contains(message.Day.Value.ToString()) &&
                                    e.MeetDate.Contains(message.Month.ToString()) &&
                                    e.MeetDate.Contains(message.Year.Value.ToString())
                    ).ToList();
                }
            }
            outputPort.Handle(new GenericResponse<SearchUserResponse>
       (new SearchUserResponse
       {
           Message = "ok",
           Status = 200,
           Results = r.DistinctBy(e => e.ID).OrderByDescending(e => e.ID).ToList()

       }));

        }
    }
    #endregion

    #region GetExcellMembers
    public class GetExcellMembersUseCase : IGetExcellMembersUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }

        public GetExcellMembersUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader


            )
        {
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetExcellMembersRequest message,
            IOutPutPort<GenericResponse<GetExcellMembersResponse>> outputPort)
        {
            var users = _db.UserTBLToMeetingTBLs.Where(e => e.MeetID == message.MeetID)
                                       .Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.UserTBLToMeetingTBLs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Creator)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Teacher)
                .ToList();
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("اعضا");
            worksheet.Cell(1, 1).Value = "نام شرکت کننده";
            worksheet.Cell(1, 2).Value = "عنوان جلسه";
            worksheet.Cell(1, 3).Value = "تاریخ";
            int row = 2;
            foreach (var u in users)
            {
                worksheet.Cell(row, 1).Value = u.UserNickName;
                worksheet.Cell(row, 2).Value = string.IsNullOrEmpty(u.MeetingTBL.Title) ? " " : u.MeetingTBL.Title;
                worksheet.Cell(row, 3).Value = u.MeetingTBL.JalaliDate;
                row++;
            }
            worksheet.Columns().AdjustToContents();
            var wwwroot = hosting.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var fontPath = "IranNastaliq";
            var bgpath = Path.Combine(wwwroot, "bg.png");
            var outputPath = Path.Combine(wwwroot, "pdf", "certificates", $@"{users.First().ID}-{users.First().MeetingTBL.JalaliDate}.xlsx");

            workbook.SaveAs(outputPath);
            scpUploader.UploadFile(outputPath);

            outputPort.Handle(new GenericResponse<GetExcellMembersResponse>
       (new GetExcellMembersResponse
       {
           Message = "ok",
           Status = 200,
           Link = Path.Join(Statics.bbb.Url, Path.Combine("cert", $@"{users.First().ID}-{users.First().MeetingTBL.JalaliDate}.xlsx"))

       }));

        }
    }
    #endregion

    #region StartNewMeet
    public class StartNewMeetUseCase : IStartNewMeetUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }
        private IBBBGetMeetings bbb { get; }
        private readonly IMediator _mediator;
        private readonly IServiceScopeFactory _serviceScopeFactory; // اضافه کنید

        public StartNewMeetUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader,
            IMediator m,
            IBBBGetMeetings bbb,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.bbb = bbb;
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            this._mediator = m;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task handleAsync(StartNewMeetRequest message,
    IOutPutPort<GenericResponse<StartNewMeetResponse>> outputPort)
        {
            var strategy = _db.Database.CreateExecutionStrategy();
            string meetid = "";
            string redirectLink = "";
            string meetTitle = "";

            await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        long idplan = 0;
                        if (message.IDPlan.HasValue)
                            idplan = message.IDPlan.Value;

                        long idRoom = 0;
                        long.TryParse(message.IDRoom, out idRoom);
                        long idUser = message.GeneralHeaders.UserTBL.ID;
                        // اعتبارسنجی‌ها
                        if (!_db.RoomTBLs.Any(e => e.FriendlyID == message.IDRoom || e.ID == idRoom))
                        {
                            outputPort.Handle(new GenericResponse<StartNewMeetResponse>(new StartNewMeetResponse
                            {
                                Message = "آی دی گروه یافت نشد",
                                Status = 404,
                            }));
                            return;
                        }

                        if (!_db.RoomTBLs.Any(e => e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.FriendlyID == message.IDRoom
                            || ( e.RoomTBLToUserTBLs.Any(r => r.ParticipatorTBL.ID == idUser && r.Type==MainEntity_Types.ROOMTOUSER_ADMIN))) &&
                            !message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_admins"))
                        {
                            outputPort.Handle(new GenericResponse<StartNewMeetResponse>(new StartNewMeetResponse
                            {
                                Message = "دسترسی ندارید",
                                Status = 400
                            }));
                            return;
                        }

                        var room = _db.RoomTBLs.Where(e => e.FriendlyID == message.IDRoom || e.ID == idRoom).Include(e=>e.RoomTBLToUserTBLs).ThenInclude(e=>e.ParticipatorTBL).First();
                        var planTBL = await _db.RoomTBLPlanTBLs.FindAsync(idplan);

                        if (planTBL == null)
                        {
                            planTBL = new RoomTBLPlanTBL
                            {
                                Dabir = message.Dabir,
                                IDRoomTBL = room.ID,
                                StartDate = DateTime.UtcNow,
                                Title = message.MeetTitle,
                                RequiredToEnterName = message.RequiredToEnterName,
                                Teachers = string.Join(" ، ", message.Teachers),
                                Status = MainEntity_Status.MEETING_STARTED_LATER,
                                Type = message.HasCertificate ? MainEntity_Types.PLAN_WITH_CERT : MainEntity_Types.PLAN_NORMAL,
                            };
                            _db.RoomTBLPlanTBLs.Add(planTBL);
                            await _db.SaveChangesAsync();
                        }

                        // تنظیم BBB_MeetID و کدها
                        if (string.IsNullOrEmpty(room.BBB_MeetID))
                        {
                            if (_db.MeetingTBLs.Any(e => e.RoomFriendlyID == room.FriendlyID))
                            {
                                room.BBB_MeetID = _db.MeetingTBLs.First(e => e.RoomFriendlyID == room.FriendlyID).MeetID.Split("-")[0];
                            }
                            else
                            {
                                while (true)
                                {
                                    room.BBB_MeetID = Statics.GenerateRoomMeetID();
                                    if (_db.RoomTBLs.Any(e => e.BBB_MeetID == room.BBB_MeetID))
                                        continue;
                                    break;
                                }
                                _db.Update(room);
                            }
                        }

                        if (string.IsNullOrEmpty(room.ViewerCode) || string.IsNullOrEmpty(room.ModeratorCode))
                        {
                            room.ModeratorCode = Statics.GenerateSegment(8);
                            room.ViewerCode = Statics.GenerateSegment(8);
                            _db.Update(room);
                        }


                        // ایجاد جلسه در بیگ بلو باتن
                        var meet = await this.bbb.CreateMeetingAsync(room);
                        if (meet.Status != "created")
                        {
                            outputPort.Handle(new GenericResponse<StartNewMeetResponse>(new StartNewMeetResponse
                            {
                                Message = "ساختن جلسه جدید با خطای بیگ بلو باتن مواجه شد" + JsonConvert.SerializeObject(meet),
                                Status = 400,
                            }));
                            return;
                        }

                        meetid = $"{meet.MeetID}";
                        meetTitle = meet.Title;

                        // ایجاد جلسه در دیتابیس
                        var meetTBL = new MeetingTBL
                        {
                            RoomFriendlyID = message.IDRoom,
                            MeetID = meetid,
                            Type = MainEntity_Types.MEET_API,
                            Status = MainEntity_Status.MEETING_STARTED_LATER,
                            JalaliDate = Statics.ToJalaliDate(DateTime.Now),
                            Title = planTBL.Title,
                            RequiredToEnterName = planTBL.RequiredToEnterName,
                            DateInsert = DateTime.UtcNow,
                            IDRoomTBLPlanTBL = planTBL.ID,
                            IDCreator = message.GeneralHeaders.UserTBL.ID,
                        };
                        meetTBL.JWT = Statics.GenerateJWTMeet(meetTBL);
                        _db.MeetingTBLs.Add(meetTBL);
                        await _db.SaveChangesAsync();
                        string userid = "";
                        if (message.GeneralHeaders?.UserTBL != null)
                        {
                            userid = message.GeneralHeaders.UserTBL.UUID;
                        }
                        else if (string.IsNullOrEmpty(userid))
                        {
                            userid = message.GeneralHeaders.BBBUserToken;
                            if (userid == null)
                                userid = Statics.GenerateSecretKey(64);
                        }
                        // گرفتن لینک ورود
                        redirectLink = await bbb.GetJoinUrlAsync(meetTBL, message.Name, userid, message.GeneralHeaders.UserTBL.NationalCode + "",
                            message.GeneralHeaders.UserTBL.PhoneNumber + "", message.GeneralHeaders.JWTUserToken, true,
                            await _commonMethods.ReturnPassword(message.IDRoom, true));

                        // ذخیره اطلاعات مورد نیاز برای Background
                        var backgroundData = new BackgroundMeetData
                        {
                            MeetID = meetid,
                            MeetTBLId = meetTBL.ID,
                            RoomId = room.ID,
                            PlanTBL = planTBL,
                            Teachers = message.Teachers.ToList(),
                            Dabir = message.Dabir,
                            UserId = message.GeneralHeaders.UserTBL.ID
                        };

                        // COMMIT تراکنش (ذخیره داده‌های اصلی)
                        await transaction.CommitAsync();

                        // **همین الان پاسخ رو به کاربر برگردون**
                        outputPort.Handle(new GenericResponse<StartNewMeetResponse>(new StartNewMeetResponse
                        {
                            Message = "ok",
                            Status = 200,
                            RedirectURL = redirectLink,
                            Meeting = new SharedOBJ_Meeting
                            {
                                Title = meetTitle
                            }
                        }));

                        // **بقیه عملیات سنگین رو در Background اجرا کن**
                        _ = Task.Run(async () => await ProcessMeetBackgroundAsync(backgroundData));

                        return;
                    }
                    catch (Exception e)
                    {
                        await transaction.RollbackAsync();
                        if (!string.IsNullOrEmpty(meetid))
                            _ = bbb.EndMeetingAsync(meetid.Split("-")[0]);

                        Statics.Log("Start meet error:", e);
                        outputPort.Handle(new GenericResponse<StartNewMeetResponse>(new StartNewMeetResponse
                        {
                            Message = "خطا در ساختن جلسه جدید: " + e.Message,
                            Status = 400,
                        }));
                    }
                }
            });
        }

        // متد مجزا برای پردازش پس‌زمینه
        private async Task ProcessMeetBackgroundAsync(BackgroundMeetData data)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<IDBContext>();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    // اضافه کردن معلمان و دبیر
                    List<UserTBL> teachers = new List<UserTBL>();

                    // پردازش دبیر
                    if (!string.IsNullOrEmpty(data.PlanTBL.Dabir))
                    {
                        UserTBL dabir = null;
                        var existingDabir = await db.UserTBLs.FirstOrDefaultAsync(e =>
                            (e.Firstname + " " + e.LastName).Trim() == data.PlanTBL.Dabir &&
                            e.Status == MainEntity_Status.USER_ACTIVE &&
                            (e.Type == MainEntity_Types.USER_TEACHER || e.Type == MainEntity_Types.USER_DABIR));

                        if (existingDabir != null)
                        {
                            dabir = existingDabir;
                        }
                        else
                        {
                            dabir = new UserTBL
                            {
                                Firstname = data.PlanTBL.Dabir,
                                Type = MainEntity_Types.USER_DABIR,
                                Status = MainEntity_Status.USER_ACTIVE,
                                Username = data.PlanTBL.Dabir,
                                UUID = new Random().NextInt64(-999999999999999999, -99999999999999999).ToString()
                            };
                            db.UserTBLs.Add(dabir);
                            await db.SaveChangesAsync();
                        }

                        if (!db.UserTBLToMeetingTBLs.Any(e => e.UUID == dabir.UUID && e.MeetID == data.MeetID))
                        {
                            db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL
                            {
                                MeetID = data.MeetID,
                                Type = MainEntity_Types.USER_DABIR,
                                Status = MainEntity_Status.MEETING_STARTED_LATER,
                                UUID = dabir.UUID,
                                UserMeetID = "Dabir",
                                Role = "Dabir",
                                UserNickName = dabir.GetName(),
                                IsCertificated = false,
                                TokenJWT = "nope",
                                TimeSpendSeconds = 0,
                            });
                            var q = new MeetPollQuizTBL
                            {
                                IDMeetingTBL = data.MeetTBLId,
                                IsForAll = false,
                                Question = $"نظرتان درباره میزان تسلط دبیر علمی محترم {$"**{dabir.GetName()}**"} چیست؟"
                            };
                            db.MeetPollQuizTBLs.Add(q);
                            await db.SaveChangesAsync();
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "ضعیف",
                                IDQuizTBL = q.ID,
                                Score = 25,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "متوسط",
                                IDQuizTBL = q.ID,
                                Score = 50,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "خوب",
                                IDQuizTBL = q.ID,
                                Score = 75,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "عالی",
                                IDQuizTBL = q.ID,
                                Score = 100,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            await db.SaveChangesAsync();
                        }
                    }

                    // پردازش معلمان
                    var teacherList = data.PlanTBL.Teachers?.Split("،").ToList() ?? new List<string>();
                    foreach (var t1 in teacherList)
                    {
                        var t = t1.Trim();
                        var existingTeacher = await db.UserTBLs.FirstOrDefaultAsync(e =>
                            ((e.Firstname + " " + e.LastName).Trim() == t || e.Username == t) &&
                             (e.Type == MainEntity_Types.USER_TEACHER || e.Type == MainEntity_Types.USER_DABIR));

                        UserTBL teacher;
                        if (existingTeacher != null)
                        {
                            teacher = existingTeacher;
                        }
                        else
                        {
                            teacher = new UserTBL
                            {
                                Firstname = t,
                                Type = MainEntity_Types.USER_TEACHER,
                                Status = MainEntity_Status.USER_ACTIVE,
                                Username = t,
                                UUID = new Random().NextInt64(-999999999999999999, -99999999999999999).ToString()
                            };
                            db.UserTBLs.Add(teacher);
                            await db.SaveChangesAsync();
                        }

                        if (!db.UserTBLToMeetingTBLs.Any(e => e.UUID == teacher.UUID && e.MeetID == data.MeetID))
                        {
                            db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL
                            {
                                MeetID = data.MeetID,
                                Type = MainEntity_Types.USER_TEACHER,
                                Status = MainEntity_Status.MEETING_STARTED_LATER,
                                UUID = teacher.UUID,
                                UserMeetID = "TEACHER",
                                Role = "Teacher",
                                UserNickName = teacher.GetName(),
                                IsCertificated = false,
                                TokenJWT = "nope",
                                TimeSpendSeconds = 0,
                            });
                            var q = new MeetPollQuizTBL
                            {
                                IDMeetingTBL = data.MeetTBLId,
                                IsForAll = false,
                                Question = $"نظرتان درباره میزان تسلط کارشناس محترم **{teacher.GetName()}** چیست؟",
                            };
                            db.MeetPollQuizTBLs.Add(q);
                            await db.SaveChangesAsync();
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "ضعیف",
                                IDQuizTBL = q.ID,
                                Score = 25,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "متوسط",
                                IDQuizTBL = q.ID,
                                Score = 50,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "خوب",
                                IDQuizTBL = q.ID,
                                Score = 75,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                            {
                                Answer = "عالی",
                                IDQuizTBL = q.ID,
                                Score = 100,
                                Type = q.Type,
                                Status = q.Status,
                            });
                            await db.SaveChangesAsync();
                        }
                    }



                    await db.SaveChangesAsync();

                    // ارسال رویداد
                    await mediator.Publish(new MeetingUpdatedEvent
                    {
                        Action = "Create",
                        MeetingId = data.MeetTBLId
                    });
                }
            }
            catch (Exception ex)
            {
                Statics.Log("Background meet processing error:", ex);
            }
        }

        // کلاس کمکی
        public class BackgroundMeetData
        {
            public string MeetID { get; set; }
            public long MeetTBLId { get; set; }
            public long RoomId { get; set; }
            public RoomTBLPlanTBL PlanTBL { get; set; }
            public List<string> Teachers { get; set; }
            public string Dabir { get; set; }
            public long UserId { get; set; }
        }

    }
    #endregion

    #region GetMemberByID
    public class GetMemberByIDUseCase : IGetMemberByIDUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }

        public GetMemberByIDUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader


            )
        {
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetMemberByIDRequest message,
            IOutPutPort<GenericResponse<GetMemberByIDResponse>> outputPort)
        {

            if (!_db.UserTBLToMeetingTBLs.Any(e => e.ID == message.ID))
            {

                outputPort.Handle(new GenericResponse<GetMemberByIDResponse>
           (new GetMemberByIDResponse
           {
               Message = "شناسه وارد شده اشتباه است",
               Status = 404,

           }));
                return;
            }
            var m = _db.UserTBLToMeetingTBLs.Where(e => e.ID == message.ID)
                       .Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                .First();
            var logs = m.UserTBLToMeetingTBLLogs.OrderByDescending(e => e.ID).Select(e => new SharedOBJ_Log
            {
                ID = e.ID,
                Time = e.DateInsert.ToString("HH:mm"),
                Title = e.Key,
                Value = UserTBLToMeetingTBLLogs.KEYS_TO_PERSION[e.Key]
            }).ToList();


            outputPort.Handle(new GenericResponse<GetMemberByIDResponse>
       (new GetMemberByIDResponse
       {
           Message = "ok",
           Status = 200,
           Certificate = new SharedOBJ_Certificate
           {
               ID = m.ID,
               Date = m.DateInsert,
               PresentTimeSeconds = _commonMethods.GetTimerStayUserInMeet(m),
               Gender = m.CertificateTBL == null || !m.CertificateTBL.IsMale.HasValue ? "تعیین نشده" : m.CertificateTBL.IsMale.Value ? "آقا" : "خانم",
               Meet = new SharedOBJ_Partis
               {
                   ID = m.MeetingTBL.ID,
                   Name = m.MeetingTBL.Title,
                   Timer = Statics.GetPrettyDateBySeconds(
                                    (long)(new[] { m.MeetingTBL.EndDate.Value, m.MeetingTBL.DateInsert.AddHours(4) }.Min() -
                                        m.MeetingTBL.DateInsert).TotalSeconds)

               },
               Parti = new SharedOBJ_Partis
               {
                   ID = m.UserTBL.ID,
                   Timer = Statics.GetPrettyDateBySeconds(_commonMethods.GetTimerStayUserInMeet(m)),
                   Name = m.UserNickName,
               },
               Room = new SharedOBJ_Partis
               {
                   Name = m.MeetingTBL.RoomTBL.Title,
                   ID = m.MeetingTBL.RoomTBL.ID
               },
           },
           Logs = logs
       }));

        }
    }
    #endregion

    #region GetAllMeetingMember
    public class GetAllMeetingMemberUseCase : IGetAllMeetingMemberUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }

        public GetAllMeetingMemberUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader


            )
        {
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetAllMeetingMemberRequest message,
            IOutPutPort<GenericResponse<GetAllMeetingMemberResponse>> outputPort)
        {
            long id = 0;
            long.TryParse(message.ID, out id);
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;

            if (!_db.UserTBLToMeetingTBLs.Any(e => e.ID == id) && !_db.UserTBLs.Any(e => e.UUID == message.ID) && !_db.UserTBLs.Any(e => e.ID == id) && !_db.CertificateTBLs.Any(e => e.ID == id))
            {

                outputPort.Handle(new GenericResponse<GetAllMeetingMemberResponse>
           (new GetAllMeetingMemberResponse
           {
               Message = "شناسه وارد شده اشتباه است",
               Status = 404,

           }));
                return;
            }
            string uuid = "";
            if (_db.UserTBLs.Any(e => e.ID == id))
            {
                uuid = _db.UserTBLs.Where(e => e.ID == id).First().UUID;
            }
            else if (_db.UserTBLs.Any(e => e.UUID == message.ID))
            {
                uuid = _db.UserTBLs.Where(e => e.UUID == message.ID).First().UUID;
            }

            else if (_db.UserTBLToMeetingTBLs.Any(e => e.ID == id))
            {
                uuid = _db.UserTBLToMeetingTBLs.Where(e => e.ID == id).First().UUID;
            }
            var meetings = _db.UserTBLToMeetingTBLs.Where(e => e.UUID == uuid)
                       .Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.UserTBLToMeetingTBLs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Creator)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Teacher)
                    .OrderByDescending(e => e.ID)
                    .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value)
                    .ToList();
            var nicknames = new List<string>();
            nicknames = meetings.Select(e => e.UserNickName).ToList();
            nicknames.DistinctBy(e => e.ToLower().Trim().Replace(" ", ""));
            var uniqueNicknames = nicknames
    .Select(n => n?.Trim()) // اول Trim کن (null-safe)
    .Where(n => !string.IsNullOrEmpty(n)) // خالی‌ها رو حذف کن
    .DistinctBy(n => n.Replace(" ", "").ToLower()) // کلید مقایسه: بدون فاصله + کوچک
    .ToList();
            nicknames = uniqueNicknames;
            var m = meetings.Select(e => new SharedOBJ_Meeting
            {
                CountPartis = e.MeetingTBL.UserTBLToMeetingTBLs.Count(),
                ID = e.MeetID,
                EndDate = Statics.GetPersianDate(
                                e.MeetingTBL.EndDate.HasValue
                                    ? e.MeetingTBL.EndDate.Value
                                    : e.MeetingTBL.DateInsert.AddHours(2)
                            ),

                StartDate = Statics.GetPersianDate(e.DateInsert),

                //Starter = new SharedOBJ_Partis
                //{
                //    ID = e.MeetingTBL.Creator.ID ,
                //    // اگر سازنده در لیست شرکت‌کننده‌ها نبود یا لیست خالی بود
                //    Name = e.MeetingTBL.UserTBLToMeetingTBLs
                //               .FirstOrDefault(r => r.UserTBL.ID == e.MeetingTBL.Creator.ID).UserNickName
                //               ?? e.MeetingTBL.Creator.Username,

                //    Timer = "0"
                //},
                ShortLink = e.MeetingTBL.RoomTBL.TTR_URL,
                Teachers = e.MeetingTBL.UserTBLToMeetingTBLs
                                .Where(x => x.Type == MainEntity_Types.USER_TEACHER && x.UserTBL != null)
                                .Select(r => new SharedOBJ_Partis
                                {
                                    ID = r.ID,
                                    Name = $"{r.UserTBL.Firstname ?? ""} {r.UserTBL.LastName ?? ""}".Trim(),
                                    Timer = "0"
                                })
                                .ToList() ?? new List<SharedOBJ_Partis>() // اگر null بود، لیست خالی برگردون
            })
                .ToList();
            foreach (var n in nicknames)
            {
                var mm = _db.UserTBLToMeetingTBLs.Where(e => e.UserNickName == n && e.MeetingTBL.UserTBLToMeetingTBLs.Count > 1).Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.UserTBLToMeetingTBLs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Creator)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Teacher)
                                        .OrderByDescending(e => e.ID)
                    .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value)
                    .ToList();

                m.AddRange(mm.Select(e => new SharedOBJ_Meeting
                {
                    CountPartis = e.MeetingTBL.UserTBLToMeetingTBLs.Count(),
                    ID = e.MeetID,
                    EndDate = Statics.GetPersianDate(
                                e.MeetingTBL.EndDate.HasValue
                                    ? e.MeetingTBL.EndDate.Value
                                    : e.MeetingTBL.DateInsert.AddHours(2)
                            ),

                    StartDate = Statics.GetPersianDate(e.DateInsert),

                    //Starter = new SharedOBJ_Partis
                    //{
                    //    ID = e.MeetingTBL.Creator.ID,
                    //    // اگر سازنده در لیست شرکت‌کننده‌ها نبود یا لیست خالی بود
                    //    Name = e.MeetingTBL.UserTBLToMeetingTBLs
                    //                   .FirstOrDefault(r => r.UserTBL.ID == e.MeetingTBL.Creator.ID).UserNickName
                    //                   ?? e.MeetingTBL.Creator.Username,

                    //    Timer = "0"
                    //},
                    ShortLink = e.MeetingTBL.RoomTBL.TTR_URL,
                    Teachers = e.MeetingTBL.UserTBLToMeetingTBLs
                                .Where(x => x.Type == MainEntity_Types.USER_TEACHER && x.UserTBL != null)
                                .Select(r => new SharedOBJ_Partis
                                {
                                    ID = r.ID,
                                    Name = $"{r.UserTBL.Firstname ?? ""} {r.UserTBL.LastName ?? ""}".Trim(),
                                    Timer = "0"
                                })
                                .ToList() ?? new List<SharedOBJ_Partis>()
                }));
            }
            outputPort.Handle(new GenericResponse<GetAllMeetingMemberResponse>
       (new GetAllMeetingMemberResponse
       {
           Message = "ok",
           Status = 200,
           Meetings = m.DistinctBy(e => e.ID).Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value).ToList()
       }));

        }
    }
    #endregion

    #region GetAllCertificateOfMember
    public class GetAllCertificateOfMemberUseCase : IGetAllCertificateOfMemberUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IScpUploader scpUploader { get; }

        public GetAllCertificateOfMemberUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader


            )
        {
            _db = repository;
            this.WordService = wordService;
            this.scpUploader = scpUploader;
            this.hosting = hosting;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetAllCertificateOfMemberRequest message,
            IOutPutPort<GenericResponse<GetAllCertificateOfMemberResponse>> outputPort)
        {
            long id = 0;
            long.TryParse(message.ID, out id);
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;

            if (!_db.UserTBLToMeetingTBLs.Any(e => e.ID == id) && !_db.UserTBLs.Any(e => e.UUID == message.ID) && !_db.UserTBLs.Any(e => e.ID == id) && !_db.CertificateTBLs.Any(e => e.ID == id))
            {

                outputPort.Handle(new GenericResponse<GetAllCertificateOfMemberResponse>
           (new GetAllCertificateOfMemberResponse
           {
               Message = "شناسه وارد شده اشتباه است",
               Status = 404,

           }));
                return;
            }
            string uuid = "";
            if (_db.UserTBLs.Any(e => e.ID == id))
            {
                uuid = _db.UserTBLs.Where(e => e.ID == id).First().UUID;
            }
            else if (_db.UserTBLs.Any(e => e.UUID == message.ID))
            {
                uuid = _db.UserTBLs.Where(e => e.UUID == message.ID).First().UUID;
            }

            else if (_db.UserTBLToMeetingTBLs.Any(e => e.ID == id))
            {
                uuid = _db.UserTBLToMeetingTBLs.Where(e => e.ID == id).First().UUID;
            }
            var meetings = _db.UserTBLToMeetingTBLs.Where(e => e.UUID == uuid && e.CertificateTBL != null)
                       .Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.UserTBLToMeetingTBLs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Creator)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Teacher)
                    .OrderByDescending(e => e.ID)
                    .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value)
                    .ToList();
            var nicknames = new List<string>();
            nicknames = meetings.Select(e => e.UserNickName).ToList();
            nicknames.DistinctBy(e => e.ToLower().Trim().Replace(" ", ""));
            var uniqueNicknames = nicknames
    .Select(n => n?.Trim()) // اول Trim کن (null-safe)
    .Where(n => !string.IsNullOrEmpty(n)) // خالی‌ها رو حذف کن
    .DistinctBy(n => n.Replace(" ", "").ToLower()) // کلید مقایسه: بدون فاصله + کوچک
    .ToList();
            nicknames = uniqueNicknames;
            var m = meetings.Select(e => new SharedOBJ_Certificate
            {
                ID = e.ID,
                Gender = !e.CertificateTBL.IsMale.HasValue ? "تعیین نشده" : e.CertificateTBL.IsMale.Value ? "آقا" : "خانم",
                Date = e.DateInsert,
                Meet = new SharedOBJ_Partis
                {
                    ID = e.MeetingTBL.ID,
                    Name = e.MeetingTBL.Title,
                    Timer = !e.MeetingTBL.EndDate.HasValue ? "" : Statics.GetPrettyDateBySeconds(
                                    (long)(new[] { e.MeetingTBL.EndDate.Value, e.MeetingTBL.DateInsert.AddHours(4) }.Min() -
                                        e.MeetingTBL.DateInsert).TotalSeconds)
                },
                Parti = new SharedOBJ_Partis
                {
                    ID = e.ID,
                    Name = e.UserNickName,
                    Timer = Statics.GetPrettyDateBySeconds(_commonMethods.GetTimerStayUserInMeet(e))
                },
                PresentTimeSeconds = _commonMethods.GetTimerStayUserInMeet(e),
                Room = new SharedOBJ_Partis
                {
                    ID = e.MeetingTBL.RoomTBL.ID,
                    Name = e.MeetingTBL.RoomTBL.Title,
                    Timer = ""
                }
            })
                .ToList();
            foreach (var n in nicknames)
            {
                var mm = _db.UserTBLToMeetingTBLs.Where(e => e.UserNickName == n && e.MeetingTBL.UserTBLToMeetingTBLs.Count > 1 && e.CertificateTBL != null).Include(e => e.UserTBL)
                    .Include(e => e.CertificateTBL)
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.UserTBLToMeetingTBLs)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Creator)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.Teacher)
                                        .OrderByDescending(e => e.ID)
                    .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value)
                    .ToList();

                m.AddRange(mm.Select(e => new SharedOBJ_Certificate
                {
                    ID = e.ID,
                    Gender = !e.CertificateTBL.IsMale.HasValue ? "تعیین نشده" : e.CertificateTBL.IsMale.Value ? "آقا" : "خانم",
                    Date = e.DateInsert,
                    Meet = new SharedOBJ_Partis
                    {
                        ID = e.MeetingTBL.ID,
                        Name = e.MeetingTBL.Title,
                        Timer = !e.MeetingTBL.EndDate.HasValue ? "" : Statics.GetPrettyDateBySeconds(
                                    (long)(new[] { e.MeetingTBL.EndDate.Value, e.MeetingTBL.DateInsert.AddHours(4) }.Min() -
                                        e.MeetingTBL.DateInsert).TotalSeconds)
                    },
                    Parti = new SharedOBJ_Partis
                    {
                        ID = e.ID,
                        Name = e.UserNickName,
                        Timer = Statics.GetPrettyDateBySeconds(_commonMethods.GetTimerStayUserInMeet(e))
                    },
                    PresentTimeSeconds = _commonMethods.GetTimerStayUserInMeet(e),
                    Room = new SharedOBJ_Partis
                    {
                        ID = e.MeetingTBL.RoomTBL.ID,
                        Name = e.MeetingTBL.RoomTBL.Title,
                        Timer = ""
                    }
                }));
            }
            outputPort.Handle(new GenericResponse<GetAllCertificateOfMemberResponse>
       (new GetAllCertificateOfMemberResponse
       {
           Message = "ok",
           Status = 200,
           Certificates = m.DistinctBy(e => e.ID).Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value).ToList()
       }));

        }
    }
    #endregion

    #region UploadFileStorage
    public class UploadFileStorageRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<UploadFileStorageResponse>>
    {
        public List<IFormFile> Files { get; set; }
        public string UploadPath { get; set; } // Optional: custom path, e.g., "images/products" or "documents/invoices"
        public string BaseUrl { get; set; } // Optional: base URL for generating file links
    }

    public class UploadFileStorageResponse : BaseParentResponce
    {
        [System.Text.Json.Serialization.JsonPropertyName("urls")]
        [Newtonsoft.Json.JsonProperty("urls")]
        public List<string> URLs { get; set; }
        public List<string> FileNames { get; set; } // Optional: return original file names
        public List<string> SavedPaths { get; set; } // Optional: return physical saved paths
        public List<long> FileSizes { get; set; } // Optional: return file sizes in bytes
    }

    public interface IUploadFileStorageUseCase
    {
        Task handleAsync(UploadFileStorageRequest message, IOutPutPort<GenericResponse<UploadFileStorageResponse>> outputPort);
    }

    public class UploadFileStorageUseCase : IUploadFileStorageUseCase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IDBContext _db;

        public UploadFileStorageUseCase(IWebHostEnvironment hostingEnvironment, IConfiguration configuration, IDBContext db)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _db = db;
        }

        public async Task handleAsync(UploadFileStorageRequest message, IOutPutPort<GenericResponse<UploadFileStorageResponse>> outputPort)
        {
            try
            {
                // Check user permission
                if (message.GeneralHeaders?.UserTBL == null || !message.GeneralHeaders.UserTBL.VerifyUserPermissions("ftp"))
                {
                    throw new Exception("NotAccess");
                }

                // Validate files
                if (message.Files == null || message.Files.Count == 0)
                {
                    outputPort.Handle(new GenericResponse<UploadFileStorageResponse>(new UploadFileStorageResponse
                    {
                        Message = "هیچ فایلی برای آپلود انتخاب نشده است",
                        Status = 400,
                    }));
                    return;
                }

                // Validate file count (limit to 10 files)
                if (message.Files.Count > 10)
                {
                    outputPort.Handle(new GenericResponse<UploadFileStorageResponse>(new UploadFileStorageResponse
                    {
                        Message = "حداکثر 10 فایل می‌تواند آپلود شود",
                        Status = 400,
                    }));
                    return;
                }

                // Allowed file extensions for photos, videos, and sounds
                var allowedExtensions = new[] { 
                // Images
                ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg", ".ico", ".tiff", ".tif",
                // Videos
                ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v", ".mpg", ".mpeg", ".3gp", ".ogv",
                // Sounds/Audio
                ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac", ".wma", ".opus", ".webm"
            };

                // Maximum file size: 2GB = 2 * 1024 * 1024 * 1024 bytes
                const long maxFileSize = 2L * 1024 * 1024 * 1024; // 2 GB

                var uploadedUrls = new List<string>();
                var uploadedFileNames = new List<string>();
                var uploadedPaths = new List<string>();
                var uploadedFileSizes = new List<long>();

                // Determine upload directory
                string baseUploadPath;
                if (!string.IsNullOrEmpty(message.UploadPath))
                {
                    // Use custom path provided in request
                    baseUploadPath = Path.Combine(@"C:\hekmatFiles", message.UploadPath);
                }
                else
                {
                    // Default path from configuration or webroot
                    var defaultPath = _configuration["UploadSettings:DefaultPath"] ?? "uploads";
                    baseUploadPath = Path.Combine(_hostingEnvironment.WebRootPath, defaultPath);
                }

                // Create directory if not exists
                if (!Directory.Exists(baseUploadPath))
                {
                    Directory.CreateDirectory(baseUploadPath);
                }

                // Process each file
                foreach (var file in message.Files)
                {
                    // Validate file size (2GB max)
                    if (file.Length > maxFileSize)
                    {
                        outputPort.Handle(new GenericResponse<UploadFileStorageResponse>(new UploadFileStorageResponse
                        {
                            Message = $"فایل {file.FileName} بزرگتر از حد مجاز (2GB) است. حجم فایل: {(file.Length / 1024.0 / 1024 / 1024):F2} GB",
                            Status = 400,
                        }));
                        return;
                    }

                    // Validate file extension
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        outputPort.Handle(new GenericResponse<UploadFileStorageResponse>(new UploadFileStorageResponse
                        {
                            Message = $"فرمت فایل {file.FileName} مجاز نیست. فرمت‌های مجاز: تصاویر، ویدئوها و فایل‌های صوتی",
                            Status = 400,
                        }));
                        return;
                    }

                    // Determine file type category for better organization
                    string fileCategory = GetFileCategory(fileExtension);

                    // Create unique filename to avoid conflicts
                    var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                    // Create organized subdirectories: Category/Year/Month
                    var yearFolder = DateTime.Now.ToString("yyyy");
                    var monthFolder = DateTime.Now.ToString("MM");
                    var finalUploadPath = Path.Combine(baseUploadPath, fileCategory, yearFolder, monthFolder);

                    if (!Directory.Exists(finalUploadPath))
                    {
                        Directory.CreateDirectory(finalUploadPath);
                    }

                    var filePath = Path.Combine(finalUploadPath, uniqueFileName);

                    // Save file to disk with progress support (optional)
                    await SaveFileWithProgressAsync(file, filePath);

                    // Generate URL for accessing the file
                    string fileUrl;
                    if (!string.IsNullOrEmpty(message.BaseUrl))
                    {
                        // Use custom base URL from request
                        var relativePath = Path.Combine(message.UploadPath ?? "uploads", fileCategory, yearFolder, monthFolder, uniqueFileName)
                            .Replace("\\", "/");
                        fileUrl = $"{message.BaseUrl.TrimEnd('/')}/{relativePath}";
                    }
                    else
                    {
                        // Generate relative URL path
                        var relativePath = Path.Combine(message.UploadPath ?? "uploads", fileCategory, yearFolder, monthFolder, uniqueFileName)
                            .Replace("\\", "/");
                        fileUrl = $"/{relativePath}";
                    }

                    // Add to response lists
                    uploadedUrls.Add(fileUrl);
                    uploadedFileNames.Add(file.FileName);
                    uploadedPaths.Add(filePath);
                    uploadedFileSizes.Add(file.Length);

                    // Save to database
                    _db.UserFileTBLs.Add(new UserFileTBL
                    {
                        FriendlyID = Statics.GenerateRandomRoomFriendlyID(),
                        IDUserTBL = message.GeneralHeaders.UserTBL.ID,
                        URL = fileUrl,
                        Title = file.FileName,
                        FileSize = file.Length,
                        FileType = fileCategory,
                        MimeType = file.ContentType,
                        Status = MainEntity_Status.FILE_INSERTED,
                        Type = MainEntity_Types.Normal,
                        DateInsert = DateTime.Now
                    });
                }

                await _db.SaveChangesAsync();

                // Success response
                outputPort.Handle(new GenericResponse<UploadFileStorageResponse>(new UploadFileStorageResponse
                {
                    URLs = uploadedUrls,
                    FileNames = uploadedFileNames,
                    SavedPaths = uploadedPaths,
                    FileSizes = uploadedFileSizes,
                    Message = $"{uploadedUrls.Count} فایل با موفقیت آپلود شد",
                    Status = 200,
                }));
            }
            catch (Exception ex)
            {
                // Error response
                outputPort.Handle(new GenericResponse<UploadFileStorageResponse>(new UploadFileStorageResponse
                {
                    Message = $"خطا در آپلود فایل‌ها: {ex.Message}",
                    Status = 500,
                }));
            }
        }

        private string GetFileCategory(string fileExtension)
        {
            // Image extensions
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg", ".ico", ".tiff", ".tif" };
            // Video extensions
            var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v", ".mpg", ".mpeg", ".3gp", ".ogv" };
            // Audio extensions
            var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".flac", ".aac", ".wma", ".opus" };

            if (imageExtensions.Contains(fileExtension))
                return "images";
            else if (videoExtensions.Contains(fileExtension))
                return "videos";
            else if (audioExtensions.Contains(fileExtension))
                return "audios";
            else
                return "others";
        }

        private async Task SaveFileWithProgressAsync(IFormFile file, string filePath)
        {
            // For large files (2GB), it's better to use buffered stream
            const int bufferSize = 81920; // 80KB buffer for better performance with large files

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
            using (var inputStream = file.OpenReadStream())
            {
                await inputStream.CopyToAsync(fileStream, bufferSize);
            }
        }
    }
    #endregion

    #region GetFileStorage
    public class GetFileStorageRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetFileStorageResponse>>
    {
        public string? FileID { get; set; } // Optional: get specific file by ID
        public string? UserID { get; set; } // Optional: filter by user ID
        public string? FileType { get; set; } // Optional: filter by type (images/videos/audios)
        public DateTime? FromDate { get; set; } // Optional: filter from date
        public DateTime? ToDate { get; set; } // Optional: filter to date
        public int PageNumber { get; set; } = 1; // Pagination
        public int PageSize { get; set; } = 20; // Pagination
        public string? SearchTerm { get; set; } // Optional: search by filename
        public string? SortBy { get; set; } = "DateInsert"; // DateInsert, FileName, FileSize
        public bool SortDescending { get; set; } = true;
    }

    public class GetFileStorageResponse : BaseParentResponce
    {
        public List<FileStorageDto> Files { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class FileStorageDto
    {
        public long ID { get; set; }
        public string FriendlyID { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileSizeFormatted => GetFileSizeFormatted();
        public string FileType { get; set; } // images/videos/audios
        public string MimeType { get; set; }
        public DateTime DateInsert { get; set; }
        public string DateInsertFormatted => DateInsert.ToString("yyyy/MM/dd HH:mm:ss");

        // User information who uploaded the file
        public UserInfoDto UploadedBy { get; set; }

        // File metadata
        public Dictionary<string, string> Metadata { get; set; }

        private string GetFileSizeFormatted()
        {
            if (FileSize >= 1024 * 1024 * 1024)
                return $"{FileSize / 1024.0 / 1024 / 1024:F2} GB";
            if (FileSize >= 1024 * 1024)
                return $"{FileSize / 1024.0 / 1024:F2} MB";
            if (FileSize >= 1024)
                return $"{FileSize / 1024.0:F2} KB";
            return $"{FileSize} B";
        }
    }

    public class UserInfoDto
    {
        public long ID { get; set; }
        public string UUID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
    }

    public interface IGetFileStorageUseCase
    {
        Task handleAsync(GetFileStorageRequest message, IOutPutPort<GenericResponse<GetFileStorageResponse>> outputPort);
    }

    public class GetFileStorageUseCase : IGetFileStorageUseCase
    {
        private readonly IDBContext _db;
        private readonly IConfiguration _configuration;

        public GetFileStorageUseCase(IDBContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task handleAsync(GetFileStorageRequest message, IOutPutPort<GenericResponse<GetFileStorageResponse>> outputPort)
        {
            try
            {
                // Check user permission (optional - based on your requirements)
                bool isAdmin = message.GeneralHeaders?.UserTBL?.VerifyUserPermissions("ftp") == true;
                bool isViewingOwnFiles = message.UserID != null && message.UserID == message.GeneralHeaders?.UserTBL?.ID.ToString();

                // If not admin and not viewing own files, restrict access
                if (!isAdmin && !isViewingOwnFiles && message.UserID != null)
                {
                    outputPort.Handle(new GenericResponse<GetFileStorageResponse>(new GetFileStorageResponse
                    {
                        Message = "شما دسترسی لازم برای مشاهده فایل‌های کاربران دیگر را ندارید",
                        Status = 403,
                    }));
                    return;
                }

                // Build query
                var query = _db.UserFileTBLs
                    .Include(f => f.UserTBL) // Assuming there's a navigation property to UserTBL
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(message.FileID))
                {
                    query = query.Where(f => f.FriendlyID == message.FileID || f.ID.ToString() == message.FileID);
                }

                if (!string.IsNullOrEmpty(message.UserID))
                {
                    query = query.Where(f => f.UserTBL.UUID == message.UserID || f.UserTBL.ID.ToString() == message.UserID);
                }
                else if (!isAdmin)
                {
                    // If not admin and no specific user requested, show only current user's files
                    query = query.Where(f => f.IDUserTBL == message.GeneralHeaders.UserTBL.ID);
                }

                if (!string.IsNullOrEmpty(message.FileType))
                {
                    query = query.Where(f => f.FileType == message.FileType);
                }

                if (message.FromDate.HasValue)
                {
                    query = query.Where(f => f.DateInsert >= message.FromDate.Value);
                }

                if (message.ToDate.HasValue)
                {
                    var toDate = message.ToDate.Value.Date.AddDays(1);
                    query = query.Where(f => f.DateInsert < toDate);
                }

                if (!string.IsNullOrEmpty(message.SearchTerm))
                {
                    query = query.Where(f => f.Title.Contains(message.SearchTerm) ||
                                            f.URL.Contains(message.SearchTerm) ||
                                            f.URL.Contains(message.SearchTerm));
                }

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply sorting
                query = ApplySorting(query, message.SortBy, message.SortDescending);

                // Apply pagination
                var files = await query
                    .Skip((message.PageNumber - 1) * message.PageSize)
                    .Take(message.PageSize)
                    .Select(f => new FileStorageDto
                    {
                        ID = f.ID,
                        FriendlyID = f.FriendlyID,
                        URL = f.URL,
                        Title = f.Title,
                        FileName = f.Title, // Or use original filename if stored separately
                        FileSize = f.FileSize,
                        FileType = f.FileType,
                        MimeType = f.MimeType,
                        DateInsert = f.DateInsert,
                        UploadedBy = new UserInfoDto
                        {
                            ID = f.UserTBL.ID,
                            UUID = f.UserTBL.UUID,
                            FullName = f.UserTBL.GetName(),
                            Username = f.UserTBL.Username,
                            Email = f.UserTBL.Email,
                            Avatar = f.UserTBL.AvatarPath,
                        },
                        Metadata = new Dictionary<string, string>
                        {
                        { "Extension", Path.GetExtension(f.URL) },
                        { "ContentType", f.MimeType ?? "Unknown" }
                        }
                    })
                    .ToListAsync();

                // Success response
                outputPort.Handle(new GenericResponse<GetFileStorageResponse>(new GetFileStorageResponse
                {
                    Files = files,
                    TotalCount = totalCount,
                    PageNumber = message.PageNumber,
                    PageSize = message.PageSize,
                    Message = $"{files.Count} فایل یافت شد",
                    Status = 200,
                }));
            }
            catch (Exception ex)
            {
                // Error response
                outputPort.Handle(new GenericResponse<GetFileStorageResponse>(new GetFileStorageResponse
                {
                    Message = $"خطا در دریافت فایل‌ها: {ex.Message}",
                    Status = 500,
                }));
            }
        }

        private IQueryable<UserFileTBL> ApplySorting(IQueryable<UserFileTBL> query, string sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "filename" => descending
                    ? query.OrderByDescending(f => f.Title)
                    : query.OrderBy(f => f.Title),
                "filesize" => descending
                    ? query.OrderByDescending(f => f.FileSize)
                    : query.OrderBy(f => f.FileSize),
                "filetype" => descending
                    ? query.OrderByDescending(f => f.FileType)
                    : query.OrderBy(f => f.FileType),
                _ => descending
                    ? query.OrderByDescending(f => f.DateInsert)
                    : query.OrderBy(f => f.DateInsert),
            };
        }
    }
    #endregion

    #region GetSingleFile
    public class GetSingleFileRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetSingleFileResponse>>
    {
        public string FileID { get; set; } // Required: file ID or FriendlyID
        public bool IncludeDownloadLink { get; set; } = false; // Generate temporary download link
    }

    public class GetSingleFileResponse : BaseParentResponce
    {
        public FileStorageDto File { get; set; }
        public string? TemporaryDownloadUrl { get; set; } // One-time or time-limited download URL
        public DateTime? DownloadUrlExpiry { get; set; }
    }

    public interface IGetSingleFileUseCase
    {
        Task handleAsync(GetSingleFileRequest message, IOutPutPort<GenericResponse<GetSingleFileResponse>> outputPort);
    }

    public class GetSingleFileUseCase : IGetSingleFileUseCase
    {
        private readonly IDBContext _db;
        private readonly IConfiguration _configuration;

        public GetSingleFileUseCase(IDBContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task handleAsync(GetSingleFileRequest message, IOutPutPort<GenericResponse<GetSingleFileResponse>> outputPort)
        {
            try
            {
                // Find the file
                var file = await _db.UserFileTBLs
                    .Include(f => f.UserTBL)
                    .FirstOrDefaultAsync(f => f.FriendlyID == message.FileID || f.ID.ToString() == message.FileID);

                if (file == null)
                {
                    outputPort.Handle(new GenericResponse<GetSingleFileResponse>(new GetSingleFileResponse
                    {
                        Message = "فایل مورد نظر یافت نشد",
                        Status = 404,
                    }));
                    return;
                }

                // Check permission
                bool isAdmin = message.GeneralHeaders?.UserTBL?.VerifyUserPermissions("ftp") == true;
                bool isOwner = file.IDUserTBL == message.GeneralHeaders?.UserTBL?.ID;

                if (!isAdmin && !isOwner)
                {
                    outputPort.Handle(new GenericResponse<GetSingleFileResponse>(new GetSingleFileResponse
                    {
                        Message = "شما دسترسی لازم برای مشاهده این فایل را ندارید",
                        Status = 403,
                    }));
                    return;
                }

                // Prepare response
                var fileDto = new FileStorageDto
                {
                    ID = file.ID,
                    FriendlyID = file.FriendlyID,
                    URL = file.URL,
                    Title = file.Title,
                    FileName = file.Title,
                    FileSize = file.FileSize,
                    FileType = file.FileType,
                    MimeType = file.MimeType,
                    DateInsert = file.DateInsert,
                    UploadedBy = new UserInfoDto
                    {
                        ID = file.UserTBL.ID,
                        UUID = file.UserTBL.UUID,
                        FullName = file.UserTBL.GetName(),
                        Username = file.UserTBL.Username,
                        Email = file.UserTBL.Email,
                        Avatar = file.UserTBL.AvatarPath,
                    },
                    Metadata = new Dictionary<string, string>
                {
                    { "Extension", Path.GetExtension(file.URL) },
                    { "ContentType", file.MimeType ?? "Unknown" }
                }
                };

                string? tempDownloadUrl = null;
                DateTime? expiry = null;

                // Generate temporary download link if requested
                if (message.IncludeDownloadLink)
                {
                    var token = GenerateDownloadToken(file.FriendlyID);
                    tempDownloadUrl = $"/api/files/download/{token}";
                    expiry = DateTime.Now.AddMinutes(30); // Link expires in 30 minutes
                }

                outputPort.Handle(new GenericResponse<GetSingleFileResponse>(new GetSingleFileResponse
                {
                    File = fileDto,
                    TemporaryDownloadUrl = tempDownloadUrl,
                    DownloadUrlExpiry = expiry,
                    Message = "فایل با موفقیت یافت شد",
                    Status = 200,
                }));
            }
            catch (Exception ex)
            {
                outputPort.Handle(new GenericResponse<GetSingleFileResponse>(new GetSingleFileResponse
                {
                    Message = $"خطا در دریافت فایل: {ex.Message}",
                    Status = 500,
                }));
            }
        }

        private string GenerateDownloadToken(string fileId)
        {
            // Simple token generation - you can enhance this with JWT or other methods
            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            // Store token in cache or database with expiry
            return $"{fileId}/{token}";
        }
    }
    #endregion
}