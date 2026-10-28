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
        public Task<MeetJSON> CreateMeetingAsync(RoomTBL room, RoomTBLPlanTBL? plan = null);
        public Task<string> GetJoinUrlAsync(MeetingTBL meet, string fullName, string userId, string nationalCode, string phone = ""
            , string vc2AuthNorm = "", bool asModerator = false, string moderatorPassowrd = "");
        public Task<List<RecordingInfo>> GetRecordingsByMeetingId(string meetingId);
        public Task<List<RecordingInfo>> GetRecordingsAsync(string? meetingId = null, string? recordId = null, string? state = null);
        public Task<bool> PublishRecordingsAsync(string recordId, bool publish);
        public Task<bool> DeleteRecordingsAsync(string recordId);
        public Task<bool> UpdateRecordingsAsync(string recordId, Dictionary<string, string> metadata);
        public Task<string> GetRecordingTextTracksAsync(string recordId);
        public Task GetMeeting(string meetingId);
        public Task<bool> EndMeetingAsync(string meetingId, string moderatorPass = "");
        public Task<AttendeeInfo> GetAttendeeByUserId(string roomId, string userId);
        public Task<List<AttendeeInfo>> GetMeetingAttendeesByRoomId(string roomId);

        // Webhook Methods
        public Task<BBBHookResponse> CreateWebhookAsync(string callbackURL, string meetingID = null);
        public Task<BBBHookResponse> CreateWebhookWithEventsAsync(string callbackURL, List<string> eventIDs, string meetingID = null);
        public Task<List<BBBHookInfo>> ListWebhooksAsync(string meetingID = null);
        public Task<bool> DestroyWebhookAsync(string hookID);
        public Task<int> DestroyAllWebhooksForMeetingAsync(string meetingID);
        public Task<BBBHookInfo> GetWebhookInfoAsync(string hookID);
        public Task<bool> UpdateWebhookCallbackURLAsync(string hookID, string newCallbackURL);
    }

    public class RecordingInfo
    {
        public string RecordID { get; set; } = string.Empty;
        public string MeetingID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Published { get; set; }
        public string State { get; set; } = "published";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int LengthMinutes { get; set; }
        public int Participants { get; set; }
        public string PlaybackFormat { get; set; } = "presentation";
        public string? PlaybackUrl { get; set; }
        public long Size { get; set; }
        public string? Thumbnail { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
