using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
using Infrastructure.API;
using Infrastructure.Word;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Presenter;
using Presistence.Entities;
using SCPHandler;
using TelegramServiceLog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();



builder.Services.AddControllers();
builder.Services.AddDbContext<DBContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DBConnecDBConnectiontionMohrez")));

builder.Services.AddScoped(typeof(ApiPresenter<>));
builder.Services.AddScoped(typeof(JsonContentResult));
builder.Services.AddScoped<IDBContext, DBContext>();
builder.Services.AddScoped<INewUserUseCase, NewUserUseCase>();
builder.Services.AddScoped<INewMeetingUseCase, NewMeetingUseCase>();
builder.Services.AddScoped<IUpdateMeetingUseCase, UpdateMeetingUseCase>();
builder.Services.AddScoped<INewUserMettingsUseCase, NewUserMettingsUseCase>();
builder.Services.AddScoped<INewUserMeetingLogUseCase, NewUserMeetingLogUseCase>();
builder.Services.AddScoped<INewUserMeetingLogFromModeratorUseCase, NewUserMeetingLogFromModeratorUseCase>();
builder.Services.AddScoped<IGetTeacherListUseCase, GetTeacherListUseCase>();
builder.Services.AddScoped<INewRoomUseCase, NewRoomUseCase>();
builder.Services.AddScoped<IGenerateCertificateUseCase, GenerateCertificateUseCase>();
builder.Services.AddScoped<IGetRoomByFriendlyIDUseCase, GetRoomByFriendlyIDUseCase>();
builder.Services.AddScoped<IGetLivesUseCase, GetLivesUseCase>();
builder.Services.AddScoped<IStartNewMeetUseCase, StartNewMeetUseCase>();
builder.Services.AddScoped<IGetMeetingURLUseCase, GetMeetingURLUseCase>();
builder.Services.AddScoped<IGetRoomsUseCase, GetRoomsUseCase>();
builder.Services.AddScoped<IGuarantCertificateUseCase, GuarantCertificateUseCase>();
builder.Services.AddScoped<IEndTheMeetingUseCase, EndTheMeetingUseCase>();
builder.Services.AddScoped<ICronJobCheckLiveMeetingTaskUseCase, CronJobCheckLiveMeetingTaskUseCase>();
builder.Services.AddScoped<IBackupDBUseCase, BackupDBUseCase>();
builder.Services.AddScoped<IGetRedirectURLUseCase, GetRedirectURLUseCase>();
builder.Services.AddScoped<IGetRedirectURLUseCase, GetRedirectURLUseCase>();
builder.Services.AddScoped<INewQuestionUseCase, NewQuestionUseCase>();
builder.Services.AddScoped<IGetQuestionOfMeetUseCase, GetQuestionOfMeetUseCase>();
builder.Services.AddScoped<IQuestionAnsweredUseCase, QuestionAnsweredUseCase>();    
builder.Services.AddScoped<IGetRoomListUseCase, GetRoomListUseCase>();



builder.Services.AddScoped<IScpUploader, ScpUploader>();
builder.Services.AddScoped<IBBBGetMeetings, BBBGetMeeting>();
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IWordService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var wwwroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    return new WordService(wwwroot);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}




app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
