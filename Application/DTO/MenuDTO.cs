using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{

   


    #region ApplicationVersion
    public class ApplicationVersionRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<ApplicationVersionResponse>>
    {
        public int VersionCode { get; set; }

    }

    public class ApplicationVersionResponse : BaseParentResponce
    {
        
    }
    #endregion




    #region GetRedirectURL
    public class GetRedirectURLRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetRedirectURLResponse>>
    {
        public string RoomID { get; set; }
        public string UUID { get; set; }
    }

    public class GetRedirectURLResponse : BaseParentResponce
    {
        public string Link { get; set; }
    }
    #endregion

    #region GetRoomList
    public class GetRoomListRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetRoomListResponse>>
    {
        public string q { get; set; }
    }

    public class GetRoomListResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion





}
