using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{

    public class DashboardBaseRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewUserResponse>>
    {

    }
    //#region NewUser
    //public class DashboardBaseRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewUserResponse>>
    //{
    //    public string? UUID { get; set; }
    //    public string? Firstname { get; set; }
    //    public string? Lastname { get; set; }
    //    public string? NationalCode { get; set; }
    //    public string? PhoneNumber { get; set; }
    //    public string? ExtId { get; set; }
    //    public string? Email { get; set; }
    //    public string? AllowedGUID { get; set; }
    //    public string? Role { get; set; }

    //}

    //public class NewUserResponse : BaseParentResponce
    //{
    //    public JWTTokenJSON Data { get; set; }
    //}
    //#endregion

    //#region NewUserMeetingLog
    //public class NewUserMeetingLogRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<NewUserMeetingLogResponse>>
    //{
    //    public string KeyLog { get; set; } //left,join, activity
    //    public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
    //    public string? OS { get; set; }
    //    public string Role { get; set; }
    //    public string MeetUserID { get; set; }
    //}

    //public class NewUserMeetingLogResponse : BaseParentResponce
    //{

    //}
    //#endregion

    //#region NewUserMeetingLogFromModerator
    //public class NewUserMeetingLogFromModeratorRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<NewUserMeetingLogFromModeratorResponse>>
    //{
    //    public string KeyLog { get; set; } //left,join, activity
    //    public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
    //    public string MeetUserID { get; set; }
    //    public string UserExtID { get; set; }
    //}

    //public class NewUserMeetingLogFromModeratorResponse : BaseParentResponce
    //{

    //}
    //#endregion

    ////NewMeeting 
    //#region NewMeeting
    //public class NewMeetingRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<NewMeetingResponse>>
    //{
    //    public string MeetID { get; set; }
    //    public string RoomID { get; set; }
    //}

    //public class NewMeetingResponse : BaseParentResponce
    //{
    //    public MeetJSON Data { get; set; }

    //}
    //#endregion


    ////UpdateMeeting 
    //#region UpdateMeeting
    //public class UpdateMeetingRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<UpdateMeetingResponse>>
    //{
    //    public string Teacher { get; set; }
    //    public string Title { get; set; }
    //    public string JalaliDate { get; set; }
    //    public string Explain { get; set; }
    //}
    //public class UpdateMeetingResponse : BaseParentResponce
    //{
    //    public MeetJSON Data { get; set; }

    //}
    //#endregion


    ////NewUserMeetings
    //#region NewUserMettings
    //public class NewUserMettingsRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<NewUserMettingsResponse>>
    //{
    //    public string MeetID { get; set; }
    //    public string Role { get; set; }
    //    public string NickName { get; set; }
    //    public string MeetUserID { get; set; }

    //}

    //public class NewUserMettingsResponse : BaseParentResponce
    //{
    //    public MeetJSON Data { get; set; }
    //    public string User { get; set; }

    //}
    //#endregion

    ////Get Teacher list [by Name]
    //#region GetTeacherList
    //public class GetTeacherListRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetTeacherListResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetTeacherListResponse : BaseParentResponce
    //{
    //    public List<MiniUserJSON> Data = new List<MiniUserJSON>();
    //}
    //#endregion

    ////Add Teacher 
    //#region NewTeacher
    //public class NewTeacherRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<NewTeacherResponse>>
    //{
    //    public string FullName { get; set; }
    //    public string? Username { get; set; }
    //}

    //public class NewTeacherResponse : BaseParentResponce
    //{
    //}
    //#endregion

    ////Add Teacher 
    //#region GuarantCertificate
    //public class GuarantCertificateRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GuarantCertificateResponse>>
    //{

    //}

    //public class GuarantCertificateResponse : BaseParentResponce
    //{
    //    public long Number{ get; set; }
    //}
    //#endregion

    ////Add Teacher 
    //#region NewRoom
    //public class NewRoomRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<NewRoomResponse>>
    //{
    //    public string FriendlyID { get; set; }
    //    public string? Room { get; set; }
    //}

    //public class NewRoomResponse : BaseParentResponce
    //{
    //}
    //#endregion

    ////Get Rooms
    //#region GetRooms
    //public class GetRoomsRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetRoomsResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetRoomsResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion

    ////Get Room with friendlyID
    //#region GetRoomByFriendlyID
    //public class GetRoomByFriendlyIDRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetRoomByFriendlyIDResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetRoomByFriendlyIDResponse : BaseParentResponce
    //{
    //    public MiniRoomJSON Data = new MiniRoomJSON();
    //}
    //#endregion


    ////Get Lives
    //#region GetLives
    //public class GetLivesRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetLivesResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetLivesResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion

    ////StartMeeting
    //#region StartMeeting
    //public class StartMeetingRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<StartMeetingResponse>>
    //{
    //    public string MeetiID { get; set; }
    //    public string MeetName { get; set; }
    //}

    //public class StartMeetingResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion

    ////GetMeetingURL
    //#region GetMeetingURL
    //public class GetMeetingURLRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetMeetingURLResponse>>
    //{
    //    public string MeetiID { get; set; }
    //    public string FullName { get; set; }
    //    public bool AsModerator { get; set; } = false;
    //    public string Password{ get; set; } = "";
    //}

    //public class GetMeetingURLResponse : BaseParentResponce
    //{
    //    public string Link { get; set; }
    //}
    //#endregion

    ////Add Teacher 
    #region GetRoomStats
    public class GetRoomStatsRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetRoomStatsResponse>>
    {
    }

    public class GetRoomStatsResponse : BaseParentResponce
    {
        public List<SharedOBJ_RoomStats> Rooms{ get; set; }

    }
    public class SharedOBJ_RoomStats
    {
        public string Title { get; set; }
        public string ID{ get; set; }
        public string LatestMeeting { get; set; }
        public int CountAllMeeting { get; set; }
        public int CountAllPartis { get; set; }
        public int CountAllCertsGenerated { get; set; }
        public int CountAllCertsReady { get; set; }
        public int CountAllTeacher { get; set; }
        public string Link{ get; set; }

    }
    #endregion




    //GetMeetings [ by title?,teacher-name?,jalali-date?, room-id? ]

    //GetUsersInAMeeting [ by meet-id ]

    //Get user meetings [ by national-code? | phone? | uuid? | email? | extId? | ]



    //CheckUser



    #region GetTrophies
    public class GetTrophiesRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetTrophiesResponse>>
    {
    }

    public class GetTrophiesResponse : BaseParentResponce
    {
        public int CountAllMeetings{ get; set; }
        public int CountWeekActiveTeacher { get; set; }
        public int CountAllPartis { get; set; }
        public int CountWeekMeeting { get; set; }
        public SharedOBJ_ServerDetails SharedOBJ_ServerDetails { get; set; }
        public SharedOBJ_ViewDetails SharedOBJ_ViewDetails { get; set; }
    }
    public class SharedOBJ_ServerDetails
    {
        public float MemoryUsage { get; set; }
        public float DiskUsage { get; set; }
        public float DatabaseUsage { get; set; }
        public float DatabaseRecords { get; set; }
    }
    public class SharedOBJ_ViewDetails
    {
        public int All{ get; set; }
        public int Today { get; set; }
        public int Unique { get; set; }
        public int Joint { get; set; }
        public double AverageTimeSpent { get; set; }
        public int Chrome{ get; set; }
        public int FireFox{ get; set; }
        public int Safari { get; set; }
        public int IE{ get; set; }
        public int Opera{ get; set; }
        public int OtherBrowser { get; set; }
        public int Mobile{ get; set; }
        public int Tablet{ get; set; }
        public int Desktop{ get; set; }
    }
    #endregion





    #region GetLastMeeting
    public class GetLastMeetingRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetLastMeetingResponse>>
    {
        public int Count { get; set; }
        public int Page { get; set; }
    }

    public class GetLastMeetingResponse : BaseParentResponce
    {
        public int CountAll { get; set; }
        public List<SharedOBJ_Rooms> SharedOBJ_Rooms { get; set; }
    }
    public class SharedOBJ_Rooms
    {
        public string MeetID { get; set; }
        public string MeetTitle { get; set; }
        public SharedOBJ_Partis RoomTitle { get; set; }
        public SharedOBJ_Partis TeacherTitle { get; set; }
        public SharedOBJ_Partis ModeratorTitle { get; set; }
        public DateTime Date { get; set; }
        public List<SharedOBJ_Partis> Partis { get; set; }
    }
    public class SharedOBJ_Partis
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Timer { get; set; }
    }
    #endregion



    #region GetLastCert
    public class GetLastCertRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetLastCertResponse>>
    {
        public int Count { get; set; }
        public int Page { get; set; }
    }

    public class GetLastCertResponse : BaseParentResponce
    {
        public int CountAll { get; set; }
        public List<SharedOBJ_Certificate> SharedOBJ_Certificates { get; set; }
    }
    public class SharedOBJ_Certificate
    {
        public SharedOBJ_Partis Room { get; set; }
        public SharedOBJ_Partis Parti { get; set; }
        public SharedOBJ_Partis Meet { get; set; }
        public int PresentTimeSeconds{ get; set; }
        public DateTime Date{ get; set; }
    }
    #endregion

    #region GetPartis
    public class GetPartisRequest : DashboardBaseRequest, IUseCaseRequest<GenericResponse<GetPartisResponse>>
    {
        public int Count { get; set; }
        public int Page { get; set; }
    }

    public class GetPartisResponse : BaseParentResponce
    {
        public int CountAll { get; set; }
        public List<SharedOBJ_PartisBig> SharedOBJ_Partis { get; set; }
    }
   
    public class SharedOBJ_PartisBig
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string LastAct{ get; set; }
        public int Cert { get; set; }
        public int Count { get; set; }
    }
    #endregion

}
