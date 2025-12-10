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
        public string ShortLink { get; set; }
        public int CountPartis { get; set; }
        public List<SharedOBJ_Partis> Teachers { get; set; }
        public SharedOBJ_Partis Starter { get; set; }

    }
    #endregion



    #region GetMeetInfo
    public class GetMeetInfoRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetMeetInfoResponse>>
    {
        public string? ID { get; set; }

    }

    public class GetMeetInfoResponse : BaseParentResponce
    {
        public SharedOBJ_MeetingInfo Meet { get; set; }
    }

    public class SharedOBJ_MeetingInfo
    {
        public string Title { get; set; }
        public string ID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public List<SharedOBJ_Partis> Teachers { get; set; }
        public int CountPartis { get; set; }
        public int CountCertificateds { get; set; }
        public SharedOBJ_Partis Room { get; set;  }
    }
    #endregion

    #region GetMembersOfMeet
    public class GetMembersOfMeetRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetMembersOfMeetResponse>>
    {
        public string? ID { get; set; }

    }

    public class GetMembersOfMeetResponse : BaseParentResponce
    {
        public List<SharedOBJ_PartisBiggest> Members { get; set; }
    }
    public class SharedOBJ_PartisBiggest
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Timer { get; set; }
        public string Phone{ get; set; }
        public string NationalCode { get; set; }
        public string Email { get; set; }
        public string Device { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
        public string Gender { get; set; }
        public bool CanTakeCertificate { get; set; }
        public bool HasTakenCertificate { get; set; }
    }
    #endregion


    #region GetMembersOfMeetOnlyCertificated
    public class GetMembersOfMeetOnlyCertificatedRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetMembersOfMeetOnlyCertificatedResponse>>
    {
        public string? ID { get; set; }

    }

    public class GetMembersOfMeetOnlyCertificatedResponse : BaseParentResponce
    {
        public List<SharedOBJ_Certificate> Certificates { get; set; }
    }

    #endregion

    #region GenerateCertificateWithUserMeetID
    public class GenerateCertificateWithUserMeetIDRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GenerateCertificateWithUserMeetIDResponse>>
    {
        public long ID { get; set; }
        public string NationalCode { get; set; }
        public bool IsMale{ get; set; }

    }

    public class GenerateCertificateWithUserMeetIDResponse : BaseParentResponce
    {
        public string Link{ get; set; }
    }

    #endregion


}