using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
using AutoMapper;
using Infrastructure.API;
using Microsoft.EntityFrameworkCore;
using Presenter;
using Presistence.Entities;




var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(typeof(Program));
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowHekmateSubdomains", policy =>
{
    policy
        .SetIsOriginAllowed(origin =>
        {
            return new Uri(origin).Host.ToLower().Split("/")[0].EndsWith(".hekmateislami.com")
                   || new Uri(origin).Host.ToLower() == "hekmateislami.com";
        })
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod();
});

    // Policy خاص برای یک API خاص
    options.AddPolicy("AllowOnlyVC", policy =>
    {
        policy
            .WithOrigins("https://vc.hekmateislami.com") // دقیقا همین ساب‌دامین
            .AllowAnyHeader()
            .AllowAnyMethod();
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
builder.Services.AddScoped<IGetRoomByFriendlyIDUseCase, GetRoomByFriendlyIDUseCase>();
builder.Services.AddScoped<IGetLivesUseCase, GetLivesUseCase>();
builder.Services.AddScoped<IStartMeetingUseCase, StartMeetingUseCase>();
builder.Services.AddScoped<IGetMeetingURLUseCase, GetMeetingURLUseCase>();
builder.Services.AddScoped<IGetRoomsUseCase, GetRoomsUseCase>();



#region Infrustructure
builder.Services.AddScoped<IBBBGetMeetings, BBBGetMeeting>();
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }
app.UseCors("AllowHekmateSubdomains");  
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
