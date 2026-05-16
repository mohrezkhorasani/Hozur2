using Application.DTO;
using Application.DTO.Generals;
using Application.DTO.Models;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
        private ITelegramService telegramService { get; }
        private IWebHostEnvironment hosting;

        public CronJobCheckLiveMeetingTaskUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb,
            ITelegramService telegramService
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
            this.telegramService = telegramService;
        }

        public async Task handleAsync(CronJobCheckLiveMeetingTaskRequest message,
            IOutPutPort<GenericResponse<CronJobCheckLiveMeetingTaskResponse>> outputPort)
        {
            int rows = 0;
            _ = TelegramHelper.SendMessage($"کرونجاب پاکسازی میت اغاز شد⌛");

            var lives = await bbb.RequestAll(new GetLivesRequest());
            if (!lives.Any())
            {
                //do stop others
                rows += _db.MeetingTBLs.Where(e => (e.Status == MainEntity_Status.MEETING_LIVE || e.Status == MainEntity_Status.MEETING_STARTED_LATER))
                    .ExecuteUpdate(e => e.SetProperty(r => r.Status, MainEntity_Status.MEETING_END_BY_CRON_JOB).SetProperty(r => r.EndDate, DateTime.Now));
            }
            else
            {
                var ids = lives.Select(e => e.RoomID).ToList();
                //_ = BackgroundRun(lives);
                if (_db.MeetingTBLs.Any(e => !ids.Contains(e.RoomFriendlyID) && (e.Status == MainEntity_Status.MEETING_LIVE || e.Status == MainEntity_Status.MEETING_STARTED_LATER)))
                {
                    rows += _db.MeetingTBLs.Where(e => !ids.Contains(e.RoomFriendlyID) && (e.Status == MainEntity_Status.MEETING_LIVE || e.Status == MainEntity_Status.MEETING_STARTED_LATER))
                        .ExecuteUpdate(e => e.SetProperty(r => r.Status, MainEntity_Status.MEETING_END_BY_CRON_JOB).SetProperty(r => r.EndDate, DateTime.Now));
                }
            }
            if (rows == 0)
            {

            }
            else
            {
                _ = telegramService.SendTelegramMessageAsync($"کرون جاب {rows} عدد رکورد در جدول میتینگ رو اپدیت کرد⚡");

            }

            _ = TelegramHelper.SendMessage($"🛑اتمام پاکسازی میت");





            outputPort.Handle(new GenericResponse<CronJobCheckLiveMeetingTaskResponse>
(new CronJobCheckLiveMeetingTaskResponse
{
    Message = "خوبستو خوبستو خیلی خوبست",
    Status = 200,

}));
            return;
        }
        public async Task BackgroundRun(List<MiniRoomJSON> meetss)
        {
            foreach (var i in meetss)
            {
                //RoomID = meetid اینجا
                try
                {
                    _db.RoomTBLs.Where(e => e.FriendlyID == i.RoomID)
.ExecuteUpdate(e => e.SetProperty(r => r.BBB_MeetID, i.BBB_MeetID));

                }
                catch (Exception e)
                {

                }

            }

        }

    }
    #endregion
    #region BackupDB
    public class BackupDBUseCase : IBackupDBUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IBBBGetMeetings bbb { get; }
        private ITelegramService telegramService { get; }
        private IWebHostEnvironment hosting;

        public BackupDBUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb,
            ITelegramService telegramService
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
            this.telegramService = telegramService;
        }

        public async Task handleAsync(BackupDBRequest message,
            IOutPutPort<GenericResponse<BackupDBResponse>> outputPort)
        {
            int rows = 0;
            _ = TelegramHelper.SendMessage($"بکاپ دیتابیس آغاز شد⌛");

            await _db.Database.ExecuteSqlRawAsync("EXEC dbo.Backup_AllUserDatabases");



            _ = TelegramHelper.SendMessage($"🛑اتمام بکاپ دیتابیس");

            outputPort.Handle(new GenericResponse<BackupDBResponse>
(new BackupDBResponse
{
    Message = "خوبستو خوبستو خیلی خوبست",
    Status = 200,

}));
            return;
        }

    }
    #endregion

}