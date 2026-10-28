using Application.Cache.Handlers;
using Application.DTO;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Services;
using Application.Usecase;
using AutoMapper;
using CoreMe.Cofigurations;
using CoreMe.Cofigurations.Auth;
using CoreMe.Presenter;
using Infrastracture;
using Infrastructure.API;
using Infrastructure.Word;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Presenter;
using Presistence.Entities;
using SCPHandler;

using Serilog;
using System.Reflection;
using TelegramServiceLog;
using BBBConfig = CoreMe.Cofigurations.BBBConfig;

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
                   || host == "192.168.0.122"
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
                .WithOrigins("https://vc.hekmateislami.com", "https://vc2.hekmateislami.com", "https://core.hekmateislami.com") // دقیقا همین ساب‌دامین
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
            .AllowCredentials().WithExposedHeaders("*"); // include this if using cookies or auth
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

var bbb = builder.Configuration.GetSection("bbb").Get<BBBConfig>();
Statics.bbb = new Application.General.BBBConfig
{
    AttendPassword = bbb.AttendPassword,
    ModeratorPassword = bbb.ModeratorPassword,
    Secret = bbb.Secret,
    Url = bbb.Url,
    UrlAPI = bbb.UrlAPI,
    Last = bbb.Last,
};



builder.Services.AddControllers();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.RegisterServicesFromAssembly(typeof(HekmatShobeCacheInvalidationHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(HekmatPlanCacheHandler).Assembly);

});

builder.Services.AddDbContext<DBContext>(option =>
    option.UseSqlServer(
        builder.Configuration.GetConnectionString("DBConnecDBConnectiontionMohrez"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            sqlOptions.CommandTimeout(300); // 5 دقیقه
        }
    ));

builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddHostedService<BackgroundTaskQueueService>();


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
builder.Services.AddScoped<IGetTheMembersUseCase, GetTheMembersUseCase>();
builder.Services.AddScoped<IEndTheMeetingUseCase, EndTheMeetingUseCase>();
builder.Services.AddScoped<ICronJobCheckLiveMeetingTaskUseCase, CronJobCheckLiveMeetingTaskUseCase>();
builder.Services.AddScoped<IBackupDBUseCase, BackupDBUseCase>();
builder.Services.AddScoped<IGetRedirectURLUseCase, GetRedirectURLUseCase>();
builder.Services.AddScoped<INewQuestionUseCase, NewQuestionUseCase>();
builder.Services.AddScoped<IGetQuestionOfMeetUseCase, GetQuestionOfMeetUseCase>();
builder.Services.AddScoped<IGetRoomsPublicUseCase, GetRoomsPublicUseCase>();
builder.Services.AddScoped<IQuestionAnsweredUseCase, QuestionAnsweredUseCase>();
builder.Services.AddScoped<ISignInUserPanelUseCase, SignInUserPanelUseCase>();
builder.Services.AddScoped<IGetAllRoomsUseCase, GetAllRoomsUseCase>();
builder.Services.AddScoped<IVerifyTokenUseCase, VerifyTokenUseCase>();
builder.Services.AddScoped<ILoginUserPanelUseCase, LoginUserPanelUseCase>();
builder.Services.AddScoped<IGetLivesPublicUseCase, GetLivesPublicUseCase>();
builder.Services.AddScoped<IContactusUseCase, ContactusUseCase>();
builder.Services.AddScoped<IConfirmOTPUseCase, ConfirmOTPUseCase>();
builder.Services.AddScoped<IVerifyCertificateUserUseCase, VerifyCertificateUserUseCase>();
builder.Services.AddScoped<IConfirmCertVerificationUseCase, ConfirmCertVerificationUseCase>();
builder.Services.AddScoped<IGetSlideShowUseCase, GetSlideShowUseCase>();
builder.Services.AddScoped<ISendOTPConfirmationUseCase, SendOTPConfirmationUseCase>();
builder.Services.AddScoped<ISendOTPRoomSubscribedUserUseCase, SendOTPRoomSubscribedUserUseCase>();
builder.Services.AddScoped<IConfirmOTPRoomSubscribedUserUseCase, ConfirmOTPRoomSubscribedUserUseCase>();
builder.Services.AddScoped<ISendOTPRoomSubscribedRegistrationUseCase, SendOTPRoomSubscribedRegistrationUseCase>();
builder.Services.AddScoped<IConfirmOTPRoomSubscribedRegistrationUseCase, ConfirmOTPRoomSubscribedRegistrationUseCase>();
builder.Services.AddScoped<IGetProfileInfoUseCase, GetProfileInfoUseCase>();
builder.Services.AddScoped<IGetReservationUseCase, GetReservationUseCase>();
builder.Services.AddScoped<INewReservationUseCase, NewReservationUseCase>();
builder.Services.AddScoped<IGetRoomMetadataUseCase, GetRoomMetadataUseCase>();
builder.Services.AddScoped<ISetRoomMetadataUseCase, SetRoomMetadataUseCase>();
builder.Services.AddScoped<IGetRoomWaitingMembersUseCase, GetRoomWaitingMembersUseCase>();
builder.Services.AddScoped<IChangeRoomMemberStatusUseCase, ChangeRoomMemberStatusUseCase>();
builder.Services.AddScoped<IDirectRoomSubscribedRegistrationUseCase, DirectRoomSubscribedRegistrationUseCase>();
builder.Services.AddScoped<IGetRoomFullDetailsUseCase, GetRoomFullDetailsUseCase>();
builder.Services.AddScoped<IEditRoomFullUseCase, EditRoomFullUseCase>();
builder.Services.AddScoped<IFollowProfileUseCase, FollowProfileUseCase>();
builder.Services.AddScoped<IAdminGetUserRoomListUseCase, AdminGetUserRoomListUseCase>();
builder.Services.AddScoped<IAdminCreateRoomUseCase, AdminCreateRoomUseCase>();
builder.Services.AddScoped<IGetPlanRoomPublicUseCase, GetPlanRoomPublicUseCase>();
builder.Services.AddScoped<IAdminChangeRoleUseCase, AdminChangeRoleUseCase>();
builder.Services.AddScoped<IAdminGetUserListUseCase, AdminGetUserListUseCase>();
builder.Services.AddScoped<IGetAllCertificateOfMemberUseCase, GetAllCertificateOfMemberUseCase>();
builder.Services.AddScoped<IGetHekmatShobeUseCase, GetHekmatShobeUseCase>();
builder.Services.AddScoped<IEditSlideShowUseCase, EditSlideShowUseCase>();
builder.Services.AddScoped<IInsertSlideShowUseCase, InsertSlideShowUseCase>();
builder.Services.AddScoped<INewPlanRoomUseCase, NewPlanRoomUseCase>();
builder.Services.AddScoped<IGetLatestMeetingsUseCase, GetLatestMeetingsUseCase>();
builder.Services.AddScoped<IDeletePlanRoomUseCase, DeletePlanRoomUseCase>();
builder.Services.AddScoped<IChangeAvatarOfRoomUseCase, ChangeAvatarOfRoomUseCase>();
builder.Services.AddScoped<IGetPlanRoomUseCase, GetPlanRoomUseCase>();
builder.Services.AddScoped<IGetLivesOfProfileUseCase, GetLivesOfProfileUseCase>();
builder.Services.AddScoped<IGetRoomCalendarActivityUseCase, GetRoomCalendarActivityUseCase>();
builder.Services.AddScoped<IGetMembersOfMeetUseCase, GetMembersOfMeetUseCase>();
builder.Services.AddScoped<IGenerateCertificateWithUserMeetIDUseCase, GenerateCertificateWithUserMeetIDUseCase>();
builder.Services.AddScoped<IGetLiveWithIDUseCase, GetLiveWithIDUseCase>();
builder.Services.AddScoped<IGetMembersOfMeetOnlyCertificatedUseCase, GetMembersOfMeetOnlyCertificatedUseCase>();
builder.Services.AddScoped<IAnswerPollListUseCase, AnswerPollListUseCase>();
builder.Services.AddScoped<IGetPollListUseCase, GetPollListUseCase>();
builder.Services.AddScoped<IGetUnansweredPollListUseCase, GetUnansweredPollListUseCase>();
builder.Services.AddScoped<IAddPollQuizUseCase, AddPollQuizUseCase>();
builder.Services.AddScoped<IGetPollListAdminUseCase, GetPollListAdminUseCase>();
builder.Services.AddScoped<IEndTheMeetingWithAPIUseCase, EndTheMeetingWithAPIUseCase>();
builder.Services.AddScoped<IUpdatePresentMinToCertOfMeetUseCase, UpdatePresentMinToCertOfMeetUseCase>();
builder.Services.AddScoped<IGetAllLivesUseCase, GetAllLivesUseCase>();
builder.Services.AddScoped<INewHookUseCase, NewHookUseCase>();
builder.Services.AddScoped<IGetHooksUseCase, GetHooksUseCase>();
builder.Services.AddScoped<IGetHookInfoUseCase, GetHookInfoUseCase>();
builder.Services.AddScoped<IDestroyWebhookAsyncUseCase, DestroyWebhookAsyncUseCase>();
builder.Services.AddScoped<IAdminCacheService, AdminCacheService>();
builder.Services.AddScoped<IDestroyAllWebhooksForMeetingAsyncUseCase, DestroyAllWebhooksForMeetingAsyncUseCase>();
builder.Services.AddScoped<IUpdateWebhookCallbackURLAsyncUseCase, UpdateWebhookCallbackURLAsyncUseCase>();
builder.Services.AddScoped<IBBBWebhookLogUseCase, BBBWebhookLogUseCase>();
builder.Services.AddScoped<IGetAllRoomsMiniUseCase, GetAllRoomsMiniUseCase>();
builder.Services.AddScoped<IGetPollReportUseCase, GetPollReportUseCase>();
builder.Services.AddScoped<IUploadFileStorageUseCase, UploadFileStorageUseCase>();
builder.Services.AddScoped<IGetSingleFileUseCase, GetSingleFileUseCase>();
builder.Services.AddScoped<IGetFileStorageUseCase, GetFileStorageUseCase>();
builder.Services.AddScoped<IUploadFileStorageUseCase, UploadFileStorageUseCase>();
builder.Services.AddScoped<IGetUserMeetLogsUseCase, GetUserMeetLogsUseCase>();
builder.Services.AddScoped<IGetAllMeetingUseCase, GetAllMeetingUseCase>();
builder.Services.AddScoped<IGetAttendsInBBBForAdminUseCase, GetAttendsInBBBForAdminUseCase>();
builder.Services.AddScoped<IEditRoomUseCase, EditRoomUseCase>();
builder.Services.AddScoped<ISencCheckListOfAttendsUseCase, SencCheckListOfAttendsUseCase>();



#region File Limit configuration
// تنظیمات برای IIS
builder.Services.Configure<IISServerOptions>(options =>
{
    //options.MaxRequestBodySize = 2L * 1024 * 1024 * 1024; // 2GB
    options.AllowSynchronousIO = true;
    options.AutomaticAuthentication = true;
    //options.MaxRequestBodyBufferSize = 2L * 1024 * 1024 * 1024;
});

builder.Services.Configure<IISOptions>(options =>
{
    options.ForwardClientCertificate = true;
    options.AutomaticAuthentication = true;
});

// تنظیمات Kestrel (برای زمانی که مستقیم اجرا می‌شود)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024;
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
    serverOptions.Limits.MaxConcurrentConnections = 1000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 1000;
});

// تنظیمات فرم برای آپلود
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; // 2GB
    options.MemoryBufferThreshold = int.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024;
});


#endregion

builder.Services.AddScoped<ISendSMS, SendSMS>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddScoped<IGeneralHeadersService, GeneralHeadersService>();
builder.Services.AddMemoryCache(); // اگر از Caching استفاده می‌کنید
builder.Services.AddHttpContextAccessor();


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

builder.Services.AddScoped<IPaymentPDFGenerator>(sp =>
{
    var env = sp.GetRequiredService<IWebHostEnvironment>();
    var wwwroot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    return new PaymentPDFGenerator(wwwroot);
});


#endregion

Directory.CreateDirectory("logs");



//vc2AuthNorm
#region JWT handlers
builder.Services.AddAuthentication("vc2AuthNorm") // "vc2AuthNorm" نام Scheme شماست
    .AddScheme<JwtAuthenticationVC2NormOptions, JwtAuthenticationVC2NormHandler>("vc2AuthNorm", "VC2Norm Authentication", options =>
    {
        // تنظیمات مربوط به توکن JWT را اینجا انجام دهید
        // این تنظیمات در کلاس JwtAuthenticationHandler استفاده خواهند شد
        options.ValidIssuer = "core.hekmateislami.com"; // مثال: تنظیم کنید اگر لازم است
        options.ValidAudience = "vc2.hekmateislami.com"; // مثال: تنظیم کنید اگر لازم است

    });

#endregion



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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Relative URL points to your swagger.json
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreMe v1");
    c.RoutePrefix = "docs"; // optional: serve Swagger UI at /docs /
});

// }


//app.UseCors("AllowHekmateSubdomains");

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAllOrigins");

//app.UseHttpsRedirection();

// --- فعال کردن احراز هویت ---
app.UseAuthentication();
// --- فعال کردن authorization ---
app.UseAuthorization();

app.UseMiddleware<GeneralHeadersMiddleware>();

app.MapControllers();


app.MapWhen(context => context.Request.Host.Host.Equals("vcfile.hekmateislami.com", StringComparison.OrdinalIgnoreCase),
    subdomainApp =>
    {
        var staticFileOptions = new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(@"C:\hekmatFiles"),
            RequestPath = "" // Serve files at the root of the subdomain
        };
        subdomainApp.UseStaticFiles(staticFileOptions);

        // Optional: If you need default files (like index.html)
        subdomainApp.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = staticFileOptions.FileProvider,
            RequestPath = staticFileOptions.RequestPath
        });
    });


//middlewares

TelegramHelper.Initialize(app.Services.GetRequiredService<IServiceScopeFactory>());
app.Run();
