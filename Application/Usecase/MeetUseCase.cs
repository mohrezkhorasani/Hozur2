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
                                var lines = e.UserTBLToMeetingTBLLogs.First().OS
                                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(l => l.Trim())
                                    .Where(l => l.Contains("/$$"));

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
                                HasTakenCertificate = e.CertificateTBL?.FilePath != null,
                                NationalCode = _commonMethods.GetNationalCode(e),
                                Role = e.Role,
                                Email = e.UserTBL?.Email ?? "",
                                Phone = e.UserTBL?.PhoneNumber ?? "",
                                Gender = e.CertificateTBL == null || !e.CertificateTBL.IsMale.HasValue
                                         ? "تعیین نشده"
                                         : e.CertificateTBL.IsMale.Value ? "آقا" : "خانم",

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
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;
            if (_db.UserTBLToMeetingTBLs.Any(e => e.ID == message.ID))
            {
                var um = _db.UserTBLToMeetingTBLs.Where(e => e.ID == message.ID)
                    .Include(e => e.UserTBL)
                    .Include(e => e.MeetingTBL).ThenInclude(e => e.RoomTBL)
                    .First();
                string role = "شرکت‌کننده";
                if (um.Role == "MODERATOR")
                {
                    role = "ارائه دهنده";
                }
                if (!_db.CertificateTBLs.Any(e => e.IDUserTBLToMeetingTBL == um.ID))
                {
                    Statics.Log("GenerateCertificateWithUserMeetIDUseCase", $"{um.ID}");


                    _db.CertificateTBLs.Add(new CertificateTBL
                    {
                        Status = MainEntity_Status.CERT_FULL,
                        Type = MainEntity_Types.CERT_NORMAL,
                        Title = "گواهینامه حضور در جلسه",
                        IDUserTBLToMeetingTBL = um.ID,

                        Validation = true
                    });
                    _db.SaveChanges();
                }
                var cert = await _db.CertificateTBLs.FirstOrDefaultAsync(e => e.IDUserTBLToMeetingTBL == um.ID);
                cert.NationalCode = message.NationalCode;
                cert.IsMale = message.IsMale;
                cert.Validation = true;

                var wwwroot = hosting.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fontPath = "IranNastaliq";
                var bgpath = Path.Combine(wwwroot, "bg.png");
                var outputPath = Path.Combine(wwwroot, "pdf", "certificates", $@"certif-{um.ID}.pdf");



                var resultPath = WordService.CreateCertificatePdf(outputPath, fontPath, bgpath, um.UserNickName, message.IsMale, message.NationalCode,
                    um.MeetingTBL.RoomTBL.Title, um.MeetingTBL.Title, Statics.ConvertPersianMonthToNumber(um.MeetingTBL.JalaliDate).Replace("-", "/"),
                    role, um.ID.ToString());

                cert.FileURL = "https://vc.hekmateislami.com/";

                //string compressedPath = PdfCompressor.CompressPdf(@$"{outputPath}");

                cert.FilePath = Path.Combine("cert", $@"certif-{um.ID}.pdf");
                _db.CertificateTBLs.Update(cert);
                _db.SaveChanges();
                Statics.Log("GenerateCertificateWithUserMeetIDUseCase", "Ta upload");

                scpUploader.UploadFile(outputPath);
                Statics.Log("GenerateCertificateWithUserMeetIDUseCase", "oops");
                outputPort.Handle(new GenericResponse<GenerateCertificateWithUserMeetIDResponse>
     (new GenerateCertificateWithUserMeetIDResponse
     {
         Message = "ok",
         Status = 403,
         Link = Path.Join(cert.FileURL, cert.FilePath)

     }));
                return;
            }

            outputPort.Handle(new GenericResponse<GenerateCertificateWithUserMeetIDResponse>
       (new GenerateCertificateWithUserMeetIDResponse
       {
           Message = "ok",
           Status = 403,

       }));

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

                        if (!_db.RoomTBLs.Any(e => e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.FriendlyID == message.IDRoom) &&
                            !message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_admins"))
                        {
                            outputPort.Handle(new GenericResponse<StartNewMeetResponse>(new StartNewMeetResponse
                            {
                                Message = "دسترسی ندارید",
                                Status = 400
                            }));
                            return;
                        }

                        var room = _db.RoomTBLs.Where(e => e.FriendlyID == message.IDRoom || e.ID == idRoom).First();
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
                            e.Type == MainEntity_Types.USER_TEACHER);

                        if (existingDabir != null)
                        {
                            dabir = existingDabir;
                        }
                        else
                        {
                            dabir = new UserTBL
                            {
                                Firstname = data.PlanTBL.Dabir,
                                Type = MainEntity_Types.USER_TEACHER,
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
                            e.Status == MainEntity_Status.USER_ACTIVE);

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

}