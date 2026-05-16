using Application.DTO;
using Application.DTO.Generals;
using Application.DTO.Models;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using CacheHandler.Events;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client.Kerberos;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Application.Usecase
{
    #region NewUser
    public class NewUserUseCase : INewUserUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewUserUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewUserRequest message,
            IOutPutPort<GenericResponse<NewUserResponse>> outputPort)
        {
            UserTBL user = new UserTBL
            {
                UUID = message.UUID,
                Email = message.Email,
                Firstname = message.Firstname,
                NationalCode = message.NationalCode,
                PhoneNumber = message.PhoneNumber,
                Type = UserTBL.MapRoleToType(message.Role),
                Explain = message.Role,
                Status = MainEntity_Status.USER_ACTIVE
            };
            user = await _commonMethods.NewUser(user);
            if (user.AddIP(message.GeneralHeaders.RequestedIP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }

            outputPort.Handle(new GenericResponse<NewUserResponse>(new NewUserResponse
            {
                Message = "ok",
                Status = 200,
                Data = new DTO.Models.JWTTokenJSON
                {
                    Token = Statics.GenerateJWTUser(user)
                }
            }));

        }
    }
    #endregion

    #region LoginUserPanel
    public class LoginUserPanelUseCase : ILoginUserPanelUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public LoginUserPanelUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(LoginUserPanelRequest message,
            IOutPutPort<GenericResponse<LoginUserPanelResponse>> outputPort)
        {
            if (_db.UserTBLs.Any(e => e.Email == message.Username || e.Username == message.Username || e.NationalCode == message.Username))
            {
                var user = _db.UserTBLs.Where(e => e.Email == message.Username || e.Username == message.Username || e.NationalCode == message.Username || e.PhoneNumber == message.Username).First();
                var verify = _commonMethods.VerifyPasswordHash(message.Password, user.PasswordHash, user.PasswordSalt);
                if (verify)
                {
                    string token = Statics.GenerateJWTLoginVC2(user, message.GeneralHeaders);
                    user.AddIP(message.GeneralHeaders.RequestedIP);
                    user.AddGuid(Guid.NewGuid());
                    outputPort.Handle(new GenericResponse<LoginUserPanelResponse>(new LoginUserPanelResponse
                    {
                        Message = "ok",
                        Status = 200,
                        Data = new DTO.Models.JWTTokenJSON
                        {
                            Token = token
                        }
                    }));
                    var hashed = _commonMethods.HashToken(token);
                    _db.UserRevokeTokenTBL.Add(new UserRevokeTokenTBL
                    {
                        IDUserTBL = user.ID,
                        IsRevoked = false,
                        TokenHash = hashed,
                        Token = token,
                        Status = MainEntity_Status.Enabled,
                        Type = MainEntity_Types.Normal,
                        Explain = "login"
                    });
                    _db.SaveChanges();
                    return;
                }
            }
            outputPort.Handle(new GenericResponse<LoginUserPanelResponse>(new LoginUserPanelResponse
            {
                Message = "کاربر یافت نشد در صورت لزوم میتوانید <a href='https://vc2.hekmateislami.com/signup'>ثبت نام</a> کنید",
                Status = 400,
                Data = new DTO.Models.JWTTokenJSON
                {
                    Token = null
                }
            }));


        }
    }
    #endregion

    #region VerifyToken
    public class VerifyTokenUseCase : IVerifyTokenUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IConfiguration _configuration;

        public VerifyTokenUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _configuration = configuration;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(VerifyTokenRequest message,
            IOutPutPort<GenericResponse<VerifyTokenResponse>> outputPort)
        {
            try
            {
                string token = message.GeneralHeaders.JWTUserToken;

                // 1. بررسی وجود توکن
                if (string.IsNullOrEmpty(token))
                {
                    outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                    {
                        Message = "توکن معتبر نمی‌باشد. لطفاً دوباره وارد شوید.",
                        Status = 401,
                        Data = null
                    }));
                    return;
                }

                // 2. اعتبارسنجی و خواندن توکن JWT
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSecret = "a1b32c5e-8f1d-5a3c-8d5e-2f6a5b4c9d0e";
                var key = Encoding.ASCII.GetBytes(jwtSecret);

                try
                {
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // اعتبارسنجی توکن
                    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                    var jwtToken = (JwtSecurityToken)validatedToken;

                    // 3. خواندن Claim ها از توکن
                    var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "UserID" || x.Type == "uid" || x.Type == JwtRegisteredClaimNames.Sub);
                    var usernameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "Username" || x.Type == JwtRegisteredClaimNames.UniqueName);
                    var roleClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "Role" || x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                    var emailClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "Email" || x.Type == JwtRegisteredClaimNames.Email);
                    var fullnameClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "IP" || x.Type == "name");

                    if (userIdClaim == null)
                    {
                        outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                        {
                            Message = "ساختار توکن نامعتبر است. لطفاً دوباره وارد شوید.",
                            Status = 401,
                            Data = null
                        }));
                        return;
                    }

                    // 4. جستجوی کاربر در دیتابیس
                    var user = await _db.UserTBLs.FirstOrDefaultAsync(u => u.UUID == userIdClaim.Value);

                    if (user == null)
                    {
                        // کاربر یافت نشد
                        outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                        {
                            Message = "کاربر یافت نشد. در صورت لزوم می‌توانید <a href='https://vc2.hekmateislami.com/signup'>ثبت نام</a> کنید",
                            Status = 404,
                            Data = null
                        }));
                        return;
                    }

                    // 5. بررسی انقضای توکن در دیتابیس (اختیاری - برای قابلیت Logout از همه دستگاه‌ها)
                    var tokenHash = _commonMethods.HashToken(token);

                    var isTokenRevoked = await _db.UserRevokeTokenTBL.AnyAsync(rt => rt.TokenHash == tokenHash && rt.IsRevoked);
                    if (isTokenRevoked)
                    {
                        outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                        {
                            Message = "توکن شما منقضی شده است. لطفاً دوباره وارد شوید.",
                            Status = 401,
                            Data = null
                        }));
                        return;
                    }

                    // 6. به‌روزرسانی آخرین فعالیت کاربر
                    //user.LastActivity = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    // 7. ساخت پاسخ موفقیت‌آمیز
                    var response = new MiniUserJSON
                    {
                        ID = user.ID,
                        UID = user.UUID?.ToString() ?? userIdClaim.Value,
                        Fullname = user.Firstname ?? fullnameClaim?.Value ?? "",
                        Username = user.Username ?? usernameClaim?.Value ?? "",
                        Email = user.Email ?? emailClaim?.Value ?? "",
                        Role = user.GetRole() ?? roleClaim?.Value ?? "User",
                        IsValid = true,
                        Permissions = user.GetUserPermissions() // متد برای گرفتن دسترسی‌ها
                    };

                    outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                    {

                        Message = "success",
                        Status = 200,
                        Data = response
                    }));
                }
                catch (SecurityTokenExpiredException)
                {
                    outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                    {
                        Message = "توکن منقضی شده است. لطفاً دوباره وارد شوید.",
                        Status = 401,
                        Data = null
                    }));
                }
                catch (SecurityTokenInvalidSignatureException)
                {
                    outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                    {
                        Message = "امضای توکن نامعتبر است. ممکن است توکن دستکاری شده باشد.",
                        Status = 401,
                        Data = null
                    }));
                }
                catch (Exception ex)
                {
                    // لاگ خطا
                    Console.WriteLine($"Token validation error: {ex.Message}");

                    outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                    {
                        Message = "خطا در اعتبارسنجی توکن. لطفاً دوباره وارد شوید.",
                        Status = 500,
                        Data = null
                    }));
                }
            }
            catch (Exception ex)
            {
                outputPort.Handle(new GenericResponse<VerifyTokenResponse>(new VerifyTokenResponse
                {
                    Message = $"خطا در سرور: {ex.Message}",
                    Status = 500,
                    Data = null
                }));
            }
        }


    }
    #endregion

    #region SignInUserPanel
    public class SignInUserPanelUseCase : ISignInUserPanelUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMediator _mediator;

        public SignInUserPanelUseCase(IDBContext repository,
            IMediator _mediator,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this._mediator = _mediator;
        }

        public async Task handleAsync(SignInUserPanelRequest message,
            IOutPutPort<GenericResponse<SignInUserPanelResponse>> outputPort)
        {
            byte[] seenHash, seenSalt;

            UserTBL user;
            _commonMethods.CreatePasswordHash(message.Password, out seenHash, out seenSalt);

            if (int.TryParse(message.Username, out int o))
            {
                outputPort.Handle(new GenericResponse<SignInUserPanelResponse>(new SignInUserPanelResponse
                {
                    Message = "نام کاربری  باید با حرف اغاز شود",
                    Status = 400,

                }));
                return;
            }
            if (message.Username.Length < 4)
            {
                outputPort.Handle(new GenericResponse<SignInUserPanelResponse>(new SignInUserPanelResponse
                {
                    Message = "نام کاربری  باید حداقل چهار حرف باشد",
                    Status = 400,

                }));
                return;
            }

            if (_db.UserTBLs.Any(e => e.Email == message.Email || e.Username == message.Username))
            {
                outputPort.Handle(new GenericResponse<SignInUserPanelResponse>(new SignInUserPanelResponse
                {
                    Message = "نام کاربری  یا ادرس ایمیل تکراری میباشد",
                    Status = 400,

                }));
                return;
            }
            if (!_db.UserTBLs.Any(e => e.Email == message.Email))
            {
                string id = IdGenerator.GenerateId(2);
                while (_db.UserTBLs.Any(e => e.UUID == id))
                    id = IdGenerator.GenerateId(2);

                user = new UserTBL
                {
                    Email = message.Email,
                    Firstname = message.Name,
                    PasswordSalt = seenSalt,
                    PasswordHash = seenHash,
                    Type = MainEntity_Types.USER_USER,
                    Status = MainEntity_Status.USER_ACTIVATED_BY_SIGN_UP_SITE,
                    IPList = new List<string>() { message.GeneralHeaders.RequestedIP },
                    AllowedGUID = new List<Guid>() { Guid.NewGuid() },
                    UUID = id,
                    Explain = message.Explain.Contains("کاربر معمولی") ? null : message.Explain,
                    Username = message.Username
                };
                _db.UserTBLs.Add(user);
                _db.SaveChanges();
                await this._mediator.Publish(new UserUpdatedEvent
                {
                    Action = "Create",
                    UserId = user.ID
                });
                string token = Statics.GenerateJWTLoginVC2(user, message.GeneralHeaders);
                outputPort.Handle(new GenericResponse<SignInUserPanelResponse>(new SignInUserPanelResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = new DTO.Models.JWTTokenJSON
                    {
                        Token = token
                    }
                }));
                var hashed = _commonMethods.HashToken(token);
                _db.UserRevokeTokenTBL.Add(new UserRevokeTokenTBL
                {
                    IDUserTBL = user.ID,
                    IsRevoked = false,
                    TokenHash = hashed,
                    Token = token,
                    Status = MainEntity_Status.Enabled,
                    Type = MainEntity_Types.Normal,
                    Explain = "signup"
                });
                _db.SaveChanges();
                return;
            }
            outputPort.Handle(new GenericResponse<SignInUserPanelResponse>(new SignInUserPanelResponse
            {
                Message = "یوزر وجود دارد",
                Status = 400,
                Data = new DTO.Models.JWTTokenJSON
                {
                    Token = null
                }
            }));


            //if (user.AddIP(message.GeneralHeaders.RequestedIP))
            //{
            //    _db.Update(user);
            //    _db.SaveChanges();
            //}



        }
    }
    #endregion

    #region NewMeeting
    public class NewMeetingUseCase : INewMeetingUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewMeetingUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewMeetingRequest message,
            IOutPutPort<GenericResponse<NewMeetingResponse>> outputPort)
        {
            try
            {
                UserTBL user = new UserTBL
                {
                    UUID = message.UUID,
                    Email = message.Email,
                    Firstname = message.Firstname,
                    NationalCode = message.NationalCode,
                    PhoneNumber = message.PhoneNumber,
                    Type = UserTBL.MapRoleToType(message.Role),
                    Status = MainEntity_Status.USER_ACTIVATED_BY_CREATING_MEETING
                };
                Statics.Log("NewMeetingUseCase", message.RoomName);
                user = await _commonMethods.NewUser(user);
                if (user.AddIP(message.GeneralHeaders.RequestedIP))
                {
                    _db.Update(user);
                    _db.SaveChanges();
                }
                if (!UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
                {
                    outputPort.Handle(new GenericResponse<NewMeetingResponse>(new NewMeetingResponse
                    {
                        Message = "ok",
                        Status = 202,
                    }));
                    return;
                }
                if (!_db.RoomTBLs.Any(e => e.FriendlyID == message.RoomID))
                {
                    _db.RoomTBLs.Add(new RoomTBL
                    {
                        FriendlyID = message.RoomID,
                        IDOwnerTBL = user.ID,
                        Title = message.RoomName == null ? "" : message.RoomName,
                        BBB_MeetID = message.MeetID.Split("-")[0],
                        RecordRoom = true,
                        RequiredAdminAcceptToView = false,
                        RequiredLoginToView = false,

                    });
                    _db.SaveChanges();
                }
                MeetingTBL? meet = new MeetingTBL
                {
                    RoomFriendlyID = message.RoomID,
                    MeetID = message.MeetID,
                    Type = MainEntity_Types.MEET_GREEN_LIGHT,
                    Status = MainEntity_Status.MEETING_STARTED_LATER,
                    JalaliDate = Statics.ToJalaliDate(DateTime.Now),

                    IDCreator = user.ID
                };

                meet = await _commonMethods.NewMeeting(meet);

                meet = await _db.MeetingTBLs.Include(e => e.Creator)
                    .Include(e => e.Teacher).Include(e => e.RoomTBL)
                    .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                    .FirstOrDefaultAsync(e => e.ID == meet.ID);
                if (meet == null) return;
                var teachers = meet.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).Select(e => e.UserTBL).ToList();

                outputPort.Handle(new GenericResponse<NewMeetingResponse>(new NewMeetingResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = new DTO.Models.MeetJSON
                    {
                        JWT = new DTO.Models.JWTTokenJSON
                        {
                            Token = meet.JWT
                        },
                        Creator = new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{meet.Creator.Firstname} {meet.Creator.LastName}",
                            ID = meet.Creator.ID,
                        },
                        Teacher = teachers.Select(r => new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{r.Firstname} {r.LastName}",
                            ID = r.ID,
                        }).ToList(),
                        Room = new DTO.Models.MiniRoomJSON
                        {
                            RoomID = meet.RoomFriendlyID,
                            Title = meet.RoomTBL.Title
                        },
                        JalaliDate = meet.JalaliDate,
                        MeetID = meet.MeetID,
                        StartedTime = meet.DateInsert.TimeOfDay.ToString(),
                        Status = MeetingTBL.GetStatusString(meet.Status),
                        Title = meet.Title
                    }
                }));
            }
            catch (Exception e)
            {
                Statics.Log($"NewMeetingUseCase exception", e.InnerException.Message);
                outputPort.Handle(new GenericResponse<NewMeetingResponse>(new NewMeetingResponse
                {
                    Message = e.Message,
                    Status = 400,
                }));
                return;
            }


        }
    }
    #endregion

    #region UpdateMeeting
    public class UpdateMeetingUseCase : IUpdateMeetingUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public UpdateMeetingUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(UpdateMeetingRequest message,
            IOutPutPort<GenericResponse<UpdateMeetingResponse>> outputPort)
        {
            var init = await _commonMethods.Initialize<UpdateMeetingRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;

            if (!await _commonMethods.CheckUserAccessibilityToMeet(user, meet, "UpdateMeeting"))
            {
                outputPort.Handle(new GenericResponse<UpdateMeetingResponse>(new UpdateMeetingResponse
                {
                    Message = "forbidden",
                    Status = 403,
                }));
                return;
            }
            if (string.IsNullOrEmpty(message.Title))
            {
                List<UserTBL> teachers = _db.UserTBLToMeetingTBLs.Where(e => e.MeetingTBL.IDTeacher != null && e.Type == MainEntity_Types.USER_TEACHER && e.MeetID == meet.MeetID).Select(e => e.UserTBL).ToList();

                outputPort.Handle(new GenericResponse<UpdateMeetingResponse>(new UpdateMeetingResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = new DTO.Models.MeetJSON
                    {
                        JWT = new DTO.Models.JWTTokenJSON
                        {
                            Token = meet.JWT
                        },
                        Creator = new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{meet.Creator.Firstname} {meet.Creator.LastName}",
                            ID = meet.Creator.ID,
                        },
                        Teacher = teachers.Select(r => new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{r.Firstname} {r.LastName}",
                            ID = r.ID,
                        }).ToList(),
                        Room = new DTO.Models.MiniRoomJSON
                        {
                            RoomID = meet.RoomFriendlyID,
                            Title = meet.RoomTBL.Title
                        },
                        JalaliDate = meet.JalaliDate,
                        MeetID = meet.MeetID,
                        StartedTime = meet.DateInsert.TimeOfDay.ToString(),
                        Status = MeetingTBL.GetStatusString(meet.Status),
                        Title = meet.Title
                    }
                }));
                return;
            }
            else
            {

                meet.Title = message.Title;
                meet.JalaliDate = message.JalaliDate;
                meet.Status = MainEntity_Status.MEETING_LIVE;
                List<UserTBL> teachers = new List<UserTBL>();

                Statics.Log("UpdateMeeting", JsonConvert.SerializeObject(message.Teacher));

                foreach (var t in message.Teacher)
                {

                    UserTBL teacher = null;
                    if (_db.UserTBLs.Any(
                        e => (e.Firstname + " " + e.LastName).Trim() == t
                            && e.Status == MainEntity_Status.USER_ACTIVE
                            && e.Type == MainEntity_Types.USER_TEACHER
                    ))
                    {
                        Statics.Log("UpdateMeeting", $"{t} existed user!");

                        teacher = _db.UserTBLs.FirstOrDefault(
                            e => (e.Firstname + " " + e.LastName).Trim() == t
                            && e.Status == MainEntity_Status.USER_ACTIVE
                            && e.Type == MainEntity_Types.USER_TEACHER
                        );
                        teachers.Add(teacher);
                    }
                    else
                    {
                        teacher = new UserTBL
                        {
                            Firstname = t,
                            Type = MainEntity_Types.USER_TEACHER,
                            Status = MainEntity_Status.USER_ACTIVE,
                            Username = t,
                            UUID = new Random().NextInt64(-999999999999999999, -99999999999999999).ToString()
                        };
                        teachers.Add(teacher);
                        _db.UserTBLs.Add(teacher);
                        _db.SaveChanges();
                    }
                    if (!_db.UserTBLToMeetingTBLs.Any(e => e.UUID == teacher.UUID && e.MeetID == meet.MeetID))
                    {
                        _db.UserTBLToMeetingTBLs.Add(new UserTBLToMeetingTBL
                        {
                            MeetID = meet.MeetID,
                            Type = MainEntity_Types.USER_TEACHER,
                            Status = MainEntity_Status.MEETING_STARTED_LATER,
                            UUID = teacher.UUID,
                            UserMeetID = "TEACHER",
                            Role = "Teacher",
                            UserNickName = "Teacher",
                            IsCertificated = false,
                            TokenJWT = "nope",
                            TimeSpendSeconds = 0,

                        });
                        _db.SaveChanges();
                    }

                }


                meet.IDTeacher = teachers[teachers.Count - 1].ID;
                meet.Teacher = teachers[teachers.Count - 1];
                _db.Update(meet);
                _db.SaveChanges();

                outputPort.Handle(new GenericResponse<UpdateMeetingResponse>(new UpdateMeetingResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = new DTO.Models.MeetJSON
                    {
                        JWT = new DTO.Models.JWTTokenJSON
                        {
                            Token = meet.JWT
                        },
                        Creator = new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{meet.Creator.Firstname} {meet.Creator.LastName}",
                            ID = meet.Creator.ID,
                        },
                        Teacher = teachers.Select(r => new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{r.Firstname} {r.LastName}",
                            ID = r.ID,
                        }).ToList(),
                        Room = new DTO.Models.MiniRoomJSON
                        {
                            RoomID = meet.RoomFriendlyID,
                            Title = meet.RoomTBL.Title
                        },
                        JalaliDate = meet.JalaliDate,
                        MeetID = meet.MeetID,
                        StartedTime = meet.DateInsert.TimeOfDay.ToString(),
                        Status = MeetingTBL.GetStatusString(meet.Status),
                        Title = meet.Title
                    }
                }));
            }

        }
    }
    #endregion

    #region NewUserMettings
    public class NewUserMettingsUseCase : INewUserMettingsUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewUserMettingsUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewUserMettingsRequest message,
            IOutPutPort<GenericResponse<NewUserMettingsResponse>> outputPort)
        {
            if (message.GeneralHeaders.JWTMeetingUserToken == null)
                return;
            UserTBL user = new UserTBL
            {
                UUID = message.UUID,
                Email = message.Email,
                Firstname = message.Firstname,
                NationalCode = message.NationalCode,
                PhoneNumber = message.PhoneNumber,
                Status = MainEntity_Status.USER_ACTIVATED_BY_JOINING_MEETING,
                Type = UserTBL.MapRoleToType(message.Role),
            };

            user = await _commonMethods.NewUser(user);
            if (user.AddIP(message.GeneralHeaders.RequestedIP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            string jwtUser = Statics.GenerateJWTUser(user);
            var meet = await _commonMethods.GetMeetWithoutID(message.MeetID);
            if (meet == null)
            {
                outputPort.Handle(new GenericResponse<NewUserMettingsResponse>(new NewUserMettingsResponse
                {
                    Message = "no meet yet",
                    Status = 404
                }));
                return;
            }


            var userMeet = await _commonMethods.NewUserMeeting(new UserTBLToMeetingTBL
            {
                MeetID = meet.MeetID,
                UUID = message.UUID,
                TokenJWT = jwtUser,
                Explain = message.Role,
                Type = UserTBL.MapRoleToType(message.Role),
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                Role = message.Role,
                UserMeetID = message.MeetUserID,
                IPList = new List<string>() { message.GeneralHeaders.RequestedIP },
                UserNickName = message.NickName,
                TimeSpendSeconds = 0,
                IsCertificated = false,

            });
            if (!userMeet.IPList.Contains(message.GeneralHeaders.RequestedIP))
            {
                userMeet.IPList.Add(message.GeneralHeaders.RequestedIP);
                _db.Update(userMeet);
                _db.SaveChanges();
            }

            meet = await _db.MeetingTBLs.Include(e => e.Creator)
                .Include(e => e.Teacher).Include(e => e.RoomTBL)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                .FirstOrDefaultAsync(e => e.ID == meet.ID);
            if (meet == null) return;
            var teachers = meet.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).Select(e => e.UserTBL).ToList();

            outputPort.Handle(new GenericResponse<NewUserMettingsResponse>(new NewUserMettingsResponse
            {
                Message = "ok",
                Status = 200,
                User = jwtUser,
                Data = new DTO.Models.MeetJSON
                {
                    JWT = new DTO.Models.JWTTokenJSON
                    {
                        Token = meet.JWT
                    },
                    Creator = new DTO.Models.MiniUserJSON
                    {
                        Fullname = $"{meet.Creator.Firstname} {meet.Creator.LastName}",
                        ID = meet.Creator.ID,
                    },
                    Teacher = teachers.Select(r => new DTO.Models.MiniUserJSON
                    {
                        Fullname = $"{r.Firstname} {r.LastName}",
                        ID = r.ID,
                    }).ToList(),
                    Room = new DTO.Models.MiniRoomJSON
                    {
                        RoomID = meet.RoomFriendlyID,
                        Title = meet.RoomTBL.Title
                    },
                    JalaliDate = meet.JalaliDate,
                    MeetID = meet.MeetID,
                    StartedTime = meet.DateInsert.TimeOfDay.ToString(),
                    Status = MeetingTBL.GetStatusString(meet.Status),
                    Title = meet.Title
                }
            }));

        }
    }
    #endregion

    #region NewUserMeetingLog
    public class NewUserMeetingLogUseCase : INewUserMeetingLogUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewUserMeetingLogUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewUserMeetingLogRequest message,
            IOutPutPort<GenericResponse<NewUserMeetingLogResponse>> outputPort)
        {
            try
            {

                outputPort.Handle(new GenericResponse<NewUserMeetingLogResponse>(new NewUserMeetingLogResponse
                {
                    Message = "ok",
                    Status = 200,

                }));
                Statics.Log("NewUserMeetingLogRequest", JsonConvert.SerializeObject(message));

                var init = await _commonMethods.Initialize<NewUserMeetingLogRequest>(message);
                var user = init.Item1;
                var meet = init.Item2;
                var userMeet = _db.UserTBLToMeetingTBLs
                    .Include(e => e.UserTBLToMeetingTBLLogs)
                    .FirstOrDefault(e => e.MeetID == meet.MeetID && e.UUID == user.UUID);
                Statics.Log("NewUserMeetingLogRequest", userMeet.ID.ToString());
                UserTBLToMeetingTBLLogs? log = new UserTBLToMeetingTBLLogs
                {
                    Type = UserTBL.MapRoleToType(message.Role),
                    Status = MainEntity_Status.LOG_NORMAL,
                    Key = message.KeyLog,
                    Value = message.ValueLog,
                    Explain = message.Role,
                    UserMeetID = message.MeetUserID,
                    IDUserTBLToMeetingTBL = userMeet.ID,
                    OS = message.OS + "/$$" + message.GeneralHeaders.Agent + "/$$" + message.GeneralHeaders.RequestedDevice + "/$$" + message.GeneralHeaders.Referer
                };
                if (userMeet.UserTBLToMeetingTBLLogs.Any(e => e.UserMeetID == message.MeetUserID))
                {
                    Statics.Log("NewUserMeetingLogRequest", $"Has meeting logs with that {message.KeyLog}");
                    var logs = userMeet.UserTBLToMeetingTBLLogs.OrderByDescending(e => e.ID);
                    if (message.KeyLog.ToLower() == "join")
                    {
                        Statics.Log("NewUserMeetingLogRequest", $"message is join {message.KeyLog}");
                        if (logs.Any(e => e.Key == "join" && e.Status == MainEntity_Status.LOG_NORMAL))
                        {
                            Statics.Log("NewUserMeetingLogRequest", $"return shod chon {message.KeyLog}");
                            return;
                        }
                    }
                    if (message.KeyLog.ToLower() == "left")
                    {
                        Statics.Log("NewUserMeetingLogRequest", $"Left omad to {message.KeyLog}");
                        _db.UserTBLToMeetingTBLLogs
                            .Where(e => e.IDUserTBLToMeetingTBL == userMeet.ID && e.Key == "JOIN")
                            .ExecuteUpdate(e => e.SetProperty(r => r.Status, MainEntity_Status.LOG_KILLED));
                        _db.UserTBLToMeetingTBLLogs
                           .Where(e => e.IDUserTBLToMeetingTBL == userMeet.ID && e.Key == "LEFT")
                           .ExecuteUpdate(e => e.SetProperty(r => r.Status, MainEntity_Status.LOG_KILLED));
                        Statics.Log("NewUserMeetingLogRequest", $"Left Tamomshod {message.KeyLog}");
                    }
                }


                _db.UserTBLToMeetingTBLLogs.Add(log);
                _db.SaveChanges();
                Statics.Log("NewUserMeetingLogRequest", $"Add shod--> {message.KeyLog}");

                outputPort.Handle(new GenericResponse<NewUserMeetingLogResponse>(new NewUserMeetingLogResponse
                {
                    Message = "inserted",
                    Status = 200,

                }));

            }
            catch (Exception e)
            {
                Statics.Log("NewUserMeetingLogRequest FAILED", e.Message);

            }
        }
    }
    #endregion

    #region NewUserMeetingLogFromModerator
    public class NewUserMeetingLogFromModeratorUseCase : INewUserMeetingLogFromModeratorUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewUserMeetingLogFromModeratorUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewUserMeetingLogFromModeratorRequest message,
            IOutPutPort<GenericResponse<NewUserMeetingLogFromModeratorResponse>> outputPort)
        {
            try
            {

                var init = await _commonMethods.Initialize<NewUserMeetingLogFromModeratorRequest>(message);
                var user = init.Item1;
                var meet = init.Item2;
                Statics.Log("NewUserMeetingLogFromModeratorUseCase", JsonConvert.SerializeObject(message));

                if (!UserTBL.TYPE_ALLOWED_LOG_FOR_OTHERS.Contains(user.Type))
                {
                    outputPort.Handle(new GenericResponse<NewUserMeetingLogFromModeratorResponse>(new NewUserMeetingLogFromModeratorResponse
                    {
                        Message = "ok",
                        Status = 403,
                    }));
                    return;
                }
                if (_db.UserTBLToMeetingTBLs.Any(e => e.MeetID == meet.MeetID))
                {
                    Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"any meet.MeetID {meet.MeetID}");


                    var userMeet = _db.UserTBLToMeetingTBLs
                        .Include(e => e.UserTBLToMeetingTBLLogs)
                        .FirstOrDefault(e => e.MeetID == meet.MeetID);

                    Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"any userMeet {userMeet.ID}");
                    Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"log creatingggg {JsonConvert.SerializeObject(message.GeneralHeaders)}");

                    UserTBLToMeetingTBLLogs? log = new UserTBLToMeetingTBLLogs
                    {
                        Type = UserTBL.MapRoleToType(message.Role),
                        Status = MainEntity_Status.MEETING_STARTED_LATER,
                        Key = message.KeyLog,
                        Value = message.ValueLog,
                        Explain = message.Role,
                        UserMeetID = message.MeetUserID,
                        IDUserTBLToMeetingTBL = userMeet.ID,
                        OS = message.ExtId + "/$$" + message.GeneralHeaders.Agent + "/$$" + message.GeneralHeaders.RequestedDevice + "/$$" + message.GeneralHeaders.Referer,

                    };
                    Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"log created");

                    if (userMeet.UserTBLToMeetingTBLLogs.Any(e => e.UserMeetID == message.MeetUserID))
                    {
                        Statics.Log("NewUserMeetingLogFromModeractorUseCase", $"Has meeting logs with that {message.KeyLog}");
                        var logs = userMeet.UserTBLToMeetingTBLLogs.OrderByDescending(e => e.ID);
                        if (message.KeyLog.ToLower() == "join")
                        {
                            Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"message is join {message.KeyLog}");
                            if (logs.Any(e => e.Key == "join" && e.Status == MainEntity_Status.LOG_NORMAL))
                            {
                                Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"return shod chon {message.KeyLog}");
                                return;
                            }
                        }
                        if (message.KeyLog.ToLower() == "left")
                        {
                            Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"Left omad to {message.KeyLog}");
                            _db.UserTBLToMeetingTBLLogs
                                .Where(e => e.IDUserTBLToMeetingTBL == userMeet.ID && e.Key == "JOIN")
                                .ExecuteUpdate(e => e.SetProperty(r => r.Status, MainEntity_Status.LOG_KILLED));
                            _db.UserTBLToMeetingTBLLogs
                               .Where(e => e.IDUserTBLToMeetingTBL == userMeet.ID && e.Key == "LEFT")
                               .ExecuteUpdate(e => e.SetProperty(r => r.Status, MainEntity_Status.LOG_KILLED));
                            Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"Left Tamomshod {message.KeyLog}");
                        }
                    }
                    _db.UserTBLToMeetingTBLLogs.Add(log);
                    Statics.Log("NewUserMeetingLogFromModeratorUseCase", $"SavingDB");

                    _db.SaveChanges();
                }

            }
            catch (Exception e)
            {
                Statics.Log("NewUserMeetingLogFromModeratorUseCase FAILED", $"{e.Message} {e.StackTrace} {e.Source} {e.HelpLink} ");
                outputPort.Handle(new GenericResponse<NewUserMeetingLogFromModeratorResponse>(new NewUserMeetingLogFromModeratorResponse
                {
                    Message = "ERRROR",
                    Status = 403,

                }));
                return;
            }
            outputPort.Handle(new GenericResponse<NewUserMeetingLogFromModeratorResponse>(new NewUserMeetingLogFromModeratorResponse
            {
                Message = "ok",
                Status = 200,

            }));

        }
    }
    #endregion

    #region GetTeacherList
    public class GetTeacherListUseCase : IGetTeacherListUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public GetTeacherListUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetTeacherListRequest message,
            IOutPutPort<GenericResponse<GetTeacherListResponse>> outputPort)
        {
            //Statics.Log("GetTeacherListUseCase", JsonConvert.SerializeObject(message.GeneralHeaders));

            //var init = await _commonMethods.Initialize<GetTeacherListRequest>(message);
            //var user = init.Item1;
            //var meet = init.Item2;
            //if (!UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
            //{
            //    outputPort.Handle(new GenericResponse<GetTeacherListResponse>(new GetTeacherListResponse
            //    {
            //        Message = "You can't access it",
            //        Status = 200,
            //        Data = meet.Teacher == null ? [] : [new MiniUserJSON{
            //            Fullname=meet.Teacher.Firstname + " " + meet.Teacher.LastName,
            //        }]
            //    }));
            //    return;
            //}

            outputPort.Handle(new GenericResponse<GetTeacherListResponse>(new GetTeacherListResponse
            {
                Message = "ok",
                Status = 200,
                Data = _db.UserTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER)
                .Select(e => new DTO.Models.MiniUserJSON
                {
                    Fullname = $"{e.Firstname} {e.LastName}",
                    ID = e.ID,
                    UID = ""
                }).ToList()
            }));

        }
    }
    #endregion

    #region NewRoom
    public class NewRoomUseCase : INewRoomUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public NewRoomUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(NewRoomRequest message,
            IOutPutPort<GenericResponse<NewRoomResponse>> outputPort)
        {
            UserTBL user = new UserTBL
            {
                UUID = message.UUID,
                Email = message.Email,
                Firstname = message.Firstname,
                NationalCode = message.NationalCode,
                PhoneNumber = message.PhoneNumber,
            };
            user = await _commonMethods.GetUserWithoutID(user);
            if (!UserTBL.TYPE_ALLOWED_ROOM_CREATE.Contains(user.Type))
            {
                return;
            }
            if (_db.RoomTBLs.Any(e => e.FriendlyID == message.FriendlyID))
            {
                return;
            }
            _db.RoomTBLs.Add(new RoomTBL
            {
                FriendlyID = message.FriendlyID,

                Title = message.Room,
                IDOwnerTBL = user.ID
            });
            _db.SaveChanges();
            outputPort.Handle(new GenericResponse<NewRoomResponse>(new NewRoomResponse
            {
                Message = "ok",
                Status = 200,
            }));

        }
    }
    #endregion

    #region GetRooms
    public class GetRoomsUseCase : IGetRoomsUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _cache;

        public GetRoomsUseCase(IDBContext repository,
            IConfiguration configuration,
            IMemoryCache memoryCache)  // ← inject IMemoryCache
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            _cache = memoryCache;
        }

        public async Task handleAsync(GetRoomsRequest message,
     IOutPutPort<GenericResponse<GetRoomsResponse>> outputPort)
        {
            try
            {
                List<DTO.Models.MiniRoomJSON>? result = null;
                long idUser = 0;
                if (message.GeneralHeaders?.UserTBL?.ID != null)
                {
                    idUser = message.GeneralHeaders.UserTBL.ID;
                }
                if (idUser == 0 && string.IsNullOrEmpty(message.UserName))
                {
                    outputPort.Handle(new GenericResponse<GetRoomsResponse>(new GetRoomsResponse
                    {
                        Message = "نام کاربری مورد نیاز است",
                        Status = 400,
                        Data = null
                    }));
                    return;
                }
                string cacheKey = $"GetRooms_User_{idUser}_Query_{message.Query ?? ""}";

                if (message.UseCache)
                {

                    // Build a unique cache key per user and query
                    if (message.GeneralHeaders.UserTBL == null)
                    {

                        return;
                    }


                    // Try to get data from cache (with error protection)
                    try
                    {
                        if (_cache.TryGetValue(cacheKey, out List<DTO.Models.MiniRoomJSON>? cachedResult))
                        {
                            result = cachedResult;
                        }
                    }
                    catch (Exception cacheEx)
                    {
                        // If cache read fails, log and continue as if cache miss
                        Statics.Log("GetRooms_CacheRead", cacheEx);
                    }
                }
                if (!await _db.RoomTBLs.AnyAsync((e => (
                            string.IsNullOrEmpty(message.Query) ||
                                e.Title.Contains(message.Query)) && e.IDOwnerTBL == idUser
                            //((e.IDOwnerTBL != idUser && e.OwnerTBL.Username == message.UserName) || (e.IDOwnerTBL == idUser && message.UserName == null) ||
                            //(e.IDOwnerTBL == idUser && message.UserName == e.OwnerTBL.Username))
                            )))
                {

                    outputPort.Handle(new GenericResponse<GetRoomsResponse>(new GetRoomsResponse
                    {
                        Message = "این کاربر گروهی ندارد",
                        Status = 404,
                        Data = []
                    }));
                    return;
                }
                if (result == null)
                {
                    // Cache miss – compute data as before
                    var rooms = await _db.RoomTBLs
                        .Where(e => (
                            string.IsNullOrEmpty(message.Query) ||
                                e.Title.Contains(message.Query)) && e.IDOwnerTBL == idUser
                        //((e.IDOwnerTBL != idUser && e.OwnerTBL.Username == message.UserName) || 
                        //(e.IDOwnerTBL == idUser && message.UserName == null) ||
                        //(e.IDOwnerTBL == idUser && message.UserName == e.OwnerTBL.Username)) 
                        )
                        .Include(e => e.MeetingTBLs)
                            .ThenInclude(m => m.UserTBLToMeetingTBLs)
                                .ThenInclude(um => um.UserTBL)
                                .Include(e => e.OwnerTBL)
                        .OrderByDescending(e => e.DateInsert)
                        .Select(e => new
                        {
                            e.FriendlyID,
                            e.Title,
                            e.DateInsert,
                            plan = e.RoomTBLPlanTBLs.OrderBy(e => e.StartDate).ToList(),
                            e.OwnerTBL,
                            e.Avatar,
                            Meetings = e.MeetingTBLs.Select(m => new
                            {
                                m.ID,
                                m.DateInsert,
                                Participants = m.UserTBLToMeetingTBLs.Select(um => new
                                {
                                    um.UserNickName,
                                    um.UserTBL.UUID,
                                    um.UserTBL.Email,
                                    um.UserTBL.NationalCode,
                                    um.UserTBL.PhoneNumber,
                                    um.UserTBL.Type
                                })
                            })
                        })
                        .ToListAsync();

                    // Compute in memory
                    result = rooms.Select(room => new DTO.Models.MiniRoomJSON
                    {
                        RoomID = room.FriendlyID,
                        Title = room.Title,
                        Created = room.DateInsert,
                        Owner = new MiniUserJSON
                        {
                            UID = room.OwnerTBL.UUID,
                            Username = room.OwnerTBL.Username,
                            Fullname = room.OwnerTBL.Firstname
                        },
                        LastMeet = room.Meetings
                            .OrderByDescending(m => m.DateInsert)
                            .FirstOrDefault()?.DateInsert ?? room.DateInsert,
                        Count = room.Meetings.Count(m => m.Participants
                            .Count(p => p.Type != MainEntity_Types.USER_TEACHER) > 1),
                        NextMeet = room.plan.Any(e => e.StartDate > DateTime.Now) ? room.plan.Where(e => e.StartDate > DateTime.Now).First().StartDate : null,
                        CountTeacher = room.Meetings
                            .SelectMany(m => m.Participants)
                            .Where(p => p.Type == MainEntity_Types.USER_TEACHER)
                            .Select(p => p.UUID ?? p.Email ?? p.NationalCode ?? p.PhoneNumber ?? p.UserNickName)
                            .Where(k => !string.IsNullOrWhiteSpace(k))
                            .Distinct()
                            .Count(),
                        Avatar = room.Avatar,
                        PlanJSONs = room.plan.Any() ? room.plan.Select(r => new MiniPlanJSON
                        {
                            Id = r.ID,
                            StartAt = r.StartDate,
                            Teacher = r.Teachers,
                            Dabir = r.Dabir,
                            Title = r.Title
                        }).ToList() : [],
                        CountPartis = room.Meetings
                            .SelectMany(m => m.Participants)
                            .Where(p => p.Type != MainEntity_Types.USER_TEACHER)
                            .SelectMany(p => new[]
                            {
                            p.UUID?.Trim().ToLower(),
                            p.UserNickName?.Trim().ToLower(),
                            p.Email?.Trim().ToLower(),
                            p.NationalCode?.Trim(),
                            p.PhoneNumber?.Trim()
                            })
                            .Where(k => !string.IsNullOrWhiteSpace(k))
                            .Distinct()
                            .Count()
                    }).OrderByDescending(e => e.NextMeet).ThenBy(e => e.LastMeet).ToList();
                    // Store in cache (with error protection)
                    if (message.UseCache)
                    {
                        try
                        {
                            var cacheOptions = new MemoryCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                                .SetPriority(CacheItemPriority.Normal);

                            _cache.Set(cacheKey, result, cacheOptions);
                        }
                        catch (Exception cacheWriteEx)
                        {
                            // If cache write fails (e.g., memory full), log and continue
                            Statics.Log("GetRooms_CacheWrite", cacheWriteEx);
                        }
                    }

                }

                outputPort.Handle(new GenericResponse<GetRoomsResponse>(new GetRoomsResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = result
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Statics.Log("GetRooms", e);
                // Optionally, you can still return an error response via outputPort
                // outputPort.Handle(new GenericResponse<GetRoomsResponse> { Status = 500, Message = "Internal error" });
            }
        }
    }
    #endregion

    #region GetRoomsPublic
    public class GetRoomsPublicUseCase : IGetRoomsPublicUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IMemoryCache _cache { get; }

        public GetRoomsPublicUseCase(IDBContext repository,
            IMemoryCache _cache,
            IConfiguration configuration)
        {
            _db = repository;
            this._cache = _cache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetRoomsPublicRequest message,
        IOutPutPort<GenericResponse<GetRoomsPublicResponse>> outputPort)
        {
            try
            {
                if (!string.IsNullOrEmpty(message.UserName) && !message.UserName.StartsWith("@"))
                {
                    throw new Exception("user must start with @");
                }
                if (!string.IsNullOrEmpty(message.UserName))
                    message.UserName = message.UserName.Replace("@", "");

                // ساخت کلید کش بر اساس پارامترهای ورودی
                var cacheKey = $"GetRoomsPublic_{message.Query ?? "null"}_{message.UserName ?? "null"}";

                // تلاش برای گرفتن از کش
                if (!_cache.TryGetValue(cacheKey, out List<DTO.Models.MiniRoomJSON> result))
                {
                    // تنظیمات کش
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromDays(1)) // 5 دقیقه کش با اسلایدینگ
                        .SetAbsoluteExpiration(TimeSpan.FromDays(2)) // حداکثر 30 دقیقه
                        .SetPriority(CacheItemPriority.High) // اولویت بالا
                        .AddExpirationToken(new CancellationChangeToken(new CancellationTokenSource(TimeSpan.FromMinutes(30)).Token));

                    // گرفتن از دیتابیس
                    var rooms = await _db.RoomTBLs
                        .Where(e => (string.IsNullOrEmpty(message.Query) || e.Title.Contains(message.Query)) &&
                            e.IsPublicRoom &&
                            ((message.UserName != null && message.UserName == e.OwnerTBL.Username) || (string.IsNullOrEmpty(message.UserName)))
                        )
                        .AsSplitQuery()
                        .Select(e => new
                        {
                            e.FriendlyID,
                            e.Avatar,
                            e.Title,
                            e.DateInsert,
                            plan = e.RoomTBLPlanTBLs.OrderBy(p => p.StartDate).ToList(),
                            e.OwnerTBL,
                            Meetings = e.MeetingTBLs.Select(m => new
                            {
                                m.ID,
                                m.DateInsert,
                                m.Title,
                                //Participants = m.UserTBLToMeetingTBLs.Select(um => new
                                //{
                                //    um.UserNickName,
                                //    um.UserTBL.UUID,
                                //    um.UserTBL.Email,
                                //    um.UserTBL.NationalCode,
                                //    um.UserTBL.PhoneNumber,
                                //    um.UserTBL.Type
                                //})
                            })
                        })
                        .ToListAsync();

                    result = rooms.Select(room => new DTO.Models.MiniRoomJSON
                    {
                        RoomID = room.FriendlyID,
                        Title = room.Title,
                        Created = room.DateInsert,
                        Avatar = room.Avatar,
                        LastMeet = room.Meetings
                            .OrderByDescending(m => m.DateInsert)
                            .FirstOrDefault()?.DateInsert ?? room.DateInsert,
                        //Count = room.Meetings.Count(m => m.Participants
                        //    .Count(p => p.Type != MainEntity_Types.USER_TEACHER) > 1),
                        NextMeet = room.plan.Any(e => e.StartDate > DateTime.Now)
                            ? room.plan.Where(e => e.StartDate > DateTime.Now).First().StartDate
                            : DateTime.MaxValue,
                        Owner = new MiniUserJSON
                        {
                            UID = room.OwnerTBL.UUID,
                            Fullname = room.OwnerTBL.Firstname,
                            Username = room.OwnerTBL.Username
                        },
                        //CountTeacher = room.Meetings
                        //    .SelectMany(m => m.Participants)
                        //    .Where(p => p.Type == MainEntity_Types.USER_TEACHER)
                        //    .Select(p => p.UUID ?? p.Email ?? p.NationalCode ?? p.PhoneNumber ?? p.UserNickName)
                        //    .Where(k => !string.IsNullOrWhiteSpace(k))
                        //    .Distinct()
                        //    .Count(),
                        LatestTitle = room.Meetings != null && room.Meetings.Count() > 0
                            ? room.Meetings.OrderByDescending(e => e.ID).First().Title
                            : "",
                        //CountPartis = room.Meetings
                        //    .SelectMany(m => m.Participants)
                        //    .Where(p => p.Type != MainEntity_Types.USER_TEACHER)
                        //    .SelectMany(p => new[]
                        //    {
                        //p.UUID?.Trim().ToLower(),
                        //p.UserNickName?.Trim().ToLower(),
                        //p.Email?.Trim().ToLower(),
                        //p.NationalCode?.Trim(),
                        //p.PhoneNumber?.Trim()
                        //    })
                        //    .Where(k => !string.IsNullOrWhiteSpace(k))
                        //    .Distinct()
                        //    .Count()
                    }).OrderBy(e => e.NextMeet).ThenBy(e => e.LastMeet).ToList();

                    // ذخیره در کش
                    _cache.Set(cacheKey, result, cacheEntryOptions);
                }
                else
                {
                    // برای دیباگ - لاگ اینکه از کش دریافت شده
                    Console.WriteLine($"Cache hit for key: {cacheKey}");
                }

                outputPort.Handle(new GenericResponse<GetRoomsPublicResponse>(new GetRoomsPublicResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = result
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Statics.Log("GetRoomsPublic", e);

                // در صورت خطا، پاسخ خطا برگردانید
                outputPort.Handle(new GenericResponse<GetRoomsPublicResponse>(new GetRoomsPublicResponse
                {
                    Message = "خطا در دریافت اطلاعات",
                    Status = 500,
                    Data = null
                }));
            }
        }
    }
    #endregion

    #region GetRoomByFriendlyID
    public class GetRoomByFriendlyIDUseCase : IGetRoomByFriendlyIDUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;

        public GetRoomByFriendlyIDUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetRoomByFriendlyIDRequest message,
            IOutPutPort<GenericResponse<GetRoomByFriendlyIDResponse>> outputPort)
        {
            long id = 0;
            try
            {
                if (message.GeneralHeaders?.UserTBL?.ID != null)
                {
                    id = message.GeneralHeaders.UserTBL.ID;
                }
            }
            catch (Exception e) { }

            outputPort.Handle(new GenericResponse<GetRoomByFriendlyIDResponse>(new GetRoomByFriendlyIDResponse
            {
                Message = "ok",
                Status = 200,
                Data = _db.RoomTBLs.Where(e => e.FriendlyID == message.Query).Include(e => e.OwnerTBL).Include(e => e.RoomTBLPlanTBLs.OrderBy(e => e.StartDate))
                .Select(e => new DTO.Models.MiniRoomJSON
                {
                    RoomID = e.FriendlyID,
                    Title = e.Title,
                    IsAdmin = e.IDOwnerTBL == id,
                    NextMeet = e.RoomTBLPlanTBLs.Any(e => e.StartDate > DateTime.Now) ? e.RoomTBLPlanTBLs.Where(e => e.StartDate > DateTime.Now).First().StartDate : DateTime.MaxValue,
                    LatestTitle = e.RoomTBLPlanTBLs.Any(e => e.StartDate > DateTime.Now) ? e.RoomTBLPlanTBLs.Where(e => e.StartDate > DateTime.Now).First().Title : "",
                    Avatar = e.Avatar,

                    Owner = new MiniUserJSON
                    {
                        Fullname = e.OwnerTBL.GetName(),
                        Username = e.OwnerTBL.Username,
                    },
                }).First()
            }));

        }
    }
    #endregion

    #region GetLives
    public class GetLivesUseCase : IGetLivesUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetLivesUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetLivesRequest message,
            IOutPutPort<GenericResponse<GetLivesResponse>> outputPort)
        {
            UserTBL user = new UserTBL
            {
                ID = 0
            };
            if (message.GeneralHeaders?.UserTBL?.ID != null)
            {
                user = message.GeneralHeaders.UserTBL;
            }


            var meets = await IBBBGetMeetings.Request(message, user);
            foreach (var m in meets)
            {
                var r = _db.RoomTBLs.First(e => e.BBB_MeetID == m.BBB_MeetID);
                m.RoomID = r.FriendlyID;
            }
            //var r = await IBBBGetMeetings.GetRecordingsByMeetingId("1khe4quugbx7ziilyojrvapwebknxbn1jydcjozl");

            //var resp = IScpUploader.ExecuteRemoteCommand("ls -lht");

            outputPort.Handle(new GenericResponse<GetLivesResponse>(new GetLivesResponse
            {
                Message = "ok",
                Status = 200,
                Data = meets.Select(e => new MiniRoomJSON
                {
                    RoomID = e.RoomID,
                    Title = e.Title,
                    Created = e.Created,
                    BBB_MeetID = e.BBB_MeetID,
                    CountPartis = e.CountPartis,

                }).ToList()
            }));

        }
    }
    #endregion

    #region GetLivesOfProfile
    public class GetLivesOfProfileUseCase : IGetLivesOfProfileUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetLivesOfProfileUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetLivesOfProfileRequest message,
            IOutPutPort<GenericResponse<GetLivesOfProfileResponse>> outputPort)
        {
            UserTBL user = new UserTBL
            {
                ID = 0
            };
            if (message.GeneralHeaders?.UserTBL?.ID != null)
            {
                user = message.GeneralHeaders.UserTBL;
            }
            if (!await _db.UserTBLs.AnyAsync(e => e.Username == message.Username))
            {
                outputPort.Handle(new GenericResponse<GetLivesOfProfileResponse>(new GetLivesOfProfileResponse
                {
                    Message = "نام کاربری یافت نشد",
                    Status = 404,
                    Data = null
                }));

                return;
            }
            var puser = await _db.UserTBLs.FirstOrDefaultAsync(e => e.Username == message.Username);

            var meets = await IBBBGetMeetings.Request(message, puser, user);
            foreach (var m in meets)
            {
                var r = _db.RoomTBLs.First(e => e.BBB_MeetID == m.BBB_MeetID);
                m.RoomID = r.FriendlyID;
            }
            //var r = await IBBBGetMeetings.GetRecordingsByMeetingId("1khe4quugbx7ziilyojrvapwebknxbn1jydcjozl");

            //var resp = IScpUploader.ExecuteRemoteCommand("ls -lht");

            outputPort.Handle(new GenericResponse<GetLivesOfProfileResponse>(new GetLivesOfProfileResponse
            {
                Message = "ok",
                Status = 200,
                Data = meets.Select(e => new MiniRoomJSON
                {
                    RoomID = e.RoomID,
                    Title = e.Title,
                    Created = e.Created,
                    BBB_MeetID = e.BBB_MeetID,
                    CountPartis = e.CountPartis,

                }).ToList()
            }));

        }
    }
    #endregion

    #region GetLivesPublic
    public class GetLivesPublicUseCase : IGetLivesPublicUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetLivesPublicUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetLivesPublicRequest message,
            IOutPutPort<GenericResponse<GetLivesPublicResponse>> outputPort)
        {

            var meets = await IBBBGetMeetings.Request(message);
            foreach (var m in meets)
            {
                var r = _db.RoomTBLs.First(e => e.BBB_MeetID == m.BBB_MeetID);
                m.RoomID = r.FriendlyID;
            }
            //var r = await IBBBGetMeetings.GetRecordingsByMeetingId("1khe4quugbx7ziilyojrvapwebknxbn1jydcjozl");

            //var resp = IScpUploader.ExecuteRemoteCommand("ls -lht");


            outputPort.Handle(new GenericResponse<GetLivesPublicResponse>(new GetLivesPublicResponse
            {
                Message = "ok",
                Status = 200,
                Data = meets.Select(e => new MiniRoomJSON
                {
                    RoomID = e.RoomID,
                    Title = e.Title,
                    Created = e.Created,
                    BBB_MeetID = e.BBB_MeetID,
                    CountPartis = e.CountPartis,
                }).ToList()
            }));

        }
    }
    #endregion

    #region GetLiveWithID
    public class GetLiveWithIDUseCase : IGetLiveWithIDUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetLiveWithIDUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetLiveWithIDRequest message,
            IOutPutPort<GenericResponse<GetLiveWithIDResponse>> outputPort)
        {
            var meets = await IBBBGetMeetings.Request(message);
            foreach (var m in meets)
            {
                var r = _db.RoomTBLs.First(e => e.BBB_MeetID == m.BBB_MeetID);
                m.RoomID = r.FriendlyID;
            }
            //var r = await IBBBGetMeetings.GetRecordingsByMeetingId("1khe4quugbx7ziilyojrvapwebknxbn1jydcjozl");

            //var resp = IScpUploader.ExecuteRemoteCommand("ls -lht");


            outputPort.Handle(new GenericResponse<GetLiveWithIDResponse>(new GetLiveWithIDResponse
            {
                Message = "ok",
                Status = 200,
                Data = meets.Select(e => new MiniRoomJSON
                {
                    RoomID = e.RoomID,
                    Title = e.Title,
                    Created = e.Created,
                    BBB_MeetID = e.BBB_MeetID,
                    CountPartis = e.CountPartis,
                }).ToList()
            }));

        }
    }
    #endregion

    #region GetRoomCalendarActivity
    public class GetRoomCalendarActivityUseCase : IGetRoomCalendarActivityUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetRoomCalendarActivityUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetRoomCalendarActivityRequest message,
            IOutPutPort<GenericResponse<GetRoomCalendarActivityResponse>> outputPort)
        {
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_meeting"))
            {
                outputPort.Handle(new GenericResponse<GetRoomCalendarActivityResponse>(new GetRoomCalendarActivityResponse
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                }));
                return;
            }
            if (!_db.RoomTBLs.Any(e => e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.FriendlyID == message.FriendlyID || (message.GeneralHeaders.UserTBL.Type == MainEntity_Types.USER_ADMINSTRATOR)))
            {
                outputPort.Handle(new GenericResponse<GetRoomCalendarActivityResponse>(new GetRoomCalendarActivityResponse
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                }));
                return;
            }
            //var room = _db.RoomTBLs.Where(e => e.FriendlyID == message.FriendlyID)
            //    .Include(e => e.MeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.CertificateTBL)
            //    .Include(e => e.MeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.MeetQuestionTBLs)
            //    .Include(e => e.MeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.MeetCommentTBLs)
            //    .Include(e => e.MeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
            //    .Include(e => e.MeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBLToMeetingTBLLogs)
            //    .Include(e => e.RoomTBLPlanTBLs)
            //    .Include(e => e.OwnerTBL)
            //    .First();



            outputPort.Handle(new GenericResponse<GetRoomCalendarActivityResponse>(new GetRoomCalendarActivityResponse
            {
                Message = "ok",
                Status = 200,
                Data = GetRoomCalendarActivities(message.FriendlyID)
            }));

        }

        public List<MiniCalendarActivity> GetRoomCalendarActivities(string friendlyId)
        {
            // 1. واکشی اتاق با تمام جلسات و ارتباطات لازم
            var room = _db.RoomTBLs
           .Where(e => e.FriendlyID == friendlyId)
           // شاخه اول: CertificateTBL
           .Include(e => e.MeetingTBLs.Where(m => m.UserTBLToMeetingTBLs.Count(r =>
               r.Type != MainEntity_Types.USER_TEACHER && r.Type != MainEntity_Types.USER_DABIR) > 1))
               .ThenInclude(m => m.UserTBLToMeetingTBLs)
                   .ThenInclude(u => u.CertificateTBL)
           // شاخه دوم: MeetQuestionTBLs
           .Include(e => e.MeetingTBLs.Where(m => m.UserTBLToMeetingTBLs.Count(r =>
               r.Type != MainEntity_Types.USER_TEACHER && r.Type != MainEntity_Types.USER_DABIR) > 1))
               .ThenInclude(m => m.UserTBLToMeetingTBLs)
                   .ThenInclude(u => u.MeetQuestionTBLs)
           // شاخه سوم: MeetCommentTBLs
           .Include(e => e.MeetingTBLs.Where(m => m.UserTBLToMeetingTBLs.Count(r =>
               r.Type != MainEntity_Types.USER_TEACHER && r.Type != MainEntity_Types.USER_DABIR) > 1))
               .ThenInclude(m => m.UserTBLToMeetingTBLs)
                   .ThenInclude(u => u.MeetCommentTBLs)
           // شاخه چهارم: UserTBL
           .Include(e => e.MeetingTBLs.Where(m => m.UserTBLToMeetingTBLs.Count(r =>
               r.Type != MainEntity_Types.USER_TEACHER && r.Type != MainEntity_Types.USER_DABIR) > 1))
               .ThenInclude(m => m.UserTBLToMeetingTBLs)
                   .ThenInclude(u => u.UserTBL)
           // شاخه پنجم: UserTBLToMeetingTBLLogs
           .Include(e => e.MeetingTBLs.Where(m => m.UserTBLToMeetingTBLs.Count(r =>
               r.Type != MainEntity_Types.USER_TEACHER && r.Type != MainEntity_Types.USER_DABIR) > 1))
               .ThenInclude(m => m.UserTBLToMeetingTBLs)
                   .ThenInclude(u => u.UserTBLToMeetingTBLLogs)
           .Include(e => e.RoomTBLPlanTBLs)
           .Include(e => e.OwnerTBL)
           .First();

            if (room == null) return new List<MiniCalendarActivity>();

            // 2. ساخت MiniRoomJSON (یک بار برای همه جلسات)
            var miniRoom = MapToMiniRoomJSON(room);

            // 3. تبدیل تمام جلسات به MeetJSON
            var allMeets = room.MeetingTBLs
                .Select(meeting => MapToMeetJSON(meeting, miniRoom))
                .ToList();

            // 4. گروه‌بندی بر اساس سال و ماه شمسی (از JalaliDate)
            var calendarGroups = allMeets
      .GroupBy(meet =>
      {
          var parts = meet.JalaliDate.Split('-');
          if (parts.Length == 3 && int.TryParse(parts[0], out int year))
          {
              // تبدیل نام ماه شمسی به عدد
              int month = ConvertPersianMonthNameToNumber(parts[1]);
              if (month > 0)
                  return new { Year = year, Month = month - 1 }; // month 0-based
          }
          return new { Year = 0, Month = -1 };
      })
      .Where(g => g.Key.Year > 0 && g.Key.Month >= 0)
      .Select(g => new MiniCalendarActivity
      {
          Year = g.Key.Year,
          Month = g.Key.Month,
          Activities = g.GroupBy(m => ExtractDayFromJalaliDate(m.JalaliDate))
                        .Select(dayGroup => new ActivityItem
                        {
                            Day = dayGroup.Key,
                            Count = dayGroup.Count(),
                            MeetJSONs = dayGroup.ToList()
                        })
                        .OrderBy(a => a.Day)
                        .ToList()
      })
      .OrderBy(c => c.Year)
      .ThenBy(c => c.Month)
      .ToList();
            return calendarGroups;
        }

        // توابع کمکی (Mapping)
        private MiniRoomJSON MapToMiniRoomJSON(RoomTBL room)
        {
            return new MiniRoomJSON
            {
                RoomID = room.FriendlyID,
                Title = room.Title,
                IsAdmin = false,
                BBB_MeetID = room.BBB_MeetID,
                Avatar = room.Avatar,
                Created = room.DateInsert,
                Count = room.MeetingTBLs?.Count ?? 0,
            };
        }
        private int ConvertPersianMonthNameToNumber(string monthName)
        {
            var persianMonths = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        {"فروردین", 1},
        {"اردیبهشت", 2},
        {"خرداد", 3},
        {"تیر", 4},
        {"مرداد", 5},
        {"شهریور", 6},
        {"مهر", 7},
        {"آبان", 8},
        {"آذر", 9},
        {"دی", 10},
        {"بهمن", 11},
        {"اسفند", 12}
    };

            return persianMonths.ContainsKey(monthName) ? persianMonths[monthName] : 0;
        }

        private int ExtractDayFromJalaliDate(string jalaliDate)
        {
            if (string.IsNullOrEmpty(jalaliDate)) return 0;
            var parts = jalaliDate.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int day))
                return day;
            return 0;
        }

        private MeetJSON MapToMeetJSON(MeetingTBL meeting, MiniRoomJSON miniRoom)
        {
            return new MeetJSON
            {
                MeetID = meeting.MeetID,
                JWT = new JWTTokenJSON { Token = meeting.JWT },
                Title = meeting.Title,
                JalaliDate = meeting.JalaliDate,
                StartedTime = meeting.DateInsert.ToString("HH:mm"),
                Status = meeting.Status == MainEntity_Status.MEETING_LIVE ? "live" : "end",
                CountPartis = meeting.UserTBLToMeetingTBLs.Count,
                JoinUrl = $"/meeting/join/{meeting.MeetID}", // آدرس نسبی یا کامل
                Room = miniRoom,
                Teacher = meeting.UserTBLToMeetingTBLs.Any(e => e.Type == MainEntity_Types.USER_TEACHER) ?
                    MapToMiniUserJSON(meeting.UserTBLToMeetingTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).ToList()) : new(),
                Creator = MapToMiniUserJSON(meeting.Creator)
            };
        }

        private List<MiniUserJSON> MapToMiniUserJSON(ICollection<UserTBLToMeetingTBL> users)
        {
            if (users == null || !users.Any()) return null;
            var min = new List<MiniUserJSON>();
            return users.Select(e => MapToMiniUserJSON(e)).ToList();
        }
        private MiniUserJSON MapToMiniUserJSON(UserTBL user)
        {
            if (user == null) return null;
            return new MiniUserJSON
            {
                ID = user.ID,
                UID = user.UUID,
                Fullname = user.GetName(),
                Username = user.Username,
                Email = user.Email,
                Role = user.GetRole(),
            };
        }
        private MiniUserJSON MapToMiniUserJSON(UserTBLToMeetingTBL u)
        {
            if (u == null || u.UserTBL == null) return null;
            var user = u.UserTBL;
            return new MiniUserJSON
            {
                ID = user.ID,
                UID = user.UUID,
                Fullname = u.UserNickName,
                Username = user.Username,
                Email = user.Email,
                Role = user.GetRole(),
            };
        }



    }
    #endregion

    #region GetMeetingURL
    public class GetMeetingURLUseCase : IGetMeetingURLUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        public GetMeetingURLUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
        }

        public async Task handleAsync(GetMeetingURLRequest message,
            IOutPutPort<GenericResponse<GetMeetingURLResponse>> outputPort)
        {
            bool modir = false;
            if (message.AsModerator)
            {
                if (message.GeneralHeaders?.UserTBL?.ID != null)
                {
                    if (_db.RoomTBLs.Any(e => e.FriendlyID == message.MeetiID && e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID))
                    {
                        modir = true;
                    }

                }
            }

            var listmeet = await IBBBGetMeetings.RequestAll(new GetLivesRequest
            {
                GeneralHeaders = message.GeneralHeaders,
                AllowedGUID = message.AllowedGUID,
                Email = message.Email,
                ExtId = message.ExtId,
                UUID = message.UUID
            });
            listmeet.ForEach(e => e.RoomID = _db.MeetingTBLs.Any(r => r.MeetID.EndsWith(e.RoomID)) ? _db.MeetingTBLs.FirstOrDefault(r => r.MeetID.EndsWith(e.RoomID)).RoomFriendlyID : e.RoomID);
            var r = _db.RoomTBLs.Where(e => e.FriendlyID == message.MeetiID).First();
            var meet = _db.MeetingTBLs.Where(e => e.RoomFriendlyID == message.MeetiID && Statics.MeetingRunning.Contains(e.Status)).OrderByDescending(e => e.ID)
                .Include(e => e.RoomTBL).First();

            string meetid = r.BBB_MeetID;

            var meets = await IBBBGetMeetings.GetJoinUrlAsync(meet, message.FullName, modir, await _commonMethods.ReturnPassword(message.MeetiID, modir));

            outputPort.Handle(new GenericResponse<GetMeetingURLResponse>(new GetMeetingURLResponse
            {
                Message = "ok",
                Status = 200,
                Link = meets
            }));

        }
    }
    #endregion

    #region GuarantCertificate
    public class GuarantCertificateUseCase : IGuarantCertificateUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        public IWordService WordService { get; }
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader scpUploader { get; }
        public GuarantCertificateUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IWordService WordService,
            IScpUploader scpUploader,
            IWebHostEnvironment hosting
            )
        {
            this.hosting = hosting;
            _db = repository;
            this.scpUploader = scpUploader;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.WordService = WordService;
        }

        public async Task handleAsync(GuarantCertificateRequest message,
            IOutPutPort<GenericResponse<GuarantCertificateResponse>> outputPort)
        {
            try
            {
                var init = await _commonMethods.Initialize<GuarantCertificateRequest>(message);
                var user = init.Item1;
                var meet = init.Item2;
                Statics.Log("GuarantCertificateUseCase", "a");
                Statics.Log("GuarantCertificateUseCase", meet == null ? "null" : meet.MeetID);
                Statics.Log("GuarantCertificateUseCase", user == null ? "null" : user.UUID);

                var check = await _commonMethods.CheckGuaranteedUser(user, meet);
                if (!check.Item1)
                {
                    Statics.Log("GuarantCertificateUseCase", check.Item2);

                    outputPort.Handle(new GenericResponse<GuarantCertificateResponse>(new GuarantCertificateResponse
                    {
                        Message = check.Item2,
                        Status = 403,
                    }));
                    return;
                }
                var um = _db.UserTBLToMeetingTBLs.Where(e => e.MeetID == meet.MeetID && e.UUID == user.UUID).First();
                Statics.Log("GuarantCertificateUseCase", um.ID.ToString());

                CertificateTBL cert = null;
                if (_db.CertificateTBLs.Any(e => e.IDUserTBLToMeetingTBL == um.ID))
                {
                    cert = await _db.CertificateTBLs.Where(e => e.IDUserTBLToMeetingTBL == um.ID).FirstAsync();
                }
                else
                {

                    cert = new CertificateTBL
                    {
                        Status = MainEntity_Status.CERT_FULL,
                        Type = MainEntity_Types.CERT_NORMAL,
                        Title = "گواهینامه حضور در جلسه",
                        IDUserTBLToMeetingTBL = um.ID,

                        Validation = check.Item1
                    };
                    Statics.Log("GuarantCertificateUseCase", $"cert is {cert.IDUserTBLToMeetingTBL}");

                    _db.CertificateTBLs.Add(cert);
                    _db.SaveChanges();
                    Statics.Log("GuarantCertificateUseCase", $"cert added {cert.IDUserTBLToMeetingTBL}");

                }

                string role = "شرکت‌کننده";
                if (um.Role == "MODERATOR")
                {
                    role = "مدرس";
                }




                //var wwwroot = hosting.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                //var fontPath = "IranNastaliq";
                //var inputFile = Path.Combine(wwwroot, "pdf", "sample.docx");
                //var bgpath = Path.Combine(wwwroot, "bg.png");
                //var outputPath = Path.Combine(wwwroot, "pdf", "certificates", $@"certif-{um.ID}.pdf");

                //var resultPath = WordService.CreateCertificatePdf(outputPath, fontPath, bgpath, um.UserNickName, meet.RoomTBL.Title, meet.Title,
                //        Statics.ConvertPersianMonthToNumber(meet.JalaliDate).Replace("-", "/"), role, um.ID.ToString());
                //cert.FileURL = "https://vc.hekmateislami.com/";
                //cert.FilePath = Path.Combine("pdf", "certificates", $"certif-{um.ID}.pdf");
                //scpUploader.UploadFile(outputPath);

                outputPort.Handle(new GenericResponse<GuarantCertificateResponse>(new GuarantCertificateResponse
                {
                    Message = $"ok{message.GeneralHeaders.JWTMeetingToken}\n {message.GeneralHeaders.JWTMeetingUserToken} \n {meet.MeetID} \n {user.UUID}",
                    Status = 200,
                    Number = um.ID
                }));
            }
            catch (Exception e)
            {

                Statics.Log("GuarantCertificateUseCase", e.Message);
                outputPort.Handle(new GenericResponse<GuarantCertificateResponse>(new GuarantCertificateResponse
                {
                    Message = e.Message + e.InnerException,
                    Status = 400,
                }));
                return;
            }
        }
    }
    #endregion

    #region GenerateCertificate
    public class GenerateCertificateUseCase : IGenerateCertificateUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        public IWordService WordService { get; }
        private IWebHostEnvironment _env;
        private IScpUploader scpUploader { get; }

        public GenerateCertificateUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader

            )
        {
            _db = repository;
            _env = hosting;
            _commonMethods = new CommonMethods(configuration, repository, _env);
            WordService = wordService;
            this.scpUploader = scpUploader;
        }

        public async Task handleAsync(GenerateCertificateRequest message,
            IOutPutPort<GenericResponse<GenerateCertificateResponse>> outputPort)
        {
            try
            {


                var init = await _commonMethods.Initialize<GenerateCertificateRequest>(message);
                var user = init.Item1;
                var meet = init.Item2;
                var checkCert = await _commonMethods.CheckGuaranteedUser(user, meet);
                if (!checkCert.Item1)
                {
                    outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                    {
                        Message = checkCert.Item2,
                        Status = 200,
                    }));
                    return;
                }
                Statics.Log("GenerateCertificateUseCase", "checkCert");
                //if (string.IsNullOrEmpty(user.NationalCode))
                //{
                //    user.NationalCode = message.NationalCode;
                //    _db.Update(user); _db.SaveChanges();
                //}
                //if (user.NationalCode != message.NationalCode)
                //{
                //    outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                //    {
                //        Message = "کد ملی وارد شده متعلق به حساب شما نیست در صورتی که میخواهید کد ملی سامانه را تغییر بدهید با پشتیبانی تماس حاصل فرمایید!",
                //        Status = 200,
                //    }));
                //    return;
                //}
                var um = _db.UserTBLToMeetingTBLs.Where(e => e.MeetID == meet.MeetID && e.UUID == user.UUID).First();
                string role = "شرکت‌کننده";
                if (um.Role == "MODERATOR")
                {
                    role = "ارائه دهنده";
                }
                if (!_db.CertificateTBLs.Any(e => e.IDUserTBLToMeetingTBL == um.ID))
                {
                    _db.CertificateTBLs.Add(new CertificateTBL
                    {
                        IDUserTBLToMeetingTBL = um.ID,
                        Status = MainEntity_Status.CERT_FULL,
                        Type = MainEntity_Types.CERT_NORMAL,
                        Title = "گواهینامه حضور در جلسه",

                        Validation = checkCert.Item1
                    });
                    _db.SaveChanges();
                    //Statics.Log("GenerateCertificateUseCase", $"{um.ID}");
                    //outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                    //{
                    //    Message = "هنوز گواهینامه برای شما صادر نشده است!",
                    //    Status = 404,
                    //}));
                    //return;
                }
                var cert = await _db.CertificateTBLs.FirstOrDefaultAsync(e => e.IDUserTBLToMeetingTBL == um.ID);
                cert.NationalCode = message.NationalCode;
                cert.IsMale = message.Sex;
                if (!cert.Validation)
                {
                    outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                    {
                        Message = "Not validate",
                        Status = 403,
                    }));
                    return;
                }
                var wwwroot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var fontPath = "IranNastaliq";
                var bgpath = Path.Combine(wwwroot, "bg.png");
                var outputPath = Path.Combine(wwwroot, "pdf", "certificates", $@"certif-{um.ID}.pdf");



                var resultPath = WordService.CreateCertificatePdf(outputPath, fontPath, bgpath, um.UserNickName, message.Sex, message.NationalCode,
                    meet.RoomTBL.Title, meet.Title, Statics.ConvertPersianMonthToNumber(meet.JalaliDate).Replace("-", "/"),
                    role, um.ID.ToString());

                cert.FileURL = "https://vc.hekmateislami.com/";

                //string compressedPath = PdfCompressor.CompressPdf(@$"{outputPath}");

                cert.FilePath = Path.Combine("cert", $@"certif-{um.ID}.pdf");
                _db.CertificateTBLs.Update(cert);
                _db.SaveChanges();
                Statics.Log("GenerateCertificateUseCase", "Ta upload");

                scpUploader.UploadFile(outputPath);
                Statics.Log("GenerateCertificateUseCase", "oops");

                outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                {
                    Message = "ok",
                    Status = 200,
                    Link = Path.Join(cert.FileURL, cert.FilePath)

                }));
            }
            catch (Exception e)
            {
                Statics.Log("GenerateCertificateUseCase", "exception " + e.Message + e.InnerException);
                outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                {
                    Message = e.Message + e.InnerException,
                    Status = 400,
                }));
            }
        }



    }
    #endregion

    #region GetTrophies
    public class GetTrophiesUseCase : IGetTrophiesUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        public IWordService WordService { get; }
        private IWebHostEnvironment _env;
        private IScpUploader scpUploader { get; }

        public GetTrophiesUseCase(IDBContext repository,
            IConfiguration configuration, IWordService wordService,
            IWebHostEnvironment hosting,
            IScpUploader scpUploader

            )
        {
            _db = repository;
            _env = hosting;
            _commonMethods = new CommonMethods(configuration, repository, _env);
            WordService = wordService;
            this.scpUploader = scpUploader;
        }

        public async Task handleAsync(GetTrophiesRequest message,
            IOutPutPort<GenericResponse<GetTrophiesResponse>> outputPort)
        {
            try
            {


                DateTime today = DateTime.Today;
                int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
                DateTime thisWeekSaturday = today.AddDays(-daysUntilSaturday); // شنبه همین هفته


                outputPort.Handle(new GenericResponse<GetTrophiesResponse>(new GetTrophiesResponse
                {
                    Message = "ok",
                    Status = 200,
                    CountAllMeetings = _db.MeetingTBLs.Count(),
                    CountAllPartis = _db.UserTBLToMeetingTBLs.Count(),
                    CountWeekActiveTeacher = _db.MeetingTBLs.Where(e => e.DateInsert > thisWeekSaturday).GroupBy(e => e.IDTeacher).Count(),
                    CountWeekMeeting = _db.MeetingTBLs.Count(e => e.DateInsert > thisWeekSaturday),
                    SharedOBJ_ServerDetails = new SharedOBJ_ServerDetails
                    {
                        DatabaseRecords = 15,
                        DatabaseUsage = 500,
                        DiskUsage = 18000,
                        MemoryUsage = 12000
                    },
                    SharedOBJ_ViewDetails = new SharedOBJ_ViewDetails
                    {
                        All = 18000,
                        Unique = 1400,
                        Chrome = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("Chrome")),
                        FireFox = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("Firefox")),
                        IE = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("IE")),
                        Opera = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("Opera")),
                        OtherBrowser = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("a")),
                        Safari = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("safari")),
                        Today = _db.UserTBLToMeetingTBLs.Count(e => e.DateInsert > DateTime.Today),
                        Joint = 0,
                        AverageTimeSpent = TimeSpan.MinValue.TotalMilliseconds,
                        Desktop = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.Contains("Windows")),
                        Mobile = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.ToLower().Contains("android")),
                        Tablet = _db.UserTBLToMeetingTBLLogs.Count(e => e.OS.ToLower().Contains("ios")),
                    }
                }));
            }
            catch (Exception e)
            {
                outputPort.Handle(new GenericResponse<GetTrophiesResponse>(new GetTrophiesResponse
                {
                    Message = e.Message + e.InnerException,
                    Status = 400,

                }));
            }
        }



    }
    #endregion

    #region EndTheMeeting From BBB
    public class EndTheMeetingUseCase : IEndTheMeetingUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb { get; }
        public EndTheMeetingUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(EndTheMeetingRequest message,
            IOutPutPort<GenericResponse<EndTheMeetingResponse>> outputPort)
        {
            var init = await _commonMethods.Initialize<EndTheMeetingRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;
            if (meet == null || user == null)
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingResponse>(new EndTheMeetingResponse
                {
                    Message = "forbidden",
                    Status = 403,
                }));
                return;
            }
            if (!_db.UserTBLToMeetingTBLs.Any(e => e.UUID == user.UUID && e.MeetID == meet.MeetID) &&
               !user.VerifyUserPermissions("manage_admins"))
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingResponse>(new EndTheMeetingResponse
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                }));
                return;
            }
            meet.Status = MainEntity_Status.MEETING_END;
            meet.EndDate = DateTime.Now;

            bool ended = await bbb.EndMeetingAsync(meet.MeetID.Split("-")[0], "mp");
            if (ended)
            {
                _db.Update(meet);
                _db.SaveChanges();
            }
            outputPort.Handle(new GenericResponse<EndTheMeetingResponse>(new EndTheMeetingResponse
            {
                Message = "ended",
                Status = 200,
            }));

        }
    }
    #endregion

    #region EndTheMeetingWithAPI
    public class EndTheMeetingWithAPIUseCase : IEndTheMeetingWithAPIUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb { get; }
        public EndTheMeetingWithAPIUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(EndTheMeetingWithAPIRequest message,
            IOutPutPort<GenericResponse<EndTheMeetingWithAPIResponse>> outputPort)
        {
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_meeting"))
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingWithAPIResponse>(new EndTheMeetingWithAPIResponse
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                }));
                return;
            }
            if (!_db.RoomTBLs.Any(e => e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.FriendlyID == message.FriendlyID))
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingWithAPIResponse>(new EndTheMeetingWithAPIResponse
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                }));
                return;
            }
            var meet = await _db.MeetingTBLs.FirstOrDefaultAsync(e => e.RoomFriendlyID == message.FriendlyID && !e.EndDate.HasValue && e.IDCreator==message.GeneralHeaders.UserTBL.ID); 

            if (meet == null )
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingWithAPIResponse>(new EndTheMeetingWithAPIResponse
                {
                    Message = "forbidden",
                    Status = 403,
                }));
                return;
            }
            meet.Status = MainEntity_Status.MEETING_END;
            meet.EndDate = DateTime.Now;

            bool ended = await bbb.EndMeetingAsync(meet.MeetID.Split("-")[0], "mp");
            if (ended)
            {
                _db.Update(meet);
                _db.SaveChanges();
            }
            outputPort.Handle(new GenericResponse<EndTheMeetingWithAPIResponse>(new EndTheMeetingWithAPIResponse
            {
                Message = "ended",
                Status = 200,
            }));

        }
    }
    #endregion

    #region NewQuestion
    public class NewQuestionUseCase : INewQuestionUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb { get; }
        public NewQuestionUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(NewQuestionRequest message,
            IOutPutPort<GenericResponse<NewQuestionResponse>> outputPort)
        {
            try
            {
                var init = await _commonMethods.Initialize<NewQuestionRequest>(message);
                var user = init.Item1;
                var meet = init.Item2;
                if (meet == null || user == null)
                {
                    outputPort.Handle(new GenericResponse<NewQuestionResponse>(new NewQuestionResponse
                    {
                        Message = "forbidden",
                        Status = 403,
                    }));
                    return;
                }
                var usermeet = _db.UserTBLToMeetingTBLs.Where(e => e.UUID == user.UUID && e.MeetID == meet.MeetID).First();
                _db.MeetQuestionTBLs.Add(new MeetQuestionTBL
                {
                    Question = message.Content,
                    Answer = "",
                    AnswerAgent = "",
                    IDUserTBLToMeeting = usermeet.ID,
                    MeetID = meet.MeetID,
                });
                _db.SaveChanges();

                outputPort.Handle(new GenericResponse<NewQuestionResponse>(new NewQuestionResponse
                {
                    Message = "ended",
                    Status = 200,
                }));

            }
            catch (Exception e)
            {
                Statics.Log("NewQuestionUseCase", e.Message);
            }

        }
    }
    #endregion

    #region GetQuestionOfMeet
    public class GetQuestionOfMeetUseCase : IGetQuestionOfMeetUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb { get; }
        public GetQuestionOfMeetUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(GetQuestionOfMeetRequest message,
            IOutPutPort<GenericResponse<GetQuestionOfMeetResponse>> outputPort)
        {
            var init = await _commonMethods.Initialize<GetQuestionOfMeetRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;
            if (meet == null || user == null)
            {
                outputPort.Handle(new GenericResponse<GetQuestionOfMeetResponse>(new GetQuestionOfMeetResponse
                {
                    Message = "forbidden",
                    Status = 403,
                }));
                return;
            }

            outputPort.Handle(new GenericResponse<GetQuestionOfMeetResponse>(new GetQuestionOfMeetResponse
            {
                Message = "ended",
                Status = 200,
                Questions = _db.MeetQuestionTBLs.Where(e => e.UserTBLToMeeting.UUID == user.UUID && e.UserTBLToMeeting.MeetID == meet.MeetID)
                .Include(e => e.UserTBLToMeeting).Select(e => new SharedOBJ_Question
                {
                    ID = e.ID,
                    NickName = e.UserTBLToMeeting.UserNickName,
                    Answered = e.Status == MainEntity_Status.QUESTION_ANSWERED,
                    Question = e.Question
                }).ToList(),
            }));

        }
    }
    #endregion

    #region QuestionAnswered
    public class QuestionAnsweredUseCase : IQuestionAnsweredUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb { get; }
        public QuestionAnsweredUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
        }

        public async Task handleAsync(QuestionAnsweredRequest message,
            IOutPutPort<GenericResponse<QuestionAnsweredResponse>> outputPort)
        {
            var init = await _commonMethods.Initialize<QuestionAnsweredRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;
            if (meet == null || user == null)
            {
                outputPort.Handle(new GenericResponse<QuestionAnsweredResponse>(new QuestionAnsweredResponse
                {
                    Message = "forbidden",
                    Status = 403,
                }));
                return;
            }
            if (!UserTBL.TYPE_ALLOWED_QUESTION_ACTION.Contains(user.Type))
            {
                outputPort.Handle(new GenericResponse<QuestionAnsweredResponse>(new QuestionAnsweredResponse
                {
                    Message = "ok",
                    Status = 202,
                }));
                return;
            }
            if (_db.MeetQuestionTBLs.Any(e => e.ID == message.ID))
            {
                var q = await _db.MeetQuestionTBLs.FindAsync(message.ID);
                q.Status = MainEntity_Status.QUESTION_ANSWERED;
                _db.Update(q);
                _db.SaveChanges();
            }
            outputPort.Handle(new GenericResponse<QuestionAnsweredResponse>(new QuestionAnsweredResponse
            {
                Message = "ended",
                Status = 200,
            }));

        }
    }
    #endregion

    #region GetSlideShow
    public class GetSlideShowUseCase : IGetSlideShowUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetSlideShowUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetSlideShowRequest message,
            IOutPutPort<GenericResponse<GetSlideShowResponse>> outputPort)
        {

            if (!_db.SlideshowTBL.Any(e => e.Status == MainEntity_Status.Enabled))
            {
                outputPort.Handle(new GenericResponse<GetSlideShowResponse>(new GetSlideShowResponse
                {
                    Message = "empty",
                    Status = 404,
                    Data = new List<MiniSlideShowJSON>() { new MiniSlideShowJSON
                    {
                        Title="اقتصاد مقاومتی",
                        Href="https://hekmateislami.com/21348",
                        Type="image",

                        Link = "https://hekmateislami.com/wp-content/uploads/2026/03/%D8%B4%D8%B9%D8%A7%D8%B1-%D8%B3%D8%A7%D9%84-1405.webp",
                        ID=-1
                    } }
                }));
                return;
            }
            List<MiniSlideShowJSON> li = [];
            if (message.Page == "admin" && message.GeneralHeaders?.UserTBL?.VerifyUserPermissions("manage_users") == true)
            {
                li = _db.SlideshowTBL.OrderByDescending(e => e.ID).Select(e => new MiniSlideShowJSON
                {
                    Href = e.Href,
                    ID = e.ID,
                    Link = Statics.CombinePath(Statics.URLFiles, e.Path),
                    Type = e.Explain,
                    Position = e.Status == MainEntity_Status.Enabled ? e.Position : 0,
                    Title = e.Name
                }).ToList();
            }
            else
            {
                li = _db.SlideshowTBL.Where(e => e.Status == MainEntity_Status.Enabled).OrderBy(e => e.Position).Select(e => new MiniSlideShowJSON
                {
                    Href = e.Href,
                    ID = e.ID,
                    Link = Statics.CombinePath(Statics.URLFiles, e.Path),
                    Position = e.Status == MainEntity_Status.Enabled ? e.Position : 0,
                    Type = e.Explain,
                    Title = e.Name
                }).ToList();
            }


            outputPort.Handle(new GenericResponse<GetSlideShowResponse>(new GetSlideShowResponse
            {
                Message = "ok",
                Status = 200,
                Data = li
            }));

        }
    }
    #endregion

    #region EditSlideShow
    public class EditSlideShowUseCase : IEditSlideShowUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public EditSlideShowUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(EditSlideShowRequest message,
            IOutPutPort<GenericResponse<EditSlideShowResponse>> outputPort)
        {
            List<MiniSlideShowJSON> li = [];
            if (message.GeneralHeaders?.UserTBL?.VerifyUserPermissions("manage_users") == true)
            {
                if (!_db.SlideshowTBL.Any(e => e.ID == message.IDSlideshow))
                {
                    outputPort.Handle(new GenericResponse<EditSlideShowResponse>(new EditSlideShowResponse
                    {
                        Message = "empty",
                        Status = 404,
                        Data = new List<MiniSlideShowJSON>() { new MiniSlideShowJSON
                    {
                        Title="اقتصاد مقاومتی",
                        Href="https://hekmateislami.com/21348",
                        Type="image",
                        Link = "https://hekmateislami.com/wp-content/uploads/2026/03/%D8%B4%D8%B9%D8%A7%D8%B1-%D8%B3%D8%A7%D9%84-1405.webp",
                        ID=-1
                    } }
                    }));
                    return;
                }
                var l = _db.SlideshowTBL.Find(message.IDSlideshow);
                if (message.Position <= 0)
                    l.Status = MainEntity_Status.Disable;
                else
                {
                    l.Status = MainEntity_Status.Enabled;
                    l.Position = message.Position;
                    if (!string.IsNullOrEmpty(message.Href))
                    {
                        l.Href = message.Href;
                    }
                }
                _db.Update(l);
                _db.SaveChanges();

                outputPort.Handle(new GenericResponse<EditSlideShowResponse>(new EditSlideShowResponse
                {
                    Message = "ok",
                    Status = 200,
                }));
                return;
            }



            outputPort.Handle(new GenericResponse<EditSlideShowResponse>(new EditSlideShowResponse
            {
                Message = "not found",
                Status = 400,
            }));

        }
    }
    #endregion

    #region InsertSlideShow
    public class InsertSlideShowUseCase : IInsertSlideShowUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public InsertSlideShowUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(InsertSlideShowRequest message,
     IOutPutPort<GenericResponse<InsertSlideShowResponse>> outputPort)
        {
            try
            {
                // بررسی دسترسی ادمین
                if (message.GeneralHeaders?.UserTBL?.VerifyUserPermissions("manage_users") != true)
                {
                    outputPort.Handle(new GenericResponse<InsertSlideShowResponse>(new InsertSlideShowResponse
                    {
                        Message = "شما دسترسی لازم را ندارید",
                        Status = 403,
                    }));
                    return;
                }

                // بررسی وجود فایل
                if (message.File == null || message.File.Length == 0)
                {
                    outputPort.Handle(new GenericResponse<InsertSlideShowResponse>(new InsertSlideShowResponse
                    {
                        Message = "فایل اسلایدشو را انتخاب کنید",
                        Status = 400,
                    }));
                    return;
                }

                // بررسی پسوند فایل
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(message.File.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    outputPort.Handle(new GenericResponse<InsertSlideShowResponse>(new InsertSlideShowResponse
                    {
                        Message = "فرمت فایل مجاز نیست. فقط تصاویر (jpg, jpeg, png, gif, webp) قابل قبول هستند",
                        Status = 400,
                    }));
                    return;
                }

                // ایجاد نام یکتا برای فایل
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // مسیر ذخیره فایل
                var uploadPath = @"C:\hekmatFiles\img\slider";

                // ایجاد پوشه اگر وجود ندارد
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // ذخیره فایل
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await message.File.CopyToAsync(stream);
                }

                // ایجاد لینک فایل
                var fileLink = $"/img/slider/{uniqueFileName}";

                // ایجاد اسلایدشوی جدید
                var slideShow = new SlideshowTBL
                {
                    Name = message.Title ?? "",
                    Href = message.Href ?? "",
                    Path = fileLink,
                    Position = message.Position > 0 ? message.Position : 0,
                    Status = message.Position > 0 ? MainEntity_Status.Enabled : MainEntity_Status.Disable,
                    DateInsert = DateTime.Now,
                    Type = MainEntity_Types.Normal,
                    Explain = "image",
                };

                // ذخیره در دیتابیس
                await _db.SlideshowTBL.AddAsync(slideShow);
                await _db.SaveChangesAsync();

                // پاسخ موفقیت‌آمیز
                outputPort.Handle(new GenericResponse<InsertSlideShowResponse>(new InsertSlideShowResponse
                {
                    Message = "اسلایدشو با موفقیت اضافه شد",
                    Status = 200,
                }));
            }
            catch (Exception ex)
            {
                // خطای سیستمی
                outputPort.Handle(new GenericResponse<InsertSlideShowResponse>(new InsertSlideShowResponse
                {
                    Message = $"خطا در ذخیره اسلایدشو: {ex.Message}",
                    Status = 500,
                }));
            }
        }
    }
    #endregion

    #region Contactus
    public class ContactusUseCase : IContactusUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public ContactusUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(ContactusRequest message,
            IOutPutPort<GenericResponse<ContactusResponse>> outputPort)
        {
            if (_db.ContactUsTBL.Count(e => e.Explain == message.GeneralHeaders.RequestedIP && e.DateInsert > DateTime.Now.AddDays(-1)) > 5)
            {
                outputPort.Handle(new GenericResponse<ContactusResponse>(new ContactusResponse
                {
                    Message = "به دلیل تکرار بیش از حد، لطفا 24 ساعت بعد مجدد تلاش کنید یا با شماره زیر تماس بگیرید.",
                    Status = 400,
                }));
                return;
            }
            _db.ContactUsTBL.Add(new ContactUsTBL
            {
                Email = message.Email,
                Name = message.Name,
                Phone = message.Phone,
                Status = MainEntity_Status.CONTACTUS_SUBMITED,
                Text = message.Text,
                Type = MainEntity_Types.CONTACTUS_WEBSITE,
                Explain = message.GeneralHeaders.RequestedIP
            });
            _db.SaveChanges();
            outputPort.Handle(new GenericResponse<ContactusResponse>(new ContactusResponse
            {
                Message = "ok",
                Status = 200,
            }));

        }
    }
    #endregion

    #region GetProfileInfo
    public class GetProfileInfoUseCase : IGetProfileInfoUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetProfileInfoUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(GetProfileInfoRequest message,
            IOutPutPort<GenericResponse<GetProfileInfoResponse>> outputPort)
        {
            message.Username = message.Username.TrimStart('@');

            if (!_db.UserTBLs.Any(e => e.Username == message.Username))
            {
                outputPort.Handle(new GenericResponse<GetProfileInfoResponse>(new GetProfileInfoResponse
                {
                    Message = "Nouser",
                    Status = 404,

                }));
                return;
            }
            if (message.GeneralHeaders.UserTBL != null)
            {

                var li = _db.UserTBLs.Where(e => e.Username == message.Username)
                    .Include(e => e.Followers).Include(e => e.Followings).Select(e => new MiniProfileJSON
                    {
                        ID = e.ID,
                        Avatar = string.IsNullOrEmpty(e.AvatarPath) ? Statics.CombinePath(Statics.URLFiles, Statics.DEF_AVATAR_PATH) : Statics.CombinePath(Statics.URLFiles, e.AvatarPath),
                        Bio = e.GetRole(),
                        FollowersCount = e.Followers.Count,
                        IsFollowing = e.Followings.Any(r => r.IDFollower == message.GeneralHeaders.UserTBL.ID),
                        Name = e.Firstname

                    }).First();

                outputPort.Handle(new GenericResponse<GetProfileInfoResponse>(new GetProfileInfoResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = li
                }));
            }
            else
            {

                var li = _db.UserTBLs.Where(e => ((int)e.Status) >= 500)
                 .Include(e => e.Followers).Include(e => e.Followings).Select(e => new MiniProfileJSON
                 {
                     ID = e.ID,
                     Avatar = Statics.CombinePath(Statics.URLFiles, string.IsNullOrEmpty(e.AvatarPath) ? Statics.DEF_AVATAR_PATH : e.AvatarPath),
                     Bio = e.GetRole(),
                     FollowersCount = e.Followers.Count,
                     IsFollowing = false,
                     Name = string.IsNullOrEmpty(e.Firstname) ? "" : e.Firstname

                 }).First();

                outputPort.Handle(new GenericResponse<GetProfileInfoResponse>(new GetProfileInfoResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = li
                }));
            }

        }
    }
    #endregion

    #region FollowProfile
    public class FollowProfileUseCase : IFollowProfileUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public FollowProfileUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
        }


        public async Task handleAsync(FollowProfileRequest message,
            IOutPutPort<GenericResponse<FollowProfileResponse>> outputPort)
        {

            message.Username = message.Username.TrimStart('@');
            if (!_db.UserTBLs.Any(e => e.Username == message.Username))
            {
                outputPort.Handle(new GenericResponse<FollowProfileResponse>(new FollowProfileResponse
                {
                    Message = "Nouser",
                    Status = 404,

                }));
                return;
            }
            var user = _db.UserTBLs.Where(e => e.Username == message.Username).First();
            if (message.GeneralHeaders.UserTBL != null)
            {

                bool isFollowing = false;

                if (_db.UserTBLToUserTBLs.Any(e => e.IDFollower == message.GeneralHeaders.UserTBL.ID && e.Following.Username == message.Username))
                {
                    _db.Database.ExecuteSqlRaw(
                       "DELETE FROM Hekmat.UserTBLToUserTBL WHERE IDFollowing = {0} AND IDFollower = {1}",
                        user.ID, message.GeneralHeaders.UserTBL.ID);
                    isFollowing = false;
                }
                else
                {
                    _db.UserTBLToUserTBLs.Add(new UserTBLToUserTBL
                    {
                        IDFollower = message.GeneralHeaders.UserTBL.ID,
                        IDFollowing = user.ID,
                    });
                    _db.SaveChanges();
                    isFollowing = true;
                }


                outputPort.Handle(new GenericResponse<FollowProfileResponse>(new FollowProfileResponse
                {
                    Message = "ok",
                    Status = 200,
                    IsFollowing = isFollowing,
                    NewFollowersCount = _db.UserTBLToUserTBLs.Count(e => e.IDFollowing == user.ID)
                }));
            }
            else
            {


                outputPort.Handle(new GenericResponse<FollowProfileResponse>(new FollowProfileResponse
                {
                    Message = "Not allowed",
                    Status = 403,
                }));
            }

        }
    }
    #endregion

    #region GetHekmatShobe
    public class GetHekmatShobeUseCase : IGetHekmatShobeUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;

        public GetHekmatShobeUseCase(IDBContext repository,
            IConfiguration configuration,
            IMemoryCache memoryCache)
        {
            _db = repository;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetHekmatShobeRequest message,
            IOutPutPort<GenericResponse<GetHekmatShobeResponse>> outputPort)
        {
            try
            {


                //Statics.Log("GetHekmatShobeUseCase", JsonConvert.SerializeObject(message.GeneralHeaders));

                string cacheKey = "HekmatShobeList";

                // Try to get data from cache
                if (!_memoryCache.TryGetValue(cacheKey, out List<MiniShobeJSON> cachedData))
                {
                    // Data not in cache, retrieve from database
                    Statics.Log("GetHekmatShobeUseCase", "Cache miss - retrieving from database");

                    var users = await _db.UserTBLs
                      .Where(e => e.IsHekmatShobe)
                      .Select(e => new { e.ID, e.AvatarPath, e.Firstname, e.LastName })
                      .ToListAsync();
                    var userIds = users.Select(u => u.ID).ToList();
                    var rooms = await _db.RoomTBLs
                        .Where(r => userIds.Contains(r.IDOwnerTBL))
                        .Select(r => new
                        {
                            r.IDOwnerTBL,
                            r.Title,
                            r.FriendlyID,
                            r.Avatar,
                            MeetingCount = r.MeetingTBLs.Count(),
                            ParticipantCount = r.MeetingTBLs.Sum(m => m.UserTBLToMeetingTBLs.Count)
                        })
                        .ToListAsync();


                    cachedData = users.Select(u => new MiniShobeJSON
                    {
                        Avatar = Statics.CombinePath(Statics.URLFiles, u.AvatarPath),
                        Fullname = $"{u.Firstname} {u.LastName}",
                        ID = u.ID,
                        Room = rooms.Where(r => r.IDOwnerTBL == u.ID)
                        .Select(r => new MiniRoomJSON
                        {
                            Title = r.Title,
                            RoomID = r.FriendlyID,
                            Avatar = Statics.CombinePath(Statics.URLFiles, r.Avatar),
                            Count = r.MeetingCount, // یا محاسبه با شرط خاص
                            CountPartis = r.ParticipantCount
                        }).ToList()
                    }).ToList();


                    // Configure cache options
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(30))  // Reset expiration if accessed
                        .SetAbsoluteExpiration(TimeSpan.FromHours(2))     // Absolute expiration time
                        .SetPriority(CacheItemPriority.Normal)            // Cache priority
                        .SetSize(1);                                      // Size for memory pressure management

                    // Add data to cache
                    _memoryCache.Set(cacheKey, cachedData, cacheOptions);

                    Statics.Log("GetHekmatShobeUseCase", $"Data cached with {cachedData.Count} items");
                }
                else
                {
                    Statics.Log("GetHekmatShobeUseCase", $"Cache hit - returning {cachedData.Count} items from cache");
                }

                outputPort.Handle(new GenericResponse<GetHekmatShobeResponse>(new GetHekmatShobeResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = cachedData
                }));
            }
            catch (Exception e)
            {
                Statics.Log("GetHekmatShobeUseCase", e);
                outputPort.Handle(new GenericResponse<GetHekmatShobeResponse>(new GetHekmatShobeResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = []
                }));
            }
        }
    }
    #endregion

    #region NewPlanRoom
    public class NewPlanRoomUseCase : INewPlanRoomUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings bbb { get; }
        private readonly IMediator _mediator;

        public NewPlanRoomUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bbb,
            IMediator _mediator
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            this.bbb = bbb;
            this._mediator = _mediator;
        }

        public async Task handleAsync(NewPlanRoomRequest message,
            IOutPutPort<GenericResponse<NewPlanRoomResponse>> outputPort)
        {
            try
            {
                if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_meeting"))
                {
                    outputPort.Handle(new GenericResponse<NewPlanRoomResponse>(new NewPlanRoomResponse
                    {
                        Message = "دسترسی ندارید",
                        Status = 400
                    }));
                    return;
                }
                if (!_db.RoomTBLs.Any(e => e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.FriendlyID == message.FriendlyID))
                {
                    outputPort.Handle(new GenericResponse<NewPlanRoomResponse>(new NewPlanRoomResponse
                    {
                        Message = "دسترسی ندارید",
                        Status = 400
                    }));
                    return;
                }
                var room = _db.RoomTBLs.Where(e => e.FriendlyID == message.FriendlyID).First();

                _db.RoomTBLPlanTBLs.Add(new RoomTBLPlanTBL
                {
                    IDRoomTBL = room.ID,
                    StartDate = message.StartDate.AddHours(-17),
                    Status = MainEntity_Status.Enabled,
                    Type = MainEntity_Types.PLAN_NORMAL,
                    Teachers = message.Teacher,
                    Dabir = message.Dabir,
                    Title = message.Title,
                });
                _db.SaveChanges();
                await _mediator.Publish(new PlanUpdateEvent
                {
                    FriendlyID = room.FriendlyID,
                    Action = "create",

                });

                outputPort.Handle(new GenericResponse<NewPlanRoomResponse>(new NewPlanRoomResponse
                {
                    Message = "fixed",
                    Status = 200,
                }));

            }
            catch (Exception e)
            {
                Statics.Log("NewPlanRoomUseCase", e.Message);
            }

        }
    }
    #endregion

    #region GetPlanRoom
    public class GetPlanRoomUseCase : IGetPlanRoomUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;
        public GetPlanRoomUseCase(IDBContext repository,
            IConfiguration configuration,
            IMediator _mediator,
            IMemoryCache memoryCache)
        {
            _db = repository;
            _configuration = configuration;
            this._mediator = _mediator;
            _memoryCache = memoryCache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetPlanRoomRequest message,
            IOutPutPort<GenericResponse<GetPlanRoomResponse>> outputPort)
        {
            try
            {
                if (!_db.RoomTBLs.Any(e => e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.FriendlyID == message.FriendlyID) &&
                                !message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_admins"))
                {
                    outputPort.Handle(new GenericResponse<GetPlanRoomResponse>(new GetPlanRoomResponse
                    {
                        Message = "دسترسی ندارید",
                        Status = 400
                    }));
                    return;
                }
                //Statics.Log("GetPlanRoomUseCase", JsonConvert.SerializeObject(message.GeneralHeaders));

                string cacheKey = @$"HekmatShobeList{message.FriendlyID}";

                // Try to get data from cache
                if (!_memoryCache.TryGetValue(cacheKey, out List<MiniPlanJSON> cachedData))
                {
                    // Data not in cache, retrieve from database
                    Statics.Log("GetPlanRoomUseCase", "Cache miss - retrieving from database");

                    var room = _db.RoomTBLs.Where(e => e.FriendlyID == message.FriendlyID).First();

                    var list = _db.RoomTBLPlanTBLs.Where(e => e.IDRoomTBL == room.ID).Include(e => e.RoomTBL)
                        .ToList();

                    cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
                    {
                        Id = e.ID,
                        StartAt = e.StartDate,
                        Teacher = e.Teachers,
                        Dabir = e.Dabir,
                        Title = e.Title,
                        RoomJSON = new MiniRoomJSON { RoomID = e.RoomTBL.FriendlyID }
                    }).ToList();

                    // Configure cache options
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5))  // Reset expiration if accessed
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1))     // Absolute expiration time
                        .SetPriority(CacheItemPriority.Normal)            // Cache priority
                        .SetSize(1);                                      // Size for memory pressure management

                    // Add data to cache
                    _memoryCache.Set(cacheKey, cachedData, cacheOptions);

                    Statics.Log("GetPlanRoomUseCase", $"Data cached with {cachedData.Count} items");
                }
                else
                {
                    Statics.Log("GetPlanRoomUseCase", $"Cache hit - returning {cachedData.Count} items from cache");
                }

                outputPort.Handle(new GenericResponse<GetPlanRoomResponse>(new GetPlanRoomResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = cachedData
                }));
            }
            catch (Exception e)
            {
                Statics.Log("GetPlanRoomUseCase", e);
                outputPort.Handle(new GenericResponse<GetPlanRoomResponse>(new GetPlanRoomResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = []
                }));
            }
        }
    }
    #endregion

    #region GetPlanRoomPublic
    public class GetPlanRoomPublicUseCase : IGetPlanRoomPublicUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;
        public GetPlanRoomPublicUseCase(IDBContext repository,
            IConfiguration configuration,
            IMediator _mediator,
            IMemoryCache memoryCache)
        {
            _db = repository;
            _configuration = configuration;
            this._mediator = _mediator;
            _memoryCache = memoryCache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetPlanRoomPublicRequest message,
            IOutPutPort<GenericResponse<GetPlanRoomPublicResponse>> outputPort)
        {
            try
            {
                //Statics.Log("GetPlanRoomPublicUseCase", JsonConvert.SerializeObject(message.GeneralHeaders));

                string cacheKey = @$"PlanRoomPublic";

                // Try to get data from cache
                if (!_memoryCache.TryGetValue(cacheKey, out List<MiniPlanJSON> cachedData))
                {
                    // Data not in cache, retrieve from database
                    Statics.Log("GetPlanRoomPublicUseCase", "Cache miss - retrieving from database");

                    var list = _db.RoomTBLPlanTBLs.Where(e => e.StartDate.HasValue && e.StartDate.Value > DateTime.Now && e.RoomTBL.IsPublicRoom).Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                        .ToList();

                    cachedData = list.OrderBy(e => e.StartDate).Select(e => new MiniPlanJSON
                    {
                        Id = e.ID,
                        StartAt = e.StartDate,
                        Teacher = e.Teachers,
                        Title = e.Title,
                        RoomJSON = new MiniRoomJSON
                        {
                            RoomID = e.RoomTBL.FriendlyID,
                            Title = e.RoomTBL.Title,
                            Owner = new MiniUserJSON
                            {
                                Fullname = e.RoomTBL.OwnerTBL.GetName(),
                                Username = e.RoomTBL.OwnerTBL.Username
                            }
                        }
                    }).ToList();

                    // Configure cache options
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5))  // Reset expiration if accessed
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1))     // Absolute expiration time
                        .SetPriority(CacheItemPriority.Normal)            // Cache priority
                        .SetSize(1);                                      // Size for memory pressure management

                    // Add data to cache
                    _memoryCache.Set(cacheKey, cachedData, cacheOptions);

                    Statics.Log("GetPlanRoomPublicUseCase", $"Data cached with {cachedData.Count} items");
                }
                else
                {
                    Statics.Log("GetPlanRoomPublicUseCase", $"Cache hit - returning {cachedData.Count} items from cache");
                }

                outputPort.Handle(new GenericResponse<GetPlanRoomPublicResponse>(new GetPlanRoomPublicResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = cachedData
                }));
            }
            catch (Exception e)
            {
                Statics.Log("GetPlanRoomPublicUseCase", e);
                outputPort.Handle(new GenericResponse<GetPlanRoomPublicResponse>(new GetPlanRoomPublicResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = []
                }));
            }
        }
    }
    #endregion

    #region GetLatestMeetings
    public class GetLatestMeetingsUseCase : IGetLatestMeetingsUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;
        public GetLatestMeetingsUseCase(IDBContext repository,
            IConfiguration configuration,
            IMediator _mediator,
            IMemoryCache memoryCache)
        {
            _db = repository;
            _configuration = configuration;
            this._mediator = _mediator;
            _memoryCache = memoryCache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetLatestMeetingsRequest message,
            IOutPutPort<GenericResponse<GetLatestMeetingsResponse>> outputPort)
        {
            try
            {
                //Statics.Log("GetLatestMeetingsUseCase", JsonConvert.SerializeObject(message.GeneralHeaders));

                string cacheKey = @$"LatestMeetings";

                // Try to get data from cache
                if (!_memoryCache.TryGetValue(cacheKey, out List<MiniPlanJSON> cachedData))
                {
                    // Data not in cache, retrieve from database
                    Statics.Log("GetLatestMeetingsUseCase", "Cache miss - retrieving from database");

                    var list = _db.RoomTBLPlanTBLs.Where(e => e.StartDate.HasValue && e.StartDate.Value <= DateTime.Now && e.RoomTBL.IsPublicRoom)
                        .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                        .ToList();

                    cachedData = list.OrderByDescending(e => e.StartDate).Select(e => new MiniPlanJSON
                    {
                        Id = e.ID,
                        StartAt = e.StartDate,
                        Teacher = e.Teachers,
                        Title = e.Title,
                        RoomJSON = new MiniRoomJSON
                        {
                            RoomID = e.RoomTBL.FriendlyID,
                            Title = e.RoomTBL.Title,
                            Avatar = e.RoomTBL.Avatar,
                            Owner = new MiniUserJSON
                            {
                                Fullname = e.RoomTBL.OwnerTBL.GetName(),
                                Username = e.RoomTBL.OwnerTBL.Username,
                            }
                        }
                    }).Take(15).ToList();

                    // Configure cache options
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(2))  // Reset expiration if accessed
                        .SetAbsoluteExpiration(TimeSpan.FromDays(1))     // Absolute expiration time
                        .SetPriority(CacheItemPriority.High)            // Cache priority
                        .SetSize(1);                                      // Size for memory pressure management

                    // Add data to cache
                    _memoryCache.Set(cacheKey, cachedData, cacheOptions);

                    Statics.Log("GetLatestMeetingsUseCase", $"Data cached with {cachedData.Count} items");
                }
                else
                {
                    Statics.Log("GetLatestMeetingsUseCase", $"Cache hit - returning {cachedData.Count} items from cache");
                }

                outputPort.Handle(new GenericResponse<GetLatestMeetingsResponse>(new GetLatestMeetingsResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = cachedData
                }));
            }
            catch (Exception e)
            {
                Statics.Log("GetLatestMeetingsUseCase", e);
                outputPort.Handle(new GenericResponse<GetLatestMeetingsResponse>(new GetLatestMeetingsResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = []
                }));
            }
        }
    }
    #endregion

    #region DeletePlanRoom
    public class DeletePlanRoomUseCase : IDeletePlanRoomUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public DeletePlanRoomUseCase(IDBContext repository,
            IConfiguration configuration,
            IMediator _mediator,
            IMemoryCache memoryCache)
        {
            _db = repository;
            _configuration = configuration;
            this._mediator = _mediator;
            _memoryCache = memoryCache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(DeletePlanRoomRequest message,
            IOutPutPort<GenericResponse<DeletePlanRoomResponse>> outputPort)
        {
            try
            {
                if (!_db.RoomTBLPlanTBLs.Any(e => e.RoomTBL.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID && e.ID == message.IDPlan) &&
                                !message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_admins"))
                {
                    outputPort.Handle(new GenericResponse<DeletePlanRoomResponse>(new DeletePlanRoomResponse
                    {
                        Message = "دسترسی ندارید",
                        Status = 400
                    }));
                    return;
                }
                var room = _db.RoomTBLPlanTBLs.Where(e => e.ID == message.IDPlan).Include(e => e.RoomTBL).First();
                _db.RoomTBLPlanTBLs.Remove(room);

                _db.SaveChanges();
                await _mediator.Publish(new PlanUpdateEvent
                {
                    FriendlyID = room.RoomTBL.FriendlyID,
                    Action = "delete",

                });
                outputPort.Handle(new GenericResponse<DeletePlanRoomResponse>(new DeletePlanRoomResponse
                {
                    Message = "ok",
                    Status = 200,
                }));
            }
            catch (Exception e)
            {
                Statics.Log("DeletePlanRoomUseCase", e);
                outputPort.Handle(new GenericResponse<DeletePlanRoomResponse>(new DeletePlanRoomResponse
                {
                    Message = "exc",
                    Status = 400,
                }));
            }
        }
    }
    #endregion
}