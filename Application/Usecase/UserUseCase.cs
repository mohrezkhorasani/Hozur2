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
using Org.BouncyCastle.Utilities.Encoders;
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
                MeetingTBL? meet = new MeetingTBL
                {
                    RoomFriendlyID = message.RoomID,
                    MeetID = message.MeetID,
                    Type = MainEntity_Types.Normal,
                    Status = MainEntity_Status.MEETING_STARTED_LATER,
                    JalaliDate = Statics.ToJalaliDate(DateTime.Now),
                    IDCreator = user.ID
                };

                meet = await _commonMethods.NewMeeting(meet);
                meet = await _db.MeetingTBLs.Include(e => e.Creator)
                    .Include(e => e.Teacher).Include(e => e.RoomTBL)
                    .FirstOrDefaultAsync(e => e.ID == meet.ID);
                if (meet == null) return;
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
                        Teacher = meet.Teacher == null ? null : new DTO.Models.MiniUserJSON
                        {
                            Fullname = $"{meet.Teacher.Firstname} {meet.Teacher.LastName}",
                            ID = meet.Teacher.ID,
                        },
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
            meet.Title = message.Title;
            meet.JalaliDate = message.JalaliDate;
            meet.Status = MainEntity_Status.MEETING_LIVE;
            UserTBL teacher = null;
            if (_db.UserTBLs.Any(
                e => (e.Firstname + " " + e.LastName).Trim() == message.Teacher
                    && e.Status == MainEntity_Status.USER_ACTIVE
                    && e.Type == MainEntity_Types.USER_TEACHER
            ))
            {
                teacher = _db.UserTBLs.FirstOrDefault(
                    e => (e.Firstname + " " + e.LastName).Trim() == message.Teacher
                    && e.Status == MainEntity_Status.USER_ACTIVE
                    && e.Type == MainEntity_Types.USER_TEACHER
                );
            }
            else
            {
                teacher = new UserTBL
                {
                    Firstname = message.Teacher,
                    Type = MainEntity_Types.USER_TEACHER,
                    Status = MainEntity_Status.USER_ACTIVE,
                    Username = message.Teacher,
                    UUID = new Random().NextInt64(-999999999999999999, -99999999999999999).ToString()
                };
                _db.UserTBLs.Add(teacher);
                _db.SaveChanges();

            }

            meet.IDTeacher = teacher.ID;
            meet.Teacher = teacher;
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
                    Teacher = new DTO.Models.MiniUserJSON
                    {
                        Fullname = $"{meet.Teacher.Firstname} {meet.Teacher.LastName}",
                        ID = meet.Teacher.ID,
                    },
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
                .FirstOrDefaultAsync(e => e.ID == meet.ID);
            if (meet == null) return;
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
                    Teacher = meet.Teacher == null ? null : new DTO.Models.MiniUserJSON
                    {
                        Fullname = $"{meet.Teacher.Firstname} {meet.Teacher.LastName}",
                        ID = meet.Teacher.ID,
                    },
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
            var init = await _commonMethods.Initialize<NewUserMeetingLogRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;
            var userMeet = _db.UserTBLToMeetingTBLs
                .FirstOrDefault(e => e.MeetID == meet.MeetID && e.UUID == user.UUID);
            UserTBLToMeetingTBLLogs? log = new UserTBLToMeetingTBLLogs
            {
                Type = UserTBL.MapRoleToType(message.Role),
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                Key = message.KeyLog,
                Value = message.ValueLog,
                Explain = message.Role,
                UserMeetID = message.MeetUserID,
                IDUserTBLToMeetingTBL = userMeet.ID,
                OS = message.OS + "/$$" + message.GeneralHeaders.Agent + "/$$" + message.GeneralHeaders.RequestedDevice + "/$$" + message.GeneralHeaders.Referer
            };
            _db.UserTBLToMeetingTBLLogs.Add(log);
            _db.SaveChanges();

            outputPort.Handle(new GenericResponse<NewUserMeetingLogResponse>(new NewUserMeetingLogResponse
            {
                Message = "ok",
                Status = 200,

            }));

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
            var init = await _commonMethods.Initialize<NewUserMeetingLogFromModeratorRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;

            if (!UserTBL.TYPE_ALLOWED_LOG_FOR_OTHERS.Contains(user.Type))
            {
                outputPort.Handle(new GenericResponse<NewUserMeetingLogFromModeratorResponse>(new NewUserMeetingLogFromModeratorResponse
                {
                    Message = "ok",
                    Status = 403,
                }));
                return;
            }
            if (_db.UserTBLToMeetingTBLs.Any(e => e.UserMeetID == message.MeetUserID && e.MeetID == meet.MeetID))
            {


                var userMeet = _db.UserTBLToMeetingTBLs
                    .FirstOrDefault(e => e.MeetID == meet.MeetID && e.UserMeetID == message.MeetUserID);
                UserTBLToMeetingTBLLogs? log = new UserTBLToMeetingTBLLogs
                {
                    Type = UserTBL.MapRoleToType(message.Role),
                    Status = MainEntity_Status.MEETING_STARTED_LATER,
                    Key = message.KeyLog,
                    Value = message.ValueLog,
                    Explain = message.Role,
                    UserMeetID = message.MeetUserID,
                    IDUserTBLToMeetingTBL = userMeet.ID,
                    OS = message.ExtId + "/$$" + message.GeneralHeaders.Agent + "/$$" + message.GeneralHeaders.RequestedDevice + "/$$" + message.GeneralHeaders.Referer
                };
                _db.UserTBLToMeetingTBLLogs.Add(log);
                _db.SaveChanges();
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

            outputPort.Handle(new GenericResponse<GetRoomsResponse>(new GetRoomsResponse
            {
                Message = "ok",
                Status = 200,
                Data = _db.RoomTBLs
                .Select(e => new DTO.Models.MiniRoomJSON
                {
                    RoomID = e.FriendlyID,
                    Title = e.Title
                }).ToList()
            }));

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
        public GetLivesUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
        }

        public async Task handleAsync(GetLivesRequest message,
            IOutPutPort<GenericResponse<GetLivesResponse>> outputPort)
        {
            var meets = await IBBBGetMeetings.Request(message);

            outputPort.Handle(new GenericResponse<GetLivesResponse>(new GetLivesResponse
            {
                Message = "ok",
                Status = 200,
                Data = meets.Select(e => new MiniRoomJSON
                {
                    RoomID = _db.MeetingTBLs.FirstOrDefault(r => r.MeetID.EndsWith(e.RoomID)).RoomFriendlyID,
                    Title = e.Title
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
            var meet = await IBBBGetMeetings.CreateMeetingAsync(message.MeetiID, message.MeetName);

            var meetTBL = _commonMethods.NewMeeting(new MeetingTBL
            {
                RoomFriendlyID = message.MeetiID,
                MeetID = meet.MeetID,
                Type = MainEntity_Types.Normal,
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                JalaliDate = Statics.ToJalaliDate(DateTime.Now),
                IDCreator = -6000
            });

            outputPort.Handle(new GenericResponse<StartMeetingResponse>(new StartMeetingResponse
            {
                Message = "ok",
                Status = 200,
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


                var check = await _commonMethods.CheckGuaranteedUser(user, meet);
                if (!check.Item1)
                {

                    outputPort.Handle(new GenericResponse<GuarantCertificateResponse>(new GuarantCertificateResponse
                    {
                        Message = check.Item2,
                        Status = 403,
                    }));
                    return;
                }
                var um = _db.UserTBLToMeetingTBLs.Where(e => e.MeetID == meet.MeetID && e.UUID == user.UUID).First();

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
                    _db.CertificateTBLs.Add(cert);
                    _db.SaveChanges();
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
                    role = "مدرس";
                }
                if (!_db.CertificateTBLs.Any(e => e.IDUserTBLToMeetingTBL == um.ID))
                {
                    outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                    {
                        Message = "هنوز گواهینامه برای شما صادر نشده است!",
                        Status = 404,
                    }));
                    return;
                }
                var cert = await _db.CertificateTBLs.FirstOrDefaultAsync(e => e.IDUserTBLToMeetingTBL == um.ID);
                cert.NationalCode = message.NationalCode;
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



                var resultPath = WordService.CreateCertificatePdf(outputPath, fontPath, bgpath, um.UserNickName , message.Sex, message.NationalCode,
                    meet.RoomTBL.Title, meet.Title,Statics.ConvertPersianMonthToNumber(meet.JalaliDate).Replace("-", "/"),
                    role, um.ID.ToString()); 

                cert.FileURL = "https://vc.hekmateislami.com/";

                //string compressedPath = PdfCompressor.CompressPdf(@$"{outputPath}");

                cert.FilePath = Path.Combine("cert", $@"certif-{um.ID}.pdf");
                _db.CertificateTBLs.Update(cert);
                _db.SaveChanges();

                scpUploader.UploadFile(outputPath);

                outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                {
                    Message = "ok",
                    Status = 200,
                    Link = Path.Join(cert.FileURL, cert.FilePath)

                }));
            }
            catch (Exception e)
            {
                outputPort.Handle(new GenericResponse<GenerateCertificateResponse>(new GenerateCertificateResponse
                {
                    Message = e.Message + e.InnerException,
                    Status = 400,

                }));
            }
        }



    }
    #endregion

}