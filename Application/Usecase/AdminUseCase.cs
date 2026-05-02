using Application.DTO;
using Application.DTO.Generals;
using Application.DTO.Models;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using CacheHandler.Events;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Application.Usecase
{
    #region AdminGetUserList
    public class AdminGetUserListUseCase : IAdminGetUserListUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public AdminGetUserListUseCase(IDBContext repository,
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


        public async Task handleAsync(AdminGetUserListRequest message,
            IOutPutPort<GenericResponse<AdminGetUserListResponse>> outputPort)
        {
            if (message.GeneralHeaders.UserTBL == null)
                return;
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_users"))
            {
                throw new Exception("FORBIDDEN ! 403");
            }
            if (!message.Count.HasValue) message.Count = 15;
            if (!message.Page.HasValue) message.Page = 0;
            List<UserTBL> users = new List<UserTBL>();
            IOrderedQueryable<UserTBL> orderedUsers;
            switch (message.OrderBy)
            {
                case OrderBy.DATE:
                    orderedUsers = _db.UserTBLs.OrderBy(e => e.DateInsert);
                    break;
                case OrderBy.DATE_DESC:
                    orderedUsers = _db.UserTBLs.OrderByDescending(e => e.DateInsert);
                    break;
                case OrderBy.NAME:
                    orderedUsers = _db.UserTBLs.OrderBy(e => e.Firstname); // assuming Name property exists
                    break;
                case OrderBy.NAME_DESC:
                    orderedUsers = _db.UserTBLs.OrderByDescending(e => e.Firstname);
                    break;
                case OrderBy.SCORE:
                    orderedUsers = _db.UserTBLs.OrderBy(e => e.Type);
                    break;
                case OrderBy.SCORE_DESC:
                    orderedUsers = _db.UserTBLs.OrderByDescending(e => e.Type);
                    break;
                default:
                    orderedUsers = _db.UserTBLs.OrderBy(e => e.DateInsert); // default fallback
                    break;
            }
            users = orderedUsers.Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value).ToList();
            outputPort.Handle(new GenericResponse<AdminGetUserListResponse>(new AdminGetUserListResponse
            {
                Message = "ok",
                Status = 200,
                Data = users.Select(e => new MiniUserJSON
                {
                    Email = e.Email,
                    Fullname = e.GetName(),
                    ID = e.ID,
                    IsValid = false,
                    Permissions = e.GetUserPermissions(),
                    Role = e.GetRole(),
                    UID = e.UUID,
                    Username = e.Username
                }).ToList()
            }));

        }
    }
    #endregion

    #region AdminChangeRole
    public class AdminChangeRoleUseCase : IAdminChangeRoleUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private readonly IMediator _mediator;

        private IScpUploader IScpUploader { get; }
        public AdminChangeRoleUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader,
            IMediator m
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
            this._mediator = m;
        }


        public async Task handleAsync(AdminChangeRoleRequest message,
            IOutPutPort<GenericResponse<AdminChangeRoleResponse>> outputPort)
        {
            if (message.GeneralHeaders.UserTBL == null)
                return;
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_users"))
            {
                throw new Exception("FORBIDDEN ! 403");
            }
            if (!_db.UserTBLs.Any(e => e.ID == message.UserId))
            {
                outputPort.Handle(new GenericResponse<AdminChangeRoleResponse>(new AdminChangeRoleResponse
                {
                    Message = "NotFound",
                    Status = 404,
                }));
                return;
            }

            var user = await _db.UserTBLs.FindAsync(message.UserId);
            user.Type = message.Role;
            user.AllowedGUID.Add(Guid.NewGuid());
            _db.Update(user); _db.SaveChanges();
            await _mediator.Publish(new UserUpdatedEvent
            {
                Action = "Update",
                UserId = user.ID,
            });
            outputPort.Handle(new GenericResponse<AdminChangeRoleResponse>(new AdminChangeRoleResponse
            {
                Message = "ok",
                Status = 200,
                Data = new MiniUserJSON
                {
                    Role = user.GetRole(),
                    Email = user.Email,
                    Fullname = user.GetName(),
                    ID = message.UserId,
                    Username = user.GetName(),
                    Permissions = user.GetUserPermissions(),
                }
            }));

        }
    }
    #endregion

    #region AdminCreateRoom
    public class AdminCreateRoomUseCase : IAdminCreateRoomUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private readonly IMediator _mediator;
        private IScpUploader IScpUploader { get; }
        public AdminCreateRoomUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IMediator m,
            IScpUploader IScpUploader
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
            this._mediator = m;
        }


        public async Task handleAsync(AdminCreateRoomRequest message,
     IOutPutPort<GenericResponse<AdminCreateRoomResponse>> outputPort)
        {
            if (message.GeneralHeaders.UserTBL == null)
                return;

            // بررسی دسترسی ادمین برای ایجاد/ویرایش اتاق برای دیگران
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_rooms_for_others"))
            {
                throw new Exception("FORBIDDEN ! 403");
            }

            // بررسی وجود کاربر هدف (صاحب اتاق)
            if (!_db.UserTBLs.Any(e => e.ID == message.UserID))
            {
                outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                {
                    Message = "UserNotFound",
                    Status = 404,
                }));
                return;
            }

            var user = await _db.UserTBLs.FindAsync(message.UserID);

            // اگر RoomID مشخص شده (یعنی عملیات ویرایش)
            if (message.ID.HasValue && message.ID.Value > 0) // فرض می‌کنیم message.ID از نوع long یا int است
            {
                var existingRoom = await _db.RoomTBLs.FindAsync(message.ID);
                if (existingRoom == null)
                {
                    outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                    {
                        Message = "RoomNotFound",
                        Status = 404,
                    }));
                    return;
                }

                // بررسی اینکه اتاق متعلق به همان کاربر باشد (اختیاری، برای امنیت)
                if (existingRoom.IDOwnerTBL != user.ID)
                {
                    outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                    {
                        Message = "RoomNotBelongToUser",
                        Status = 403,
                    }));
                    return;
                }

                // بررسی تکراری نبودن Title برای این کاربر (در صورت تغییر عنوان)
                if (_db.RoomTBLs.Any(e => e.Title == message.Title && e.IDOwnerTBL == user.ID && e.ID != message.ID))
                {
                    outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                    {
                        Message = "there is a room with this title for him",
                        Status = 401,
                    }));
                    return;
                }

                // به‌روزرسانی فیلدهای قابل تغییر
                existingRoom.Title = message.Title;
                existingRoom.IsPublicRoom = message.IsPublic;
                existingRoom.RecordRoom = message.IsRecordable;
                existingRoom.RequiredLoginToView = message.IsRequiredLogin;
                existingRoom.RequiredAdminAcceptToView = message.IsRequiredAccept;
                // در صورت نیاز می‌توانید کدهای دسترسی را نیز تغییر دهید (مثلاً ModeratorCode, ViewerCode)
                // اما معمولاً در ویرایش نباید آن‌ها را عوض کرد مگر اینکه منطق خاصی داشته باشید.

                await _db.SaveChangesAsync();


                await _mediator.Publish(new RoomUpdatedEvent
                {
                    RoomId = existingRoom.ID,
                    Action = "Update"
                });
                outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = new MiniRoomJSON
                    {
                        BBB_MeetID = existingRoom.BBB_MeetID,
                        RoomID = existingRoom.FriendlyID,
                        Owner = new MiniUserJSON { ID = existingRoom.IDOwnerTBL },
                        Title = existingRoom.Title,
                    }
                }));
                return;
            }
            else
            {
                // --- عملیات Insert (ساخت اتاق جدید) مانند قبل ---
                if (_db.RoomTBLs.Any(e => e.Title == message.Title && e.IDOwnerTBL == user.ID))
                {
                    outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                    {
                        Message = "there is a room with this title for him",
                        Status = 401,
                    }));
                    return;
                }

                var room = new RoomTBL
                {
                    FriendlyID = Statics.GenerateRandomRoomFriendlyID(),
                    BBB_MeetID = Statics.GenerateRoomMeetID(),
                    IDOwnerTBL = user.ID,
                    IsPublicRoom = message.IsPublic,
                    ModeratorCode = Statics.GenerateSegment(8),
                    ViewerCode = Statics.GenerateSegment(8),
                    Title = message.Title,
                    RecordRoom = message.IsRecordable,
                    RequiredLoginToView = message.IsRequiredLogin,
                    RequiredAdminAcceptToView = message.IsRequiredAccept,
                    Type = MainEntity_Types.ROOM_CREATED_BY_ADMIN,
                    Status = MainEntity_Status.Enabled,
                    TTR_URL = ""
                };

                while (_db.RoomTBLs.Any(e => e.BBB_MeetID == room.BBB_MeetID))
                {
                    room.BBB_MeetID = Statics.GenerateRoomMeetID();
                }
                while (_db.RoomTBLs.Any(e => e.FriendlyID == room.FriendlyID))
                {
                    room.FriendlyID = Statics.GenerateRandomRoomFriendlyID();
                }

                _db.RoomTBLs.Add(room);
                await _db.SaveChangesAsync();
                await _mediator.Publish(new RoomUpdatedEvent
                {
                    RoomId = room.ID,
                    Action = "Create"
                });
                outputPort.Handle(new GenericResponse<AdminCreateRoomResponse>(new AdminCreateRoomResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = new MiniRoomJSON
                    {
                        BBB_MeetID = room.BBB_MeetID,
                        RoomID = room.FriendlyID,
                        Owner = new MiniUserJSON { ID = room.IDOwnerTBL },
                        Title = room.Title,
                    }
                }));
            }
        }
    }
    #endregion

    #region AdminGetUserRoomList
    public class AdminGetUserRoomListUseCase : IAdminGetUserRoomListUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public AdminGetUserRoomListUseCase(IDBContext repository,
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


        public async Task handleAsync(AdminGetUserRoomListRequest message,
            IOutPutPort<GenericResponse<AdminGetUserRoomListResponse>> outputPort)
        {
            if (message.GeneralHeaders.UserTBL == null)
                return;
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_rooms_for_others"))
            {
                throw new Exception("FORBIDDEN ! 403");
            }
            if (!message.Count.HasValue) message.Count = 15;
            if (!message.Page.HasValue) message.Page = 0;
            List<RoomTBL> users = new List<RoomTBL>();
            IQueryable<RoomTBL> roomQuery = _db.RoomTBLs.Where(e => e.IDOwnerTBL == message.OwnerID);
            IOrderedQueryable<RoomTBL> orderedUsers;
            switch (message.OrderBy)
            {
                case OrderBy.DATE:
                    orderedUsers = roomQuery.OrderBy(e => e.DateInsert);
                    break;
                case OrderBy.DATE_DESC:
                    orderedUsers = roomQuery.OrderByDescending(e => e.DateInsert);
                    break;
                case OrderBy.NAME:
                    orderedUsers = roomQuery.OrderBy(e => e.Title); // assuming Name property exists
                    break;
                case OrderBy.NAME_DESC:
                    orderedUsers = roomQuery.OrderByDescending(e => e.Title);
                    break;
                case OrderBy.SCORE:
                    orderedUsers = roomQuery.OrderBy(e => e.Type);
                    break;
                case OrderBy.SCORE_DESC:
                    orderedUsers = roomQuery.OrderByDescending(e => e.Type);
                    break;
                default:
                    orderedUsers = roomQuery.OrderBy(e => e.DateInsert); // default fallback
                    break;
            }
            users = orderedUsers.Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value).ToList();
            outputPort.Handle(new GenericResponse<AdminGetUserRoomListResponse>(new AdminGetUserRoomListResponse
            {
                Message = "ok",
                Status = 200,
                Data = users.Select(e => new MiniRoomJSON
                {
                    Title = e.Title,
                    BBB_MeetID = e.BBB_MeetID,
                    RoomID = e.FriendlyID,
                    Created = e.DateInsert
                }).ToList()
            }));

        }
    }
    #endregion

    #region ChangeAvatarOfRoom
    public class ChangeAvatarOfRoomUseCase : IChangeAvatarOfRoomUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private readonly IMediator _mediator;

        private IScpUploader IScpUploader { get; }
        public ChangeAvatarOfRoomUseCase(IDBContext repository,
            IConfiguration configuration,
            IBBBGetMeetings bBBGetMeetings,
            IScpUploader IScpUploader,
            IMediator m
            )
        {
            _db = repository;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
            IBBBGetMeetings = bBBGetMeetings;
            this.IScpUploader = IScpUploader;
            this._mediator = m;
        }


        public async Task handleAsync(ChangeAvatarOfRoomRequest message,
            IOutPutPort<GenericResponse<ChangeAvatarOfRoomResponse>> outputPort)
        {
            if (message.GeneralHeaders?.UserTBL?.ID == null)
                return;
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_meeting"))
            {
                throw new Exception("FORBIDDEN ! 403");
            }
            if (!_db.RoomTBLs.Any(e => e.FriendlyID == message.RoomID && e.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID))
            {
                outputPort.Handle(new GenericResponse<ChangeAvatarOfRoomResponse>(new ChangeAvatarOfRoomResponse
                {
                    Message = "FORBIDDEN",
                    Status = 403,
                }));
                return;
            }

            var room = await _db.RoomTBLs.FirstOrDefaultAsync(e => e.FriendlyID == message.RoomID);
            room.Avatar = message.AvatarPath;
            _db.Update(room); _db.SaveChanges();
            await _mediator.Publish(new PlanUpdateEvent
            {
                Action = "Update",
                FriendlyID = room.FriendlyID,
            }); await _mediator.Publish(new RoomUpdatedEvent
            {
                Action = "Update",
                RoomId = room.ID,
            });
            outputPort.Handle(new GenericResponse<ChangeAvatarOfRoomResponse>(new ChangeAvatarOfRoomResponse
            {
                Message = "ok",
                Status = 200,
            }));

        }
    }
    #endregion
}
