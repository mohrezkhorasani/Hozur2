using Application.DTO;
using Application.General;
using Application.Interface.IUsecase;
using AutoMapper;
using CoreMe.VM;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Presenter;

namespace CoreMe.Controllers
{
    [ApiController]
    [Route("bbbHook")]
    public class BBBHookController : ControllerBase
    {
        private readonly ILogger<BBBHookController> _logger;
        private readonly IWebHostEnvironment _hosting;
        public ICronJobCheckLiveMeetingTaskUseCase _ICronJobCheckLiveMeetingTaskUseCase { get; }
        public ApiPresenter<CronJobCheckLiveMeetingTaskResponse> _CronJobCheckLiveMeetingTaskResponse { get; }
        public IBackupDBUseCase _IBackupDBUseCase { get; }
        public ApiPresenter<BackupDBResponse> _BackupDBResponse { get; }

        public BBBHookController(ILogger<BBBHookController> logger, IWebHostEnvironment hosting, ICronJobCheckLiveMeetingTaskUseCase iCronJobCheckLiveMeetingTaskUseCase, ApiPresenter<CronJobCheckLiveMeetingTaskResponse> cronJobCheckLiveMeetingTaskResponse, IBackupDBUseCase iBackupDBUseCase, ApiPresenter<BackupDBResponse> backupDBResponse)
        {
            _logger = logger;
            _hosting = hosting;
            _ICronJobCheckLiveMeetingTaskUseCase = iCronJobCheckLiveMeetingTaskUseCase;
            _CronJobCheckLiveMeetingTaskResponse = cronJobCheckLiveMeetingTaskResponse;
            _IBackupDBUseCase = iBackupDBUseCase;
            _BackupDBResponse = backupDBResponse;
        }

        [HttpGet("/api/bbb-webhook")]
        public async Task<IActionResult> BbbWebhook()
        {
            Statics.Log("BBBWebhook", $"hook called");
            try
            {
                string rawQueryString = Request.QueryString.ToString();
                Statics.Log("BBBWebhook", $"Raw form data:{rawQueryString}");


                // Read the raw form data that BBB sends
                var formData = await Request.ReadFormAsync();

                // Log the raw data for debugging
                Statics.Log("BBBWebhook", $"Raw form data: {string.Join(", ", formData.Keys)}");

                // BBB sends data as form fields, not JSON
                // The event data is in the "event" field as a JSON string
                string eventJson = formData["event"];

                if (string.IsNullOrEmpty(eventJson))
                {
                    Statics.Log("BBBWebhook", "No event data received");
                    return Ok();
                }

                Statics.Log("BBBWebhook", $"Event JSON: {eventJson}");

                // Parse the JSON string
                JObject payload = JObject.Parse(eventJson);

                // Extract event type - different path in BBB webhook structure
                var eventType = payload["data"]?["attributes"]?["id"]?.ToString();

                Statics.Log("BBBWebhook", $"Received event: {eventType}");

                switch (eventType)
                {
                    case "meeting-created":
                        await HandleMeetingCreated(payload);
                        break;

                    case "user-joined":
                        await HandleUserJoined(payload);
                        break;

                    case "user-left":
                        await HandleUserLeft(payload);
                        break;

                    case "meeting-ended":
                        await HandleMeetingEnded(payload);
                        break;

                    case "user-audio-voice-enabled":
                        await HandleAudioEnabled(payload);
                        break;

                    case "user-audio-voice-disabled":
                        await HandleAudioDisabled(payload);
                        break;

                    case "user-cam-broadcast-start":
                        await HandleCameraStarted(payload);
                        break;

                    case "user-cam-broadcast-end":
                        await HandleCameraStopped(payload);
                        break;

                    case "user-presenter-assigned":
                        await HandlePresenterAssigned(payload);
                        break;

                    case "poll-started":
                        await HandlePollStarted(payload);
                        break;

                    case "chat-group-message-sent":
                        await HandleChatMessage(payload);
                        break;

                    case "rap-published":
                        await HandleRecordingPublished(payload);
                        break;

                    default:
                        Statics.Log("BBBWebhook", $"Unhandled event type: {eventType}");
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Statics.Log("BBBWebhook Error", $"{ex.Message}\n{ex.StackTrace}");
                return Ok(); // Always return 200 - BBB requires success response
            }
        }

        // Separate handler methods for better organization
        private async Task HandleUserJoined(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userId = payload["data"]?["attributes"]?["user"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();
                string userRole = payload["data"]?["attributes"]?["user"]?["role"]?.ToString();

                Statics.Log("BBBWebhook", $"User Joined - Meeting: {meetingId}, User: {userName}, Role: {userRole}");

                // Update your database - add user to participant list
                // var room = await db.Rooms.FirstOrDefaultAsync(r => r.BBB_MeetID == meetingId);
                // Update participant count, etc.
            }
            catch (Exception ex)
            {
                Statics.Log("HandleUserJoined Error", ex.Message);
            }
        }

        private async Task HandleUserLeft(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userId = payload["data"]?["attributes"]?["user"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();
                bool hasLeft = payload["data"]?["attributes"]?["user"]?["has_left"]?.Value<bool>() ?? false;

                Statics.Log("BBBWebhook", $"User Left - Meeting: {meetingId}, User: {userName}");

                // Update your database - remove user from participant list
                // Update participant count
            }
            catch (Exception ex)
            {
                Statics.Log("HandleUserLeft Error", ex.Message);
            }
        }

        private async Task HandleMeetingCreated(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string meetingName = payload["data"]?["attributes"]?["meeting"]?["name"]?.ToString();

                Statics.Log("BBBWebhook", $"Meeting Created - ID: {meetingId}, Name: {meetingName}");

                // Record meeting start time in database
            }
            catch (Exception ex)
            {
                Statics.Log("HandleMeetingCreated Error", ex.Message);
            }
        }

        private async Task HandleMeetingEnded(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();

                Statics.Log("BBBWebhook", $"Meeting Ended - ID: {meetingId}");

                // Clean up all participants for this meeting
                // Mark meeting as ended in database
                // Calculate duration, etc.
            }
            catch (Exception ex)
            {
                Statics.Log("HandleMeetingEnded Error", ex.Message);
            }
        }

        private async Task HandleAudioEnabled(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userId = payload["data"]?["attributes"]?["user"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();

                Statics.Log("BBBWebhook", $"Audio Enabled - Meeting: {meetingId}, User: {userName}");

                // Update user audio status in database
            }
            catch (Exception ex)
            {
                Statics.Log("HandleAudioEnabled Error", ex.Message);
            }
        }

        private async Task HandleAudioDisabled(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userId = payload["data"]?["attributes"]?["user"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();

                Statics.Log("BBBWebhook", $"Audio Disabled - Meeting: {meetingId}, User: {userName}");

                // Update user audio status in database
            }
            catch (Exception ex)
            {
                Statics.Log("HandleAudioDisabled Error", ex.Message);
            }
        }

        private async Task HandleCameraStarted(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();

                Statics.Log("BBBWebhook", $"Camera Started - Meeting: {meetingId}, User: {userName}");
            }
            catch (Exception ex)
            {
                Statics.Log("HandleCameraStarted Error", ex.Message);
            }
        }

        private async Task HandleCameraStopped(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();

                Statics.Log("BBBWebhook", $"Camera Stopped - Meeting: {meetingId}, User: {userName}");
            }
            catch (Exception ex)
            {
                Statics.Log("HandleCameraStopped Error", ex.Message);
            }
        }

        private async Task HandlePresenterAssigned(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();

                Statics.Log("BBBWebhook", $"Presenter Assigned - Meeting: {meetingId}, User: {userName}");
            }
            catch (Exception ex)
            {
                Statics.Log("HandlePresenterAssigned Error", ex.Message);
            }
        }

        private async Task HandlePollStarted(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string pollId = payload["data"]?["attributes"]?["poll"]?["id"]?.ToString();
                string pollQuestion = payload["data"]?["attributes"]?["poll"]?["question"]?.ToString();

                Statics.Log("BBBWebhook", $"Poll Started - Meeting: {meetingId}, Poll ID: {pollId}");
            }
            catch (Exception ex)
            {
                Statics.Log("HandlePollStarted Error", ex.Message);
            }
        }

        private async Task HandleChatMessage(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string userName = payload["data"]?["attributes"]?["user"]?["name"]?.ToString();
                string message = payload["data"]?["attributes"]?["chat"]?["message"]?.ToString();

                Statics.Log("BBBWebhook", $"Chat Message - Meeting: {meetingId}, User: {userName}, Message: {message}");

                // Optionally store chat messages in your database
            }
            catch (Exception ex)
            {
                Statics.Log("HandleChatMessage Error", ex.Message);
            }
        }

        private async Task HandleRecordingPublished(JObject payload)
        {
            try
            {
                string meetingId = payload["data"]?["attributes"]?["meeting"]?["id"]?.ToString();
                string recordingId = payload["data"]?["attributes"]?["recording"]?["id"]?.ToString();
                string recordingUrl = payload["data"]?["attributes"]?["recording"]?["url"]?.ToString();

                Statics.Log("BBBWebhook", $"Recording Published - Meeting: {meetingId}, URL: {recordingUrl}");

                // Update your database with recording URL
                // Notify users that recording is ready
            }
            catch (Exception ex)
            {
                Statics.Log("HandleRecordingPublished Error", ex.Message);
            }
        }

















    }

}
