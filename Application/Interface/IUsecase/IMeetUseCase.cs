

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{

    public interface INewRoomDashboardUseCase : IUseCaseRequestHandler<NewRoomDashboardRequest, GenericResponse<NewRoomDashboardResponse>> { }
    public interface IGetMeetingFromRoomIDUseCase : IUseCaseRequestHandler<GetMeetingFromRoomIDRequest, GenericResponse<GetMeetingFromRoomIDResponse>> { }
    public interface IGetMeetInfoUseCase : IUseCaseRequestHandler<GetMeetInfoRequest, GenericResponse<GetMeetInfoResponse>> { }
    public interface IGetMembersOfMeetUseCase : IUseCaseRequestHandler<GetMembersOfMeetRequest, GenericResponse<GetMembersOfMeetResponse>> { }
    public interface IGetMembersOfMeetOnlyCertificatedUseCase : IUseCaseRequestHandler<GetMembersOfMeetOnlyCertificatedRequest, GenericResponse<GetMembersOfMeetOnlyCertificatedResponse>> { }
    public interface IGenerateCertificateWithUserMeetIDUseCase : IUseCaseRequestHandler<GenerateCertificateWithUserMeetIDRequest, GenericResponse<GenerateCertificateWithUserMeetIDResponse>> { }
}
