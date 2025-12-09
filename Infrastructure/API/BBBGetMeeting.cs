using Application.DTO;
using Application.DTO.Models;
using Application.General;
using Application.Interface.IInfrastructure;
using Infrastructure.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Infrastructure.API
{
    public class BBBGetMeeting : IBBBGetMeetings
    {

        public async Task<List<MiniRoomJSON>> Request(GetLivesRequest req)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.BBB_BASE_URL}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Select(e => new MiniRoomJSON
            {
                RoomID = e.MetaDatas[0].RoomID,
                Title = e.MeetingName,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();

        }
        public async Task<List<MiniRoomJSON>> GetLivesShort(GetLivesRequest req)
        {
            string queryString = $"";
            string apiCall = "getMeetings";
            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);
            // Full URL with checksum
            string url = $"{Statics.BBB_BASE_URL}?{queryString}&checksum={checksum}";
            string fullUrl = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string xmlResponse = await client.GetStringAsync(fullUrl);

            // Deserialize XML → Object
            XmlSerializer serializer = new XmlSerializer(typeof(BBBResponse));
            using StringReader reader = new StringReader(xmlResponse);
            BBBResponse result = (BBBResponse)serializer.Deserialize(reader);
            return result.Meetings.MeetingList.Select(e => new MiniRoomJSON
            {
                RoomID = e.InternalMeetingID,
                Title = e.MeetingName,
                Created = new DateTime(long.Parse(e.CreateTime))
            }).ToList();

        }
        public async Task<MeetJSON> CreateMeetingAsync(string meetingID, string meetingName)
        {
            string apiCall = "create";

            // Parameters (can be customized)
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}" + 
                                 $"&name={Uri.EscapeDataString(meetingName)}" +
                                 $"&attendeePW={Statics.BBB_ATTEND_PW}" +
                                 $"&moderatorPW={Statics.BBB_MODERATOR_PW}" +
                                 $"&loginUrl=https://vc.hekmateislami.com/rooms/{meetingID}/join" +
                                 $"&allowStartStopRecording=true" +
                                 $"&autoStartRecording=false";

            // Compute checksum
            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);

            string url = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";

            using HttpClient client = new HttpClient();
            try
            {
                string xmlResponse = await client.GetStringAsync(url);

                // Deserialize XML
                XmlSerializer serializer = new XmlSerializer(typeof(BBBCreateResponse));
                using StringReader reader = new StringReader(xmlResponse);
                BBBCreateResponse result = (BBBCreateResponse)serializer.Deserialize(reader);
                return new MeetJSON
                {
                    MeetID = result.MeetingID,
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public async Task<string> GetJoinUrlAsync(string meetingID, string fullName, bool asModerator = false, string moderatorPassowrd = "")
        {
            if (moderatorPassowrd != Statics.BBB_MODERATOR_PW)
            {
                asModerator = false;
            }

            string apiCall = "join";
            string password = asModerator ? Statics.BBB_MODERATOR_PW : Statics.BBB_ATTEND_PW;

            // Parameters
            string queryString =
                $"fullName={Uri.EscapeDataString(fullName)}" +
                $"&meetingID={Uri.EscapeDataString(meetingID)}" +
                $"&password={Uri.EscapeDataString(password)}" +
                $"&redirect=true";

            // Compute checksum
            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);

            // ✅ Full join URL
            string url = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";
            using HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(url);
            // If you just want to open it or return it:
            return url;

            // Optional: If you want to GET it (usually not needed, since it's a redirect):
            /*

            */
        }   
        public async Task<bool> IsMeetingRunningAsync(string meetingID)
        {
            string apiCall = "isMeetingRunning";

            // Query string
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}";

            // Compute checksum
            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);

            string url = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";

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

            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);

            // Full URL
            string fullUrl = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";

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

            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);

            // Full URL
            string fullUrl = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";

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





        public async Task<bool> EndMeetingAsync(string meetingID,string pass="")
        {
            if (string.IsNullOrWhiteSpace(meetingID))
                return false;

            string apiCall = "end";
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}" +
                                 $"&password={pass}";

            string checksumInput = apiCall + queryString + Statics.BBB_SECRET;
            string checksum = Statics.ComputeSha1(checksumInput);

            string url = $"{Statics.BBB_BASE_URL}{apiCall}?{queryString}&checksum={checksum}";

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
    }
}
