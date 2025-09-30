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
            if (user.AddIP(message.IP))
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
            UserTBL user = new UserTBL
            {
                UUID = message.UUID,
                Email = message.Email,
                Firstname = message.Firstname,
                NationalCode = message.NationalCode,
                PhoneNumber = message.PhoneNumber,
            };
            user = await _commonMethods.NewUser(user);
            if (user.AddIP(message.IP))
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
            UserTBL user = new UserTBL
            {
                UUID = message.UUID,
                Email = message.Email,
                Firstname = message.Firstname,
                NationalCode = message.NationalCode,
                PhoneNumber = message.PhoneNumber,
            };
            user = await _commonMethods.NewUser(user);
            if (user.AddIP(message.IP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            if (!UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
            {
                outputPort.Handle(new GenericResponse<UpdateMeetingResponse>(new UpdateMeetingResponse
                {
                    Message = "ok",
                    Status = 202,
                }));
                return;
            }
            MeetingTBL meet = new MeetingTBL
            {
                RoomFriendlyID = message.GeneralHeaders.RoomID,
                MeetID = message.GeneralHeaders.MeetIdentificationCode.ToString(),
            };

            meet = await _commonMethods.NewMeeting(meet);
            meet.Title = message.Title;
            meet.JalaliDate = message.JalaliDate;
            UserTBL teacher = await _commonMethods.NewUser(new UserTBL
            {
                Firstname = message.Teacher,
                Type = MainEntity_Types.USER_TEACHER,
                Status = MainEntity_Status.USER_ACTIVE,
                Username = message.Teacher,
            });
            meet.IDTeacher = teacher.ID;
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
            };
            user = await _commonMethods.NewUser(user);
            if (user.AddIP(message.IP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            if (!UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
            {
                outputPort.Handle(new GenericResponse<NewUserMettingsResponse>(new NewUserMettingsResponse
                {
                    Message = "ok",
                    Status = 202,
                }));
                return;
            }
            var meet = await _commonMethods.GetMeetWithoutID(message.MeetID);

            var userMeet = await _commonMethods.NewUserMeeting(new UserTBLToMeetingTBL
            {
                MeetID = meet.MeetID,
                UUID = user.UUID,
                TokenJWT = message.GeneralHeaders.JWTUserToken,
                Explain = message.Role,
                Type = UserTBL.MapRoleToType(message.Role),
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                Role = message.Role,
                IPList = new List<string>() { message.IP },
                UserNickName = message.NickName,
                TimeSpendSeconds = 0,
                IsCertificated = false,
                
            });
            if (!userMeet.IPList.Contains(message.IP))
            {
                userMeet.IPList.Add(message.IP);
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
            UserTBL user = new UserTBL
            {
                UUID = message.UUID,
                Email = message.Email,
                Firstname = message.Firstname,
                NationalCode = message.NationalCode,
                PhoneNumber = message.PhoneNumber,
            };
            user = await _commonMethods.NewUser(user);
            if (user.AddIP(message.IP))
            {
                _db.Update(user);
                _db.SaveChanges();
            }
            if (!UserTBL.TYPE_ALLOWED_MEET_CREATE.Contains(user.Type))
            {
                outputPort.Handle(new GenericResponse<NewUserMeetingLogResponse>(new NewUserMeetingLogResponse
                {
                    Message = "ok",
                    Status = 202,
                }));
                return;
            }
            var meet = await _commonMethods.GetUserMeetWithoutID(new UserTBLToMeetingTBL
            {
                MeetID = message.MeetID,
                UUID = message.UUID,
            });

            UserTBLToMeetingTBLLogs? log = new UserTBLToMeetingTBLLogs
            {
                Type = UserTBL.MapRoleToType(message.Role),
                Status = MainEntity_Status.MEETING_STARTED_LATER,
                Key = message.KeyLog,
                Value = message.ValueLog,
                Explain = message.Role,
                IDUserTBLToMeetingTBL = meet.ID,
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


}