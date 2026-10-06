using Application.Interface.IUsecase;

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
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Application.DTO;

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
            users = orderedUsers
                //.Where(e=>e.AvatarPath!=null)
                .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value).OrderByDescending(e=>e.LastUpdate).ThenByDescending(e=>e.AvatarPath).ToList();
            outputPort.Handle(new GenericResponse<AdminGetUserListResponse>(new AdminGetUserListResponse
            {
                Message = "ok",
                Status = 200,
                Data = users.Select(e => new MiniUserJSON
                {
                    Email = e.Email,
                    Fullname = e.GetName(),
                    Firstname = e.Firstname,
                    LastName = e.LastName,
                    ID = e.ID,
                    IsValid = e.Status == MainEntity_Status.USER_ACTIVE || e.Status == MainEntity_Status.USER_ACTIVE || (int)e.Status == 100,
                    Status = e.Status,
                    IsHekmatShobe = e.IsHekmatShobe,
                    IsVIPMember = e.IsMemberVIP,
                    AvatarPath =string.IsNullOrWhiteSpace(e.AvatarPath)?null: Statics.CombinePath(Statics.URLFiles, e.AvatarPath),
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
                        IsSubscribed = existingRoom.IsSubscribed,
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
                        IsSubscribed = room.IsSubscribed,
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
                    IsSubscribed = e.IsSubscribed,
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
            _ = _mediator.Publish(new PlanUpdateEvent
            {
                Action = "Update",
                FriendlyID = room.FriendlyID,
            }); _ = _mediator.Publish(new RoomUpdatedEvent
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

    #region GetAllRooms
    public class GetAllRoomsUseCase : IGetAllRoomsUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IMemoryCache _cache { get; }

        public GetAllRoomsUseCase(IDBContext repository,
            IMemoryCache _cache,
            IConfiguration configuration)
        {
            _db = repository;
            this._cache = _cache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetAllRoomsRequest message,
        IOutPutPort<GenericResponse<GetAllRoomsResponse>> outputPort)
        {
            try
            {
                if (message.GeneralHeaders.UserTBL == null)
                    return;
                if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_users"))
                {
                    throw new Exception("FORBIDDEN ! 403");
                }


                if (!string.IsNullOrEmpty(message.UserName) && !message.UserName.StartsWith("@"))
                {
                    throw new Exception("user must start with @");
                }
                if (!string.IsNullOrEmpty(message.UserName))
                    message.UserName = message.UserName.Replace("@", "");

                // ساخت کلید کش بر اساس پارامترهای ورودی
                var cacheKey = $"GetAllRooms_{message.Query ?? "null"}_{message.UserName ?? "null"}";

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
                            ((message.UserName != null && message.UserName == e.OwnerTBL.Username) || (string.IsNullOrEmpty(message.UserName)))
                        )
                        .AsSplitQuery()
                        .Select(e => new
                        {
                            e.FriendlyID,
                            e.IsSubscribed,
                            e.Avatar,
                            e.Title,
                            e.Logo,
                            e.DateInsert,
                            e.NajmID,
                            e.NajmIDBoss,
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
                        IsSubscribed = room.IsSubscribed,
                        Title = room.Title,
                        Created = room.DateInsert,
                        Avatar = room.Logo,
                        NajmID = room.NajmID,
                        NajmIDBoss = room.NajmIDBoss,
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
                        NextPlan = room.plan.Any(e => e.StartDate > DateTime.Now) ? room.plan.Where(e => e.StartDate > DateTime.Now).Select(e => new MiniPlanJSON
                        {
                            Dabir = e.Dabir,
                            Id = e.ID,
                            StartAt = e.StartDate,
                            Title = e.Title,
                            Teacher = e.Teachers
                        }).First() : null,
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

                outputPort.Handle(new GenericResponse<GetAllRoomsResponse>(new GetAllRoomsResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = result
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Statics.Log("GetAllRooms", e);

                // در صورت خطا، پاسخ خطا برگردانید
                outputPort.Handle(new GenericResponse<GetAllRoomsResponse>(new GetAllRoomsResponse
                {
                    Message = "خطا در دریافت اطلاعات",
                    Status = 500,
                    Data = null
                }));
            }
        }
    }
    #endregion

    #region GetAllLives
    public class GetAllLivesUseCase : IGetAllLivesUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private IScpUploader IScpUploader { get; }
        public GetAllLivesUseCase(IDBContext repository,
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


        public async Task handleAsync(GetAllLivesRequest message,
            IOutPutPort<GenericResponse<GetAllLivesResponse>> outputPort)
        {

            var meets = await IBBBGetMeetings.RequestAll(message);
            foreach (var m in meets)
            {
                var r = _db.RoomTBLs.First(e => e.BBB_MeetID == m.BBB_MeetID);
                m.RoomID = r.FriendlyID;
            }
            //var r = await IBBBGetMeetings.GetRecordingsByMeetingId("1khe4quugbx7ziilyojrvapwebknxbn1jydcjozl");

            //var resp = IScpUploader.ExecuteRemoteCommand("ls -lht");


            outputPort.Handle(new GenericResponse<GetAllLivesResponse>(new GetAllLivesResponse
            {
                Message = "ok",
                Status = 200,
                IsLive = meets.Any(),
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

    #region GetAllRoomsMini
    public class GetAllRoomsMiniUseCase : IGetAllRoomsMiniUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IMemoryCache _cache { get; }

        public GetAllRoomsMiniUseCase(IDBContext repository,
            IMemoryCache _cache,
            IConfiguration configuration)
        {
            _db = repository;
            this._cache = _cache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetAllRoomsMiniRequest message,
        IOutPutPort<GenericResponse<GetAllRoomsMiniResponse>> outputPort)
        {
            try
            {
                if (message.GeneralHeaders.UserTBL == null)
                    return;
                if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_users"))
                {
                    throw new Exception("FORBIDDEN ! 403");
                }
                outputPort.Handle(new GenericResponse<GetAllRoomsMiniResponse>(new GetAllRoomsMiniResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = _db.RoomTBLs.Include(e => e.OwnerTBL).Select(e => new MicroRoomJSON
                    {
                        BBBMeetID = e.BBB_MeetID,
                        FriendlyID = e.FriendlyID,
                        Owner = new MiniUserJSON
                        {
                            Fullname = e.OwnerTBL.GetName(),
                            ID = e.OwnerTBL.ID
                        },
                        Title = e.Title
                    }).ToList()
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Statics.Log("GetAllRoomsMini", e);

                // در صورت خطا، پاسخ خطا برگردانید
                outputPort.Handle(new GenericResponse<GetAllRoomsMiniResponse>(new GetAllRoomsMiniResponse
                {
                    Message = "خطا در دریافت اطلاعات",
                    Status = 500,
                    Data = null
                }));
            }
        }
    }
    #endregion

    #region AddPollQuiz
    public class AddPollQuizUseCase : IAddPollQuizUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IBBBGetMeetings IBBBGetMeetings { get; }
        private readonly IMediator _mediator;
        private IScpUploader IScpUploader { get; }
        public AddPollQuizUseCase(IDBContext repository,
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


        public async Task handleAsync(AddPollQuizRequest message,
     IOutPutPort<GenericResponse<AddPollQuizResponse>> outputPort)
        {
            if (message.GeneralHeaders.UserTBL == null)
                return;

            // بررسی دسترسی ادمین برای ایجاد/ویرایش اتاق برای دیگران
            if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("edit_poll"))
            {
                throw new Exception("FORBIDDEN ! 403");
            }

            // بررسی وجود کاربر هدف (صاحب اتاق)s
            var poll = new MeetPollQuizTBL
            {
                DateInsert = DateTime.Now,
                IDMeetingTBL = null,
                IsForAll = true,
                Question = message.Question,
                Status = MainEntity_Status.LOG_NORMAL,
                Type = message.Type,
            };
            _db.MeetPollQuizTBLs.Add(poll);
            _db.SaveChanges();
            foreach (var i in message.AnsPollRequestJSONs)
            {
                _db.MeetPollQuizAnsTBLs.Add(new MeetPollQuizAnsTBL
                {
                    Answer = i.Content,
                    IDQuizTBL = poll.ID,
                    Type = poll.Type,
                    Status = poll.Status,
                    Score = i.Score,
                });
            }
            _db.SaveChanges();
            outputPort.Handle(new GenericResponse<AddPollQuizResponse>(new AddPollQuizResponse
            {
                Message = "ok",
                Status = 200,

            }));
        }
    }

    #endregion

    #region GetPollListAdmin
    public class GetPollListAdminUseCase : IGetPollListAdminUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;
        public GetPollListAdminUseCase(IDBContext repository,
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

        public async Task handleAsync(GetPollListAdminRequest message,
            IOutPutPort<GenericResponse<GetPollListAdminResponse>> outputPort)
        {
            try
            {
                if (message.GeneralHeaders.UserTBL == null)
                    return;

                // بررسی دسترسی ادمین برای ایجاد/ویرایش اتاق برای دیگران
                if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("edit_poll"))
                {
                    throw new Exception("FORBIDDEN ! 403");
                }


                var q = _db.MeetPollQuizTBLs.Include(e => e.MeetPollQuizAnsTBLs).Select(e => new PollJSON
                {
                    ID = e.ID,
                    Question = e.Question,
                    Status = e.Status,
                    Type = e.Type,
                    Answers = e.MeetPollQuizAnsTBLs != null & e.MeetPollQuizAnsTBLs.Count > 0 ? e.MeetPollQuizAnsTBLs.Select(r => new PoolAnsJSON
                    {
                        Content = r.Answer,
                        ID = r.ID
                    }).ToList() : new List<PoolAnsJSON>(),
                }).ToList();

                outputPort.Handle(new GenericResponse<GetPollListAdminResponse>(new GetPollListAdminResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = q
                }));
            }
            catch (Exception e)
            {
                Statics.Log("GetPollListAdminUseCase", e);
                outputPort.Handle(new GenericResponse<GetPollListAdminResponse>(new GetPollListAdminResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = []
                }));
            }
        }
    }
    #endregion

    #region GetPollReport
    public class GetPollReportUseCase : IGetPollReportUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IMediator _mediator;

        public GetPollReportUseCase(IDBContext repository,
            IConfiguration configuration,
            IMediator mediator,
            IMemoryCache memoryCache)
        {
            _db = repository;
            _configuration = configuration;
            _mediator = mediator;
            _memoryCache = memoryCache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetPollReportRequest message,
    IOutPutPort<GenericResponse<GetPollReportResponse>> outputPort)
        {
            try
            {
                var user = message.GeneralHeaders?.UserTBL;
                var meet = await _db.MeetingTBLs.FirstOrDefaultAsync(e => e.MeetID == message.MeetID);

                if (user == null || meet == null || !user.VerifyUserPermissions("view_poll_reports"))
                {
                    outputPort.Handle(new GenericResponse<GetPollReportResponse>(new GetPollReportResponse
                    {
                        Message = "دسترسی ندارید",
                        Status = 400
                    }));
                    return;
                }

                // Get all users in this meeting
                var allMeetingUsers = _db.UserTBLToMeetingTBLs
                    .Where(e => e.MeetID == meet.MeetID && e.Status == MainEntity_Status.LOG_NORMAL)
                    .Select(e => new { e.ID, e.UUID })
                    .ToList();

                int totalParticipants = allMeetingUsers.Count;

                // **اصلاح شده:** فقط پرسش‌هایی که پاسخ مربوط به این میت دارند
                var pollQuery = _db.MeetPollQuizTBLs
                    .Where(e => e.MeetPollQuizAnsTBLs
                        .Any(r => r.MeetPollQuizAnsTBLToUserMeetTBLs
                            .Any(f => f.UserTBLToMeetingTBL.MeetID == meet.MeetID)));

                // Filter by QID if provided
                if (message.QID.HasValue && message.QID.Value > 0)
                {
                    pollQuery = pollQuery.Where(e => e.ID == message.QID.Value);
                }

                // **اصلاح شده:** Include کامل با فیلتر در ThenInclude
                var polls = pollQuery
                    .Include(e => e.MeetPollQuizAnsTBLs)
                        .ThenInclude(e => e.MeetPollQuizAnsTBLToUserMeetTBLs)
                            .ThenInclude(e => e.UserTBLToMeetingTBL)
                    .ToList();

                // **مرحله مهم:** فیلتر کردن پاسخ‌ها در حافظه برای فقط همین میت
                foreach (var poll in polls)
                {
                    foreach (var answer in poll.MeetPollQuizAnsTBLs)
                    {
                        // فقط پاسخ‌هایی که UserTBLToMeetingTBL آنها null نباشد و MeetID برابر باشد
                        answer.MeetPollQuizAnsTBLToUserMeetTBLs = answer.MeetPollQuizAnsTBLToUserMeetTBLs?
                            .Where(a => a.UserTBLToMeetingTBL != null &&
                                        a.UserTBLToMeetingTBL.MeetID == meet.MeetID)
                            .ToList() ?? new List<MeetPollQuizAnsTBLToUserMeetTBL>();
                    }
                }

                var reportData = new List<PollReportJSON>();

                foreach (var poll in polls)
                {
                    var pollReport = new PollReportJSON
                    {
                        ID = poll.ID,
                        Question = poll.Question,
                        Type = poll.Type,
                        AnswersReport = new List<AnswerReportJSON>()
                    };

                    // **اصلاح شده:** فیلتر بر اساس UserID با در نظر گرفتن null بودن UserTBLToMeetingTBL
                    var answersForThisPoll = poll.MeetPollQuizAnsTBLs;

                    if (!string.IsNullOrEmpty(message.UserID) && _db.UserTBLs.Any(e => e.UUID == message.UserID))
                    {
                        var targetUserMeet = allMeetingUsers.FirstOrDefault(u => u.UUID == message.UserID);
                        if (targetUserMeet != null)
                        {
                            foreach (var answer in answersForThisPoll)
                            {
                                // فقط پاسخ‌های کاربر مورد نظر و مربوط به این میت
                                answer.MeetPollQuizAnsTBLToUserMeetTBLs = answer.MeetPollQuizAnsTBLToUserMeetTBLs?
                                    .Where(a => a.IDUserTBLToMeetingTBL == targetUserMeet.ID &&
                                                a.UserTBLToMeetingTBL != null &&
                                                a.UserTBLToMeetingTBL.MeetID == meet.MeetID)
                                    .ToList() ?? new List<MeetPollQuizAnsTBLToUserMeetTBL>();
                            }
                        }
                    }

                    // Process based on type - بقیه کد شما به همان شکل می‌ماند
                    if (poll.Type == MainEntity_Types.QUIZ_OPTIONAL || poll.Type == MainEntity_Types.QUIZ_OPTIONAL_NOSCORE)
                    {
                        int totalAnswersForPoll = 0;

                        foreach (var answer in answersForThisPoll)
                        {
                            var userAnswers = answer.MeetPollQuizAnsTBLToUserMeetTBLs?
                                .Where(a => (a.Status == MainEntity_Status.Default || a.Status == MainEntity_Status.LOG_NORMAL) &&
                                            a.UserTBLToMeetingTBL != null &&
                                            a.UserTBLToMeetingTBL.MeetID == meet.MeetID)
                                .ToList() ?? new List<MeetPollQuizAnsTBLToUserMeetTBL>();

                            totalAnswersForPoll += userAnswers.Count;
                        }

                        foreach (var answer in answersForThisPoll)
                        {
                            var userAnswers = answer.MeetPollQuizAnsTBLToUserMeetTBLs?
                                .Where(a => (a.Status == MainEntity_Status.Default || a.Status == MainEntity_Status.LOG_NORMAL) &&
                                            a.UserTBLToMeetingTBL != null &&
                                            a.UserTBLToMeetingTBL.MeetID == meet.MeetID)
                                .ToList() ?? new List<MeetPollQuizAnsTBLToUserMeetTBL>();

                            int answerCount = userAnswers.Count;
                            double percentage = totalAnswersForPoll > 0 ? (double)answerCount / totalAnswersForPoll * 100 : 0;

                            double? averageScore = null;
                            if (poll.Type == MainEntity_Types.QUIZ_OPTIONAL && answer.Score > 0)
                            {
                                var scores = userAnswers.Select(u => (double?)answer.Score).ToList();
                                averageScore = scores.Any() ? scores.Average() : 0;
                            }

                            pollReport.AnswersReport.Add(new AnswerReportJSON
                            {
                                ID = answer.ID,
                                Answer = answer.Answer,
                                Score = answer.Score,
                                Count = answerCount,
                                Percentage = Math.Round(percentage, 2),
                                AverageScore = averageScore.HasValue ? Math.Round(averageScore.Value, 2) : (double?)null,
                                UserAnswers = userAnswers.Select(u => new UserAnswerDetailJSON
                                {
                                    UserMeetID = u.IDUserTBLToMeetingTBL ?? 0,
                                    Content = u.Content,
                                    DateInsert = u.DateInsert,
                                    NickName = u.UserTBLToMeetingTBL.UserNickName

                                }).ToList()
                            });
                        }

                        if (poll.Type == MainEntity_Types.QUIZ_OPTIONAL)
                        {
                            var allScores = new List<double>();
                            foreach (var answer in answersForThisPoll)
                            {
                                var userAnswersCount = answer.MeetPollQuizAnsTBLToUserMeetTBLs?
                                    .Where(a => (a.Status == MainEntity_Status.Default || a.Status == MainEntity_Status.LOG_NORMAL) &&
                                                a.UserTBLToMeetingTBL != null &&
                                                a.UserTBLToMeetingTBL.MeetID == meet.MeetID)
                                    .Count() ?? 0;

                                for (int i = 0; i < userAnswersCount; i++)
                                {
                                    allScores.Add(answer.Score);
                                }
                            }
                            pollReport.OverallAverageScore = allScores.Any() ? Math.Round(allScores.Average(), 2) : 0;
                        }
                    }
                    else if (poll.Type == MainEntity_Types.QUIZ_INPUT)
                    {
                        foreach (var answer in answersForThisPoll)
                        {
                            var userAnswers = answer.MeetPollQuizAnsTBLToUserMeetTBLs?
                                .Where(a => (a.Status == MainEntity_Status.Default || a.Status == MainEntity_Status.LOG_NORMAL) &&
                                            a.UserTBLToMeetingTBL != null &&
                                            a.UserTBLToMeetingTBL.MeetID == meet.MeetID)
                                .ToList() ?? new List<MeetPollQuizAnsTBLToUserMeetTBL>();

                            pollReport.AnswersReport.Add(new AnswerReportJSON
                            {
                                ID = answer.ID,
                                Answer = answer.Answer,
                                Score = answer.Score,
                                Count = userAnswers.Count,
                                Percentage = null,
                                AverageScore = null,
                                UserAnswers = userAnswers.Select(u => new UserAnswerDetailJSON
                                {
                                    UserMeetID = u.IDUserTBLToMeetingTBL ?? 0,
                                    Content = u.Content,
                                    NickName = u.UserTBLToMeetingTBL.UserNickName,
                                    DateInsert = u.DateInsert,
                                }).ToList()
                            });
                        }
                    }

                    reportData.Add(pollReport);
                }

                outputPort.Handle(new GenericResponse<GetPollReportResponse>(new GetPollReportResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = reportData,
                    TotalParticipants = totalParticipants
                }));
            }
            catch (Exception e)
            {
                Statics.Log("GetPollReportUseCase", e);
                outputPort.Handle(new GenericResponse<GetPollReportResponse>(new GetPollReportResponse
                {
                    Message = "خطا در دریافت گزارش",
                    Status = 400,
                    Data = new List<PollReportJSON>()
                }));
            }
        }
    }
    #endregion

    #region GetAllMeeting
    public class GetAllMeetingUseCase : IGetAllMeetingUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IMemoryCache _cache { get; }

        public GetAllMeetingUseCase(IDBContext repository,
            IMemoryCache _cache,
            IConfiguration configuration)
        {
            _db = repository;
            this._cache = _cache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetAllMeetingRequest message,
        IOutPutPort<GenericResponse<GetAllMeetingResponse>> outputPort)
        {
            try
            {
                if (message.GeneralHeaders.UserTBL == null)
                    return;
                if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_meeting"))
                {
                    throw new Exception("FORBIDDEN ! 403");
                }
                var meetings = await _db.MeetingTBLs.Where(e => e.RoomTBL.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID ||
                    e.RoomTBL.RoomTBLToUserTBLs.Any(r => r.ParticipatorTBL.ID == message.GeneralHeaders.UserTBL.ID))
                    .Include(e => e.RoomTBL).ThenInclude(e => e.OwnerTBL)
                    .Include(e => e.Teacher)
                    .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(r => r.UserTBL)
                    .OrderByDescending(e => e.ID)
                    .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value)
                    .ToListAsync();

                var resultList = meetings.Select(e =>
                {
                    var teachers = new List<MiniUserJSON>();

                    if (e.Teacher != null)
                    {
                        teachers.Add(new MiniUserJSON
                        {
                            ID = e.Teacher.ID,
                            Fullname = e.Teacher.GetName(),
                            Firstname = e.Teacher.Firstname,
                            LastName = e.Teacher.LastName,
                            Username = e.Teacher.Username,
                            AvatarPath = e.Teacher.AvatarPath,
                            UID = e.Teacher.UUID
                        });
                    }

                    if (e.UserTBLToMeetingTBLs != null)
                    {
                        var joinedTeachers = e.UserTBLToMeetingTBLs
                            .Where(r => r.Type == MainEntity_Types.USER_TEACHER && r.UserTBL != null)
                            .Select(r => new MiniUserJSON
                            {
                                ID = r.UserTBL.ID,
                                Fullname = r.UserTBL.GetName(),
                                Firstname = r.UserTBL.Firstname,
                                LastName = r.UserTBL.LastName,
                                Username = r.UserTBL.Username,
                                AvatarPath = r.UserTBL.AvatarPath,
                                UID = r.UserTBL.UUID
                            });

                        teachers.AddRange(joinedTeachers);
                    }

                    var uniqueTeachers = teachers.GroupBy(t => t.ID).Select(g => g.First()).ToList();

                    return new MiniMeet
                    {
                        Title = e.Title,
                        Time = $"{e.DateInsert.ToString("HH:mm")}",
                        Date = $"{e.JalaliDate}",
                        MeetID = e.MeetID,
                        ID = e.ID,
                        EndDateTime = e.EndDate.HasValue ? e.EndDate.Value : DateTime.MinValue,
                        PartiCount = e.UserTBLToMeetingTBLs?.Count() ?? 0,
                        TeacherList = uniqueTeachers,
                        Room = new MicroRoomJSON
                        {
                            FriendlyID = e.RoomFriendlyID,
                            Owner = e.RoomTBL?.OwnerTBL != null ? new MiniUserJSON
                            {
                                Fullname = e.RoomTBL.OwnerTBL.GetName(),
                                ID = e.RoomTBL.OwnerTBL.ID
                            } : null,
                            Title = e.RoomTBL?.Title,
                        }
                    };
                }).ToList();

                outputPort.Handle(new GenericResponse<GetAllMeetingResponse>(new GetAllMeetingResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = resultList
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Statics.Log("GetAllMeeting", e);

                // در صورت خطا، پاسخ خطا برگردانید
                outputPort.Handle(new GenericResponse<GetAllMeetingResponse>(new GetAllMeetingResponse
                {
                    Message = "خطا در دریافت اطلاعات",
                    Status = 500,
                    Data = null
                }));
            }
        }
    }
    #endregion

    #region GetUserMeetLogs
    public class GetUserMeetLogsUseCase : IGetUserMeetLogsUseCase
    {
        public IDBContext _db { get; }
        public CommonMethods _commonMethods { get; }
        private IWebHostEnvironment hosting;
        private IMemoryCache _cache { get; }

        public GetUserMeetLogsUseCase(IDBContext repository,
            IMemoryCache _cache,
            IConfiguration configuration)
        {
            _db = repository;
            this._cache = _cache;
            _commonMethods = new CommonMethods(configuration, repository, hosting);
        }

        public async Task handleAsync(GetUserMeetLogsRequest message,
        IOutPutPort<GenericResponse<GetUserMeetLogsResponse>> outputPort)
        {
            try
            {
                if (message.GeneralHeaders.UserTBL == null)
                    return;
                if (!message.GeneralHeaders.UserTBL.VerifyUserPermissions("create_meeting"))
                {
                    throw new Exception("FORBIDDEN ! 403");
                }
                if(!_db.UserTBLToMeetingTBLs.Any(e=>e.ID==message.UserMeetID ))
                {
                    throw new Exception("FORBIDDEN ! 404");
                }
                if(!message.GeneralHeaders.UserTBL.VerifyUserPermissions("manage_admins") 
                    && !_db.UserTBLToMeetingTBLs.Any(e=>e.MeetingTBL.RoomTBL.IDOwnerTBL == message.GeneralHeaders.UserTBL.ID))
                {
                    throw new Exception("FORBIDDEN ! 403");

                }
                outputPort.Handle(new GenericResponse<GetUserMeetLogsResponse>(new GetUserMeetLogsResponse
                {
                    Message = "ok",
                    Status = 200,
                    Data = _db.UserTBLToMeetingTBLLogs.Where(e => e.IDUserTBLToMeetingTBL == message.UserMeetID)
                    .Include(e => e.UserTBLToMeetingTBL).ThenInclude(e => e.UserTBL).OrderBy(e => e.DateInsert)
                    .Skip(message.Count.Value * message.Page.Value).Take(message.Count.Value)
                    .Select(e => new MiniLogJSON
                    {
                        ID = e.ID,
                        User = new MicroUserJSON
                        {
                            ID=e.UserTBLToMeetingTBL.UserTBL.ID,
                            Fullname=e.UserTBLToMeetingTBL.UserTBL.GetName()
                        },
                        Content=Statics.GetContentFromLog(e.Key,e.Value),
                        Date =e.DateInsert,
                        Key=e.Key,
                        IP=e.OS,
                        Value=e.Value
                    }).ToList(),
                    TotalCount= _db.UserTBLToMeetingTBLLogs.Count(e => e.IDUserTBLToMeetingTBL == message.UserMeetID)
                }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Statics.Log("GetUserMeetLogs", e);

                // در صورت خطا، پاسخ خطا برگردانید
                outputPort.Handle(new GenericResponse<GetUserMeetLogsResponse>(new GetUserMeetLogsResponse
                {
                    Message = "خطا در دریافت اطلاعات",
                    Status = 500,
                    Data = null
                }));
            }
        }
    }
    #endregion

}
