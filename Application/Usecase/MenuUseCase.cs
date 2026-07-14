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
    #region ApplicationVersion
    public class ApplicationVersionUseCase : IApplicationVersionUseCase
    {
        public IDBContext _repository { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public ApplicationVersionUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _repository = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(ApplicationVersionRequest message,
            IOutPutPort<GenericResponse<ApplicationVersionResponse>> outputPort)
        {

            outputPort.Handle(new GenericResponse<ApplicationVersionResponse>
       (new ApplicationVersionResponse
       {
           Message = "ok",
           Status = 200,

       }));

        }
    }
    #endregion
    #region GetRedirectURL    ( Duplicated! ) 
    public class GetRedirectURLUseCase : IGetRedirectURLUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb;

        public GetRedirectURLUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(GetRedirectURLRequest message,
            IOutPutPort<GenericResponse<GetRedirectURLResponse>> outputPort)
        {
            long idroom = 0;
            long.TryParse(message.RoomID, out idroom);
            if (!_db.RoomTBLs.Any(e => e.FriendlyID == message.RoomID || e.ID == idroom || e.BBB_MeetID == message.RoomID))
            {
                outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
        (new GetRedirectURLResponse
        {
            Message = "ok",
            Status = 200,
            Link = ""

        }));

                return;
            }

            var room = _db.RoomTBLs.First(e => e.FriendlyID == message.RoomID || e.ID == idroom || e.BBB_MeetID == message.RoomID);
            var lives = await bbb.GetLivesShort(new GetLivesRequest { });
            var livesID = lives.Select(e => e.BBB_MeetID).ToList();
            var meetsTBL = _db.MeetingTBLs.Where(e => e.RoomFriendlyID == room.FriendlyID &&
            (e.Status == Domain.Entity.MainEntity_Status.MEETING_LIVE || e.Status == Domain.Entity.MainEntity_Status.MEETING_STARTED_LATER))
                .Include(e => e.RoomTBL).ToList();
            MeetingTBL meetTBL = null;
            if (meetsTBL.Count > 1)
            {
                meetTBL = meetsTBL.First(e => livesID.Contains(e.RoomTBL.BBB_MeetID));
            }
            else if (meetsTBL.Count > 0)
            {
                meetTBL = meetsTBL.First();
            }
            else
            {
                outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
         (new GetRedirectURLResponse
         {
             Message = "ok",
             Status = 200,
             Link = $"https://vc.hekmateislami.com/rooms/{room.FriendlyID}"

         }));
                return;
            }
            if (meetTBL.Type == MainEntity_Types.MEET_API)
            {
                if (lives.Any(e => e.BBB_MeetID == room.BBB_MeetID))
                {
                    if (meetTBL.RequiredToEnterName)
                    {
                        outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
   (new GetRedirectURLResponse
   {
       Message = "ok",
       Status = 200,
       Link = $"https://vc.hekmateislami.com/rooms/{room.FriendlyID}/join?mode=api"

   }));
                        return;
                    }
                    else
                    {
                        var count = _db.UserTBLToMeetingTBLs.Count(e => e.MeetID == meetTBL.MeetID);
                        var join = await bbb.GetJoinUrlAsync(meetTBL, (1111111 + count).ToString(),Statics.GenerateSecretKey(66), "","","",false, "");
                        outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
   (new GetRedirectURLResponse
   {
       Message = "ok",
       Status = 200,
       Link = $"{join}"

   }));
                        return;
                    }
                }
                else
                {
                    outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
     (new GetRedirectURLResponse
     {
         Message = "ok",
         Status = 200,
         Link = $"https://vc.hekmateislami.com/rooms/{room.FriendlyID}/join"

     }));
                    return;
                }
            }
            else
            {
                outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
        (new GetRedirectURLResponse
        {
            Message = "ok",
            Status = 200,
            Link = $"https://vc.hekmateislami.com/rooms/{room.FriendlyID}/join"

        }));
                return;
            }

            outputPort.Handle(new GenericResponse<GetRedirectURLResponse>
               (new GetRedirectURLResponse
               {
                   Message = "ok",
                   Status = 200,

               }));

        }
    }
    #endregion


    #region GetRoomList
    public class GetRoomListUseCase : IGetRoomListUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb;

        public GetRoomListUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(GetRoomListRequest message,
            IOutPutPort<GenericResponse<GetRoomListResponse>> outputPort)
        {
            var rr = _db.RoomTBLs.Where(e => e.Title.Contains(message.q)).Select(e => new DTO.Models.MiniRoomJSON
            {
                Title = e.Title,
                CountTeacher=0,
                Count=0,
                Created=DateTime.Now,
                LastMeet=DateTime.Now,
                CountPartis=0,
                BBB_MeetID ="aa",
                RoomID = e.FriendlyID,
            }
            ).ToList();

            outputPort.Handle(new GenericResponse<GetRoomListResponse>
               (new GetRoomListResponse
               {
                   Message = "ok",
                   Status = 200,
                   Data = rr
               }));

        }
    }
    #endregion





}