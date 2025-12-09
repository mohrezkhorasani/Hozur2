using Application.DTO;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface IBBBGetMeetings
    {
        public Task<List<MiniRoomJSON>> Request(GetLivesRequest req);
        public Task<List<MiniRoomJSON>> GetLivesShort(GetLivesRequest req);
        public Task<MeetJSON> CreateMeetingAsync(string meetingID, string meetingName);
        public Task<string> GetJoinUrlAsync(string meetingID, string fullName,bool asmoderator, string moderatorPassowrd);
        public Task<List<RecordingInfo>> GetRecordingsByMeetingId(string meetingId);
        public Task GetMeeting(string meetingId);
        public Task<bool> EndMeetingAsync(string meetingId,string moderatorPass="");
    }
    public class RecordingInfo
    {
        public string RecordID { get; set; }
        public string MeetingID { get; set; }
        public string Name { get; set; }
        public bool Published { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string PlaybackFormat { get; set; }
        public string PlaybackUrl { get; set; }
        public long Size { get; set; }
        public string Thumbnail { get; set; } // optional
    }
}
