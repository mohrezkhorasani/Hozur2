using Application.DTO;
using Application.DTO.Models;
using Application.Interface.IUsecase;
using Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface IBBBGetMeetings
    {
        public Task<List<MiniRoomJSON>> Request(GetLivesRequest req, UserTBL user);
        public Task<List<MiniRoomJSON>> Request(GetLivesOfProfileRequest req, UserTBL user, UserTBL currentUser);
        public Task<List<MiniRoomJSON>> Request(GetLivesRequest req);
        public Task<List<MiniRoomJSON>> RequestAll(GetLivesRequest req);
        public Task<List<MiniRoomJSON>> Request(GetLiveWithIDRequest req);
        public Task<List<MiniRoomJSON>> GetLivesShort(GetLivesRequest req);
        public Task<MeetJSON> CreateMeetingAsync(RoomTBL room);
        public Task<string> GetJoinUrlAsync(MeetingTBL meet, string fullName, string userId, string nationalCode, string phone = ""
            , string vc2AuthNorm = "", bool asModerator = false, string moderatorPassowrd = "");
        public Task<List<RecordingInfo>> GetRecordingsByMeetingId(string meetingId);
        public Task GetMeeting(string meetingId);
        public Task<bool> EndMeetingAsync(string meetingId,string moderatorPass="");
        public Task<AttendeeInfo> GetAttendeeByUserId(string roomId, string userId);
        public Task<List<AttendeeInfo>> GetMeetingAttendeesByRoomId(string roomId);
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
