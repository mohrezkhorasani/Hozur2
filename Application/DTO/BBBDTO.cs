using Application.DTO.Generals;
using Application.DTO.Models;
using Application.General;
using Application.Interface.IUsecase;
using Application.Usecase;
using Domain.Entity;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{
    //#region NewUser
    //public class NewUserRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewUserResponse>>
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
    //    public string? RoomName { get; set; }

    //}

    //public class NewUserResponse : BaseParentResponce
    //{
    //    public JWTTokenJSON Data { get; set; }
    //}
    //#endregion
    //#region LoginUserPanel
    //public class LoginUserPanelRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<LoginUserPanelResponse>>
    //{
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //}

    //public class LoginUserPanelResponse : BaseParentResponce
    //{
    //    public JWTTokenJSON Data { get; set; }
    //}
    //#endregion

    //#region SignInUserPanel
    //public class SignInUserPanelRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<SignInUserPanelResponse>>
    //{
    //    public string Name { get; set; }
    //    public string Username { get; set; }
    //    public string Password { get; set; }
    //    public string Email { get; set; }
    //    public string Explain { get; set; }
    //}

    //public class SignInUserPanelResponse : BaseParentResponce
    //{
    //    public JWTTokenJSON Data { get; set; }
    //}
    //#endregion

    //#region VerifyToken
    //public class VerifyTokenRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<VerifyTokenResponse>>
    //{
    //}

    //public class VerifyTokenResponse : BaseParentResponce
    //{
    //    public MiniUserJSON Data { get; set; }
    //}
    //#endregion



    //#region NewUserMeetingLog
    //public class NewUserMeetingLogRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewUserMeetingLogResponse>>
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
    //public class NewUserMeetingLogFromModeratorRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewUserMeetingLogFromModeratorResponse>>
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
    //public class NewMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewMeetingResponse>>
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
    //public class UpdateMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<UpdateMeetingResponse>>
    //{
    //    public string[] Teacher { get; set; }
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
    //public class NewUserMettingsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewUserMettingsResponse>>
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
    //public class GetTeacherListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetTeacherListResponse>>
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
    //public class NewTeacherRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewTeacherResponse>>
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
    //public class GuarantCertificateRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GuarantCertificateResponse>>
    //{

    //}

    //public class GuarantCertificateResponse : BaseParentResponce
    //{
    //    public long Number { get; set; }
    //}
    //#endregion

    ////Add Teacher 
    //#region NewRoom
    //public class NewRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewRoomResponse>>
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
    //public class GetRoomsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomsResponse>>
    //{
    //    public bool UseCache { get; set; } = false;
    //    public string Query { get; set; }
    //    public string? UserName { get; set; }
    //}

    //public class GetRoomsResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion
    ////Get Rooms
    //#region GetRoomsPublic
    //public class GetRoomsPublicRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomsPublicResponse>>
    //{
    //    public string Query { get; set; }
    //    public string? UserName { get; set; }
    //}

    //public class GetRoomsPublicResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion

    ////Get Room with friendlyID
    //#region GetRoomByFriendlyID
    //public class GetRoomByFriendlyIDRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetRoomByFriendlyIDResponse>>
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
    //public class GetLivesRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetLivesResponse>>
    //{
    //    public string Query { get; set; }
    //    public bool Public { get; set; } = true;
    //}

    //public class GetLivesResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion
    ////Get Lives
    //#region GetLivesOfProfile
    //public class GetLivesOfProfileRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetLivesOfProfileResponse>>
    //{
    //    public string Query { get; set; }
    //    public string Username { get; set; }
    //    public bool Public { get; set; } = true;
    //}

    //public class GetLivesOfProfileResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion

    //#region GetLivesPublic
    //public class GetLivesPublicRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetLivesPublicResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetLivesPublicResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //    public bool IsLive { get; set; }
    //}
    //#endregion

    //#region GetLiveWithID
    //public class GetLiveWithIDRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetLiveWithIDResponse>>
    //{
    //    public string ID { get; set; }
    //}

    //public class GetLiveWithIDResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion




    #region hooks

    #region NewHook
    public class NewHookRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewHookResponse>>
    {
        public string CallbackURL { get; set; }
        public string? MeetID { get; set; }
        public List<string>? EventIDs { get; set; } // Optional: specific events to subscribe to
    }

    public class NewHookResponse : BaseParentResponce
    {
        public string HookID { get; set; }
        public string CallbackURL { get; set; }
        public string MeetingID { get; set; }
    }
    #endregion

    #region GetHooks
    public class GetHooksRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetHooksResponse>>
    {
        public string? MeetingID { get; set; } // Optional: filter by meeting ID
    }

    public class GetHooksResponse : BaseParentResponce
    {
        public List<BBBHookInfoDTO> Hooks { get; set; } = new();
    }

    public class BBBHookInfoDTO
    {
        public string HookID { get; set; }
        public string CallbackURL { get; set; }
        public string MeetingID { get; set; }
        public string Title { get; set; }
        public bool Permanent { get; set; }
        public bool RawData { get; set; }
        public MicroRoomJSON Room { get; set; }
        public List<string> EventIDs { get; set; } = new();
    }
    #endregion

    #region GetHookInfo
    public class GetHookInfoRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetHookInfoResponse>>
    {
        public string HookID { get; set; }
    }

    public class GetHookInfoResponse : BaseParentResponce
    {
        public BBBHookInfoDTO Hook { get; set; }
    }
    #endregion

    #region DestroyWebhookAsync
    public class DestroyWebhookAsyncRequest : NewUserRequest, IUseCaseRequest<GenericResponse<DestroyWebhookAsyncResponse>>
    {
        public string HookID { get; set; }
    }

    public class DestroyWebhookAsyncResponse : BaseParentResponce
    {
        public bool Destroyed { get; set; }
    }
    #endregion

    #region DestroyAllWebhooksForMeetingAsync
    public class DestroyAllWebhooksForMeetingAsyncRequest : NewUserRequest, IUseCaseRequest<GenericResponse<DestroyAllWebhooksForMeetingAsyncResponse>>
    {
        public string MeetingID { get; set; }
    }

    public class DestroyAllWebhooksForMeetingAsyncResponse : BaseParentResponce
    {
        public int DestroyedCount { get; set; }
    }
    #endregion

    #region UpdateWebhookCallbackURLAsync
    public class UpdateWebhookCallbackURLAsyncRequest : NewUserRequest, IUseCaseRequest<GenericResponse<UpdateWebhookCallbackURLAsyncResponse>>
    {
        public string HookID { get; set; }
        public string NewCallbackURL { get; set; }
    }

    public class UpdateWebhookCallbackURLAsyncResponse : BaseParentResponce
    {
        public bool Updated { get; set; }
    }
    #endregion

    #endregion

    #region BBBWebhookLog
    public class BBBWebhookLogRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<BBBWebhookLogResponse>>
    {
        public string? HookID { get; set; }
        public string? MeetingID { get; set; }
        public string? InternalMeetingID { get; set; }
        public string? Domain { get; set; }
        public string? Event { get; set; }
        public DateTime? Timestamp { get; set; }
        public Dictionary<string, object>? Data { get; set; }

        // Extracted user data
        public string? NationalCode { get; set; }
        public string? Phone { get; set; }
        public string? UserId { get; set; }
        public string? Vc2AuthNorm { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }

        // متد دریافت اطلاعات کاربر
        public BBBWebhookUser GetUserInfo(BBBWebhookLogRequest request)
        {
            if (request.Data == null)
                return null;

            try
            {
                // ابتدا چک کن که user مستقیماً در Data موجود باشه
                if (request.Data.ContainsKey("user"))
                {
                    var userObj = request.Data["user"];
                    if (userObj != null)
                    {
                        // تبدیل به JObject
                        var jUser = JObject.FromObject(userObj);

                        return new BBBWebhookUser
                        {
                            InternalUserId = jUser["internalUserId"]?.ToString() ?? jUser["internal-user-id"]?.ToString(),
                            ExternalUserId = jUser["externalUserId"]?.ToString() ?? jUser["external-user-id"]?.ToString(),
                        };
                    }
                }

                // اگر user مستقیم نبود، از rawData استفاده کن
                if (request.Data.ContainsKey("rawData"))
                {
                    var rawDataJson = request.Data["rawData"]?.ToString();
                    if (!string.IsNullOrEmpty(rawDataJson))
                    {
                        // پارس کردن JSON به JObject
                        var rawData = JObject.Parse(rawDataJson);

                        // استخراج attributes
                        var attributes = rawData["data"]?["attributes"];
                        if (attributes != null)
                        {
                            var userToken = attributes["user"];
                            if (userToken != null)
                            {
                                return new BBBWebhookUser
                                {
                                    InternalUserId = userToken["internal-user-id"]?.ToString(),
                                    ExternalUserId = userToken["external-user-id"]?.ToString(),
                                    Name = userToken["name"]?.ToString(),
                                    Role = userToken["role"]?.ToString(),
                                    Presenter = userToken["presenter"]?.Value<bool>() ?? false,
                                    Guest = userToken["guest"]?.Value<bool>() ?? false,
                                    RaiseHand = userToken["raise-hand"]?.Value<bool>() ?? false,
                                    UserData = userToken["userdata"] != null ? new BBBWebhookUserData
                                    {
                                        UserID5 = userToken["userdata"]?["userID5"]?.ToString(),
                                        Vc2AuthNorm = userToken["userdata"]?["vc2AuthNorm"]?.ToString(),
                                        NationalCode = userToken["userdata"]?["nationalCode"]?.ToString(),
                                        Phone = userToken["userdata"]?["phone"]?.ToString()
                                    } : null
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Statics.Log("GetUserInfo Error", ex.Message);
                return null;
            }
        }

        public BBBWebhookMeeting GetMeetingInfo(BBBWebhookLogRequest request)
        {
            if (request.Data == null)
                return null;

            try
            {
                // ابتدا چک کن که meeting مستقیماً در Data موجود باشه
                if (request.Data.ContainsKey("meeting"))
                {
                    var meetingObj = request.Data["meeting"] as dynamic;
                    if (meetingObj != null)
                    {
                        return new BBBWebhookMeeting
                        {
                            InternalMeetingId = meetingObj.InternalMeetingId ?? meetingObj.internalMeetingId,
                            ExternalMeetingId = meetingObj.ExternalMeetingId ?? meetingObj.externalMeetingId,
                            Name = meetingObj.Name ?? meetingObj.name,
                            IsBreakout = meetingObj.IsBreakout ?? meetingObj.isBreakout ?? false,
                            Duration = meetingObj.Duration ?? meetingObj.duration ?? 0,
                            CreateTime = meetingObj.CreateTime ?? meetingObj.createTime ?? 0,
                            Record = meetingObj.Record ?? meetingObj.record ?? false
                        };
                    }
                }

                // اگر meeting مستقیم نبود، از rawData استفاده کن
                if (request.Data.ContainsKey("rawData"))
                {
                    var rawDataJson = request.Data["rawData"]?.ToString();
                    if (!string.IsNullOrEmpty(rawDataJson))
                    {
                        var rawData = JObject.Parse(rawDataJson);
                        var attributes = rawData["data"]?["attributes"];

                        if (attributes != null)
                        {
                            var meetingToken = attributes["meeting"];
                            if (meetingToken != null)
                            {
                                return new BBBWebhookMeeting
                                {
                                    InternalMeetingId = meetingToken["internal-meeting-id"]?.ToString(),
                                    ExternalMeetingId = meetingToken["external-meeting-id"]?.ToString(),
                                    Name = meetingToken["name"]?.ToString(),
                                    IsBreakout = meetingToken["is-breakout"]?.Value<bool>() ?? false,
                                    Duration = meetingToken["duration"]?.Value<int>() ?? 0,
                                    CreateTime = meetingToken["create-time"]?.Value<long>() ?? 0,
                                    Record = meetingToken["record"]?.Value<bool>() ?? false
                                };
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Statics.Log("GetMeetingInfo Error", ex.Message);
                return null;
            }
        }
    }


    public class BBBWebhookLogResponse : BaseParentResponce
    {
        public bool Logged { get; set; }
        public string? EventId { get; set; }

    }
    #endregion


    ////Add Teacher 
    //#region GenerateCertificate
    //public class GenerateCertificateRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GenerateCertificateResponse>>
    //{
    //    public string NationalCode { get; set; }
    //    public bool Sex { get; set; }//True = MALE //FALSE = FEMALE
    //}

    //public class GenerateCertificateResponse : BaseParentResponce
    //{
    //    public string Link { get; set; }
    //}
    //#endregion

    //#region EndTheMeeting
    //public class EndTheMeetingRequest : NewUserRequest, IUseCaseRequest<GenericResponse<EndTheMeetingResponse>>
    //{
    //}

    //public class EndTheMeetingResponse : BaseParentResponce
    //{

    //}
    //#endregion
    //#region EndTheMeetingWithAPI
    //public class EndTheMeetingWithAPIRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<EndTheMeetingWithAPIResponse>>
    //{
    //    public string FriendlyID { get; set; }
    //}

    //public class EndTheMeetingWithAPIResponse : BaseParentResponce
    //{

    //}
    //#endregion


    ////GetMeetings [ by title?,teacher-name?,jalali-date?, room-id? ]

    ////GetUsersInAMeeting [ by meet-id ]

    ////Get user meetings [ by national-code? | phone? | uuid? | email? | extId? | ]



    ////CheckUser

    //#region NewPlanRoom
    //public class NewPlanRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<NewPlanRoomResponse>>
    //{
    //    public string FriendlyID { get; set; }
    //    public string? Title { get; set; }
    //    public string? Teacher { get; set; }
    //    public string? Dabir { get; set; }
    //    public bool HasCert { get; set; } = true;
    //    public DateTime StartDate { get; set; }
    //}

    //public class NewPlanRoomResponse : BaseParentResponce
    //{
    //}
    //#endregion

    //#region NewReservation
    //public class NewReservationRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<NewReservationResponse>>
    //{
    //    public string Name { get; set; }
    //    public string Phone { get; set; }
    //    public string NationalCode { get; set; }
    //    public long PlanID { get; set; }
    //}

    //public class NewReservationResponse : BaseParentResponce
    //{
    //}
    //#endregion

    //#region GetReservation
    //public class GetReservationRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<GetReservationResponse>>
    //{
    //    public long PlanID { get; set; }
    //}

    //public class GetReservationResponse : BaseParentResponce
    //{
    //    public MiniReservationJSON Data { get; set; }
    //}
    //#endregion

    //#region InsertSlideShow
    //public class InsertSlideShowRequest : NewUserRequest, IUseCaseRequest<GenericResponse<InsertSlideShowResponse>>
    //{
    //    public int Position { get; set; } //0 = disable
    //    public string? Title { get; set; } //0 = disable
    //    public string? Href { get; set; } //0 = disable
    //    public IFormFile? File { get; set; } //0 = disable

    //}

    //public class InsertSlideShowResponse : BaseParentResponce
    //{
    //    public List<MiniSlideShowJSON> Data = new List<MiniSlideShowJSON>();
    //}
    //#endregion

    //#region Contactus
    //public class ContactusRequest : NewUserRequest, IUseCaseRequest<GenericResponse<ContactusResponse>>
    //{
    //    public string Name { get; set; }
    //    public string Email { get; set; }
    //    public string? Phone { get; set; }
    //    public string Text { get; set; }
    //}

    //public class ContactusResponse : BaseParentResponce
    //{
    //}
    //#endregion

    //#region GetProfileInfo
    //public class GetProfileInfoRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetProfileInfoResponse>>
    //{
    //    public string Username { get; set; }
    //}

    //public class GetProfileInfoResponse : BaseParentResponce
    //{
    //    public MiniProfileJSON Data { get; set; }
    //}
    //#endregion

    //#region FollowProfile
    //public class FollowProfileRequest : NewUserRequest, IUseCaseRequest<GenericResponse<FollowProfileResponse>>
    //{
    //    public string Username { get; set; }
    //}

    //public class FollowProfileResponse : BaseParentResponce
    //{
    //    public int NewFollowersCount { get; set; }
    //    public bool IsFollowing { get; set; }
    //}
    //#endregion

    //#region GetHekmatShobe
    //public class GetHekmatShobeRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetHekmatShobeResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetHekmatShobeResponse : BaseParentResponce
    //{
    //    public List<MiniShobeJSON> Data = new List<MiniShobeJSON>();
    //}
    //#endregion

    //#region GetPlanRoom
    //public class GetPlanRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPlanRoomResponse>>
    //{
    //    public string FriendlyID { get; set; }
    //}

    //public class GetPlanRoomResponse : BaseParentResponce
    //{
    //    public List<MiniPlanJSON> Data = new List<MiniPlanJSON>();
    //}
    //#endregion
    //#region GetPlanRoomPublic
    //public class GetPlanRoomPublicRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPlanRoomPublicResponse>>
    //{
    //}

    //public class GetPlanRoomPublicResponse : BaseParentResponce
    //{
    //    public List<MiniPlanJSON> Data = new List<MiniPlanJSON>();
    //}
    //#endregion
    //#region GetLatestMeetings
    //public class GetLatestMeetingsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetLatestMeetingsResponse>>
    //{
    //}

    //public class GetLatestMeetingsResponse : BaseParentResponce
    //{
    //    public List<MiniPlanJSON> Data = new List<MiniPlanJSON>();
    //}
    //#endregion

    //#region DeletePlanRoom
    //public class DeletePlanRoomRequest : NewUserRequest, IUseCaseRequest<GenericResponse<DeletePlanRoomResponse>>
    //{
    //    public long IDPlan { get; set; }
    //}

    //public class DeletePlanRoomResponse : BaseParentResponce
    //{
    //}
    //#endregion


    //#region GetPollList
    //public class GetPollListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPollListResponse>>
    //{
    //}

    //public class GetPollListResponse : BaseParentResponce
    //{
    //    public List<PollJSON> Data = new List<PollJSON>();
    //}
    //#endregion
    //#region AnswerPollList
    //public class AnswerPollListRequest : NewUserRequest, IUseCaseRequest<GenericResponse<AnswerPollListResponse>>
    //{
    //    public List<AnswerItemRequestJSON> AnswerItemRequestJSONs { get; set; }

    //}
    //public class AnswerItemRequestJSON
    //{
    //    public long ID { get; set; }
    //    public string Content { get; set; }
    //}
    //public class AnswerPollListResponse : BaseParentResponce
    //{
    //}
    //#endregion

    //#region UpdatePresentMinToCertOfMeet
    //public class UpdatePresentMinToCertOfMeetRequest : NewUserRequest, IUseCaseRequest<GenericResponse<UpdatePresentMinToCertOfMeetResponse>>
    //{
    //    public int Minutes { get; set; }

    //}
    //public class UpdatePresentMinToCertOfMeetResponse : BaseParentResponce
    //{
    //}
    //#endregion







    ////Admins
    //#region AdminGetUserList
    //public class AdminGetUserListRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminGetUserListResponse>>
    //{
    //    public string Query { get; set; }
    //    public OrderBy OrderBy { get; set; }

    //}

    //public class AdminGetUserListResponse : BaseParentResponce
    //{
    //    public List<MiniUserJSON> Data = new List<MiniUserJSON>();
    //}
    //#endregion
    //#region AdminGetUserRoomList
    //public class AdminGetUserRoomListRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminGetUserRoomListResponse>>
    //{
    //    public string Query { get; set; }
    //    public long OwnerID { get; set; }
    //    public OrderBy OrderBy { get; set; }

    //}

    //public class AdminGetUserRoomListResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion
    //#region AdminChangeRole
    //public class AdminChangeRoleRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminChangeRoleResponse>>
    //{
    //    public MainEntity_Types Role { get; set; }
    //    public long UserId { get; set; }
    //}

    //public class AdminChangeRoleResponse : BaseParentResponce
    //{
    //    public MiniUserJSON Data = new MiniUserJSON();
    //}
    //#endregion
    //#region ChangeAvatarOfRoom
    //public class ChangeAvatarOfRoomRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<ChangeAvatarOfRoomResponse>>
    //{
    //    public string RoomID { get; set; }
    //    public string AvatarPath { get; set; }
    //}

    //public class ChangeAvatarOfRoomResponse : BaseParentResponce
    //{
    //}
    //#endregion

    //#region AdminCreateRoom
    //public class AdminCreateRoomRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AdminCreateRoomResponse>>
    //{
    //    public long? ID { get; set; }
    //    public string Title { get; set; }
    //    public long UserID { get; set; }
    //    public bool IsPublic { get; set; } = false;
    //    public bool IsRequiredLogin { get; set; } = false;
    //    public bool IsRequiredAccept { get; set; } = false;
    //    public bool IsRecordable { get; set; } = false;

    //}

    //public class AdminCreateRoomResponse : BaseParentResponce
    //{
    //    public MiniRoomJSON Data = new MiniRoomJSON();
    //}
    //#endregion

    //#region GetAllRooms
    //public class GetAllRoomsRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetAllRoomsResponse>>
    //{
    //    public string Query { get; set; }
    //    public string? UserName { get; set; }
    //}

    //public class GetAllRoomsResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //}
    //#endregion


    //#region GetAllLives
    //public class GetAllLivesRequest : GetLivesRequest, IUseCaseRequest<GenericResponse<GetAllLivesResponse>>
    //{
    //    public string Query { get; set; }
    //}

    //public class GetAllLivesResponse : BaseParentResponce
    //{
    //    public List<MiniRoomJSON> Data = new List<MiniRoomJSON>();
    //    public bool IsLive { get; set; }
    //}
    //#endregion



    //#region AddPollQuiz
    //public class AddPollQuizRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<AddPollQuizResponse>>
    //{
    //    public string Question { get; set; }
    //    public MainEntity_Types Type { get; set; } = MainEntity_Types.QUIZ_OPTIONAL;
    //    public List<AnsPollRequestJSON> AnsPollRequestJSONs { get; set; }
    //}
    //public class AnsPollRequestJSON
    //{
    //    public string Content { get; set; }
    //    public int Score { get; set; }
    //}

    //public class AddPollQuizResponse : BaseParentResponce
    //{
    //}
    //#endregion
    //#region GetPollListAdmin
    //public class GetPollListAdminRequest : NewUserRequest, IUseCaseRequest<GenericResponse<GetPollListAdminResponse>>
    //{
    //}

    //public class GetPollListAdminResponse : BaseParentResponce
    //{
    //    public List<PollJSON> Data = new List<PollJSON>();
    //}
    //#endregion

}
