using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
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
    #region NewUser
    public class NewUserUseCase : INewUserUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IHostingEnvironment hosting;

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
        private IHostingEnvironment hosting;

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
        private IHostingEnvironment hosting;

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
        private IHostingEnvironment hosting;

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

            var userMeet = await _commonMethods.NewUserMeeting(new UserTBLToMeetingTBL
            {
                MeetID = meet.MeetID,
                UUID = user.UUID,
                TokenJWT = jwtUser,
                Explain = message.Role,
                Type = UserTBL.MapRoleToType(message.Role),
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                Role = message.Role,
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
        private IHostingEnvironment hosting;

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
                IDUserTBLToMeetingTBL = userMeet.ID,
                OS = message.OS + "/$$" + message.GeneralHeaders.Agent + "/$$" + message.GeneralHeaders.RequestedDevice + "/$$" + message.GeneralHeaders.RefererGoogle
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

    #region GetTeacherList
    public class GetTeacherListUseCase : IGetTeacherListUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IHostingEnvironment hosting;

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
                return;
            }

            outputPort.Handle(new GenericResponse<GetTeacherListResponse>(new GetTeacherListResponse
            {
                Message = "ok",
                Status = 200,
                Data = _db.UserTBLs.Where(e => e.Type == MainEntity_Types.USER_TEACHER).Select(e => new DTO.Models.MiniUserJSON
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
        private IHostingEnvironment hosting;

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
        private IHostingEnvironment hosting;

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
        private IHostingEnvironment hosting;

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

}