using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
           Message = "دسترسی ندارید",
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
            if (_db.MeetingTBLs.Any(e => e.RoomFriendlyID == message.ID))
            {
                var room = _db.RoomTBLs.First(e => e.FriendlyID == message.ID);
                var meets = _db.MeetingTBLs
                    .Where(e => e.RoomFriendlyID == message.ID &&
                        e.Type == MainEntity_Types.ROOM_NORMAL &&
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
           Message = "دسترسی ندارید",
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
            if (_db.MeetingTBLs.Any(e => e.MeetID == message.ID))
            {
                var m = _db.MeetingTBLs
                    .Where(e => e.MeetID == message.ID)
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
                            EndDate = m.EndDate.HasValue ? Statics.GetPersianDate(m.EndDate.Value) : "در حال پخش",
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
           Message = "دسترسی ندارید",
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
            if (!message.Count.HasValue)
                message.Count = 50;
            if (!message.Page.HasValue)
                message.Page = 0;
            if (_db.MeetingTBLs.Any(e => e.MeetID == message.ID))
            {
                var m = _db.UserTBLToMeetingTBLs
                    .Where(e => e.MeetID == message.ID)
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
                        Members = m.Select(e => new SharedOBJ_PartisBiggest
                        {
                            ID = e.ID,
                            Name = e.UserNickName,
                            Timer = Statics.GetPrettyDateBySeconds(_commonMethods.GetTimerStayUserInMeet(e)),
                            CanTakeCertificate = e.CertificateTBL != null && e.CertificateTBL.Status == MainEntity_Status.CERT_FULL && e.CertificateTBL.Validation,
                            HasTakenCertificate = e.CertificateTBL?.FilePath != null,
                            NationalCode = _commonMethods.GetNationalCode(e),
                            Email = e.UserTBL.Email,
                            Phone = e.UserTBL.PhoneNumber,
                            Gender=e.CertificateTBL==null || !e.CertificateTBL.IsMale.HasValue? "تعیین نشده":e.CertificateTBL.IsMale.Value?"آقا":"خانم"

                        }).ToList()
                    }));
                return;
            }

            outputPort.Handle(new GenericResponse<GetMembersOfMeetResponse>
       (new GetMembersOfMeetResponse
       {
           Message = "دسترسی ندارید",
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
            if (_db.MeetingTBLs.Any(e => e.MeetID == message.ID))
            {
                var m = await _db.CertificateTBLs
                    .Where(c => c.UserTBLToMeetingTBL.MeetID == message.ID)
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
                            Gender=e.IsMale.HasValue? "تعیین نشده":e.IsMale.Value?"آقا":"خانم",
                            Date = e.DateInsert,
                            Meet = new SharedOBJ_Partis
                            {
                                ID = e.UserTBLToMeetingTBL.MeetingTBL.ID,
                                Name = e.UserTBLToMeetingTBL?.MeetingTBL?.Title,
                                Timer = Statics.GetPrettyDateBySeconds(
                                    (long)(new[] { e.UserTBLToMeetingTBL.MeetingTBL.EndDate.Value, e.UserTBLToMeetingTBL.MeetingTBL.DateInsert.AddHours(4) }.Min() -
                                        e.UserTBLToMeetingTBL.MeetingTBL.DateInsert).TotalSeconds)
                            },
                            Parti = new SharedOBJ_Partis
                            {
                                ID = e.IDUserTBLToMeetingTBL,
                                Name = e.UserTBLToMeetingTBL.UserNickName,
                                Timer = Statics.GetPrettyDateBySeconds(_commonMethods.GetTimerStayUserInMeet(e.UserTBLToMeetingTBL))
                            },
                            PresentTimeSeconds = _commonMethods.GetTimerStayUserInMeet(e.UserTBLToMeetingTBL),
                            Room = new SharedOBJ_Partis
                            {
                                ID = e.UserTBLToMeetingTBL.MeetingTBL.RoomTBL.ID,
                                Name = e.UserTBLToMeetingTBL.MeetingTBL.RoomTBL.Title,
                                Timer = ""
                            }
                        }).ToList()
                    }));
                return;
            }

            outputPort.Handle(new GenericResponse<GetMembersOfMeetOnlyCertificatedResponse>
       (new GetMembersOfMeetOnlyCertificatedResponse
       {
           Message = "دسترسی ندارید",
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
                    .Include(e=>e.MeetingTBL).ThenInclude(e=>e.RoomTBL)
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
         Message = "دسترسی ندارید",
         Status = 403,
         Link = Path.Join(cert.FileURL, cert.FilePath)

     }));
                return;
            }

            outputPort.Handle(new GenericResponse<GenerateCertificateWithUserMeetIDResponse>
       (new GenerateCertificateWithUserMeetIDResponse
       {
           Message = "دسترسی ندارید",
           Status = 403,

       }));

        }
    }
    #endregion

}