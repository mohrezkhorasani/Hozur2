using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usecase
{
    #region CronJobCheckLiveMeetingTask
    public class CronJobCheckLiveMeetingTaskUseCase : ICronJobCheckLiveMeetingTaskUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IBBBGetMeetings bbb { get; }
        private IWebHostEnvironment hosting;

        public CronJobCheckLiveMeetingTaskUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(CronJobCheckLiveMeetingTaskRequest message,
            IOutPutPort<GenericResponse<CronJobCheckLiveMeetingTaskResponse>> outputPort)
        {

            var lives = await bbb.GetLivesShort(new GetLivesRequest());
                outputPort.Handle(new GenericResponse<CronJobCheckLiveMeetingTaskResponse>
(new CronJobCheckLiveMeetingTaskResponse
{
    Message = "اضافه شد",
    Status = 200,

}));
                return;
            }

    }
    #endregion

}