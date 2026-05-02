using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using Domain.Entity;
using Microsoft.AspNetCore.Http;
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
        public string? RoomName { get; set; }

    }

    public class NewUserResponse : BaseParentResponce
    {
        public JWTTokenJSON Data { get; set; }
    }
    #endregion
    #region LoginUserPanel
    public class LoginUserPanelRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<LoginUserPanelResponse>>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginUserPanelResponse : BaseParentResponce
    {
        public JWTTokenJSON Data { get; set; }
    }
    #endregion

    #region SignInUserPanel
    public class SignInUserPanelRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<SignInUserPanelResponse>>
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Explain { get; set; }
    }

    public class SignInUserPanelResponse : BaseParentResponce
    {
        public JWTTokenJSON Data { get; set; }
    }
    #endregion

    #region VerifyToken
    public class VerifyTokenRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<VerifyTokenResponse>>
    {
    }

    public class VerifyTokenResponse : BaseParentResponce
    {
        public MiniUserJSON Data { get; set; }
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
        public string[] Teacher { get; set; }
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
        public long Number{ get; set; }
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
        public bool UseCache { get; set; } = false;
        public string Query { get; set; }
        public string? UserName { get; set; }
    }

    public class GetRoomsResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion
    //Get Rooms
    #region GetRoomsPublic
    public class GetRoomsPublicRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomsPublicResponse>>
    {
        public string Query { get; set; }
        public string? UserName { get; set; }
    }

    public class GetRoomsPublicResponse : BaseParentResponce
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
        public bool Public { get; set; } = true;
    }

    public class GetLivesResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion

    #region GetLivesPublic
    public class GetLivesPublicRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetLivesPublicResponse>>
    {
        public string Query { get; set; }
    }

    public class GetLivesPublicResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion

    #region GetLiveWithID
    public class GetLiveWithIDRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetLiveWithIDResponse>>
    {
        public string ID { get; set; }
    }

    public class GetLiveWithIDResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion


    #region NewQuestion
    public class NewQuestionRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewQuestionResponse>>
    {
        public string Content { get; set; }
    }

    public class NewQuestionResponse : BaseParentResponce
    {
    }
    #endregion
    #region GetQuestionOfMeet
    public class GetQuestionOfMeetRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetQuestionOfMeetResponse>>
    {
    }

    public class GetQuestionOfMeetResponse : BaseParentResponce
    {
        public List<SharedOBJ_Question> Questions { get; set; }
    }
    public class SharedOBJ_Question
    {
        public long ID { get; set; }
        public string Question { get; set; }
        public string NickName { get; set; }
        public bool Answered { get; set; }
    }
    #endregion

    #region QuestionAnswered
    public class QuestionAnsweredRequest : NewUserRequest, IUseCaseRequest<GenericResponse<QuestionAnsweredResponse>>
    {
        public long ID { get; set; }
    }

    public class QuestionAnsweredResponse : BaseParentResponce
    {
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

    //Add Teacher 
    #region GenerateCertificate
    public class GenerateCertificateRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GenerateCertificateResponse>>
    {
        public string NationalCode { get; set; }
        public bool Sex { get; set; }//True = MALE //FALSE = FEMALE
    }

    public class GenerateCertificateResponse : BaseParentResponce
    {
        public string Link { get; set; }
    }
    #endregion

    #region EndTheMeeting
    public class EndTheMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<EndTheMeetingResponse>>
    {
    }

    public class EndTheMeetingResponse : BaseParentResponce
    {

    }
    #endregion


    //GetMeetings [ by title?,teacher-name?,jalali-date?, room-id? ]

    //GetUsersInAMeeting [ by meet-id ]

    //Get user meetings [ by national-code? | phone? | uuid? | email? | extId? | ]

 

    //CheckUser

    #region NewPlanRoom
    public class NewPlanRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewPlanRoomResponse>>
    {
        public string FriendlyID{ get; set; }
        public string? Title { get; set; }
        public string? Teacher { get; set; }
        public string? Dabir { get; set; }
        public DateTime StartDate{ get; set; }
    }

    public class NewPlanRoomResponse : BaseParentResponce
    {
    }
    #endregion

    #region GetSlideShow
    public class GetSlideShowRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetSlideShowResponse>>
    {
        public string Page { get; set; }
        
    }

    public class GetSlideShowResponse : BaseParentResponce
    {
        public List<MiniSlideShowJSON> Data = new List<MiniSlideShowJSON>();
    }
    #endregion

    #region EditSlideShow
    public class EditSlideShowRequest : NewUserRequest, IUseCaseRequest<GenericResponse<EditSlideShowResponse>>
    {
        public long IDSlideshow{ get; set; }
        public int Position{ get; set; } //0 = disable
        public string? Href{ get; set; } //0 = disable
        
    }

    public class EditSlideShowResponse : BaseParentResponce
    {
        public List<MiniSlideShowJSON> Data = new List<MiniSlideShowJSON>();
    }
    #endregion
    #region InsertSlideShow
    public class InsertSlideShowRequest : NewUserRequest, IUseCaseRequest<GenericResponse<InsertSlideShowResponse>>
    {
        public int Position{ get; set; } //0 = disable
        public string? Title{ get; set; } //0 = disable
        public string? Href{ get; set; } //0 = disable
        public IFormFile? File{ get; set; } //0 = disable

    }

    public class InsertSlideShowResponse : BaseParentResponce
    {
        public List<MiniSlideShowJSON> Data = new List<MiniSlideShowJSON>();
    }
    #endregion

    #region Contactus
    public class ContactusRequest : NewUserRequest, IUseCaseRequest<GenericResponse<ContactusResponse>>
    {
        public string Name { get; set; }
        public string Email{ get; set; }
        public string? Phone { get; set; }
        public string Text { get; set; }
    }

    public class ContactusResponse : BaseParentResponce
    {
    }
    #endregion

    #region GetProfileInfo
    public class GetProfileInfoRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetProfileInfoResponse>>
    {
        public string Username { get; set; }
    }

    public class GetProfileInfoResponse : BaseParentResponce
    {
        public MiniProfileJSON Data { get; set; }
    }
    #endregion

    #region FollowProfile
    public class FollowProfileRequest : NewUserRequest, IUseCaseRequest<GenericResponse<FollowProfileResponse>>
    {
        public string Username { get; set; }
    }

    public class FollowProfileResponse : BaseParentResponce
    {
        public int NewFollowersCount { get; set; }
        public bool IsFollowing{ get; set; }
    }
    #endregion

    #region GetHekmatShobe
    public class GetHekmatShobeRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetHekmatShobeResponse>>
    {
        public string Query { get; set; }
    }

    public class GetHekmatShobeResponse : BaseParentResponce
    {
        public List<MiniShobeJSON> Data = new List<MiniShobeJSON>();
    }
    #endregion
    
    #region GetPlanRoom
    public class GetPlanRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPlanRoomResponse>>
    {
        public string FriendlyID { get; set; }
    }

    public class GetPlanRoomResponse : BaseParentResponce
    {
        public List<MiniPlanJSON> Data = new List<MiniPlanJSON>();
    }
    #endregion
    #region GetPlanRoomPublic
    public class GetPlanRoomPublicRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPlanRoomPublicResponse>>
    {
    }

    public class GetPlanRoomPublicResponse : BaseParentResponce
    {
        public List<MiniPlanJSON> Data = new List<MiniPlanJSON>();
    }
    #endregion
    #region GetLatestMeetings
    public class GetLatestMeetingsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetLatestMeetingsResponse>>
    {
    }

    public class GetLatestMeetingsResponse : BaseParentResponce
    {
        public List<MiniPlanJSON> Data = new List<MiniPlanJSON>();
    }
    #endregion

    #region DeletePlanRoom
    public class DeletePlanRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<DeletePlanRoomResponse>>
    {
        public long IDPlan { get; set; }
    }

    public class DeletePlanRoomResponse : BaseParentResponce
    {
    }
#endregion

    //Admins
    #region AdminGetUserList
    public class AdminGetUserListRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminGetUserListResponse>>
    {
        public string Query { get; set; }
        public OrderBy OrderBy { get; set; }

    }

    public class AdminGetUserListResponse : BaseParentResponce
    {
        public List<MiniUserJSON> Data = new List<MiniUserJSON>();
    }
    #endregion
    #region AdminGetUserRoomList
    public class AdminGetUserRoomListRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminGetUserRoomListResponse>>
    {
        public string Query { get; set; }
        public long OwnerID{ get; set; }
        public OrderBy OrderBy { get; set; }

    }

    public class AdminGetUserRoomListResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion
    #region AdminChangeRole
    public class AdminChangeRoleRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminChangeRoleResponse>>
    {
        public MainEntity_Types Role{ get; set; }
        public long UserId { get; set; }
    }

    public class AdminChangeRoleResponse : BaseParentResponce
    {
        public MiniUserJSON Data = new MiniUserJSON();
    }
    #endregion
    #region ChangeAvatarOfRoom
    public class ChangeAvatarOfRoomRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<ChangeAvatarOfRoomResponse>>
    {
        public string RoomID { get; set; }
        public string AvatarPath{ get; set; }
    }

    public class ChangeAvatarOfRoomResponse : BaseParentResponce
    {
    }
    #endregion

    #region AdminCreateRoom
    public class AdminCreateRoomRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminCreateRoomResponse>>
    {
        public long? ID { get; set; }
        public string Title{ get; set; }
        public long UserID{ get; set; }
        public bool IsPublic { get; set; } = false; 
        public bool IsRequiredLogin { get; set; } = false;
        public bool IsRequiredAccept { get; set; } = false;
        public bool IsRecordable { get; set; } = false;

    }

    public class AdminCreateRoomResponse : BaseParentResponce
    {
        public MiniRoomJSON Data = new MiniRoomJSON();
    }
    #endregion
}
