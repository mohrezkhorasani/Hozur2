

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{

    public interface IApplicationVersionUseCase : IUseCaseRequestHandler<ApplicationVersionRequest, GenericResponse<ApplicationVersionResponse>> { }
}
