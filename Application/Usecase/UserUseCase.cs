using Application.DTO;
using Application.DTO.Generals;
using Application.DTO.Models;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Domain.Entity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Kerberos;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Linq;
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
            if (message.GeneralHeaders.JWTUserToken == null)
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
            Statics.Log("GetTeacherListUseCase", JsonConvert.SerializeObject(message.GeneralHeaders));

            var init = await _commonMethods.Initialize<GetTeacherListRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;
            if (!UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
            {
                outputPort.Handle(new GenericResponse<GetTeacherListResponse>(new GetTeacherListResponse
                {
                    Message = "You can't access it",
                    Status = 200,
                    Data = meet.Teacher == null ? [] : [new MiniUserJSON{
                        Fullname=meet.Teacher.Firstname + " " + meet.Teacher.LastName,
                    }]
                }));
                return;
            }

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

        public GetRoomsUseCase(IDBContext repository,
            IConfiguration configuration)
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetRoomsRequest message,
     IOutPutPort<GenericResponse<GetRoomsResponse>> outputPort)
        {
            try
            {


                // اول اتاق‌ها رو با اطلاعات پایه می‌گیریم (سبک و سریع)
                var rooms = await _db.RoomTBLs
                    .Where(e => (string.IsNullOrEmpty(message.Query) || e.Title.Contains(message.Query)) && e.Type == MainEntity_Types.ROOM_NORMAL)
                    .Include(e => e.MeetingTBLs)
                        .ThenInclude(m => m.UserTBLToMeetingTBLs)
                            .ThenInclude(um => um.UserTBL)
                    .OrderByDescending(e => e.DateInsert)
                    .Select(e => new
                    {
                        e.FriendlyID,
                        e.Title,
                        e.DateInsert,
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
                    .ToListAsync(); // ← اینجا می‌ریزیم توی حافظه

                // حالا توی حافظه محاسبه می‌کنیم (بدون هیچ مشکلی!)
                var result = rooms.Select(room => new DTO.Models.MiniRoomJSON
                {
                    RoomID = room.FriendlyID,
                    Title = room.Title,
                    Created = room.DateInsert,

                    // آخرین جلسه
                    LastMeet = room.Meetings
                        .OrderByDescending(m => m.DateInsert)
                        .FirstOrDefault()?.DateInsert ?? room.DateInsert,

                    // تعداد جلساتی که حداقل ۲ نفر (غیر معلم) داشتن
                    Count = room.Meetings.Count(m => m.Participants
                        .Count(p => p.Type != MainEntity_Types.USER_TEACHER) > 1),

                    // تعداد معلم‌های یکتا
                    CountTeacher = room.Meetings
                        .SelectMany(m => m.Participants)
                        .Where(p => p.Type == MainEntity_Types.USER_TEACHER)
                        .Select(p => p.UUID ?? p.Email ?? p.NationalCode ?? p.PhoneNumber ?? p.UserNickName)
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .Distinct()
                        .Count(),

                    // تعداد شرکت‌کننده‌های یکتا (دانش‌آموز) با همه شناسه‌ها
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
                }).OrderByDescending(e => e.LastMeet).ToList();

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
            outputPort.Handle(new GenericResponse<GetRoomByFriendlyIDResponse>(new GetRoomByFriendlyIDResponse
            {
                Message = "ok",
                Status = 200,
                Data = _db.RoomTBLs.Where(e => e.FriendlyID == message.Query)
                .Select(e => new DTO.Models.MiniRoomJSON
                {
                    RoomID = e.FriendlyID,
                    Title = e.Title
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
            var meets = await IBBBGetMeetings.Request(message);

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

                }).ToList()
            }));

        }
    }
    #endregion

    #region StartMeeting
    public class StartMeetingUseCase : IStartMeetingUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        public StartMeetingUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
        }

        public async Task handleAsync(StartMeetingRequest message,
            IOutPutPort<GenericResponse<StartMeetingResponse>> outputPort)
        {

            var meet = await IBBBGetMeetings.CreateMeetingAsync(message.MeetiID, message.MeetName, "ff");

            var meetTBL = _commonMethods.NewMeeting(new MeetingTBL
            {
                RoomFriendlyID = message.MeetiID,
                MeetID = meet.MeetID,
                Type = MainEntity_Types.Normal,
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                JalaliDate = Statics.ToJalaliDate(DateTime.Now),
                IDCreator = -6000
            });

            var joinURL = await IBBBGetMeetings.GetJoinUrlAsync(meet.MeetID, "مجمع عالی حکمت اسلامی", true, Statics.BBB_MODERATOR_PW);
            var joinURLOthers = await IBBBGetMeetings.GetJoinUrlAsync(meet.MeetID, "مهمان", true, Statics.BBB_ATTEND_PW);
            outputPort.Handle(new GenericResponse<StartMeetingResponse>(new StartMeetingResponse
            {
                Message = "ok",
                Status = 200,
                Join = joinURL,
                JoinOthers=joinURLOthers
            }));

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
            var listmeet = await IBBBGetMeetings.Request(new GetLivesRequest
            {
                GeneralHeaders = message.GeneralHeaders,
                AllowedGUID = message.AllowedGUID,
                Email = message.Email,
                ExtId = message.ExtId,
                UUID = message.UUID
            });
            listmeet.ForEach(e => e.RoomID = _db.MeetingTBLs.Any(r => r.MeetID.EndsWith(e.RoomID)) ? _db.MeetingTBLs.FirstOrDefault(r => r.MeetID.EndsWith(e.RoomID)).RoomFriendlyID : e.RoomID);


            var meets = await IBBBGetMeetings.GetJoinUrlAsync(message.MeetiID, message.FullName, message.AsModerator, message.Password);

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
                    Message = $"ok{message.GeneralHeaders.JWTMeetingToken}\n {message.GeneralHeaders.JWTUserToken} \n {meet.MeetID} \n {user.UUID}",
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

    #region EndTheMeeting
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
            if (!await _commonMethods.CheckUserAccessibilityToMeet(user, meet, "EndTheMeeting"))
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingResponse>(new EndTheMeetingResponse
                {
                    Message = "forbidden",
                    Status = 403,
                }));
                return;
            }
            if (meet == null || user == null)
            {
                outputPort.Handle(new GenericResponse<EndTheMeetingResponse>(new EndTheMeetingResponse
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
            outputPort.Handle(new GenericResponse<EndTheMeetingResponse>(new EndTheMeetingResponse
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

}