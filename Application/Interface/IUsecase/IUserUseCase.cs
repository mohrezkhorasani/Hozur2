

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{
    public interface INewUserUseCase : IUseCaseRequestHandler<NewUserRequest, GenericResponse<NewUserResponse>> { }
    public interface INewMeetingUseCase : IUseCaseRequestHandler<NewMeetingRequest, GenericResponse<NewMeetingResponse>> { }
    public interface IUpdateMeetingUseCase : IUseCaseRequestHandler<UpdateMeetingRequest, GenericResponse<UpdateMeetingResponse>> { }
}
