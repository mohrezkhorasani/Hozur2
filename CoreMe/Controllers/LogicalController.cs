using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IInfrastructure;
using Application.Interface.IUsecase;
using Application.Interfaces;
using Application.Usecase;
using AutoMapper;
using CoreMe.Presenter;
using CoreMe.VM;
using Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Presenter;
using System.ComponentModel;
using System.Runtime.InteropServices.Marshalling;

namespace CoreMe.Controllers
{
    [ApiController]
    [EnableCors("AllowHekmateSubdomains")]
    [Route("admin")]
    public class LogicalController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<TaskController> _logger;
        private readonly IWebHostEnvironment _hosting;
        public readonly IDBContext _db;
        public readonly IConfiguration configuration;
        public readonly CommonMethods _commonMethods;
        private readonly IBBBGetMeetings bbb;
        public LogicalController(IMapper mapper,
                               ILogger<TaskController> logger,
                               IWebHostEnvironment hosting,
                               IConfiguration configuration,
                               IBBBGetMeetings bbb,
                               IDBContext db)
        {
            _mapper = mapper;
            _logger = logger;
            _hosting = hosting;
            _db = db;
            this.bbb = bbb;
            _commonMethods = new CommonMethods(configuration, _db, hosting);
        }

        #region logical

        [HttpPost("/user/EditMemberList")]
        //[Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> EditMemberList([FromBody] List<EditMemberVM> vm)
        {
            var head = HttpContext.GetGeneralHeaders();

            GetTheMembersRequest message = new GetTheMembersRequest();
            message.GeneralHeaders = head;

            var init = await _commonMethods.Initialize<GetTheMembersRequest>(message);
            var user = init.Item1;
            var meet = init.Item2;
            if (meet == null || user == null)
            {
                return Ok(new
                {
                    Message = "forbidden",
                    Status = 403,
                });
            }

            if (!_db.UserTBLToMeetingTBLs.Any(e => e.UUID == user.UUID && e.MeetID == meet.MeetID) &&
               !user.VerifyUserPermissions("edit_meeting_member"))
            {
                return Ok(new
                {
                    Message = "دسترسی ندارید",
                    Status = 400
                });
            }
            foreach (var i in vm)
            {
                if (_db.UserTBLToMeetingTBLs.Any(e => e.ID == i.UserMeetID))
                {
                    var um = _db.UserTBLToMeetingTBLs.Find(i.UserMeetID);
                    um.IsMember = i.IsMember;
                    um.CodeUnique = Statics.GenerateSecretKey(16);
                    _db.Update(um);
                }
            }
            _db.SaveChanges();
            return Ok(new
            {
                status = 200,
                message = "ok"
            });
        }



        [HttpGet("/room/getDetails")]
        public async Task<IActionResult> GetDetails([FromQuery] string fid)
        {
            if (!_db.RoomTBLs.Any(e => e.FriendlyID == fid))
            {
                return Ok(new
                {
                    message = "not found",
                    status = 404
                });

            }
            var room = _db.RoomTBLs.Include(e => e.OwnerTBL).FirstOrDefault(e => e.FriendlyID == fid);

            var response = new
            {
                title = room.Title,
                host = new
                {
                    id = room.OwnerTBL.ID,
                    name = room.OwnerTBL.GetName()
                },
                bio = room.Bio,
                friendlyID = fid,
                logo = room.Logo,

            };
            return Ok(new
            {
                message = "ok",
                status = 200,
                data = response
            });

        }
        [HttpGet("/room/GetMeetingDetail")]
        public async Task<IActionResult> GetMeetingDetail([FromQuery] string fid)
        {
            if (!_db.MeetingTBLs.Any(e => e.RoomFriendlyID == fid))
            {
                return Ok(new
                {
                    message = "not found",
                    status = 404
                });

            }
            var room = _db.MeetingTBLs.Where(e => e.RoomFriendlyID == fid && e.Type != MainEntity_Types.MEET_IGNORE)
                .Include(e => e.UserTBLToMeetingTBLs).ThenInclude(e => e.UserTBL)
                .Include(e => e.Teacher)
                .Include(e => e.Creator)
                .Include(e => e.MeetPollQuizTBLs).ThenInclude(e => e.MeetPollQuizAnsTBLs)
                .OrderByDescending(e => e.ID)
                .ToList();
            var sumMin = 0;
            var countPartis = 0;
            var countMeet = 0;
            var happyPercentSum = 0;
            var happyPercentCount = 0;
            MeetingTBL lastMeet = null;
            List<UserTBLToMeetingTBL> lastMeetUsers = new List<UserTBLToMeetingTBL>();
            foreach (var e in room)
            {
                if (!e.EndDate.HasValue)
                {
                    e.EndDate = e.DateInsert.AddMinutes((new Random()).Next(90, 120));
                    _db.Update(e);
                    _db.SaveChanges();
                }
                if (e.EndDate.HasValue && (e.EndDate.Value - e.DateInsert).TotalMinutes < 30)
                    continue;

                if (lastMeet == null)
                {
                    lastMeet = e;
                    if (e.UserTBLToMeetingTBLs.Any(r => r.Type == MainEntity_Types.USER_TEACHER))
                    {
                        lastMeetUsers = e.UserTBLToMeetingTBLs.Where(r => r.Type == MainEntity_Types.USER_TEACHER).ToList();
                    }
                    else
                    {
                        lastMeetUsers = new List<UserTBLToMeetingTBL>();
                    }
                }
                countMeet++;
                countPartis += e.UserTBLToMeetingTBLs.Count(e => e.Type == MainEntity_Types.USER_VIEWER);
                sumMin += Math.Abs(((int)(e.EndDate.Value - e.DateInsert).TotalMinutes));

                var quizSumScore = 0;
                var quizCountScore = 0;
                foreach (var i in e.MeetPollQuizTBLs)
                {
                    if (i.MeetPollQuizAnsTBLs.Any() && i.Type == MainEntity_Types.QUIZ_OPTIONAL)
                    {
                        foreach (var j in i.MeetPollQuizAnsTBLs)
                        {
                            quizCountScore++;
                            quizSumScore += j.Score;
                        }
                    }
                }
                happyPercentSum += quizSumScore;
                happyPercentCount += quizCountScore * 100;
            }
            if (happyPercentCount == 0)
                happyPercentCount = 1;
            var happyPercent = (int)((happyPercentSum * 100) / happyPercentCount);

            var response = new
            {
                countPartis = countPartis,
                countMeet = countMeet,
                sumMinutes = sumMin,
                percentage = happyPercent,

                lastMeeting = new
                {
                    id = lastMeet?.ID,
                    teachers = lastMeetUsers.Select(e => new
                    {
                        id = e.UserTBL.ID,
                        name = e.UserTBL.GetName(),
                    }),
                    title = lastMeet?.Title,
                    date = lastMeet?.DateInsert,
                    summary = string.IsNullOrEmpty(lastMeet?.Explain) ? "در این جلسه به تبین نظریه اصالت وجود و اعتباری بودن ماهیت پرداخته شد و پرسش‌های پژوهشگران مورد پاسخگویی قرار گرفت." : lastMeet.Explain,
                },
                friendlyID = fid,

            };
            return Ok(new
            {
                message = "ok",
                status = 200,
                data = response
            });

        }

        [HttpGet("/room/GetTopTeachers")]
        public async Task<IActionResult> GetTopTeachers([FromQuery] string fid)
        {
            if (string.IsNullOrEmpty(fid))
            {
                return Ok(new
                {
                    message = "not found",
                    status = 404
                });
            }

            var validMeetings = _db.MeetingTBLs
                .Where(m => m.RoomFriendlyID == fid && m.Type != MainEntity_Types.MEET_IGNORE);

            var teacherJoins = await validMeetings
                .SelectMany(m => m.UserTBLToMeetingTBLs)
                .Where(u => u.Type == MainEntity_Types.USER_TEACHER && u.UserTBL.Status == MainEntity_Status.USER_ACTIVE)
                .Select(u => new
                {
                    Teacher = u.UserTBL,
                    MeetID = u.MeetID
                })
                .ToListAsync();

            var directTeachers = await validMeetings
                .Where(m => m.IDTeacher != null && m.Teacher != null && m.Teacher.Status == MainEntity_Status.USER_ACTIVE)
                .Select(m => new
                {
                    Teacher = m.Teacher,
                    MeetID = m.MeetID
                })
                .ToListAsync();

            var teachers = teacherJoins
                .Concat(directTeachers)
                .GroupBy(t => new
                {
                    t.Teacher.ID,
                    t.Teacher.UUID,
                    t.Teacher.Firstname,
                    t.Teacher.LastName,
                    t.Teacher.Username,
                    t.Teacher.AvatarPath
                })
                .Select(g => new
                {
                    id = (long?)g.Key.ID,
                    uuid = g.Key.UUID,
                    name = (!string.IsNullOrWhiteSpace(g.Key.Firstname) || !string.IsNullOrWhiteSpace(g.Key.LastName))
                        ? ($"{g.Key.Firstname} {g.Key.LastName}").Trim()
                        : (g.Key.Username ?? ""),
                    firstname = g.Key.Firstname,
                    lastname = g.Key.LastName,
                    avatar = g.Key.AvatarPath,
                    count = g.Select(x => x.MeetID).Distinct().Count()
                })
                .OrderByDescending(x => x.count)
                .ToList();

            if (!teachers.Any())
            {
                return Ok(new
                {
                    message = "not found",
                    status = 404
                });
            }

            return Ok(new
            {
                message = "ok",
                status = 200,
                data = teachers
            });
        }

        [HttpGet("/room/GetRoomMeetingHistory")]
        public async Task<IActionResult> GetRoomMeetingHistory([FromQuery] string fid, [FromQuery] int page = 1, [FromQuery] int count = 10)
        {
            if (page < 1) page = 1;
            if (count < 1) count = 10;
            if (count > 50) count = 50;

            var query = _db.MeetingTBLs
                .Where(e => e.RoomFriendlyID == fid && e.Type != MainEntity_Types.MEET_IGNORE);


            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / count);

            var meetings = await query
                .OrderByDescending(e => e.DateInsert)
                .Skip((page - 1) * count)
                .Take(count)
                .Select(m => new
                {
                    id = m.ID,
                    meetID = m.MeetID,
                    title = m.Title,
                    explain = m.Explain,
                    dateInsert = m.DateInsert,
                    endDate = m.EndDate,
                    jalaliDate = m.JalaliDate,
                    isRecorded = m.IsRecorded,
                    status = (int)m.Status,
                    statusTitle = MeetingTBL.GetStatusString(m.Status),
                    teacher = m.Teacher != null ? new
                    {
                        id = m.Teacher.ID,
                        name = m.Teacher.GetName(),
                        avatar = m.Teacher.AvatarPath
                    } : null,
                    teachers = m.UserTBLToMeetingTBLs
                        .Where(u => u.Type == MainEntity_Types.USER_TEACHER)
                        .Select(u => new
                        {
                            id = u.UserTBL.ID,
                            name = u.UserTBL != null ? u.UserTBL.GetName() : u.UserNickName,
                            avatar = u.UserTBL != null ? u.UserTBL.AvatarPath : null
                        }).ToList(),
                    participantsCount = m.UserTBLToMeetingTBLs.Count(u => u.Type == MainEntity_Types.USER_VIEWER)
                })
                .ToListAsync();

            return Ok(new
            {
                message = "ok",
                status = 200,
                data = new
                {
                    items = meetings,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    page = page,
                    count = count
                }
            });
        }

        [HttpPost("/user/EditUserInfo")]
        public async Task<IActionResult> EditUserInfo([FromBody] EditUserInfoVM model)
        {
            if (model == null || model.UserId <= 0)
            {
                return Ok(new
                {
                    message = "شناسه کاربر معتبر نیست",
                    status = 400
                });
            }

            var user = await _db.UserTBLs.FirstOrDefaultAsync(u => u.ID == model.UserId);
            if (user == null)
            {
                return Ok(new
                {
                    message = "کاربر یافت نشد",
                    status = 404
                });
            }

            if (model.Firstname != null) user.Firstname = model.Firstname;
            if (model.LastName != null) user.LastName = model.LastName;
            if (model.IsHekmatShobe.HasValue) user.IsHekmatShobe = model.IsHekmatShobe.Value;
            if (model.IsMemberVIP.HasValue) user.IsMemberVIP = model.IsMemberVIP.Value;
            if (model.Status.HasValue) user.Status = model.Status.Value;
            if (model.AvatarPath != null) user.AvatarPath = model.AvatarPath;

            _db.UserTBLs.Update(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "اطلاعات کاربر با موفقیت بروزرسانی شد",
                status = 200,
                data = new
                {
                    id = user.ID,
                    firstname = user.Firstname,
                    lastname = user.LastName,
                    isHekmatShobe = user.IsHekmatShobe,
                    isMemberVIP = user.IsMemberVIP,
                    status = (int)user.Status,
                    avatarPath = user.AvatarPath
                }
            });
        }

        #endregion
    }

    public class EditUserInfoVM
    {
        public long UserId { get; set; }
        public string? Firstname { get; set; }
        public string? LastName { get; set; }
        public bool? IsHekmatShobe { get; set; }
        public bool? IsMemberVIP { get; set; }
        public MainEntity_Status? Status { get; set; }
        public string? AvatarPath { get; set; }
    }
}
