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
                    if (um != null)
                    {
                        um.IsMember = i.IsMember;
                        if (i.AttendanceType == "hozuri" || i.IsHozuri == true)
                        {
                            um.Explain = "HOZURI";
                        }
                        else if (i.AttendanceType == "virtual" || i.IsHozuri == false)
                        {
                            um.Explain = "VIRTUAL";
                        }
                        um.CodeUnique = Statics.GenerateSecretKey(16);
                        _db.Update(um);
                    }
                }
                else if (i.UserMeetID <= 0 && (!string.IsNullOrWhiteSpace(i.Firstname) || !string.IsNullOrWhiteSpace(i.NationalCode)))
                {
                    var userEntity = !string.IsNullOrWhiteSpace(i.NationalCode)
                        ? _db.UserTBLs.FirstOrDefault(u => u.NationalCode == i.NationalCode.Trim())
                        : null;

                    if (userEntity == null)
                    {
                        userEntity = new UserTBL
                        {
                            Firstname = i.Firstname?.Trim() ?? "",
                            LastName = i.LastName?.Trim() ?? "",
                            NationalCode = i.NationalCode?.Trim() ?? "",
                            Status = MainEntity_Status.USER_ACTIVE,
                            Type = MainEntity_Types.USER_VIEWER,
                            UUID = Guid.NewGuid().ToString(),
                            DateInsert = DateTime.UtcNow,
                        };
                        _db.UserTBLs.Add(userEntity);
                        _db.SaveChanges();
                    }

                    var newUm = new UserTBLToMeetingTBL
                    {
                        MeetID = meet.MeetID,
                        UUID = userEntity.UUID,
                        UserMeetID = "HOZURI_" + userEntity.ID,
                        UserNickName = $"{i.Firstname} {i.LastName}".Trim(),
                        NationalCode = i.NationalCode?.Trim() ?? "",
                        Role = "VIEWER",
                        IsMember = i.IsMember,
                        Status = MainEntity_Status.USERMEET_JOINED,
                        TokenJWT = "hozuri...",
                        Type = MainEntity_Types.USER_VIEWER,
                        DateInsert = DateTime.UtcNow,
                        Explain = (i.AttendanceType == "virtual" || i.IsHozuri == false) ? "VIRTUAL" : "HOZURI",
                        CodeUnique = Statics.GenerateSecretKey(16)
                    };
                    _db.UserTBLToMeetingTBLs.Add(newUm);
                }
            }
            _db.SaveChanges();
            return Ok(new
            {
                status = 200,
                message = "ok"
            });
        }

        [HttpPost("/user/AddHozuriMember")]
        public async Task<IActionResult> AddHozuriMember([FromBody] AddHozuriMemberVM vm)
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
                    message = "forbidden",
                    status = 403,
                });
            }

            if (!_db.UserTBLToMeetingTBLs.Any(e => e.UUID == user.UUID && e.MeetID == meet.MeetID) &&
               !user.VerifyUserPermissions("edit_meeting_member"))
            {
                return Ok(new
                {
                    message = "دسترسی ندارید",
                    status = 400
                });
            }

            if (string.IsNullOrWhiteSpace(vm.Firstname) || string.IsNullOrWhiteSpace(vm.LastName))
            {
                return Ok(new
                {
                    message = "نام و نام خانوادگی الزامی است",
                    status = 400
                });
            }

            var userEntity = !string.IsNullOrWhiteSpace(vm.NationalCode)
                ? _db.UserTBLs.FirstOrDefault(u => u.NationalCode == vm.NationalCode.Trim())
                : null;

            if (userEntity == null)
            {
                userEntity = new UserTBL
                {
                    Firstname = vm.Firstname.Trim(),
                    LastName = vm.LastName.Trim(),
                    NationalCode = string.IsNullOrWhiteSpace(vm.NationalCode) ? null : vm.NationalCode?.Trim(),
                    Status = MainEntity_Status.USER_ACTIVE,
                    Type = MainEntity_Types.USER_VIEWER,
                    UUID = Guid.NewGuid().ToString(),
                    DateInsert = DateTime.UtcNow,
                };
                _db.UserTBLs.Add(userEntity);
                _db.SaveChanges();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(vm.Firstname)) userEntity.Firstname = vm.Firstname.Trim();
                if (!string.IsNullOrWhiteSpace(vm.LastName)) userEntity.LastName = vm.LastName.Trim();
                _db.UserTBLs.Update(userEntity);
                _db.SaveChanges();
            }

            var existingUserMeet = _db.UserTBLToMeetingTBLs.FirstOrDefault(e => e.UUID == userEntity.UUID && e.MeetID == meet.MeetID);
            if (existingUserMeet != null)
            {
                existingUserMeet.IsMember = true;
                existingUserMeet.Explain = "HOZURI";
                if (!string.IsNullOrWhiteSpace(vm.NationalCode))
                {
                    existingUserMeet.NationalCode = vm.NationalCode.Trim();
                }
                _db.Update(existingUserMeet);
                _db.SaveChanges();

                return Ok(new
                {
                    status = 200,
                    message = "ok",
                    data = new
                    {
                        id = userEntity.ID,
                        userMeetID = existingUserMeet.ID,
                        fullname = $"{userEntity.Firstname} {userEntity.LastName}".Trim(),
                        isVIPMember = existingUserMeet.IsMember,
                        isRegistered = !string.IsNullOrEmpty(userEntity.Email) || !string.IsNullOrEmpty(userEntity.NationalCode),
                        isHozuri = true,
                        attendanceType = "hozuri",
                        nationalCode = userEntity.NationalCode
                    }
                });
            }

            var newUm = new UserTBLToMeetingTBL
            {
                MeetID = meet.MeetID,
                UUID = userEntity.UUID,
                UserMeetID = "HOZURI_" + userEntity.ID,
                UserNickName = $"{userEntity.Firstname} {userEntity.LastName}".Trim(),
                NationalCode = vm.NationalCode?.Trim() ?? "",
                Role = "VIEWER",
                TokenJWT = "hozuri...",
                IsMember = true,
                Status = MainEntity_Status.USERMEET_JOINED,
                Type = MainEntity_Types.USER_HOZURI,
                DateInsert = DateTime.UtcNow,
                Explain = "HOZURI",
                CodeUnique = Statics.GenerateSecretKey(16)
            };
            _db.UserTBLToMeetingTBLs.Add(newUm);
            _db.SaveChanges();

            return Ok(new
            {
                status = 200,
                message = "ok",
                data = new
                {
                    id = userEntity.ID,
                    userMeetID = newUm.ID,
                    fullname = $"{userEntity.Firstname} {userEntity.LastName}".Trim(),
                    isVIPMember = newUm.IsMember,
                    isRegistered = !string.IsNullOrEmpty(userEntity.Email) || !string.IsNullOrEmpty(userEntity.NationalCode),
                    isHozuri = true,
                    attendanceType = "hozuri",
                    nationalCode = userEntity.NationalCode
                }
            });
        }



        [HttpGet("/room/getDetails")]
        public async Task<IActionResult> GetDetails([FromQuery] string fid)
        {
            var head = HttpContext.GetGeneralHeaders();
            if (!_db.RoomTBLs.Any(e => e.FriendlyID == fid))
            {
                return Ok(new
                {
                    message = "not found",
                    status = 404
                });
            }

            // --- گرفتن اطلاعات کاربر از هدرها ---
            long userId = 0;
            string uuid = "";
            try
            {
                var userIdHeader = head.UserTBL?.ID;
                var uuidHeader = head.UserTBL?.UUID;
                userId = userIdHeader.Value ;

                uuid = uuidHeader ?? "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine(uuid);

            var room = await _db.RoomTBLs
                .Include(e => e.OwnerTBL)
                .Include(e => e.RoomTBLToUserTBLs)
                .Include(e => e.RoomMetaTBLs)
                .FirstOrDefaultAsync(e => e.FriendlyID == fid);

            if (room == null)
                return Ok(new { message = "not found", status = 404 });

            // --- پیدا کردن اشتراک کاربر ---
            var userSub = userId > 0
                ? room.RoomTBLToUserTBLs.FirstOrDefault(r => r.UUIDParticipatorTBL == uuid && r.Status == MainEntity_Status.Enabled)
                : null;

            // --- محاسبه isAdmin ---
            var isAdmin = userId > 0 && (
                room.IDOwnerTBL == userId ||
                room.RoomTBLToUserTBLs.Any(r =>
                    r.UUIDParticipatorTBL == uuid &&
                    r.Type == MainEntity_Types.ROOMTOUSER_ADMIN)
            );

            var now = DateTime.UtcNow;
            var upcomingPlans = await _db.RoomTBLPlanTBLs
                .Where(p => p.IDRoomTBL == room.ID
                         && (p.Status == MainEntity_Status.Enabled || p.Status == MainEntity_Status.MEETING_STARTED_LATER)
                         && p.StartDate.HasValue
                         && p.StartDate.Value >= now.AddMinutes(-30))
                .OrderBy(p => p.StartDate)
                .Select(p => new
                {
                    id = p.ID,
                    title = p.Title,
                    teachers = p.Teachers,
                    dabir = p.Dabir,
                    startDate = p.StartDate,
                    explain = p.Explain,
                    requiredToEnterName = p.RequiredToEnterName,
                    presentationFileURL = p.PresentationFileURL
                })
                .ToListAsync();

            var response = new
            {
                title = room.Title,
                host = new
                {
                    id = room.OwnerTBL != null ? room.OwnerTBL.ID : 0,
                    name = room.OwnerTBL != null ? room.OwnerTBL.GetName() : ""
                },
                bio = room.Bio,
                friendlyID = fid,
                logo = room.Logo,
                najmID = room.NajmID,
                isSubscribedByUser = userSub != null,
                subscriptionStatus = userSub != null ? (int?)userSub.Status : null,
                isAdmin = isAdmin,
                IsSubscribed = room.IsSubscribed,
                IsPrivateRoom = room.IsPrivateRoom,
                coverImage = room.RoomMetaTBLs.FirstOrDefault(e => e.Key == MetaConfigKeys.ImageCover).Value,
                description = room.Bio,
                najmIDBoss = room.NajmIDBoss,
                upcomingPlans = upcomingPlans
            };

            return Ok(new
            {
                message = "ok" + JsonConvert.SerializeObject(head),
                status = 200,
                data = response,
            });
        }

        #region Najm Boss Integration
        private static (string Token, DateTime ExpiresAt) _cachedNajmToken;
        private static readonly SemaphoreSlim _najmTokenLock = new SemaphoreSlim(1, 1);

        private async Task<string?> GetNajmApiTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedNajmToken.Token) && DateTime.UtcNow < _cachedNajmToken.ExpiresAt.AddSeconds(-10))
            {
                return _cachedNajmToken.Token;
            }

            await _najmTokenLock.WaitAsync();
            try
            {
                if (!string.IsNullOrEmpty(_cachedNajmToken.Token) && DateTime.UtcNow < _cachedNajmToken.ExpiresAt.AddSeconds(-10))
                {
                    return _cachedNajmToken.Token;
                }

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                var response = await client.GetAsync("http://192.168.0.4:1940/api/token/generate?minutes=1");
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                {
                    string token = tokenProp.GetString() ?? "";
                    int minutes = 1;
                    if (doc.RootElement.TryGetProperty("expiresInMinutes", out var expProp))
                    {
                        minutes = expProp.GetInt32();
                    }
                    _cachedNajmToken = (token, DateTime.UtcNow.AddMinutes(minutes));
                    return token;
                }
                return null;
            }
            catch (Exception ex)
            {
                Statics.Log("GetNajmApiToken", ex);
                return null;
            }
            finally
            {
                _najmTokenLock.Release();
            }
        }

        private async Task<List<NajmBossItem>?> FetchNajmBossesFromApiAsync()
        {
            try
            {
                string? token = await GetNajmApiTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
                using var request = new HttpRequestMessage(HttpMethod.Get, "http://192.168.0.4:1940/api/groups/bosses");
                request.Headers.Add("X-Api-Token", token);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    // If failed/expired, reset cached token and retry once
                    _cachedNajmToken = (string.Empty, DateTime.MinValue);
                    token = await GetNajmApiTokenAsync();
                    if (!string.IsNullOrEmpty(token))
                    {
                        using var retryReq = new HttpRequestMessage(HttpMethod.Get, "http://192.168.0.4:1940/api/groups/bosses");
                        retryReq.Headers.Add("X-Api-Token", token);
                        response = await client.SendAsync(retryReq);
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var list = new List<NajmBossItem>();
                System.Text.Json.JsonElement dataEl = doc.RootElement;

                if (dataEl.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    dataEl.TryGetProperty("Data", out var dataProp))
                {
                    dataEl = dataProp;
                }
                if (dataEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in dataEl.EnumerateArray())
                    {
                        string id = item.TryGetProperty("NajmId", out var idProp) ? idProp.ToString() : "";
                        string fn = item.TryGetProperty("FirstName", out var fnProp) ? fnProp.GetString() ?? "" : "";
                        string ln = item.TryGetProperty("Lastname", out var lnProp) ? lnProp.GetString() ?? "" : "";
                        string full = item.TryGetProperty("FullName", out var fullProp) ? fullProp.GetString() ?? "" : "";
                        string nc = item.TryGetProperty("NationalCode", out var ncProp) ? ncProp.GetString() ?? "" : "";
                        string bbb = item.TryGetProperty("bbbID", out var bbbProp) ? bbbProp.GetString() ?? "" : "";
                        if (string.IsNullOrWhiteSpace(fn) && string.IsNullOrWhiteSpace(ln) && string.IsNullOrWhiteSpace(full))
                            continue;
                        list.Add(new NajmBossItem
                        {
                            NajmId = id,
                            FirstName = fn,
                            Lastname = ln,
                            FullName = full,
                            NationalCode = nc,
                            BbbID = bbb
                        });
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                Statics.Log("FetchNajmBossesFromApi", ex);
                return null;
            }
        }

        [HttpGet("/room/GetNajmBosses")]
        [HttpGet("/user/GetNajmBosses")]
        public async Task<IActionResult> GetNajmBosses([FromQuery] string? q = null)
        {
            var bosses = await FetchNajmBossesFromApiAsync();
            if (bosses == null)
            {
                return StatusCode(502, new { status = 502, message = "خطا در ارتباط با سامانه نجم یا دریافت لیست مسئولین" });
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                bosses = bosses.Where(b =>
                    (!string.IsNullOrEmpty(b.FullName) && b.FullName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(b.FirstName) && b.FirstName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(b.Lastname) && b.Lastname.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                    b.NajmId.Equals(q, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(new
            {
                status = 200,
                message = "ok",
                data = bosses,
                count = bosses.Count
            });
        }

        [HttpPost("/room/SetRoomNajmBoss")]
        [HttpPost("/user/SetRoomNajmBoss")]
        public async Task<IActionResult> SetRoomNajmBoss([FromBody] SetRoomNajmBossVM? vm)
        {
            string friendlyId = vm?.FriendlyID ?? Request.Query["FriendlyID"].ToString();
            string? najmId = vm?.NajmId ?? Request.Query["NajmId"].ToString();

            if (string.IsNullOrWhiteSpace(friendlyId) && Request.HasFormContentType)
            {
                friendlyId = Request.Form["FriendlyID"].ToString();
                if (string.IsNullOrWhiteSpace(najmId))
                {
                    najmId = Request.Form["NajmId"].ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(friendlyId))
            {
                return BadRequest(new { status = 400, message = "شناسه اتاق (FriendlyID) الزامی است." });
            }

            var room = await _db.RoomTBLs.FirstOrDefaultAsync(r => r.FriendlyID == friendlyId);
            if (room == null)
            {
                return NotFound(new { status = 404, message = "اتاق مورد نظر یافت نشد." });
            }

            // If najmId is provided, verify against Najm bosses API with fresh token
            string matchedBossName = "";
            if (!string.IsNullOrWhiteSpace(najmId) && najmId != "0")
            {
                var bosses = await FetchNajmBossesFromApiAsync();
                if (bosses == null)
                {
                    return StatusCode(502, new { status = 502, message = "خطا در دریافت لیست مسئولین از سامانه نجم برای اعتبارسنجی." });
                }

                var matched = bosses.FirstOrDefault(b => b.NajmId.Equals(najmId.Trim(), StringComparison.OrdinalIgnoreCase));
                if (matched == null)
                {
                    return BadRequest(new { status = 400, message = $"شناسه نجم ({najmId}) در لیست مسئولین سامانه نجم یافت نشد." });
                }

                matchedBossName = matched.FullName;
                room.NajmIDBoss = matched.NajmId;
                room.NajmID = matched.NajmId;
            }
            else
            {
                // Clear assignment
                room.NajmIDBoss = null;
                room.NajmID = null;
            }

            room.UpdatedAt = DateTime.UtcNow;
            _db.RoomTBLs.Update(room);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                status = 200,
                message = "مسئول اتاق در سامانه نجم با موفقیت ذخیره شد.",
                data = new
                {
                    friendlyID = room.FriendlyID,
                    najmId = room.NajmIDBoss,
                    bossName = matchedBossName
                }
            });
        }
        #endregion

        [HttpGet("/room/GetUpcomingPlans")]
        public async Task<IActionResult> GetUpcomingPlans([FromQuery] string fid)
        {
            if (string.IsNullOrEmpty(fid))
            {
                return Ok(new { message = "not found", status = 404 });
            }

            var room = await _db.RoomTBLs.FirstOrDefaultAsync(e => e.FriendlyID == fid);
            if (room == null)
            {
                return Ok(new { message = "not found", status = 404 });
            }

            var now = DateTime.UtcNow;
            var upcomingPlans = await _db.RoomTBLPlanTBLs
                .Where(p => p.IDRoomTBL == room.ID
                         && (p.Status == MainEntity_Status.Enabled || p.Status == MainEntity_Status.MEETING_STARTED_LATER)
                         && p.StartDate.HasValue
                         && p.StartDate.Value >= now.AddMinutes(-30))
                .OrderBy(p => p.StartDate)
                .Select(p => new
                {
                    id = p.ID,
                    title = p.Title,
                    teachers = p.Teachers,
                    dabir = p.Dabir,
                    startDate = p.StartDate,
                    explain = p.Explain,
                    requiredToEnterName = p.RequiredToEnterName,
                    presentationFileURL = p.PresentationFileURL
                })
                .ToListAsync();

            return Ok(new
            {
                message = "ok",
                status = 200,
                data = upcomingPlans
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
                .Where(m => m.RoomFriendlyID == fid
                         && m.Type != MainEntity_Types.MEET_IGNORE
                         && m.Status != MainEntity_Status.Delete
                         && m.Status != MainEntity_Status.Disable
                         && m.Status != MainEntity_Status.Canceled);

            var teacherJoins = await validMeetings
                .SelectMany(m => m.UserTBLToMeetingTBLs)
                .Where(u => u.Type == MainEntity_Types.USER_TEACHER
                         && u.UserTBL.Status == MainEntity_Status.USER_ACTIVE
                         && u.MeetingTBL.Type != MainEntity_Types.MEET_IGNORE
                         && u.Type != MainEntity_Types.MEET_IGNORE
                         && u.Status != MainEntity_Status.Delete
                         && u.Status != MainEntity_Status.Disable)
                .Select(u => new
                {
                    Teacher = u.UserTBL,
                    MeetID = u.MeetID
                })
                .ToListAsync();

            var directTeachers = await validMeetings
                .Where(m => m.IDTeacher != null
                         && m.Teacher != null
                         && m.Teacher.Status == MainEntity_Status.USER_ACTIVE
                         && m.Type != MainEntity_Types.MEET_IGNORE
                         && m.Status != MainEntity_Status.Delete
                         && m.Status != MainEntity_Status.Disable
                         && m.Status != MainEntity_Status.Canceled)
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
                    avatar = string.IsNullOrEmpty(g.Key.AvatarPath) ? Statics.CombinePath(Statics.URLFiles, Statics.DEF_AVATAR_TEACHER_PATH) : Statics.CombinePath(Statics.URLFiles, g.Key.AvatarPath),
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

            var meetIds = meetings.Select(m => m.id).ToList();
            var mediaLogs = await _db.MeetingLogTBLs
                .Where(l => meetIds.Contains(l.IDMeetingTBL) &&
                       (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL" ||
                        l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL" ||
                        l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"))
                .OrderByDescending(l => l.ID)
                .ToListAsync();

            var enrichedMeetings = meetings.Select(m =>
            {
                var voiceLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL"));
                var videoLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL"));
                var reportLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"));

                return new
                {
                    id = m.id,
                    meetID = m.meetID,
                    title = m.title,
                    explain = m.explain,
                    dateInsert = m.dateInsert,
                    endDate = m.endDate,
                    jalaliDate = m.jalaliDate,
                    isRecorded = m.isRecorded,
                    status = m.status,
                    statusTitle = m.statusTitle,
                    teacher = m.teacher,
                    teachers = m.teachers,
                    participantsCount = m.participantsCount,
                    voiceUrl = voiceLog?.Value,
                    voiceType = voiceLog != null ? (int)voiceLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    videoUrl = videoLog?.Value,
                    videoType = videoLog != null ? (int)videoLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    reportUrl = reportLog?.Value,
                    reportType = reportLog != null ? (int)reportLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE
                };
            }).ToList();

            return Ok(new
            {
                message = "ok",
                status = 200,
                data = new
                {
                    items = enrichedMeetings,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    page = page,
                    count = count
                }
            });
        }

        private async Task<(bool IsAllowed, string ErrorMessage, UserTBL? User)> VerifyModeratorOrAboveAsync()
        {
            var head = HttpContext.GetGeneralHeaders();
            UserTBL? user = head?.UserTBL;
            if (user == null && !string.IsNullOrEmpty(head?.JWTUserToken))
            {
                user = await _commonMethods.GetUser(head.JWTUserToken);
            }
            if (user == null && !string.IsNullOrEmpty(head?.JWTMeetingUserToken))
            {
                user = await _commonMethods.GetUser(head.JWTMeetingUserToken);
            }

            if (user == null)
            {
                return (false, "کاربر احراز هویت نشده است", null);
            }

            var permissions = user.GetUserPermissions();
            bool isModeratorOrAbove = permissions.Contains("create_rooms") ||
                                      user.Type == MainEntity_Types.USER_MODERATOR ||
                                      user.Type == MainEntity_Types.USER_ADMIN ||
                                      user.Type == MainEntity_Types.USER_ADMINSTRATOR;

            if (!isModeratorOrAbove)
            {
                return (false, "دسترسی غیرمجاز: تنها کاربران در سطح USER_MODERATOR و بالاتر مجاز به انجام این عملیات هستند", user);
            }

            return (true, string.Empty, user);
        }

        [HttpGet("/admin/GetAdminMeetings")]
        public async Task<IActionResult> GetAdminMeetings([FromQuery] int page = 1, [FromQuery] int count = 15, [FromQuery] string q = "", [FromQuery] int? type = null)
        {
            var authCheck = await VerifyModeratorOrAboveAsync();
            if (!authCheck.IsAllowed)
            {
                return Ok(new { message = authCheck.ErrorMessage, status = 403 });
            }

            if (page < 1) page = 1;
            if (count < 1) count = 15;
            if (count > 100) count = 100;

            var query = _db.MeetingTBLs.AsQueryable();

            // Filter for type 100 or 101 by default unless specific type requested
            if (type.HasValue)
            {
                query = query.Where(e => (int)e.Type == type.Value);
            }
            else
            {
                query = query.Where(e => e.Type == MainEntity_Types.MEET_GREEN_LIGHT || e.Type == MainEntity_Types.MEET_API);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string search = q.Trim();
                query = query.Where(e => (e.Title != null && e.Title.Contains(search)) ||
                                         (e.MeetID != null && e.MeetID.Contains(search)) ||
                                         (e.RoomFriendlyID != null && e.RoomFriendlyID.Contains(search)) ||
                                         (e.RoomTBL != null && e.RoomTBL.Title != null && e.RoomTBL.Title.Contains(search)) ||
                                         (e.Teacher != null && ((e.Teacher.Firstname != null && e.Teacher.Firstname.Contains(search)) || (e.Teacher.LastName != null && e.Teacher.LastName.Contains(search)))));
            }

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
                    type = (int)m.Type,
                    status = (int)m.Status,
                    statusTitle = MeetingTBL.GetStatusString(m.Status),
                    dateInsert = m.DateInsert,
                    endDate = m.EndDate,
                    jalaliDate = m.JalaliDate,
                    isRecorded = m.IsRecorded,
                    explain = m.Explain,
                    roomFriendlyID = m.RoomFriendlyID,
                    roomTitle = m.RoomTBL != null ? m.RoomTBL.Title : null,
                    teacher = m.Teacher != null ? new
                    {
                        id = m.Teacher.ID,
                        name = m.Teacher.GetName(),
                        avatar = m.Teacher.AvatarPath
                    } : null,
                    participantsCount = m.UserTBLToMeetingTBLs.Count(u => u.Type == MainEntity_Types.USER_VIEWER)
                })
                .ToListAsync();

            var meetIds = meetings.Select(m => m.id).ToList();
            var mediaLogs = await _db.MeetingLogTBLs
                .Where(l => meetIds.Contains(l.IDMeetingTBL) &&
                       (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL" ||
                        l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL" ||
                        l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"))
                .OrderByDescending(l => l.ID)
                .ToListAsync();

            var enriched = meetings.Select(m =>
            {
                var voiceLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL"));
                var videoLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL"));
                var reportLog = mediaLogs.FirstOrDefault(l => l.IDMeetingTBL == m.id && (l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"));

                return new
                {
                    id = m.id,
                    meetID = m.meetID,
                    title = m.title,
                    type = m.type,
                    status = m.status,
                    statusTitle = m.statusTitle,
                    dateInsert = m.dateInsert,
                    endDate = m.endDate,
                    jalaliDate = m.jalaliDate,
                    isRecorded = m.isRecorded,
                    explain = m.explain,
                    roomFriendlyID = m.roomFriendlyID,
                    roomTitle = m.roomTitle,
                    teacher = m.teacher,
                    participantsCount = m.participantsCount,
                    voiceUrl = voiceLog?.Value,
                    voiceType = voiceLog != null ? (int)voiceLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    voiceLogId = voiceLog?.ID,
                    voiceExplain = voiceLog?.Explain,
                    videoUrl = videoLog?.Value,
                    videoType = videoLog != null ? (int)videoLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    videoLogId = videoLog?.ID,
                    videoExplain = videoLog?.Explain,
                    reportUrl = reportLog?.Value,
                    reportType = reportLog != null ? (int)reportLog.Type : (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    reportLogId = reportLog?.ID,
                    reportExplain = reportLog?.Explain
                };
            }).ToList();

            return Ok(new
            {
                message = "ok",
                status = 200,
                data = new
                {
                    items = enriched,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    page = page,
                    count = count
                }
            });
        }

        [HttpPost("/admin/UpdateMeetingMedia")]
        public async Task<IActionResult> UpdateMeetingMedia([FromBody] UpdateMeetingMediaVM model)
        {
            var authCheck = await VerifyModeratorOrAboveAsync();
            if (!authCheck.IsAllowed)
            {
                return Ok(new { message = authCheck.ErrorMessage, status = 403 });
            }

            if (model == null || model.MeetingId <= 0)
            {
                return Ok(new { message = "شناسه جلسه معتبر نیست", status = 400 });
            }

            var meeting = await _db.MeetingTBLs.FirstOrDefaultAsync(m => m.ID == model.MeetingId);
            if (meeting == null)
            {
                return Ok(new { message = "جلسه یافت نشد", status = 404 });
            }

            // Update Type if provided
            if (model.Type.HasValue)
            {
                meeting.Type = (MainEntity_Types)model.Type.Value;
            }

            // Update Title if provided
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                meeting.Title = model.Title.Trim();
            }

            // Update or Insert Voice URL log
            if (model.VoiceUrl != null)
            {
                var voiceLog = await _db.MeetingLogTBLs
                    .FirstOrDefaultAsync(l => l.IDMeetingTBL == meeting.ID && (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL"));
                var voiceType = model.VoiceType.HasValue && Enum.IsDefined(typeof(MainEntity_Types), model.VoiceType.Value)
                    ? (MainEntity_Types)model.VoiceType.Value
                    : MainEntity_Types.MEETLOG_DOWNLOADABLE;

                if (voiceLog != null)
                {
                    voiceLog.Key = MeetingLogTBL.KEY_SOUND;
                    voiceLog.Value = model.VoiceUrl.Trim();
                    voiceLog.Type = voiceType;
                    voiceLog.Status = MainEntity_Status.LOG_NORMAL;
                    voiceLog.DateInsert = DateTime.UtcNow;
                }
                else if (!string.IsNullOrWhiteSpace(model.VoiceUrl))
                {
                    _db.MeetingLogTBLs.Add(new MeetingLogTBL
                    {
                        IDMeetingTBL = meeting.ID,
                        Key = MeetingLogTBL.KEY_SOUND,
                        Value = model.VoiceUrl.Trim(),
                        Type = voiceType,
                        Status = MainEntity_Status.LOG_NORMAL,
                        DateInsert = DateTime.UtcNow
                    });
                }
            }

            // Update or Insert Video URL log
            if (model.VideoUrl != null)
            {
                var videoLog = await _db.MeetingLogTBLs
                    .FirstOrDefaultAsync(l => l.IDMeetingTBL == meeting.ID && (l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL"));
                var videoType = model.VideoType.HasValue && Enum.IsDefined(typeof(MainEntity_Types), model.VideoType.Value)
                    ? (MainEntity_Types)model.VideoType.Value
                    : MainEntity_Types.MEETLOG_DOWNLOADABLE;

                if (videoLog != null)
                {
                    videoLog.Key = MeetingLogTBL.KEY_VIDEO;
                    videoLog.Value = model.VideoUrl.Trim();
                    videoLog.Type = videoType;
                    videoLog.Status = MainEntity_Status.LOG_NORMAL;
                    videoLog.DateInsert = DateTime.UtcNow;
                }
                else if (!string.IsNullOrWhiteSpace(model.VideoUrl))
                {
                    _db.MeetingLogTBLs.Add(new MeetingLogTBL
                    {
                        IDMeetingTBL = meeting.ID,
                        Key = MeetingLogTBL.KEY_VIDEO,
                        Value = model.VideoUrl.Trim(),
                        Type = videoType,
                        Status = MainEntity_Status.LOG_NORMAL,
                        DateInsert = DateTime.UtcNow
                    });
                }
            }

            // Update or Insert Report URL log
            if (model.ReportUrl != null)
            {
                var reportLog = await _db.MeetingLogTBLs
                    .FirstOrDefaultAsync(l => l.IDMeetingTBL == meeting.ID && (l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"));
                var reportType = model.ReportType.HasValue && Enum.IsDefined(typeof(MainEntity_Types), model.ReportType.Value)
                    ? (MainEntity_Types)model.ReportType.Value
                    : MainEntity_Types.MEETLOG_DOWNLOADABLE;

                if (reportLog != null)
                {
                    reportLog.Key = MeetingLogTBL.KEY_REPORT;
                    reportLog.Value = model.ReportUrl.Trim();
                    reportLog.Type = reportType;
                    reportLog.Status = MainEntity_Status.LOG_NORMAL;
                    reportLog.DateInsert = DateTime.UtcNow;
                }
                else if (!string.IsNullOrWhiteSpace(model.ReportUrl))
                {
                    _db.MeetingLogTBLs.Add(new MeetingLogTBL
                    {
                        IDMeetingTBL = meeting.ID,
                        Key = MeetingLogTBL.KEY_REPORT,
                        Value = model.ReportUrl.Trim(),
                        Type = reportType,
                        Status = MainEntity_Status.LOG_NORMAL,
                        DateInsert = DateTime.UtcNow
                    });
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "تغییرات با موفقیت ذخیره شد",
                status = 200,
                data = new
                {
                    id = meeting.ID,
                    type = (int)meeting.Type,
                    title = meeting.Title,
                    voiceUrl = model.VoiceUrl,
                    voiceType = model.VoiceType ?? (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    videoUrl = model.VideoUrl,
                    videoType = model.VideoType ?? (int)MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    reportUrl = model.ReportUrl,
                    reportType = model.ReportType ?? (int)MainEntity_Types.MEETLOG_DOWNLOADABLE
                }
            });
        }

        [HttpPost("/admin/SaveMeetingMediaLog")]
        public async Task<IActionResult> SaveMeetingMediaLog([FromBody] SaveMeetingMediaLogVM model)
        {
            var authCheck = await VerifyModeratorOrAboveAsync();
            if (!authCheck.IsAllowed)
            {
                return Ok(new { message = authCheck.ErrorMessage, status = 403 });
            }

            if (model == null)
            {
                return Ok(new { message = "داده‌های ارسالی معتبر نیست", status = 400 });
            }

            MeetingTBL? meeting = null;
            if (model.MeetingId > 0)
            {
                meeting = await _db.MeetingTBLs.FirstOrDefaultAsync(m => m.ID == model.MeetingId);
            }
            else if (!string.IsNullOrWhiteSpace(model.MeetID))
            {
                meeting = await _db.MeetingTBLs.FirstOrDefaultAsync(m => m.MeetID == model.MeetID);
            }

            if (meeting == null)
            {
                return Ok(new { message = "جلسه مورد نظر یافت نشد", status = 404 });
            }

            string rawKey = (model.Key ?? "").Trim().ToLower();
            string standardKey = MeetingLogTBL.KEY_SOUND;
            string legacyKey = "VOICE_URL";

            if (rawKey.Contains("video") || rawKey == "video_url")
            {
                standardKey = MeetingLogTBL.KEY_VIDEO;
                legacyKey = "VIDEO_URL";
            }
            else if (rawKey.Contains("report") || rawKey == "report_url")
            {
                standardKey = MeetingLogTBL.KEY_REPORT;
                legacyKey = "REPORT_URL";
            }
            else if (rawKey.Contains("sound") || rawKey.Contains("voice") || rawKey == "voice_url")
            {
                standardKey = MeetingLogTBL.KEY_SOUND;
                legacyKey = "VOICE_URL";
            }

            MainEntity_Types logType = MainEntity_Types.MEETLOG_DOWNLOADABLE;
            if (model.Type == (int)MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN || model.Type == 103)
            {
                logType = MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN;
            }
            else if (model.Type == (int)MainEntity_Types.MEETLOG_NORMAL || model.Type == 1)
            {
                logType = MainEntity_Types.MEETLOG_NORMAL;
            }
            else
            {
                logType = MainEntity_Types.MEETLOG_DOWNLOADABLE;
            }

            MeetingLogTBL? existingLog = null;
            if (model.ID.HasValue && model.ID.Value > 0)
            {
                existingLog = await _db.MeetingLogTBLs.FirstOrDefaultAsync(l => l.ID == model.ID.Value && l.IDMeetingTBL == meeting.ID);
            }

            if (existingLog == null)
            {
                existingLog = await _db.MeetingLogTBLs.FirstOrDefaultAsync(l => l.IDMeetingTBL == meeting.ID && (l.Key == standardKey || l.Key == legacyKey));
            }

            if (string.IsNullOrWhiteSpace(model.Value))
            {
                if (existingLog != null)
                {
                    _db.MeetingLogTBLs.Remove(existingLog);
                    await _db.SaveChangesAsync();
                }
                return Ok(new
                {
                    message = "رسانه با موفقیت حذف شد",
                    status = 200,
                    data = new { meetingId = meeting.ID, key = standardKey, deleted = true }
                });
            }

            if (existingLog != null)
            {
                existingLog.Key = standardKey;
                existingLog.Value = model.Value.Trim();
                existingLog.Type = logType;
                existingLog.Status = MainEntity_Status.LOG_NORMAL;
                if (!string.IsNullOrWhiteSpace(model.Explain))
                {
                    existingLog.Explain = model.Explain.Trim();
                }
                existingLog.DateInsert = DateTime.UtcNow;
            }
            else
            {
                existingLog = new MeetingLogTBL
                {
                    IDMeetingTBL = meeting.ID,
                    Key = standardKey,
                    Value = model.Value.Trim(),
                    Type = logType,
                    Status = MainEntity_Status.LOG_NORMAL,
                    Explain = model.Explain?.Trim(),
                    DateInsert = DateTime.UtcNow
                };
                _db.MeetingLogTBLs.Add(existingLog);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "اطلاعات رسانه با موفقیت ذخیره شد",
                status = 200,
                data = new
                {
                    id = existingLog.ID,
                    meetingId = meeting.ID,
                    key = existingLog.Key,
                    value = existingLog.Value,
                    type = (int)existingLog.Type,
                    typeName = existingLog.Type == MainEntity_Types.MEETLOG_DOWNLOADABLE ? "قابل دانلود" :
                               existingLog.Type == MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN ? "فقط پخش (بدون دانلود)" : "عادی",
                    isDownloadable = existingLog.Type == MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    explain = existingLog.Explain
                }
            });
        }

        [HttpGet("/admin/GetMeetingMediaLogs/{meetingId}")]
        public async Task<IActionResult> GetMeetingMediaLogs(long meetingId)
        {
            var authCheck = await VerifyModeratorOrAboveAsync();
            if (!authCheck.IsAllowed)
            {
                return Ok(new { message = authCheck.ErrorMessage, status = 403 });
            }

            var logs = await _db.MeetingLogTBLs
                .Where(l => l.IDMeetingTBL == meetingId &&
                       (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL" ||
                        l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL" ||
                        l.Key == MeetingLogTBL.KEY_REPORT || l.Key == "REPORT_URL"))
                .OrderByDescending(l => l.ID)
                .Select(l => new
                {
                    id = l.ID,
                    meetingId = l.IDMeetingTBL,
                    key = l.Key,
                    standardKey = (l.Key == MeetingLogTBL.KEY_SOUND || l.Key == "VOICE_URL") ? MeetingLogTBL.KEY_SOUND :
                                  (l.Key == MeetingLogTBL.KEY_VIDEO || l.Key == "VIDEO_URL") ? MeetingLogTBL.KEY_VIDEO :
                                  MeetingLogTBL.KEY_REPORT,
                    value = l.Value,
                    type = (int)l.Type,
                    typeName = l.Type == MainEntity_Types.MEETLOG_DOWNLOADABLE ? "قابل دانلود" :
                               l.Type == MainEntity_Types.MEETLOG_DOWNLOAD_FORBIDDEN ? "فقط پخش (بدون دانلود)" : "عادی",
                    isDownloadable = l.Type == MainEntity_Types.MEETLOG_DOWNLOADABLE,
                    explain = l.Explain,
                    dateInsert = l.DateInsert
                })
                .ToListAsync();

            return Ok(new
            {
                message = "ok",
                status = 200,
                data = logs
            });
        }

        [HttpDelete("/admin/DeleteMeetingMediaLog/{id}")]
        public async Task<IActionResult> DeleteMeetingMediaLog(long id)
        {
            var authCheck = await VerifyModeratorOrAboveAsync();
            if (!authCheck.IsAllowed)
            {
                return Ok(new { message = authCheck.ErrorMessage, status = 403 });
            }

            var log = await _db.MeetingLogTBLs.FindAsync(id);
            if (log == null)
            {
                return Ok(new { message = "رکورد یافت نشد", status = 404 });
            }

            _db.MeetingLogTBLs.Remove(log);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "رکورد با موفقیت حذف شد",
                status = 200
            });
        }

        [HttpPost("/user/EditUserInfo")]
        public async Task<IActionResult> EditUserInfo([FromForm] EditUserInfoVM model)
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

            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                var uploadsFolder = Path.Combine("C:\\hekmatFiles", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}.{model.AvatarFile.FileName.Split(".")[1]}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }
                user.AvatarPath = $"/uploads/avatars/{uniqueFileName}";
            }
            else if (model.AvatarPath != null)
            {
                user.AvatarPath = model.AvatarPath;
            }
            user.LastUpdate = DateTime.UtcNow;
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


        [HttpGet("/user/SameUser")]
        public async Task<IActionResult> SameUser([FromQuery] long uid1, [FromQuery] long uid2)
        {
            if (_db.UserTBLs.Any(e => e.ID == uid1))
            {
                return Ok(new
                {
                    message = $"not found {uid1}",
                    status = 404
                });
            }
            if (_db.UserTBLs.Any(e => e.ID == uid2))
            {
                return Ok(new
                {
                    message = $"not found {uid2}",
                    status = 404
                });
            }

            var from = await _db.UserTBLs.Where(e => e.ID == uid1).Include(e => e.TeacherMeetings).Include(e => e.UserTBLToMeetingTBLs).FirstAsync();
            var to = await _db.UserTBLs.Where(e => e.ID == uid2).Include(e => e.TeacherMeetings)
                //.Include(e => e.RoomTBLs).Include(e => e.RoomTBLToUserTBLs).Include(e => e.Followings).Include(e => e.Followers).Include(e => e.)
                .Include(e => e.UserTBLToMeetingTBLs).FirstAsync();

            foreach (var m in from.TeacherMeetings)
            {
                m.IDTeacher = to.ID;
                _db.Update(m);
            }
            foreach (var m in from.UserTBLToMeetingTBLs)
            {
                m.UUID = to.UUID;
                _db.Update(m);
            }

            _db.SaveChanges();



            return Ok(new
            {
                message = "ok",
                status = 200,
                //data = teachers
            });
        }


        #region Room Metadata & Full Edit Endpoints
        [HttpGet("/room/GetRoomMetadata")]
        public async Task<IActionResult> GetRoomMetadata([FromQuery] string friendlyId, [FromQuery] string key,
            [FromServices] IGetRoomMetadataUseCase useCase, [FromServices] ApiPresenter<GetRoomMetadataResponse> presenter)
        {
            var req = new GetRoomMetadataRequest { FriendlyID = friendlyId, Key = key };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await useCase.handleAsync(req, presenter);
            return presenter.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpPost("/room/SetRoomMetadata")]
        public async Task<IActionResult> SetRoomMetadata([FromBody] SetRoomMetadataRequest req,
            [FromServices] ISetRoomMetadataUseCase useCase, [FromServices] ApiPresenter<SetRoomMetadataResponse> presenter)
        {
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await useCase.handleAsync(req, presenter);
            return presenter.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpGet("/room/GetRoomFullDetails/{friendlyId}")]
        public async Task<IActionResult> GetRoomFullDetails(string friendlyId,
            [FromServices] IGetRoomFullDetailsUseCase useCase, [FromServices] ApiPresenter<GetRoomFullDetailsResponse> presenter)
        {
            var req = new GetRoomFullDetailsRequest { FriendlyID = friendlyId };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await useCase.handleAsync(req, presenter);
            return presenter.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpGet("/admin/GetRoomWaitingMembers")]
        public async Task<IActionResult> GetRoomWaitingMembers([FromQuery] string roomId,
            [FromServices] IGetRoomWaitingMembersUseCase useCase, [FromServices] ApiPresenter<GetRoomWaitingMembersResponse> presenter)
        {
            var req = new GetRoomWaitingMembersRequest { RoomId = roomId };
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await useCase.handleAsync(req, presenter);
            return presenter.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpPost("/admin/ChangeRoomMemberStatus")]
        public async Task<IActionResult> ChangeRoomMemberStatus([FromBody] ChangeRoomMemberStatusRequest req,
            [FromServices] IChangeRoomMemberStatusUseCase useCase, [FromServices] ApiPresenter<ChangeRoomMemberStatusResponse> presenter)
        {
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await useCase.handleAsync(req, presenter);
            return presenter.jsonContentResult;
        }

        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        [HttpPost("/room/EditRoomFull")]
        public async Task<IActionResult> EditRoomFull([FromBody] EditRoomFullRequest req,
            [FromServices] IEditRoomFullUseCase useCase, [FromServices] ApiPresenter<EditRoomFullResponse> presenter)
        {
            req.GeneralHeaders = HttpContext.GetGeneralHeaders();
            await useCase.handleAsync(req, presenter);
            return presenter.jsonContentResult;
        }
        #endregion

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
        public IFormFile? AvatarFile { get; set; }
    }
}
