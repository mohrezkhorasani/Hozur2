

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{

    public interface INewRoomDashboardUseCase : IUseCaseRequestHandler<NewRoomDashboardRequest, GenericResponse<NewRoomDashboardResponse>> { }
    public interface IGetMeetingFromRoomIDUseCase : IUseCaseRequestHandler<GetMeetingFromRoomIDRequest, GenericResponse<GetMeetingFromRoomIDResponse>> { }
}
