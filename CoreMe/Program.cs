using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
using AutoMapper;
using Infrastructure.API;
using Infrastructure.Word;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Presenter;
using Presistence.Entities;
using SCPHandler;

using Serilog;
using TelegramServiceLog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(_ => { }, typeof(Program));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowHekmateSubdomains", builder =>
    {
        builder.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;

            var uri = new Uri(origin);
            var host = uri.Host.ToLower();

            // Allow local dev + production subdomains
            return host == "localhost"
                   || host == "127.0.0.1"
                   || host.EndsWith(".hekmateislami.com")
                   || host == "hekmateislami.com"
                   || host == "hek.either.ir"
                   || host.EndsWith(".either.ir");
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();  // must have concrete origin for credentials
    });

    // Policy خاص برای یک API خاص
    options.AddPolicy("AllowOnlyVC", policy =>
        {
            policy
                .WithOrigins("https://vc.hekmateislami.com") // دقیقا همین ساب‌دامین
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;

                var uri = new Uri(origin);
                var host = uri.Host.ToLower();

                return host == "localhost"
                       || host == "127.0.0.1"
                       || host.EndsWith(".hekmateislami.com")
                       || host.EndsWith(".either.ir");
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // include this if using cookies or auth
    });
    options.AddPolicy("AllowSwagger", builder =>
    {
        builder.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrEmpty(origin)) return false;
            var host = new Uri(origin).Host.ToLower();
            return host == "localhost"
                   || host == "127.0.0.1"
                   || host.EndsWith(".hekmateislami.com")
                   || host.EndsWith(".either.ir");
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // if Swagger UI uses credentials
    });
});



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
builder.Services.AddScoped<INewQuestionUseCase, NewQuestionUseCase>();
builder.Services.AddScoped<IGetQuestionOfMeetUseCase, GetQuestionOfMeetUseCase>();



#region Infrustructure
builder.Services.AddScoped<IScpUploader, ScpUploader>();
builder.Services.AddScoped<IBBBGetMeetings, BBBGetMeeting>();
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IWordService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var wwwroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    return new WordService(wwwroot);
});
#endregion
Directory.CreateDirectory("logs");
File.AppendAllText($"logs/std-{DateTime.Today:yyyy-MM-dd}.log",
    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [INF] اپ استارت شد — {Environment.NewLine}");


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreMe", Version = "v1" });

    // Remove servers for local development
    c.AddServer(new Microsoft.OpenApi.OpenApiServer { Url = "/" });
});


var app = builder.Build();
// if (app.Environment.IsDevelopment())
// {
//app.UseCors("AllowAllOrigins");
app.UseCors("AllowAllOrigins"); 

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Relative URL points to your swagger.json
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreMe v1");
    c.RoutePrefix = string.Empty; // optional: serve Swagger UI at root /
});

// }


//app.UseCors("AllowHekmateSubdomains");



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//middlewares

TelegramHelper.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
app.Run();
