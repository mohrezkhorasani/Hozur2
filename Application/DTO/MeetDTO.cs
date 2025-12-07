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


}