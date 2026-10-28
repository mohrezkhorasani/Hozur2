using Application.DTO;
using Application.DTO.Models;
using Newtonsoft.Json;
using Application.General;
using Application.Interface.IInfrastructure;
using DocumentFormat.OpenXml.Wordprocessing;
using Domain.Entity;
using Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using BBBHookInfo = Application.DTO.Models.BBBHookInfo;
using BBBHookResponse = Application.DTO.Models.BBBHookResponse;

namespace Infrastructure.API
{
    public class BBBGetMeeting : IBBBGetMeetings
    {
        public const string URL = "https://vc.hekmateislami.com";
        public async Task<List<MiniRoomJSON>> Request(GetLivesRequest req, UserTBL user)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.bbb.UrlAPI}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Where(e => e.MetaDatas.OwnerID == user.ID || e.MetaDatas.AdminIDs.Any(r=>r==user.ID)).Select(e => new MiniRoomJSON
            {
                RoomID = e.MetaDatas.RoomID,
                //RoomID = e.InternalMeetingID,
                Title = e.MeetingName,
                BBB_MeetID = e.MeetingID,
                CountPartis = e.ParticipantCount,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();
        }
        public async Task<List<MiniRoomJSON>> Request(GetLivesOfProfileRequest req, UserTBL user, UserTBL currentUser)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.bbb.UrlAPI}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Where(e => ((!e.MetaDatas.IsSuperPrivate || e.MetaDatas.RoomPublic) && e.MetaDatas.OwnerID == user.ID) ||
             (e.MetaDatas.OwnerID == user.ID && user.ID == currentUser.ID)
            ).Select(e => new MiniRoomJSON
            {
                RoomID = e.MetaDatas.RoomID,
                //RoomID = e.InternalMeetingID,
                Title = e.MeetingName,
                BBB_MeetID = e.MeetingID,
                CountPartis = e.ParticipantCount,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();
        }
        public async Task<List<MiniRoomJSON>> Request(GetLivesRequest req)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.bbb.UrlAPI}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Where(e => e.MetaDatas.RoomPublic).Select(e => new MiniRoomJSON
            {
                RoomID = e.MetaDatas.RoomID,
                //RoomID = e.InternalMeetingID,
                Title = e.MeetingName,
                BBB_MeetID = e.MeetingID,
                CountPartis = e.ParticipantCount,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();
        }
        public async Task<List<MiniRoomJSON>> RequestAll(GetLivesRequest req)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.bbb.UrlAPI}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Select(e => new MiniRoomJSON
            {
                RoomID = e.MetaDatas.RoomID,
                //RoomID = e.InternalMeetingID,
                Title = e.MeetingName,
                BBB_MeetID = e.MeetingID,
                CountPartis = e.ParticipantCount,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();
        }
        public async Task<List<MiniRoomJSON>> Request(GetLiveWithIDRequest req)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.bbb.UrlAPI}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Where(e => e.MetaDatas.RoomID == req.ID).Select(e => new MiniRoomJSON
            {
                RoomID = e.MetaDatas.RoomID,
                //RoomID = e.InternalMeetingID,
                Title = e.MeetingName,
                BBB_MeetID = e.MeetingID,
                CountPartis = e.ParticipantCount,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();
        }


        public async Task<List<MiniRoomJSON>> GetLivesShort(GetLivesRequest req)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.bbb.UrlAPI}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);

            return result.Meetings.MeetingList.Where(e => e.MetaDatas.RoomPublic).Select(e => new MiniRoomJSON
            {
                BBB_MeetID = e.MeetingID,
                RoomID = e.InternalMeetingID,
                Title = e.MeetingName,

                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();
        }
        public async Task<MeetJSON> CreateMeetingAsync(RoomTBL room, RoomTBLPlanTBL? plan = null)
        {
            string apiCall = "create";
            string joinUrl = $"{URL}/rooms/{room.FriendlyID}/join";

            string metadataXml = $@"
    <meta name=""bbb-context-name"">اتاق جدید</meta>
    <meta name=""bbb-context-id"">y3x-nhd-imk-7wu</meta>
    <meta name=""public"">{room.IsPublicRoom.ToString()}</meta>";

            // همه مقادیر باید UrlEncoded شوند
            string queryString = $"meetingID={Uri.EscapeDataString(room.BBB_MeetID)}" +
                                 $"&name={Uri.EscapeDataString(room.Title)}" +
                                 $"&attendeePW={Uri.EscapeDataString(room.ViewerCode)}" +     // اصلاح شد
                                 $"&moderatorPW={Uri.EscapeDataString(room.ModeratorCode)}" + // اصلاح شد
                                 $"&loginURL={Uri.EscapeDataString(joinUrl)}" +
                                 $"&allowStartStopRecording={(room.RecordRoom ? "true" : "false")}" +
                                 $"&autoStartRecording=false" +
                                 $"&muteOnStart=true" +
                                 $"&lockSettingsDisableMic=true" +
                                 $"&record={(room.RecordRoom ? "true" : "false")}" +
                                 $"&lockSettingsDisableCam=true" +
                                 $"&lockSettingsLockOnJoin=false" +
                                 $"&endCallbackURL=https://core.hekmateislami.com/api/bbb-webhook" +
                                 $"&lockSettingsLockOnJoinConfigurable=true" +
                                 $"&duration=300" +
                                 $"&logo={Uri.EscapeDataString($"{URL}/images/logo.png")}";

                                 //$"&metadata={Uri.EscapeDataString(metadata/Xml)}";
            queryString += $"&meta_public={Uri.EscapeDataString(room.IsPublicRoom.ToString().ToLower())}";
            queryString += $"&meta_roomID={Uri.EscapeDataString(room.FriendlyID)}";
            queryString += $"&meta_owner={Uri.EscapeDataString(room.IDOwnerTBL.ToString())}";
            queryString += $"&meta_smvbem={Uri.EscapeDataString(room.IsShowMemberVIPBeforeEndTheMeeting.ToString().ToLower())}";
            if (room.RoomTBLToUserTBLs != null && room.RoomTBLToUserTBLs.Any())
            {
                string adminIdsString = string.Join(",", room.RoomTBLToUserTBLs.Select(u => u.ParticipatorTBL.ID.ToString()));
                queryString += $"&meta_admins={Uri.EscapeDataString(adminIdsString)}";
            }
            queryString += $"&meta_supprivate={Uri.EscapeDataString(room.IsSuperPrivateRoom.ToString().ToLower())}";
            queryString += $"&bbb-context-id={Uri.EscapeDataString(room.FriendlyID)}";

            if (plan != null)
            {
                var parsedTeachers = plan.GetParsedTeachers();
                if (parsedTeachers.Any())
                {
                    var teachersObj = parsedTeachers.Select(t => new { name = t.Name, position = t.Position ?? "" }).ToList();
                    string teachersJson = JsonConvert.SerializeObject(teachersObj);
                    queryString += $"&meta_teachers={Uri.EscapeDataString(teachersJson)}";
                }
                if (!string.IsNullOrWhiteSpace(plan.Dabir))
                {
                    var dabir = plan.GetParsedDabir();
                    var dabirObj = new { name = dabir.Name, position = dabir.Position ?? "" };
                    string dabirJson = JsonConvert.SerializeObject(dabirObj);
                    queryString += $"&meta_dabir={Uri.EscapeDataString(dabirJson)}";
                }
            }
            else if (room.RoomTBLPlanTBLs != null && room.RoomTBLPlanTBLs.Any())
            {
                var latestPlan = room.RoomTBLPlanTBLs.OrderByDescending(p => p.StartDate).FirstOrDefault();
                if (latestPlan != null)
                {
                    var parsedTeachers = latestPlan.GetParsedTeachers();
                    if (parsedTeachers.Any())
                    {
                        var teachersObj = parsedTeachers.Select(t => new { name = t.Name, position = t.Position ?? "" }).ToList();
                        string teachersJson = JsonConvert.SerializeObject(teachersObj);
                        queryString += $"&meta_teachers={Uri.EscapeDataString(teachersJson)}";
                    }
                    if (!string.IsNullOrWhiteSpace(latestPlan.Dabir))
                    {
                        var dabir = latestPlan.GetParsedDabir();
                        var dabirObj = new { name = dabir.Name, position = dabir.Position ?? "" };
                        string dabirJson = JsonConvert.SerializeObject(dabirObj);
                        queryString += $"&meta_dabir={Uri.EscapeDataString(dabirJson)}";
                    }
                }
            }


            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();

            try
            {
                Statics.Log("BBBCreate Request", url);
                string xmlResponse = await client.GetStringAsync(url);
                Statics.Log("BBBCreate Response", xmlResponse);

                XmlSerializer serializer = new XmlSerializer(typeof(BBBCreateResponse));
                using StringReader reader = new StringReader(xmlResponse);
                BBBCreateResponse result = (BBBCreateResponse)serializer.Deserialize(reader);

                if (string.IsNullOrEmpty(result.MeetingID))
                {
                    return new MeetJSON { MeetID = room.BBB_MeetID, Status = "failed", JoinUrl = joinUrl };
                }

                // ✅ Register webhook for this meeting
                await RegisterWebhookForMeetingAsync(room.BBB_MeetID);

                return new MeetJSON
                {
                    MeetID = result.InternalMeetingID,
                    Status = "created",
                    Title = room.Title,
                    JalaliDate = Statics.GetPersianDate(DateTime.UtcNow),
                    StartedTime = DateTime.UtcNow.ToString("HH:mm"),
                    JoinUrl = joinUrl,
                    Room = new MiniRoomJSON
                    {
                        RoomID = result.MeetingID,
                        Title = room.Title,
                        Created = DateTime.UtcNow,
                        LastMeet = DateTime.UtcNow,
                        Count = 0,
                        CountTeacher = 0,
                        CountPartis = 0
                    },
                    Teacher = new List<MiniUserJSON>(),
                    Creator = null,
                    JWT = null
                };
            }
            catch (Exception e)
            {
                Statics.Log("BBBCreate Error", $"err = {e.Message}\n{e.StackTrace}");
                return new MeetJSON { MeetID = room.BBB_MeetID, Status = "failed", JoinUrl = joinUrl };
            }
        }


        public async Task<List<AttendeeInfo>> GetMeetingAttendeesByRoomId(string roomId)
        {
            try
            {
                string apiCall = "getMeetings";
                string checksumInput = apiCall + "" + Statics.bbb.Secret;
                string checksum = Statics.ComputeSha1(checksumInput);
                string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?checksum={checksum}";

                using HttpClient client = new HttpClient();
                string xmlResponse = await client.GetStringAsync(fullUrl);

                XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
                using StringReader reader = new StringReader(xmlResponse);
                BBBResponse result = (BBBResponse)serializer.Deserialize(reader);

                // Find meeting by RoomID
                var meeting = result.Meetings.MeetingList
                    .FirstOrDefault(m => m.MetaDatas?.RoomID == roomId);

                if (meeting == null)
                    return new List<AttendeeInfo>();

                // Extract attendees
                return meeting.Attendees?.AttendeeList?.Select(a => new AttendeeInfo
                {
                    UserID = a.UserID,
                    FullName = a.FullName,
                    Role = a.Role,
                    NationalCode = a.CustomData?.NationalCode,
                    Phone = a.CustomData?.Phone,
                    UserID5 = a.CustomData?.UserID5,
                    Vc2AuthNorm = a.CustomData?.Vc2AuthNorm
                }).ToList() ?? new List<AttendeeInfo>();
            }
            catch (Exception e)
            {
                Statics.Log("GetMeetingAttendeesByRoomId Error", e.Message);
                return new List<AttendeeInfo>();
            }
        }
        public async Task<AttendeeInfo> GetAttendeeByUserId(string roomId, string userId)
        {
            try
            {
                var attendees = await GetMeetingAttendeesByRoomId(roomId);
                return attendees.FirstOrDefault(a => a.UserID == userId);
            }
            catch (Exception e)
            {
                Statics.Log("GetAttendeeByUserId Error", e.Message);
                return null;
            }
        }
        public async Task<AttendeeInfo> GetAttendeeByNationalCode(string roomId, string nationalCode)
        {
            try
            {
                var attendees = await GetMeetingAttendeesByRoomId(roomId);
                return attendees.FirstOrDefault(a => a.NationalCode == nationalCode);
            }
            catch (Exception e)
            {
                Statics.Log("GetAttendeeByNationalCode Error", e.Message);
                return null;
            }
        }
        public async Task<AttendeeInfo> GetAttendeeByPhoneNumber(string roomId, string phoneNumber)
        {
            try
            {
                var attendees = await GetMeetingAttendeesByRoomId(roomId);
                return attendees.FirstOrDefault(a => a.Phone == phoneNumber);
            }
            catch (Exception e)
            {
                Statics.Log("GetAttendeeByPhoneNumber Error", e.Message);
                return null;
            }
        }

        public async Task<string> GetJoinUrlAsync(MeetingTBL meet, string fullName, string userId, string nationalCode, string phone = ""
            , string vc2AuthNorm = "", bool asModerator = false, string moderatorPassowrd = "")
        {
            string apiCall = "join";
            string meetingID = meet.RoomTBL.BBB_MeetID;

            // Parameters
            string queryString =
                $"fullName={Uri.EscapeDataString(fullName)}" +
                $"&meetingID={Uri.EscapeDataString(meetingID)}" +
                $"&userID={Uri.EscapeDataString(userId)}" +  // ✅ اضافه کردن userId
                $"&logoutURL={Uri.EscapeDataString($"https://vc2.hekmateislami.com/meeting/ended?roomid={meet.RoomFriendlyID}")}" +
                $"&password={Uri.EscapeDataString(moderatorPassowrd)}" +
                $"&redirect=true";
            if (!string.IsNullOrEmpty(nationalCode))
                queryString += $"&userdata-nationalCode={Uri.EscapeDataString(nationalCode)}";
            queryString += $"&userdata-vc2AuthNorm={Uri.EscapeDataString(vc2AuthNorm)}";
            if (!string.IsNullOrEmpty(phone))
                queryString += $"&userdata-phone={Uri.EscapeDataString(phone)}";
            queryString += $"&userdata-userID5={Uri.EscapeDataString(userId)}";

            // Compute checksum
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);

            // Full join URL
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            try
            {
                await client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                Statics.Log("IBBBMeeting", e.Message);
            }

            return url;
        }

        public async Task<bool> IsMeetingRunningAsync(string meetingID)
        {
            string apiCall = "isMeetingRunning";

            // Query string
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}";

            // Compute checksum
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);

            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            try
            {
                string xmlResponse = await client.GetStringAsync(url);

                // Deserialize XML
                XmlSerializer serializer = new XmlSerializer(typeof(BBBRunningResponse));
                using StringReader reader = new StringReader(xmlResponse);
                BBBRunningResponse result = (BBBRunningResponse)serializer.Deserialize(reader);

                // Return true if running = "true" and returncode = "SUCCESS"
                return result.ReturnCode?.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) == true
                       && result.Running?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<RecordingInfo>> GetRecordingsByMeetingId(string meetingId)
        {
            return await GetRecordingsAsync(meetingId: meetingId);
        }

        public async Task<List<RecordingInfo>> GetRecordingsAsync(string? meetingId = null, string? recordId = null, string? state = null)
        {
            string apiCall = "getRecordings";
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(meetingId))
                queryParams.Add($"meetingID={Uri.EscapeDataString(meetingId)}");

            if (!string.IsNullOrWhiteSpace(recordId))
                queryParams.Add($"recordID={Uri.EscapeDataString(recordId)}");

            if (!string.IsNullOrWhiteSpace(state))
                queryParams.Add($"state={Uri.EscapeDataString(state)}");

            string queryString = string.Join("&", queryParams);
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);

            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?" + (string.IsNullOrEmpty(queryString) ? "" : $"{queryString}&") + $"checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            string xmlResponse;
            try
            {
                xmlResponse = await client.GetStringAsync(fullUrl);
            }
            catch (Exception ex)
            {
                Statics.Log("GetRecordingsAsync Error", ex.Message);
                return new List<RecordingInfo>();
            }

            var doc = XDocument.Parse(xmlResponse);
            var returnCode = doc.Root?.Element("returncode")?.Value;
            var messageKey = doc.Root?.Element("messageKey")?.Value;

            if (returnCode != "SUCCESS")
            {
                if (messageKey == "noRecordings")
                    return new List<RecordingInfo>();

                var message = doc.Root?.Element("message")?.Value ?? "Unknown error";
                throw new Exception($"BBB API Error: {messageKey} - {message}");
            }

            var recordings = new List<RecordingInfo>();
            foreach (var rec in doc.Root?.Element("recordings")?.Elements("recording") ?? Enumerable.Empty<XElement>())
            {
                var playback = rec.Element("playback")?.Element("format");
                var urlElement = playback?.Element("url");

                var metadataDict = new Dictionary<string, string>();
                var metadataElem = rec.Element("metadata");
                if (metadataElem != null)
                {
                    foreach (var elem in metadataElem.Elements())
                    {
                        metadataDict[elem.Name.LocalName] = elem.Value;
                    }
                }

                long startMs = long.TryParse(rec.Element("startTime")?.Value, out long st) ? st : 0;
                long endMs = long.TryParse(rec.Element("endTime")?.Value, out long et) ? et : 0;
                int lengthMinutes = int.TryParse(playback?.Element("length")?.Value, out int len) ? len : 0;
                if (lengthMinutes == 0 && endMs > startMs)
                {
                    lengthMinutes = (int)Math.Max(1, (endMs - startMs) / 60000);
                }

                int participants = int.TryParse(rec.Element("participants")?.Value, out int part) ? part : 0;

                recordings.Add(new RecordingInfo
                {
                    RecordID = rec.Element("recordID")?.Value ?? "",
                    MeetingID = rec.Element("meetingID")?.Value ?? "",
                    Name = rec.Element("name")?.Value ?? "",
                    Published = bool.TryParse(rec.Element("published")?.Value, out bool p) && p,
                    State = rec.Element("state")?.Value ?? "published",
                    StartTime = startMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(startMs).UtcDateTime : DateTime.UtcNow,
                    EndTime = endMs > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(endMs).UtcDateTime : DateTime.UtcNow,
                    LengthMinutes = lengthMinutes,
                    Participants = participants,
                    PlaybackFormat = playback?.Element("type")?.Value ?? "presentation",
                    PlaybackUrl = urlElement?.Value,
                    Size = long.TryParse(rec.Element("size")?.Value, out long s) ? s : 0,
                    Thumbnail = playback?.Element("preview")?.Elements("image")
                                .FirstOrDefault(i => i.Attribute("alt")?.Value.Contains("thumbnail") == true)?.Value
                                ?? playback?.Element("preview")?.Elements("images")?.Elements("image")?.FirstOrDefault()?.Value,
                    Metadata = metadataDict
                });
            }

            return recordings;
        }

        public async Task<bool> PublishRecordingsAsync(string recordId, bool publish)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("recordId is required");

            string apiCall = "publishRecordings";
            string queryString = $"publish={(publish ? "true" : "false")}&recordID={Uri.EscapeDataString(recordId)}";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            string xmlResponse = await client.GetStringAsync(fullUrl);
            var doc = XDocument.Parse(xmlResponse);
            return doc.Root?.Element("returncode")?.Value == "SUCCESS";
        }

        public async Task<bool> DeleteRecordingsAsync(string recordId)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("recordId is required");

            string apiCall = "deleteRecordings";
            string queryString = $"recordID={Uri.EscapeDataString(recordId)}";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            string xmlResponse = await client.GetStringAsync(fullUrl);
            var doc = XDocument.Parse(xmlResponse);
            return doc.Root?.Element("returncode")?.Value == "SUCCESS";
        }

        public async Task<bool> UpdateRecordingsAsync(string recordId, Dictionary<string, string> metadata)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("recordId is required");

            string apiCall = "updateRecordings";
            var queryParams = new List<string> { $"recordID={Uri.EscapeDataString(recordId)}" };

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    string key = kvp.Key.StartsWith("meta_") ? kvp.Key : $"meta_{kvp.Key}";
                    queryParams.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(kvp.Value)}");
                }
            }

            string queryString = string.Join("&", queryParams);
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            string xmlResponse = await client.GetStringAsync(fullUrl);
            var doc = XDocument.Parse(xmlResponse);
            return doc.Root?.Element("returncode")?.Value == "SUCCESS";
        }

        public async Task<string> GetRecordingTextTracksAsync(string recordId)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("recordId is required");

            string apiCall = "getRecordingTextTracks";
            string queryString = $"recordID={Uri.EscapeDataString(recordId)}";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            return await client.GetStringAsync(fullUrl);
        }

        public async Task GetMeeting(string meetingId)
        {
            if (string.IsNullOrWhiteSpace(meetingId))
                throw new ArgumentException("meetingId is required");

            string apiCall = "getMeetings";

            // Build query string: only filter by meetingID (you can also add recordID if needed)
            string queryString = $"meetingID={Uri.EscapeDataString(meetingId)}";

            // For more filtering: $"meetingID={meetingId}&recordID=..."

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);

            // Full URL
            string fullUrl = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Parse BBB XML response
            var doc = XDocument.Parse(xmlResponse);

            var ns = doc.Root.GetDefaultNamespace();

            if (doc.Root.Element("returncode")?.Value != "SUCCESS")
            {
                var message = doc.Root.Element("message")?.Value ?? "Unknown error";
                var messageKey = doc.Root.Element("messageKey")?.Value;
                throw new Exception($"BBB API Error: {messageKey} - {message}");
            }

        }





        public async Task<bool> EndMeetingAsync(string meetingID, string pass = "")
        {
            if (string.IsNullOrWhiteSpace(meetingID))
                return false;

            string apiCall = "end";
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}" +
                                 $"&password={pass}";

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);

            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);

            try
            {
                string xmlResponse = await client.GetStringAsync(url);

                var doc = XDocument.Parse(xmlResponse);
                var returncode = doc.Root?.Element("returncode")?.Value;

                return returncode?.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch (Exception ex)
            {
                // اگر خطا داد (مثلاً جلسه وجود نداشت، checksum اشتباه بود، سرور قطع بود و ...)
                // لاگ رو می‌تونی بعداً با Serilog یا هر چیزی بزنی
                Console.WriteLine($"EndMeetingAsync failed for {meetingID}: {ex.Message}");
                return false;
            }
        }


        // Updated helper method to check if hook exists
        private async Task<bool> HookExistsAsync(string meetingID, string callbackURL)
        {
            string apiCall = "hooks";
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}";
            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();

            try
            {
                string response = await client.GetStringAsync(url);
                Statics.Log("BBBHookExists Request", url);
                Statics.Log("BBBHookExists Response", response);

                // Parse XML response to check if hook already exists
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);

                // Check for hooks with matching callbackURL
                XmlNodeList hooks = xmlDoc.SelectNodes("//hook");
                foreach (XmlNode hook in hooks)
                {
                    string existingCallbackURL = hook.SelectSingleNode("callbackURL")?.InnerText;
                    if (existingCallbackURL == callbackURL)
                    {
                        Statics.Log("BBBHookExists", $"Hook already exists for meeting {meetingID}");
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Statics.Log("BBBHookExists Error", e.Message);
                return false;
            }
        }

        // New method that checks and creates webhook if needed
        private async Task RegisterWebhookForMeetingAsync(string meetingID)
        {
            string callbackURL = @$"{Statics.URL}/api/bbb-webhook";

            try
            {
                // Check if webhook already exists
                bool hookExists = await HookExistsAsync(meetingID, callbackURL);

                if (!hookExists)
                {
                    // Create new webhook for all events
                    //await CreateAllEventsWebhookAsync(meetingID);
                    Statics.Log("BBBWebhook", $"Webhook registered for meeting {meetingID}");

                }
                else
                {
                    Statics.Log("BBBWebhook", $"Webhook already exists for meeting {meetingID}, skipping creation");
                }
            }
            catch (Exception e)
            {
                Statics.Log("BBBWebhook Error", $"Failed to register webhook for meeting {meetingID}: {e.Message}");
                // Don't throw - webhook failure shouldn't break meeting creation
            }
        }

        // Updated webhook creation method with production URL option
        private async Task CreateAllEventsWebhookAsync(string meetingID)
        {
            string apiCall = "hooks/create";
            // TODO: Change this to your production URL: https://core.hekmateislami.com/api/bbb-webhook
            string callbackURL = @$"{Statics.URL}/api/bbb-webhook";

            // No "&eventID=..." parameter to receive ALL events
            string queryString = $"callbackURL={Uri.EscapeDataString(callbackURL)}" +
                                 $"&meetingID={Uri.EscapeDataString(meetingID)}";

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();

            try
            {
                string response = await client.GetStringAsync(url);
                Statics.Log("BBBHookCreate Response", response);

                // Optional: Parse the hook ID to save for potential deletion later
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(response);
                string hookId = xmlDoc.SelectSingleNode("//hookID")?.InnerText;
                Statics.Log("BBBHookCreate", $"Webhook created with ID: {hookId} for meeting {meetingID}");
            }
            catch (Exception e)
            {
                Statics.Log("BBBHookCreate Error", e.Message);
                throw; // Re-throw to let RegisterWebhookForMeetingAsync handle it
            }
        }

        // Add these methods to your BBBGetMeeting class

        /// <summary>
        /// Creates a webhook for receiving BBB callbacks
        /// </summary>
        /// <param name="callbackURL">The URL where BBB will send POST requests when events occur</param>
        /// <param name="meetingID">Optional: Only receive events for a specific meeting</param>
        /// <returns>Hook object containing hookID and callbackURL</returns>
        public async Task<BBBHookResponse> CreateWebhookAsync(string callbackURL, string meetingID = null)
        {
            if (string.IsNullOrWhiteSpace(callbackURL))
                throw new ArgumentException("callbackURL is required");

            string apiCall = "hooks/create";

            // Build query string
            string queryString = $"callbackURL={Uri.EscapeDataString(callbackURL)}";
            if (!string.IsNullOrWhiteSpace(meetingID))
                queryString += $"&meetingID={Uri.EscapeDataString(meetingID)}";

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                Statics.Log("BBBCreateWebhook Request", url);
                string xmlResponse = await client.GetStringAsync(url);
                Statics.Log("BBBCreateWebhook Response", xmlResponse);

                // Deserialize XML response
                XmlSerializer serializer = new XmlSerializer(typeof(BBBHookResponse));
                using StringReader reader = new StringReader(xmlResponse);
                BBBHookResponse result = (BBBHookResponse)serializer.Deserialize(reader);

                if (result.ReturnCode != "SUCCESS")
                    throw new Exception($"Failed to create webhook: {result.Message}");

                return result;
            }
            catch (Exception e)
            {
                Statics.Log("BBBCreateWebhook Error", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates a webhook for specific event types
        /// </summary>
        /// <param name="callbackURL">The URL where BBB will send POST requests when events occur</param>
        /// <param name="eventIDs">List of specific event IDs to subscribe to (null or empty for all events)</param>
        /// <param name="meetingID">Optional: Only receive events for a specific meeting</param>
        /// <returns>Hook object containing hookID and callbackURL</returns>
        public async Task<BBBHookResponse> CreateWebhookWithEventsAsync(string callbackURL, List<string> eventIDs, string meetingID = null)
        {
            if (string.IsNullOrWhiteSpace(callbackURL))
                throw new ArgumentException("callbackURL is required");

            string apiCall = "hooks/create";

            // Build query string
            string queryString = $"callbackURL={Uri.EscapeDataString(callbackURL)}";

            if (eventIDs != null && eventIDs.Any())
            {
                foreach (var eventID in eventIDs)
                    queryString += $"&eventID={Uri.EscapeDataString(eventID)}";
            }

            if (!string.IsNullOrWhiteSpace(meetingID))
                queryString += $"&meetingID={Uri.EscapeDataString(meetingID)}";

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                Statics.Log("BBBCreateWebhookWithEvents Request", url);
                string xmlResponse = await client.GetStringAsync(url);
                Statics.Log("BBBCreateWebhookWithEvents Response", xmlResponse);

                XmlSerializer serializer = new XmlSerializer(typeof(BBBHookResponse));
                using StringReader reader = new StringReader(xmlResponse);
                BBBHookResponse result = (BBBHookResponse)serializer.Deserialize(reader);

                if (result.ReturnCode != "SUCCESS")
                    throw new Exception($"Failed to create webhook with events: {result.Message}");

                return result;
            }
            catch (Exception e)
            {
                Statics.Log("BBBCreateWebhookWithEvents Error", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Lists all registered webhooks
        /// </summary>
        /// <param name="meetingID">Optional: Filter hooks by meetingID</param>
        /// <returns>List of registered webhooks</returns>
        public async Task<List<BBBHookInfo>> ListWebhooksAsync(string meetingID = null)
        {
            string apiCall = "hooks/list";

            string queryString = "";
            if (!string.IsNullOrWhiteSpace(meetingID))
                queryString = $"meetingID={Uri.EscapeDataString(meetingID)}";

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                Statics.Log("BBBListWebhooks Request", url);
                string xmlResponse = await client.GetStringAsync(url);
                Statics.Log("BBBListWebhooks Response", xmlResponse);

                // Parse XML manually because it has a different structure
                var doc = XDocument.Parse(xmlResponse);

                if (doc.Root?.Element("returncode")?.Value != "SUCCESS")
                {
                    var message = doc.Root?.Element("message")?.Value ?? "Unknown error";
                    throw new Exception($"Failed to list webhooks: {message}");
                }

                var hooks = new List<BBBHookInfo>();
                var hooksElement = doc.Root?.Element("hooks");

                if (hooksElement != null)
                {
                    foreach (var hookElement in hooksElement.Elements("hook"))
                    {
                        var hook = new BBBHookInfo
                        {
                            HookID = hookElement.Element("hookID")?.Value,
                            CallbackURL = hookElement.Element("callbackURL")?.Value,
                            MeetingID = hookElement.Element("meetingID")?.Value,
                            Permanent = bool.TryParse(hookElement.Element("permanent")?.Value, out bool perm) ? perm : false,
                            RawData = hookElement.Element("rawData")?.Value == "true"
                        };

                        // Get event IDs if any
                        var eventIDsElement = hookElement.Element("eventIDs");
                        if (eventIDsElement != null)
                        {
                            hook.EventIDs = eventIDsElement.Elements("eventID")
                                .Select(e => e.Value)
                                .ToList();
                        }

                        hooks.Add(hook);
                    }
                }

                return hooks;
            }
            catch (Exception e)
            {
                Statics.Log("BBBListWebhooks Error", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Destroys/removes a specific webhook
        /// </summary>
        /// <param name="hookID">The ID of the webhook to destroy</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> DestroyWebhookAsync(string hookID)
        {
            if (string.IsNullOrWhiteSpace(hookID))
                throw new ArgumentException("hookID is required");

            string apiCall = "hooks/destroy";
            string queryString = $"hookID={Uri.EscapeDataString(hookID)}";

            string checksumInput = apiCall + queryString + Statics.bbb.Secret;
            string checksum = Statics.ComputeSha1(checksumInput);
            string url = $"{Statics.bbb.UrlAPI}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            try
            {
                Statics.Log("BBBDestroyWebhook Request", url);
                string xmlResponse = await client.GetStringAsync(url);
                Statics.Log("BBBDestroyWebhook Response", xmlResponse);

                var doc = XDocument.Parse(xmlResponse);
                var returncode = doc.Root?.Element("returncode")?.Value;

                return returncode?.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch (Exception e)
            {
                Statics.Log("BBBDestroyWebhook Error", e.Message);
                return false;
            }
        }

        /// <summary>
        /// Destroys all webhooks for a specific meeting
        /// </summary>
        /// <param name="meetingID">The meeting ID to destroy all webhooks for</param>
        /// <returns>Number of webhooks destroyed</returns>
        public async Task<int> DestroyAllWebhooksForMeetingAsync(string meetingID)
        {
            if (string.IsNullOrWhiteSpace(meetingID))
                throw new ArgumentException("meetingID is required");

            var hooks = await ListWebhooksAsync(meetingID);
            int destroyed = 0;

            foreach (var hook in hooks)
            {
                if (await DestroyWebhookAsync(hook.HookID))
                    destroyed++;
            }

            Statics.Log("BBBDestroyAllWebhooks", $"Destroyed {destroyed} webhooks for meeting {meetingID}");
            return destroyed;
        }

        /// <summary>
        /// Gets detailed information about a specific webhook
        /// </summary>
        /// <param name="hookID">The ID of the webhook to get info for</param>
        /// <returns>Webhook information or null if not found</returns>
        public async Task<BBBHookInfo> GetWebhookInfoAsync(string hookID)
        {
            if (string.IsNullOrWhiteSpace(hookID))
                throw new ArgumentException("hookID is required");

            var allHooks = await ListWebhooksAsync();
            return allHooks.FirstOrDefault(h => h.HookID == hookID);
        }

        /// <summary>
        /// Updates an existing webhook's callback URL
        /// </summary>
        /// <param name="hookID">The ID of the webhook to update</param>
        /// <param name="newCallbackURL">The new callback URL</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateWebhookCallbackURLAsync(string hookID, string newCallbackURL)
        {
            if (string.IsNullOrWhiteSpace(hookID))
                throw new ArgumentException("hookID is required");
            if (string.IsNullOrWhiteSpace(newCallbackURL))
                throw new ArgumentException("newCallbackURL is required");

            // BBB doesn't have a direct update endpoint, so we need to destroy and recreate
            var hook = await GetWebhookInfoAsync(hookID);
            if (hook == null)
                return false;

            bool destroyed = await DestroyWebhookAsync(hookID);
            if (!destroyed)
                return false;

            var newHook = await CreateWebhookAsync(newCallbackURL, hook.MeetingID);
            return newHook != null && newHook.ReturnCode == "SUCCESS";
        }


    }
}
