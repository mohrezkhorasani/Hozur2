using Application.DTO;
using Application.DTO.Models;
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
            return result.Meetings.MeetingList.Where(e => e.MetaDatas.OwnerID == user.ID).Select(e => new MiniRoomJSON
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
             (e.MetaDatas.OwnerID == user.ID && user.ID==currentUser.ID)
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
        public async Task<MeetJSON> CreateMeetingAsync(RoomTBL room)
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
                                 $"&logo={Uri.EscapeDataString($"{URL}/images/logo.png")}" +

                                 $"&metadata={Uri.EscapeDataString(metadataXml)}";
            queryString += $"&meta_public={Uri.EscapeDataString(room.IsPublicRoom.ToString().ToLower())}";
            queryString += $"&meta_roomID={Uri.EscapeDataString(room.FriendlyID)}";
            queryString += $"&meta_owner={Uri.EscapeDataString(room.IDOwnerTBL.ToString())}";
            queryString += $"&meta_supprivate={Uri.EscapeDataString(room.IsSuperPrivateRoom.ToString().ToLower())}";
            queryString += $"&bbb-context-id={Uri.EscapeDataString(room.FriendlyID)}";


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
        public async Task<string> GetJoinUrlAsync(MeetingTBL meet, string fullName, string userId,string nationalCode,string phone=""
            ,string vc2AuthNorm="", bool asModerator = false, string moderatorPassowrd = "")
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
            if(!string.IsNullOrEmpty(nationalCode))
                queryString += $"&userdata-nationalCode={Uri.EscapeDataString(nationalCode)}";
            queryString += $"&userdata-vc2AuthNorm={Uri.EscapeDataString(vc2AuthNorm)}";
            if(!string.IsNullOrEmpty(phone))
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
            if (string.IsNullOrWhiteSpace(meetingId))
                throw new ArgumentException("meetingId is required");

            string apiCall = "getRecordings";

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

            var recordings = new List<RecordingInfo>();

            foreach (var rec in doc.Root.Element("recordings")?.Elements("recording") ?? Enumerable.Empty<XElement>())
            {
                var playback = rec.Element("playback")?.Element("format");
                var urlElement = playback?.Element("url");

                recordings.Add(new RecordingInfo
                {
                    RecordID = rec.Element("recordID")?.Value ?? "",
                    MeetingID = rec.Element("meetingID")?.Value ?? "",
                    Name = rec.Element("name")?.Value ?? "",
                    Published = bool.TryParse(rec.Element("published")?.Value, out bool p) && p,
                    StartTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(rec.Element("startTime")?.Value ?? "0")).UtcDateTime,
                    EndTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(rec.Element("endTime")?.Value ?? "0")).UtcDateTime,
                    PlaybackFormat = playback?.Element("type")?.Value ?? "",
                    PlaybackUrl = urlElement?.Value,
                    Size = long.TryParse(rec.Element("size")?.Value, out long s) ? s : 0,
                    Thumbnail = playback?.Element("preview")?.Elements("image")
                                .FirstOrDefault(i => i.Attribute("alt").Value.Contains("thumbnail"))?.Value
                });
            }

            return recordings;
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
                    await CreateAllEventsWebhookAsync(meetingID);
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
    }
}
