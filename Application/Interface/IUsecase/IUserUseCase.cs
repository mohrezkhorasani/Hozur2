

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{
    public interface INewUserUseCase : IUseCaseRequestHandler<NewUserRequest, GenericResponse<NewUserResponse>> { }
    public interface INewMeetingUseCase : IUseCaseRequestHandler<NewMeetingRequest, GenericResponse<NewMeetingResponse>> { }
    public interface IUpdateMeetingUseCase : IUseCaseRequestHandler<UpdateMeetingRequest, GenericResponse<UpdateMeetingResponse>> { }
    public interface INewUserMettingsUseCase : IUseCaseRequestHandler<NewUserMettingsRequest, GenericResponse<NewUserMettingsResponse>> { }
    public interface INewUserMeetingLogUseCase : IUseCaseRequestHandler<NewUserMeetingLogRequest, GenericResponse<NewUserMeetingLogResponse>> { }
    public interface IGetTeacherListUseCase : IUseCaseRequestHandler<GetTeacherListRequest, GenericResponse<GetTeacherListResponse>> { }
    public interface IGetRoomsUseCase : IUseCaseRequestHandler<GetRoomsRequest, GenericResponse<GetRoomsResponse>> { }
    public interface IGetLivesUseCase : IUseCaseRequestHandler<GetLivesRequest, GenericResponse<GetLivesResponse>> { }
    public interface IStartMeetingUseCase : IUseCaseRequestHandler<StartMeetingRequest, GenericResponse<StartMeetingResponse>> { }
    public interface IGetRoomByFriendlyIDUseCase : IUseCaseRequestHandler<GetRoomByFriendlyIDRequest, GenericResponse<GetRoomByFriendlyIDResponse>> { }
    public interface INewRoomUseCase : IUseCaseRequestHandler<NewRoomRequest, GenericResponse<NewRoomResponse>> { }
}
