using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Domain.Entity;
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
                var meets = _db.MeetingTBLs.Where(e => e.RoomFriendlyID == message.ID && e.Type == MainEntity_Types.ROOM_NORMAL && e.RoomTBL.Type == MainEntity_Types.ROOM_NORMAL)
                    .OrderByDescending(e=>e.ID)
                    .Skip(message.Page.Value * message.Count.Value)
                    .Take(message.Count.Value)
                    .Include(e=>e.RoomTBL)
                    .Include(e=>e.UserTBLToMeetingTBLs).ThenInclude(e=>e.UserTBL)
                    .Include(e=>e.UserTBLToMeetingTBLs).ThenInclude(e=>e.CertificateTBL)
                    .Include(e=>e.Creator)
                    .Include(e=>e.Teacher)
                    .ToList();

                outputPort.Handle(new GenericResponse<GetMeetingFromRoomIDResponse>
           (new GetMeetingFromRoomIDResponse
           {
               Message = "دسترسی ندارید",
               Status = 403,
               Data = meets.Select(e => new SharedOBJ_Meeting
               {
                   CountPartis = e.UserTBLToMeetingTBLs
                    .Count(),
                   EndDate = Statics.GetPersianDate(e.EndDate.HasValue ? e.EndDate.Value : e.DateInsert.AddHours(2)),
                   StartDate = Statics.GetPersianDate(e.DateInsert),
                   Starter = new SharedOBJ_Partis
                   {
                       ID = e.Creator.ID,
                       Name = e.UserTBLToMeetingTBLs.Where(r => r.UserTBL.ID == e.Creator.ID).First().UserNickName,
                       Timer = "0"
                   },
                   Teachers = e.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).Select(r => new SharedOBJ_Partis
                   {
                       ID= r.ID,
                       Name=r.UserTBL.Firstname+" "+r.UserTBL?.LastName,
                       Timer= "0"
                   }
                   
                   ).ToList()

               }).ToList()
           }));
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

}