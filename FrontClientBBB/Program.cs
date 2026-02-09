using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
using FrontClientBBB.Components;
using Infrastructure.API;
using Infrastructure.Word;
using Microsoft.EntityFrameworkCore;
using Presistence.Entities;
using SCPHandler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();



builder.Services.AddDbContext<DBContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DBConnecDBConnectiontionMohrez")));

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
builder.Services.AddScoped<IGetRoomStatsUseCase, GetRoomStatsUseCase>();
builder.Services.AddScoped<IGetLastMeetingUseCase, GetLastMeetingUseCase>();
builder.Services.AddScoped<IGetPartisUseCase, GetPartisUseCase>();
builder.Services.AddScoped<IGetLastCertUseCase, GetLastCertUseCase>();
builder.Services.AddHttpContextAccessor();

#region Infrustructure
builder.Services.AddScoped<IScpUploader, ScpUploader>();
builder.Services.AddScoped<IBBBGetMeetings, BBBGetMeeting>();
builder.Services.AddScoped<IWordService>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var wwwroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    return new WordService(wwwroot);
});
#endregion

builder.Services.AddScoped<IDBContext, DBContext>();
builder.Services.AddScoped<IGetTrophiesUseCase, GetTrophiesUseCase>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
