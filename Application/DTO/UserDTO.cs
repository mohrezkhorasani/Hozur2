using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{
    #region NewUser
    public class NewUserRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewUserResponse>>
    {
        public string? UUID { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? NationalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ExtId { get; set; }
        public string? Email { get; set; }
        public string? AllowedGUID { get; set; }
        public string? Role { get; set; }

    }

    public class NewUserResponse : BaseParentResponce
    {
        public JWTTokenJSON Data { get; set; }
    }
    #endregion

    #region NewUserMeetingLog
    public class NewUserMeetingLogRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewUserMeetingLogResponse>>
    {
        public string KeyLog { get; set; } //left,join, activity
        public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
        public string? OS { get; set; }
        public string Role { get; set; }
        public string MeetUserID { get; set; }
    }

    public class NewUserMeetingLogResponse : BaseParentResponce
    {

    }
    #endregion

    #region NewUserMeetingLogFromModerator
    public class NewUserMeetingLogFromModeratorRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewUserMeetingLogFromModeratorResponse>>
    {
        public string KeyLog { get; set; } //left,join, activity
        public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
        public string MeetUserID { get; set; }
        public string UserExtID { get; set; }
    }

    public class NewUserMeetingLogFromModeratorResponse : BaseParentResponce
    {

    }
    #endregion

    //NewMeeting 
    #region NewMeeting
    public class NewMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewMeetingResponse>>
    {
        public string MeetID { get; set; }
        public string RoomID { get; set; }
    }

    public class NewMeetingResponse : BaseParentResponce
    {
        public MeetJSON Data { get; set; }

    }
    #endregion


    //UpdateMeeting 
    #region UpdateMeeting
    public class UpdateMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<UpdateMeetingResponse>>
    {
        public string Teacher { get; set; }
        public string Title { get; set; }
        public string JalaliDate { get; set; }
        public string Explain { get; set; }
    }
    public class UpdateMeetingResponse : BaseParentResponce
    {
        public MeetJSON Data { get; set; }

    }
    #endregion


    //NewUserMeetings
    #region NewUserMettings
    public class NewUserMettingsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewUserMettingsResponse>>
    {
        public string MeetID { get; set; }
        public string Role { get; set; }
        public string NickName { get; set; }
        public string MeetUserID { get; set; }

    }

    public class NewUserMettingsResponse : BaseParentResponce
    {
        public MeetJSON Data { get; set; }
        public string User { get; set; }

    }
    #endregion

    //Get Teacher list [by Name]
    #region GetTeacherList
    public class GetTeacherListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetTeacherListResponse>>
    {
        public string Query { get; set; }
    }

    public class GetTeacherListResponse : BaseParentResponce
    {
        public List<MiniUserJSON> Data = new List<MiniUserJSON>();
    }
    #endregion

    //Add Teacher 
    #region NewTeacher
    public class NewTeacherRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewTeacherResponse>>
    {
        public string FullName { get; set; }
        public string? Username { get; set; }
    }

    public class NewTeacherResponse : BaseParentResponce
    {
    }
    #endregion

    //Add Teacher 
    #region GuarantCertificate
    public class GuarantCertificateRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GuarantCertificateResponse>>
    {
    }

    public class GuarantCertificateResponse : BaseParentResponce
    {
    }
    #endregion

    //Add Teacher 
    #region NewRoom
    public class NewRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewRoomResponse>>
    {
        public string FriendlyID { get; set; }
        public string? Room { get; set; }
    }

    public class NewRoomResponse : BaseParentResponce
    {
    }
    #endregion

    //Get Rooms
    #region GetRooms
    public class GetRoomsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomsResponse>>
    {
        public string Query { get; set; }
    }

    public class GetRoomsResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion

    //Get Room with friendlyID
    #region GetRoomByFriendlyID
    public class GetRoomByFriendlyIDRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomByFriendlyIDResponse>>
    {
        public string Query { get; set; }
    }

    public class GetRoomByFriendlyIDResponse : BaseParentResponce
    {
        public MiniRoomJSON Data = new MiniRoomJSON();
    }
    #endregion


    //Get Lives
    #region GetLives
    public class GetLivesRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetLivesResponse>>
    {
        public string Query { get; set; }
    }

    public class GetLivesResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion

    //StartMeeting
    #region StartMeeting
    public class StartMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<StartMeetingResponse>>
    {
        public string MeetiID { get; set; }
        public string MeetName { get; set; }
    }

    public class StartMeetingResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion

    //GetMeetingURL
    #region GetMeetingURL
    public class GetMeetingURLRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetMeetingURLResponse>>
    {
        public string MeetiID { get; set; }
        public string FullName { get; set; }
        public bool AsModerator { get; set; } = false;
        public string Password{ get; set; } = "";
    }

    public class GetMeetingURLResponse : BaseParentResponce
    {
        public string Link { get; set; }
    }
    #endregion


    //GuarantCertificate

    //GetCertificateFile

    //GetMeetings [ by title?,teacher-name?,jalali-date?, room-id? ]

    //GetRooms [ by title ]

    //GetUsersInAMeeting [ by meet-id ]

    //Get user meetings [ by national-code? | phone? | uuid? | email? | extId? | ]



    //CheckUser





}
