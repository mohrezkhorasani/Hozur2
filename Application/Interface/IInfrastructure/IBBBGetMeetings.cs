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
        // Webhook Methods
        /// <summary>
        /// Creates a webhook for receiving BBB callbacks
        /// </summary>
        /// <param name="callbackURL">The URL where BBB will send POST requests when events occur</param>
        /// <param name="meetingID">Optional: Only receive events for a specific meeting</param>
        /// <returns>Hook object containing hookID and callbackURL</returns>
        public Task<BBBHookResponse> CreateWebhookAsync(string callbackURL, string meetingID = null);

        /// <summary>
        /// Creates a webhook for specific event types
        /// </summary>
        /// <param name="callbackURL">The URL where BBB will send POST requests when events occur</param>
        /// <param name="eventIDs">List of specific event IDs to subscribe to (null or empty for all events)</param>
        /// <param name="meetingID">Optional: Only receive events for a specific meeting</param>
        /// <returns>Hook object containing hookID and callbackURL</returns>
        public Task<BBBHookResponse> CreateWebhookWithEventsAsync(string callbackURL, List<string> eventIDs, string meetingID = null);

        /// <summary>
        /// Lists all registered webhooks
        /// </summary>
        /// <param name="meetingID">Optional: Filter hooks by meetingID</param>
        /// <returns>List of registered webhooks</returns>
        public Task<List<BBBHookInfo>> ListWebhooksAsync(string meetingID = null);

        /// <summary>
        /// Destroys/removes a specific webhook
        /// </summary>
        /// <param name="hookID">The ID of the webhook to destroy</param>
        /// <returns>True if successful, false otherwise</returns>
        public Task<bool> DestroyWebhookAsync(string hookID);

        /// <summary>
        /// Destroys all webhooks for a specific meeting
        /// </summary>
        /// <param name="meetingID">The meeting ID to destroy all webhooks for</param>
        /// <returns>Number of webhooks destroyed</returns>
        public Task<int> DestroyAllWebhooksForMeetingAsync(string meetingID);

        /// <summary>
        /// Gets detailed information about a specific webhook
        /// </summary>
        /// <param name="hookID">The ID of the webhook to get info for</param>
        /// <returns>Webhook information or null if not found</returns>
        public Task<BBBHookInfo> GetWebhookInfoAsync(string hookID);

        /// <summary>
        /// Updates an existing webhook's callback URL
        /// </summary>
        /// <param name="hookID">The ID of the webhook to update</param>
        /// <param name="newCallbackURL">The new callback URL</param>
        /// <returns>True if successful, false otherwise</returns>
        public Task<bool> UpdateWebhookCallbackURLAsync(string hookID, string newCallbackURL);
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
