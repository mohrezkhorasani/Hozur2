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
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NajmBridge.Cofigurations;
using NajmBridge.Presenter;
using Presistence.Entities;
using SCPHandler;
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



builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequireApiTokenAttribute>(); // همه کنترلرها نیاز به توکن دارن
}); 




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
builder.Services.AddScoped<IStartMeetingUseCase, StartMeetingUseCase>();
builder.Services.AddScoped<IGetMeetingURLUseCase, GetMeetingURLUseCase>();
builder.Services.AddScoped<IGetRoomsUseCase, GetRoomsUseCase>();
builder.Services.AddScoped<IGuarantCertificateUseCase, GuarantCertificateUseCase>();
builder.Services.AddScoped<IEndTheMeetingUseCase, EndTheMeetingUseCase>();  
builder.Services.AddScoped<IGetMeetingFromRoomIDUseCase, GetMeetingFromRoomIDUseCase>();  
builder.Services.AddScoped<ICronJobCheckLiveMeetingTaskUseCase, CronJobCheckLiveMeetingTaskUseCase>();
builder.Services.AddScoped<IGetMeetInfoUseCase, GetMeetInfoUseCase>();
builder.Services.AddScoped<IGetMembersOfMeetUseCase, GetMembersOfMeetUseCase>();
builder.Services.AddScoped<IGetMembersOfMeetOnlyCertificatedUseCase, GetMembersOfMeetOnlyCertificatedUseCase>();
builder.Services.AddScoped<IGenerateCertificateWithUserMeetIDUseCase, GenerateCertificateWithUserMeetIDUseCase>();



#region Infrustructure
builder.Services.AddScoped<IScpUploader, ScpUploader>();
builder.Services.AddScoped<ITelegramService, TelegramService>();
builder.Services.AddScoped<IBBBGetMeetings, BBBGetMeeting>();
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NajmBridge API", Version = "v1" });

    c.AddSecurityDefinition("ApiToken", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "api-token",
        In = ParameterLocation.Header,
        Description = "توکن: X7kP9mN2vL8qW3eR5tY6uI4oP9aS2dF1gH3jK5lM7nQ8rT9vU0wX2yZ4"
    });
    c.AddSecurityRequirement(document =>
    {
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("ApiToken")] = new List<string>()
        };
        return requirement;
    });


    c.AddServer(new Microsoft.OpenApi.OpenApiServer { Url = "/" });

});

var app = builder.Build();
// if (app.Environment.IsDevelopment())
// {
//app.UseCors("AllowAllOrigins");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Relative URL points to your swagger.json
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NajmBridge API");
    c.RoutePrefix = string.Empty; // optional: serve Swagger UI at root /
});


//app.UseCors("AllowHekmateSubdomains");



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//middlewares


app.Run();
