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


    #region GetAttendsInBBBForAdmin
    public class GetAttendsInBBBForAdminRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetAttendsInBBBForAdminResponse>>
    {

    }
    public class GetAttendsInBBBForAdminResponse : BaseParentResponce
    {
        public Object Data { get; set; }

    }
    #endregion
    #region SencCheckListOfAttends
    public class SencCheckListOfAttendsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<SencCheckListOfAttendsResponse>>
    {
        public List<AttendListItem> Attends { get; set; }
    }
    public class AttendListItem
    {
        public long ID { get; set; }
        public bool IsMember { get; set; }
    }
    public class SencCheckListOfAttendsResponse : BaseParentResponce
    {

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
        public long Number { get; set; }
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
        public bool IsSubscribedByUser { get; set; } = false;
        public int? SubscriptionStatus { get; set; } = null;
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
    //Get Lives
    #region GetLivesOfProfile
    public class GetLivesOfProfileRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetLivesOfProfileResponse>>
    {
        public string Query { get; set; }
        public string Username { get; set; }
        public bool Public { get; set; } = true;
    }

    public class GetLivesOfProfileResponse : BaseParentResponce
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
        public bool IsLive { get; set; }
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


    #region GetRoomCalendarActivity
    public class GetRoomCalendarActivityRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetRoomCalendarActivityResponse>>
    {
        public string FriendlyID { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
    }

    public class GetRoomCalendarActivityResponse : BaseParentResponce
    {
        public List<MiniCalendarActivity> Data = new List<MiniCalendarActivity>();
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
        public string NationalCode { get; set; }
        public bool AsModerator { get; set; } = false;
        public string Password { get; set; } = "";
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

    #region GetTheMembers
    public class GetTheMembersRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetTheMembersResponse>>
    {
    }

    public class GetTheMembersResponse : BaseParentResponce
    {
        public List<MiniUserToMeetJSON> Data { get; set; }

    }
    #endregion
    #region EndTheMeeting
    public class EndTheMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<EndTheMeetingResponse>>
    {
        public bool SendToNajm { get; set; } = false;
    }

    public class EndTheMeetingResponse : BaseParentResponce
    {
    }
    #endregion
    #region EndTheMeetingWithAPI
    public class EndTheMeetingWithAPIRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<EndTheMeetingWithAPIResponse>>
    {
        public string FriendlyID { get; set; }
        public int Type { get; set; }
    }

    public class EndTheMeetingWithAPIResponse : BaseParentResponce
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
        public string FriendlyID { get; set; }
        public string? Title { get; set; }
        public string? Teacher { get; set; }
        public string? Dabir { get; set; }
        public bool HasCert { get; set; } = true;
        public DateTime StartDate { get; set; }
        public IFormFile? Banner { get; set; }
        public IFormFile? BannerPresentation { get; set; }
        public bool IsBannerAndPresentationSame { get; set; } = true;
    }

    public class NewPlanRoomResponse : BaseParentResponce
    {
    }
    #endregion

    #region NewReservation
    public class NewReservationRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewReservationResponse>>
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string NationalCode { get; set; }
        public long PlanID { get; set; }
    }

    public class NewReservationResponse : BaseParentResponce
    {
    }
    #endregion

    #region GetReservation
    public class GetReservationRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetReservationResponse>>
    {
        public long PlanID { get; set; }
    }

    public class GetReservationResponse : BaseParentResponce
    {
        public MiniReservationJSON Data { get; set; }
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
        public long IDSlideshow { get; set; }
        public int Position { get; set; } //0 = disable
        public string? Href { get; set; } //0 = disable

    }

    public class EditSlideShowResponse : BaseParentResponce
    {
        public List<MiniSlideShowJSON> Data = new List<MiniSlideShowJSON>();
    }
    #endregion
    #region InsertSlideShow
    public class InsertSlideShowRequest : NewUserRequest, IUseCaseRequest<GenericResponse<InsertSlideShowResponse>>
    {
        public int Position { get; set; } //0 = disable
        public string? Title { get; set; } //0 = disable
        public string? Href { get; set; } //0 = disable
        public IFormFile? File { get; set; } //0 = disable

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
        public string Email { get; set; }
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
        public bool IsFollowing { get; set; }
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


    #region GetPollList
    public class GetPollListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPollListResponse>>
    {
    }

    public class GetPollListResponse : BaseParentResponce
    {
        public List<PollJSON> Data = new List<PollJSON>();
    }
    #endregion
    #region GetUnansweredPollList
    public class GetUnansweredPollListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetUnansweredPollListResponse>>
    {
    }

    public class GetUnansweredPollListResponse : BaseParentResponce
    {
        public List<PollJSON> Data = new List<PollJSON>();
    }
    #endregion
    #region AnswerPollList
    public class AnswerPollListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<AnswerPollListResponse>>
    {
        public List<AnswerItemRequestJSON> AnswerItemRequestJSONs { get; set; }

    }
    public class AnswerItemRequestJSON
    {
        public long ID { get; set; }
        public string Content { get; set; }
    }
    public class AnswerPollListResponse : BaseParentResponce
    {
    }
    #endregion

    #region UpdatePresentMinToCertOfMeet
    public class UpdatePresentMinToCertOfMeetRequest : NewUserRequest, IUseCaseRequest<GenericResponse<UpdatePresentMinToCertOfMeetResponse>>
    {
        public int Minutes { get; set; }

    }
    public class UpdatePresentMinToCertOfMeetResponse : BaseParentResponce
    {
    }
    #endregion




    #region SendOTPConfirmation
    public class SendOTPConfirmationRequest : NewUserRequest, IUseCaseRequest<GenericResponse<SendOTPConfirmationResponse>>
    {
        public string? Phone { get; set; }
        public string? NationalCode { get; set; }
    }

    public class SendOTPConfirmationResponse : BaseParentResponce
    {
        public string Guid { get; set; }
        public string? MaskedPhone { get; set; }
        public string? Phone { get; set; }
    }
    #endregion
    #region ConfirmOTP
    public class ConfirmOTPRequest : NewUserRequest, IUseCaseRequest<GenericResponse<ConfirmOTPResponse>>
    {
        public string Code { get; set; }
        public string Guid { get; set; }
    }

    public class ConfirmOTPResponse : BaseParentResponce
    {
        public string Token { get; set; }
        public string? Fullname { get; set; }
        public string? NationalCode { get; set; }
        public string? MaskedPhone { get; set; }
        public string? Phone { get; set; }
    }
    #endregion

    #region SendOTPRoomSubscribedUser
    public class SendOTPRoomSubscribedUserRequest : NewUserRequest, IUseCaseRequest<GenericResponse<SendOTPRoomSubscribedUserResponse>>
    {
        public string NationalCode { get; set; }
    }

    public class SendOTPRoomSubscribedUserResponse : BaseParentResponce
    {
        public string Guid { get; set; }
        public string? MaskedPhone { get; set; }
        public string? Phone { get; set; }
    }
    #endregion
    #region ConfirmOTPRoomSubscribedUser
    public class ConfirmOTPRoomSubscribedUserRequest : NewUserRequest, IUseCaseRequest<GenericResponse<ConfirmOTPRoomSubscribedUserResponse>>
    {
        public string Code { get; set; }
        public string Guid { get; set; }
    }

    public class ConfirmOTPRoomSubscribedUserResponse : BaseParentResponce
    {
        public string Token { get; set; }
        public string? Fullname { get; set; }
        public string? NationalCode { get; set; }
        public string? MaskedPhone { get; set; }
        public string? Phone { get; set; }
    }
    #endregion

    #region SendOTPRoomSubscribedRegistration
    public class SendOTPRoomSubscribedRegistrationRequest : NewUserRequest, IUseCaseRequest<GenericResponse<SendOTPRoomSubscribedRegistrationResponse>>
    {
        public string NationalCode { get; set; }
        public string Phone { get; set; }
    }

    public class SendOTPRoomSubscribedRegistrationResponse : BaseParentResponce
    {
        public string Guid { get; set; }
        public string? MaskedPhone { get; set; }
        public string? Phone { get; set; }
    }
    #endregion

    #region ConfirmOTPRoomSubscribedRegistration
    public class ConfirmOTPRoomSubscribedRegistrationRequest : NewUserRequest, IUseCaseRequest<GenericResponse<ConfirmOTPRoomSubscribedRegistrationResponse>>
    {
        public string Code { get; set; }
        public string Guid { get; set; }
        public string Phone { get; set; }
        public string NationalCode { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string RoomId { get; set; }
        public string? ReceiptUrl { get; set; }
    }

    public class ConfirmOTPRoomSubscribedRegistrationResponse : BaseParentResponce
    {
        public string Token { get; set; }
        public string? Fullname { get; set; }
    }
    #endregion

    #region VerifyCertificateUser
    public class VerifyCertificateUserRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<VerifyCertificateUserResponse>>
    {
        public string? Phone { get; set; }
        public string? NationalCode { get; set; }
        public string? Name { get; set; }
        public string? UUID { get; set; }
    }

    public class VerifyCertificateUserResponse : BaseParentResponce
    {
        public string? Guid { get; set; }
        public string? MaskedPhone { get; set; }
        public string? Phone { get; set; }
    }
    #endregion

    #region ConfirmCertVerification
    public class ConfirmCertVerificationRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<ConfirmCertVerificationResponse>>
    {
        public string Guid { get; set; }
        public string Code { get; set; }
        public string? Phone { get; set; }
        public string? NationalCode { get; set; }
        public string? Name { get; set; }
    }

    public class ConfirmCertVerificationResponse : BaseParentResponce
    {
        public string? Token { get; set; }
        public string? Fullname { get; set; }
        public string? NationalCode { get; set; }
        public string? Phone { get; set; }
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
        public long OwnerID { get; set; }
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
        public MainEntity_Types Role { get; set; }
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
        public string AvatarPath { get; set; }
    }

    public class ChangeAvatarOfRoomResponse : BaseParentResponce
    {
    }
    #endregion

    #region AdminCreateRoom
    public class AdminCreateRoomRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminCreateRoomResponse>>
    {
        public long? ID { get; set; }
        public string Title { get; set; }
        public long UserID { get; set; }
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

    #region GetAllRooms
    public class GetAllRoomsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetAllRoomsResponse>>
    {
        public string Query { get; set; }
        public string? UserName { get; set; }
    }

    public class GetAllRoomsResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    }
    #endregion

    #region GetAllRoomsMini
    public class GetAllRoomsMiniRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetAllRoomsMiniResponse>>
    {
        public string Query { get; set; }
        public string? UserName { get; set; }
    }

    public class GetAllRoomsMiniResponse : BaseParentResponce
    {
        public List<MicroRoomJSON> Data = new List<MicroRoomJSON>();
    }
    #endregion
    #region GetAllMeeting
    public class GetAllMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetAllMeetingResponse>>
    {
        public string Query { get; set; }
        public string? UserName { get; set; }
    }

    public class GetAllMeetingResponse : BaseParentResponce
    {
        public List<MiniMeet> Data = new List<MiniMeet>();
    }
    #endregion

    #region GetUserMeetLogs
    public class GetUserMeetLogsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetUserMeetLogsResponse>>
    {
        public long? UserMeetID{ get; set; }
    }

    public class GetUserMeetLogsResponse : BaseParentResponce
    {
        public List<MiniLogJSON> Data = new List<MiniLogJSON>();
        public int TotalCount { get; set; }
    }
    #endregion


    #region GetAllLives
    public class GetAllLivesRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetAllLivesResponse>>
    {
        public string Query { get; set; }
    }

    public class GetAllLivesResponse : BaseParentResponce
    {
        public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
        public bool IsLive { get; set; }
    }
    #endregion



    #region AddPollQuiz
    public class AddPollQuizRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AddPollQuizResponse>>
    {
        public string Question { get; set; }
        public MainEntity_Types Type { get; set; } = MainEntity_Types.QUIZ_OPTIONAL;
        public List<AnsPollRequestJSON> AnsPollRequestJSONs { get; set; }
    }
    public class AnsPollRequestJSON
    {
        public string Content { get; set; }
        public int Score { get; set; }
    }

    public class AddPollQuizResponse : BaseParentResponce
    {
    }
    #endregion
    #region GetPollListAdmin
    public class GetPollListAdminRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPollListAdminResponse>>
    {
    }

    public class GetPollListAdminResponse : BaseParentResponce
    {
        public List<PollJSON> Data = new List<PollJSON>();
    }
    #endregion


    #region Request/Response Models
    public class GetPollReportRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetPollReportResponse>>
    {
        public string MeetID { get; set; }
        public long? QID { get; set; }      // nullable - if provided, report for specific question
        public string? UserID { get; set; }  // nullable - if provided, report for specific user
    }

    public class GetPollReportResponse : BaseParentResponce
    {
        public List<PollReportJSON> Data { get; set; } = new List<PollReportJSON>();
        public int TotalParticipants { get; set; }
    }


    #endregion

    #region Room Metadata & Full Edit DTOs
    public class GetRoomMetadataRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomMetadataResponse>>
    {
        public string FriendlyID { get; set; }
        public string Key { get; set; }
    }

    public class GetRoomMetadataResponse : BaseParentResponce
    {
        public string Key { get; set; }
        public string? Value { get; set; }
        public MainEntity_Types DataType { get; set; }
    }

    public class SetRoomMetadataRequest : NewUserRequest, IUseCaseRequest<GenericResponse<SetRoomMetadataResponse>>
    {
        public string FriendlyID { get; set; }
        public string Key { get; set; }
        public string? Value { get; set; }
        public MainEntity_Types DataType { get; set; } = MainEntity_Types.META_STRING;
    }

    public class SetRoomMetadataResponse : BaseParentResponce
    {
        public bool Success { get; set; }
    }

    public class DirectRoomSubscribedRegistrationRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<DirectRoomSubscribedRegistrationResponse>>
    {
        public string RoomId { get; set; }
        public string? ReceiptUrl { get; set; }
    }

    public class DirectRoomSubscribedRegistrationResponse : BaseParentResponce
    {
    }

    public class GetRoomWaitingMembersRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomWaitingMembersResponse>>
    {
        public string RoomId { get; set; }
    }

    public class GetRoomWaitingMembersResponse : BaseParentResponce
    {
        public List<WaitingMemberJSON> Members { get; set; } = new List<WaitingMemberJSON>();
    }

    public class WaitingMemberJSON
    {
        public long UserId { get; set; }
        public string Fullname { get; set; }
        public string NationalCode { get; set; }
        public string Phone { get; set; }
        public int Status { get; set; }
        public string Explain { get; set; }
    }

    public class ChangeRoomMemberStatusRequest : NewUserRequest, IUseCaseRequest<GenericResponse<ChangeRoomMemberStatusResponse>>
    {
        public string RoomId { get; set; }
        public long UserId { get; set; }
        public int Status { get; set; } // e.g. 100 for Accept, 0 for Reject
    }

    public class ChangeRoomMemberStatusResponse : BaseParentResponce
    {
        public bool Success { get; set; }
    }

    public class RoomUserMemberDTO
    {
        public long ID { get; set; }
        public string UUID { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public MainEntity_Types Type { get; set; } // 551 for ROOMTOUSER_ADMIN, 550 for Member, etc.
        public int Status { get; set; }
    }

    public class GetRoomFullDetailsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomFullDetailsResponse>>
    {
        public string FriendlyID { get; set; }
    }

    public class GetRoomFullDetailsResponse : BaseParentResponce
    {
        public string FriendlyID { get; set; }
        public string Title { get; set; }
        public string? Explain { get; set; }
        public string? Bio { get; set; }
        public string? TTR_URL { get; set; }
        public string? Avatar { get; set; }
        public string? Logo { get; set; }
        public string? PresentationPath { get; set; }
        public string? CoverImage { get; set; }
        public string? NajmID { get; set; }
        public bool RequiredAdminAcceptToView { get; set; }
        public bool RequiredLoginToView { get; set; }
        public bool RecordRoom { get; set; }
        public bool IsPublicRoom { get; set; }
        public bool IsSuperPrivateRoom { get; set; }
        public bool IsPrivateRoom { get; set; }
        public bool IsSubscribed { get; set; }
        public bool IsShowMemberVIPBeforeEndTheMeeting { get; set; }
        public long OwnerID { get; set; }
        public bool IsOwner { get; set; }
        public bool IsRoomAdmin { get; set; }
        public bool CanEdit { get; set; }
        public List<RoomUserMemberDTO> Members { get; set; } = new List<RoomUserMemberDTO>();
        public Dictionary<string, string?> Metadatas { get; set; } = new Dictionary<string, string?>();
    }

    public class EditRoomFullRequest : NewUserRequest, IUseCaseRequest<GenericResponse<EditRoomFullResponse>>
    {
        public string FriendlyID { get; set; }
        public string Title { get; set; }
        public string? Explain { get; set; }
        public string? Bio { get; set; }
        public string? TTR_URL { get; set; }
        public string? Avatar { get; set; }
        public string? Logo { get; set; }
        public string? PresentationPath { get; set; }
        public string? CoverImage { get; set; }
        public bool RequiredAdminAcceptToView { get; set; }
        public bool RequiredLoginToView { get; set; }
        public bool RecordRoom { get; set; }
        public bool IsSuperPrivateRoom { get; set; }
        public bool IsPrivateRoom { get; set; }
        public bool IsSubscribed { get; set; }
        public bool IsShowMemberVIPBeforeEndTheMeeting { get; set; }
        public List<string>? AddAdminUUIDs { get; set; }
        public List<string>? RemoveAdminUUIDs { get; set; }
    }

    public class EditRoomFullResponse : BaseParentResponce
    {
    }
    #endregion

}
