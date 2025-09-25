using Application.DTO.Generals;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using static Application.General.Statics;

namespace Application.DTO
{

   


    #region ApplicationVersion
    public class ApplicationVersionRequest : BaseParentRequest, IUseCaseRequest<GenericResponse<ApplicationVersionResponse>>
    {
        public int VersionCode { get; set; }

    }

    public class ApplicationVersionResponse : BaseParentResponce
    {
        
    }
    #endregion






}
