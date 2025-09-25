using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usecase
{
    #region ApplicationVersion
    public class ApplicationVersionUseCase : IApplicationVersionUseCase
    {
        public IDBContext _repository { get; }
        public CommonMethods _commonMethods { get; }
        private IHostingEnvironment hosting;

        public ApplicationVersionUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _repository = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(ApplicationVersionRequest message,
            IOutPutPort<GenericResponse<ApplicationVersionResponse>> outputPort)
        {

            outputPort.Handle(new GenericResponse<ApplicationVersionResponse>
       (new ApplicationVersionResponse
       {
           Message = "ok",
           Status = 200,

       }));

        }
    }
    #endregion

}