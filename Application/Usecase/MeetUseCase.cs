using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Hosting;
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

}