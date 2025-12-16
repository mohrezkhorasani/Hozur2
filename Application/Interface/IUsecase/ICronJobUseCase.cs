

using Application.DTO;
using Application.DTO.Generals;

namespace Application.Interface.IUsecase
{

    public interface ICronJobCheckLiveMeetingTaskUseCase : IUseCaseRequestHandler<CronJobCheckLiveMeetingTaskRequest, GenericResponse<CronJobCheckLiveMeetingTaskResponse>> { }
    public interface IBackupDBUseCase : IUseCaseRequestHandler<BackupDBRequest, GenericResponse<BackupDBResponse>> { }
}
