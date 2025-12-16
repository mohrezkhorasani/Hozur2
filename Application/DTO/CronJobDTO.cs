using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using Domain.Entity;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{



    #region CronJobCheckLiveMeetingTask
    public class CronJobCheckLiveMeetingTaskRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<CronJobCheckLiveMeetingTaskResponse>>
    {
    }

    public class CronJobCheckLiveMeetingTaskResponse : BaseParentResponce
    {

    }
    #endregion


    #region BackupDB
    public class BackupDBRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<BackupDBResponse>>
    {
    }

    public class BackupDBResponse : BaseParentResponce
    {

    }
    #endregion



}