using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using Domain.Entity;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{



    #region NewRoomDashboard
    public class NewRoomDashboardRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewRoomDashboardResponse>>
    {
        public long? ID { get; set; }
        public MainEntity_Types RoomType { get; set; }
        public string Title { get; set; }

    }

    public class NewRoomDashboardResponse : BaseParentResponce
    {

    }
    #endregion

    #region GetMeetingFromRoomID
    public class GetMeetingFromRoomIDRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetMeetingFromRoomIDResponse>>
    {
        public string? ID { get; set; }

    }

    public class GetMeetingFromRoomIDResponse : BaseParentResponce
    {
        public List<SharedOBJ_Meeting> Data { get; set; }
    }

    public class SharedOBJ_Meeting
    {
        public string Title { get; set; }
        public string ID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int CountPartis { get; set; }
        public List<SharedOBJ_Partis> Teachers { get; set; }
        public SharedOBJ_Partis Starter { get; set; }

    }
    #endregion




}