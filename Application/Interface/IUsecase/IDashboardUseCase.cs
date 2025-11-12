

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{
    //public interface INewUserUseCase : IUseCaseRequestHandler<NewUserRequest, GenericResponse<NewUserResponse>> { }
    //public interface INewMeetingUseCase : IUseCaseRequestHandler<NewMeetingRequest, GenericResponse<NewMeetingResponse>> { }
    //public interface IUpdateMeetingUseCase : IUseCaseRequestHandler<UpdateMeetingRequest, GenericResponse<UpdateMeetingResponse>> { }
    //public interface INewUserMettingsUseCase : IUseCaseRequestHandler<NewUserMettingsRequest, GenericResponse<NewUserMettingsResponse>> { }
    //public interface INewUserMeetingLogUseCase : IUseCaseRequestHandler<NewUserMeetingLogRequest, GenericResponse<NewUserMeetingLogResponse>> { }
    //public interface INewUserMeetingLogFromModeratorUseCase : IUseCaseRequestHandler<NewUserMeetingLogFromModeratorRequest, GenericResponse<NewUserMeetingLogFromModeratorResponse>> { }
    //public interface IGetTeacherListUseCase : IUseCaseRequestHandler<GetTeacherListRequest, GenericResponse<GetTeacherListResponse>> { }
    //public interface IGetRoomsUseCase : IUseCaseRequestHandler<GetRoomsRequest, GenericResponse<GetRoomsResponse>> { }
    //public interface IGetLivesUseCase : IUseCaseRequestHandler<GetLivesRequest, GenericResponse<GetLivesResponse>> { }
    //public interface IStartMeetingUseCase : IUseCaseRequestHandler<StartMeetingRequest, GenericResponse<StartMeetingResponse>> { }
    //public interface IGetMeetingURLUseCase : IUseCaseRequestHandler<GetMeetingURLRequest, GenericResponse<GetMeetingURLResponse>> { }
    //public interface IGuarantCertificateUseCase : IUseCaseRequestHandler<GuarantCertificateRequest, GenericResponse<GuarantCertificateResponse>> { }
    //public interface IGetRoomByFriendlyIDUseCase : IUseCaseRequestHandler<GetRoomByFriendlyIDRequest, GenericResponse<GetRoomByFriendlyIDResponse>> { }
    //public interface INewRoomUseCase : IUseCaseRequestHandler<NewRoomRequest, GenericResponse<NewRoomResponse>> { }
    public interface IGetRoomStatsUseCase : IUseCaseRequestHandler<GetRoomStatsRequest, GenericResponse<GetRoomStatsResponse>> { }
    public interface IGetTrophiesUseCase : IUseCaseRequestHandler<GetTrophiesRequest, GenericResponse<GetTrophiesResponse>> { }
    public interface IGetLastMeetingUseCase : IUseCaseRequestHandler<GetLastMeetingRequest, GenericResponse<GetLastMeetingResponse>> { }
}
