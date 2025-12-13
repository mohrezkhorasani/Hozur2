

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
    public interface ISearchUserUseCase : IUseCaseRequestHandler<SearchUserRequest, GenericResponse<SearchUserResponse>> { }
    public interface IGetExcellMembersUseCase : IUseCaseRequestHandler<GetExcellMembersRequest, GenericResponse<GetExcellMembersResponse>> { }
    public interface IGetMemberByIDUseCase : IUseCaseRequestHandler<GetMemberByIDRequest, GenericResponse<GetMemberByIDResponse>> { }
    public interface IGetAllMeetingMemberUseCase : IUseCaseRequestHandler<GetAllMeetingMemberRequest, GenericResponse<GetAllMeetingMemberResponse>> { }
    public interface IGetAllCertificateOfMemberUseCase : IUseCaseRequestHandler<GetAllCertificateOfMemberRequest, GenericResponse<GetAllCertificateOfMemberResponse>> { }
}
