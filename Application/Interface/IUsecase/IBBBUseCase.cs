

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{
    //public interface INewUserUseCase : IUseCaseRequestHandler<NewUserRequest, GenericResponse<NewUserResponse>> { }
    //public interface ILoginUserPanelUseCase : IUseCaseRequestHandler<LoginUserPanelRequest, GenericResponse<LoginUserPanelResponse>> { }
    //public interface IVerifyTokenUseCase : IUseCaseRequestHandler<VerifyTokenRequest, GenericResponse<VerifyTokenResponse>> { }
    //public interface ISignInUserPanelUseCase : IUseCaseRequestHandler<SignInUserPanelRequest, GenericResponse<SignInUserPanelResponse>> { }
    //public interface INewMeetingUseCase : IUseCaseRequestHandler<NewMeetingRequest, GenericResponse<NewMeetingResponse>> { }
    //public interface IUpdateMeetingUseCase : IUseCaseRequestHandler<UpdateMeetingRequest, GenericResponse<UpdateMeetingResponse>> { }
    //public interface INewUserMettingsUseCase : IUseCaseRequestHandler<NewUserMettingsRequest, GenericResponse<NewUserMettingsResponse>> { }
    //public interface INewUserMeetingLogUseCase : IUseCaseRequestHandler<NewUserMeetingLogRequest, GenericResponse<NewUserMeetingLogResponse>> { }
    //public interface INewUserMeetingLogFromModeratorUseCase : IUseCaseRequestHandler<NewUserMeetingLogFromModeratorRequest, GenericResponse<NewUserMeetingLogFromModeratorResponse>> { }
    //public interface IGetTeacherListUseCase : IUseCaseRequestHandler<GetTeacherListRequest, GenericResponse<GetTeacherListResponse>> { }
    //public interface IGetRoomsUseCase : IUseCaseRequestHandler<GetRoomsRequest, GenericResponse<GetRoomsResponse>> { }
    //public interface IGetRoomsPublicUseCase : IUseCaseRequestHandler<GetRoomsPublicRequest, GenericResponse<GetRoomsPublicResponse>> { }
    //public interface IGetLivesOfProfileUseCase : IUseCaseRequestHandler<GetLivesOfProfileRequest, GenericResponse<GetLivesOfProfileResponse>> { }
    //public interface IGetLivesUseCase : IUseCaseRequestHandler<GetLivesRequest, GenericResponse<GetLivesResponse>> { }
    //public interface IGetLivesPublicUseCase : IUseCaseRequestHandler<GetLivesPublicRequest, GenericResponse<GetLivesPublicResponse>> { }
    //public interface IGetLiveWithIDUseCase : IUseCaseRequestHandler<GetLiveWithIDRequest, GenericResponse<GetLiveWithIDResponse>> { }
    //public interface IGuarantCertificateUseCase : IUseCaseRequestHandler<GuarantCertificateRequest, GenericResponse<GuarantCertificateResponse>> { }
    //public interface IGetRoomByFriendlyIDUseCase : IUseCaseRequestHandler<GetRoomByFriendlyIDRequest, GenericResponse<GetRoomByFriendlyIDResponse>> { }
    //public interface INewRoomUseCase : IUseCaseRequestHandler<NewRoomRequest, GenericResponse<NewRoomResponse>> { }
    //public interface IGenerateCertificateUseCase : IUseCaseRequestHandler<GenerateCertificateRequest, GenericResponse<GenerateCertificateResponse>> { }
    //public interface IEndTheMeetingUseCase : IUseCaseRequestHandler<EndTheMeetingRequest, GenericResponse<EndTheMeetingResponse>> { }
    //public interface IEndTheMeetingWithAPIUseCase : IUseCaseRequestHandler<EndTheMeetingWithAPIRequest, GenericResponse<EndTheMeetingWithAPIResponse>> { }
    public interface INewHookUseCase : IUseCaseRequestHandler<NewHookRequest, GenericResponse<NewHookResponse>> { }
    public interface IGetHooksUseCase : IUseCaseRequestHandler<GetHooksRequest, GenericResponse<GetHooksResponse>> { }
    public interface IGetHookInfoUseCase : IUseCaseRequestHandler<GetHookInfoRequest, GenericResponse<GetHookInfoResponse>> { }
    public interface IDestroyWebhookAsyncUseCase : IUseCaseRequestHandler<DestroyWebhookAsyncRequest, GenericResponse<DestroyWebhookAsyncResponse>> { }
    public interface IDestroyAllWebhooksForMeetingAsyncUseCase : IUseCaseRequestHandler<DestroyAllWebhooksForMeetingAsyncRequest, GenericResponse<DestroyAllWebhooksForMeetingAsyncResponse>> { }
    public interface IUpdateWebhookCallbackURLAsyncUseCase : IUseCaseRequestHandler<UpdateWebhookCallbackURLAsyncRequest, GenericResponse<UpdateWebhookCallbackURLAsyncResponse>> { }
    public interface IBBBWebhookLogUseCase : IUseCaseRequestHandler<BBBWebhookLogRequest, GenericResponse<BBBWebhookLogResponse>> { }
    //public interface IInsertSlideShowUseCase : IUseCaseRequestHandler<InsertSlideShowRequest, GenericResponse<InsertSlideShowResponse>> { }
    //public interface IContactusUseCase : IUseCaseRequestHandler<ContactusRequest, GenericResponse<ContactusResponse>> { }
    //public interface IGetProfileInfoUseCase : IUseCaseRequestHandler<GetProfileInfoRequest, GenericResponse<GetProfileInfoResponse>> { }
    //public interface IFollowProfileUseCase : IUseCaseRequestHandler<FollowProfileRequest, GenericResponse<FollowProfileResponse>> { }
    //public interface IGetHekmatShobeUseCase : IUseCaseRequestHandler<GetHekmatShobeRequest, GenericResponse<GetHekmatShobeResponse>> { }
    //public interface IGetPollListUseCase : IUseCaseRequestHandler<GetPollListRequest, GenericResponse<GetPollListResponse>> { }
    //public interface IAnswerPollListUseCase : IUseCaseRequestHandler<AnswerPollListRequest, GenericResponse<AnswerPollListResponse>> { }
    //public interface IUpdatePresentMinToCertOfMeetUseCase : IUseCaseRequestHandler<UpdatePresentMinToCertOfMeetRequest, GenericResponse<UpdatePresentMinToCertOfMeetResponse>> { }
    //public interface IGetPlanRoomUseCase : IUseCaseRequestHandler<GetPlanRoomRequest, GenericResponse<GetPlanRoomResponse>> { }
    //public interface IGetPlanRoomPublicUseCase : IUseCaseRequestHandler<GetPlanRoomPublicRequest, GenericResponse<GetPlanRoomPublicResponse>> { }
    //public interface IGetLatestMeetingsUseCase : IUseCaseRequestHandler<GetLatestMeetingsRequest, GenericResponse<GetLatestMeetingsResponse>> { }
    //public interface IDeletePlanRoomUseCase : IUseCaseRequestHandler<DeletePlanRoomRequest, GenericResponse<DeletePlanRoomResponse>> { }
    //public interface INewPlanRoomUseCase : IUseCaseRequestHandler<NewPlanRoomRequest, GenericResponse<NewPlanRoomResponse>> { }
    //public interface IGetReservationUseCase : IUseCaseRequestHandler<GetReservationRequest, GenericResponse<GetReservationResponse>> { }
    //public interface INewReservationUseCase : IUseCaseRequestHandler<NewReservationRequest, GenericResponse<NewReservationResponse>> { }



    //public interface IAdminGetUserListUseCase : IUseCaseRequestHandler<AdminGetUserListRequest, GenericResponse<AdminGetUserListResponse>> { }
    //public interface IAdminGetUserRoomListUseCase : IUseCaseRequestHandler<AdminGetUserRoomListRequest, GenericResponse<AdminGetUserRoomListResponse>> { }
    //public interface IAdminChangeRoleUseCase : IUseCaseRequestHandler<AdminChangeRoleRequest, GenericResponse<AdminChangeRoleResponse>> { }
    //public interface IChangeAvatarOfRoomUseCase : IUseCaseRequestHandler<ChangeAvatarOfRoomRequest, GenericResponse<ChangeAvatarOfRoomResponse>> { }
    //public interface IGetAllLivesUseCase : IUseCaseRequestHandler<GetAllLivesRequest, GenericResponse<GetAllLivesResponse>> { }
    //public interface IAdminCreateRoomUseCase : IUseCaseRequestHandler<AdminCreateRoomRequest, GenericResponse<AdminCreateRoomResponse>> { }
    //public interface IGetAllRoomsUseCase : IUseCaseRequestHandler<GetAllRoomsRequest, GenericResponse<GetAllRoomsResponse>> { }
    //public interface IAddPollQuizUseCase : IUseCaseRequestHandler<AddPollQuizRequest, GenericResponse<AddPollQuizResponse>> { }
    //public interface IGetPollListAdminUseCase : IUseCaseRequestHandler<GetPollListAdminRequest, GenericResponse<GetPollListAdminResponse>> { }
}
