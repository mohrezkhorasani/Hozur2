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
                RoomID=e.CreateTime,
                Title=e.MeetingName
            }).ToList();

        }
        public async Task<bool> CreateMeetingAsync(string meetingID, string meetingName)
        {
            string apiCall = "create";

            // Parameters (can be customized)
            string queryString = $"meetingID={Uri.EscapeDataString(meetingID)}" +
                                 $"&name={Uri.EscapeDataString(meetingName)}" +
                                 $"&attendeePW=ap" +
                                 $"&moderatorPW=mp" +
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
                return result.ReturnCode?.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) == true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
