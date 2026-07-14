using Application.DTO;
using Application.DTO.Generals;
using Application.General;
using Application.Interface.IUsecase;
using Application.Usecase;
using AutoMapper;
using CoreMe.Cofigurations;
using CoreMe.Presenter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Presenter;
using System.Web;

namespace CoreMe.Controllers
{
    [ApiController]
    [Route("bbb/hooks")]
    [EnableCors("AllowHekmateSubdomains")]
    public class BBBHookController : ControllerBase
    {
        private readonly ILogger<BBBHookController> _logger;
        private readonly IMapper _mapper;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory; // ⭐ اضافه کنید

        // Webhook UseCases
        public INewHookUseCase _INewHookUseCase { get; }
        public ApiPresenter<NewHookResponse> _NewHookResponse { get; }

        public IGetHooksUseCase _IGetHooksUseCase { get; }
        public ApiPresenter<GetHooksResponse> _GetHooksResponse { get; }

        public IGetHookInfoUseCase _IGetHookInfoUseCase { get; }
        public ApiPresenter<GetHookInfoResponse> _GetHookInfoResponse { get; }

        public IDestroyWebhookAsyncUseCase _IDestroyWebhookAsyncUseCase { get; }
        public ApiPresenter<DestroyWebhookAsyncResponse> _DestroyWebhookAsyncResponse { get; }

        public IDestroyAllWebhooksForMeetingAsyncUseCase _IDestroyAllWebhooksForMeetingAsyncUseCase { get; }
        public ApiPresenter<DestroyAllWebhooksForMeetingAsyncResponse> _DestroyAllWebhooksForMeetingAsyncResponse { get; }

        public IUpdateWebhookCallbackURLAsyncUseCase _IUpdateWebhookCallbackURLAsyncUseCase { get; }
        public ApiPresenter<UpdateWebhookCallbackURLAsyncResponse> _UpdateWebhookCallbackURLAsyncResponse { get; }
        public IBBBWebhookLogUseCase _IBBBWebhookLogUseCase { get; }
        public ApiPresenter<BBBWebhookLogResponse> _BBBWebhookLogResponse { get; }

        public BBBHookController(ILogger<BBBHookController> logger, IMapper mapper, IBackgroundTaskQueue taskQueue, IServiceScopeFactory serviceScopeFactory, INewHookUseCase iNewHookUseCase, ApiPresenter<NewHookResponse> newHookResponse, IGetHooksUseCase iGetHooksUseCase, ApiPresenter<GetHooksResponse> getHooksResponse, IGetHookInfoUseCase iGetHookInfoUseCase, ApiPresenter<GetHookInfoResponse> getHookInfoResponse, IDestroyWebhookAsyncUseCase iDestroyWebhookAsyncUseCase, ApiPresenter<DestroyWebhookAsyncResponse> destroyWebhookAsyncResponse, IDestroyAllWebhooksForMeetingAsyncUseCase iDestroyAllWebhooksForMeetingAsyncUseCase, ApiPresenter<DestroyAllWebhooksForMeetingAsyncResponse> destroyAllWebhooksForMeetingAsyncResponse, IUpdateWebhookCallbackURLAsyncUseCase iUpdateWebhookCallbackURLAsyncUseCase, ApiPresenter<UpdateWebhookCallbackURLAsyncResponse> updateWebhookCallbackURLAsyncResponse, IBBBWebhookLogUseCase iBBBWebhookLogUseCase, ApiPresenter<BBBWebhookLogResponse> bBBWebhookLogResponse)
        {
            _logger = logger;
            _mapper = mapper;
            _taskQueue = taskQueue;
            _serviceScopeFactory = serviceScopeFactory;
            _INewHookUseCase = iNewHookUseCase;
            _NewHookResponse = newHookResponse;
            _IGetHooksUseCase = iGetHooksUseCase;
            _GetHooksResponse = getHooksResponse;
            _IGetHookInfoUseCase = iGetHookInfoUseCase;
            _GetHookInfoResponse = getHookInfoResponse;
            _IDestroyWebhookAsyncUseCase = iDestroyWebhookAsyncUseCase;
            _DestroyWebhookAsyncResponse = destroyWebhookAsyncResponse;
            _IDestroyAllWebhooksForMeetingAsyncUseCase = iDestroyAllWebhooksForMeetingAsyncUseCase;
            _DestroyAllWebhooksForMeetingAsyncResponse = destroyAllWebhooksForMeetingAsyncResponse;
            _IUpdateWebhookCallbackURLAsyncUseCase = iUpdateWebhookCallbackURLAsyncUseCase;
            _UpdateWebhookCallbackURLAsyncResponse = updateWebhookCallbackURLAsyncResponse;
            _IBBBWebhookLogUseCase = iBBBWebhookLogUseCase;
            _BBBWebhookLogResponse = bBBWebhookLogResponse;
        }





        /// <summary>
        /// Create a new webhook for receiving BBB callbacks
        /// </summary>
        /// <param name="callbackURL">The URL where BBB will send POST requests when events occur</param>
        /// <param name="meetingID">Optional: Only receive events for a specific meeting</param>
        /// <param name="eventIDs">Optional: Comma-separated list of specific event IDs to subscribe to</param>
        /// <returns>Hook information</returns>
        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> CreateWebhook(
            [FromQuery] string callbackURL,
            [FromQuery] string? meetingID = null,
            [FromQuery] string? eventIDs = null)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new NewHookRequest
            {
                GeneralHeaders = headers,
                CallbackURL = callbackURL,
                MeetID = meetingID,
                EventIDs = !string.IsNullOrEmpty(eventIDs)
                    ? eventIDs.Split(',').Select(e => e.Trim()).ToList()
                    : null
            };

            await _INewHookUseCase.handleAsync(request, _NewHookResponse);
            return _NewHookResponse.jsonContentResult;
        }

        /// <summary>
        /// Create a webhook for all events
        /// </summary>
        [HttpPost("create-all")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> CreateWebhookForAllEvents(
            [FromQuery] string callbackURL,
            [FromQuery] string? meetingID = null)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new NewHookRequest
            {
                GeneralHeaders = headers,
                CallbackURL = callbackURL,
                MeetID = meetingID,
                EventIDs = BBBWebhookHelper.GetAllEvents() // Use all events
            };

            await _INewHookUseCase.handleAsync(request, _NewHookResponse);
            return _NewHookResponse.jsonContentResult;
        }

        /// <summary>
        /// Create a webhook for recording-related events only
        /// </summary>
        [HttpPost("create-recording")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> CreateWebhookForRecordingEvents(
            [FromQuery] string callbackURL,
            [FromQuery] string? meetingID = null)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new NewHookRequest
            {
                GeneralHeaders = headers,
                CallbackURL = callbackURL,
                MeetID = meetingID,
                EventIDs = BBBWebhookHelper.GetRecordingEvents()
            };

            await _INewHookUseCase.handleAsync(request, _NewHookResponse);
            return _NewHookResponse.jsonContentResult;
        }

        /// <summary>
        /// Create a webhook for participant events only
        /// </summary>
        [HttpPost("create-participant")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> CreateWebhookForParticipantEvents(
            [FromQuery] string callbackURL,
            [FromQuery] string? meetingID = null)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new NewHookRequest
            {
                GeneralHeaders = headers,
                CallbackURL = callbackURL,
                MeetID = meetingID,
                EventIDs = BBBWebhookHelper.GetParticipantEvents()
            };

            await _INewHookUseCase.handleAsync(request, _NewHookResponse);
            return _NewHookResponse.jsonContentResult;
        }

        /// <summary>
        /// Get all registered webhooks
        /// </summary>
        /// <param name="meetingID">Optional: Filter by meeting ID</param>
        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetHooks([FromQuery] string? meetingID = null)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new GetHooksRequest
            {
                GeneralHeaders = headers,
                MeetingID = meetingID
            };

            await _IGetHooksUseCase.handleAsync(request, _GetHooksResponse);
            return _GetHooksResponse.jsonContentResult;
        }

        /// <summary>
        /// Get detailed information about a specific webhook
        /// </summary>
        /// <param name="hookID">The ID of the webhook</param>
        [HttpGet("info/{hookID}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> GetHookInfo([FromRoute] string hookID)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new GetHookInfoRequest
            {
                GeneralHeaders = headers,
                HookID = hookID
            };

            await _IGetHookInfoUseCase.handleAsync(request, _GetHookInfoResponse);
            return _GetHookInfoResponse.jsonContentResult;
        }

        /// <summary>
        /// Destroy/delete a specific webhook
        /// </summary>
        /// <param name="hookID">The ID of the webhook to destroy</param>
        [HttpDelete("destroy/{hookID}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> DestroyWebhook([FromRoute] string hookID)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new DestroyWebhookAsyncRequest
            {
                GeneralHeaders = headers,
                HookID = hookID
            };

            await _IDestroyWebhookAsyncUseCase.handleAsync(request, _DestroyWebhookAsyncResponse);
            return _DestroyWebhookAsyncResponse.jsonContentResult;
        }

        /// <summary>
        /// Destroy all webhooks for a specific meeting
        /// </summary>
        /// <param name="meetingID">The meeting ID</param>
        [HttpDelete("destroy-all/{meetingID}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> DestroyAllWebhooksForMeeting([FromRoute] string meetingID)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new DestroyAllWebhooksForMeetingAsyncRequest
            {
                GeneralHeaders = headers,
                MeetingID = meetingID
            };

            await _IDestroyAllWebhooksForMeetingAsyncUseCase.handleAsync(request, _DestroyAllWebhooksForMeetingAsyncResponse);
            return _DestroyAllWebhooksForMeetingAsyncResponse.jsonContentResult;
        }

        /// <summary>
        /// Update a webhook's callback URL
        /// </summary>
        /// <param name="hookID">The ID of the webhook to update</param>
        /// <param name="newCallbackURL">The new callback URL</param>
        [HttpPut("update/{hookID}")]
        [Authorize(AuthenticationSchemes = "vc2AuthNorm")]
        public async Task<IActionResult> UpdateWebhookCallbackURL(
            [FromRoute] string hookID,
            [FromQuery] string newCallbackURL)
        {
            var headers = HttpContext.GetGeneralHeaders();

            var request = new UpdateWebhookCallbackURLAsyncRequest
            {
                GeneralHeaders = headers,
                HookID = hookID,
                NewCallbackURL = newCallbackURL
            };

            await _IUpdateWebhookCallbackURLAsyncUseCase.handleAsync(request, _UpdateWebhookCallbackURLAsyncResponse);
            return _UpdateWebhookCallbackURLAsyncResponse.jsonContentResult;
        }



        [HttpPost("logEvent")]
        [AllowAnonymous]
        public async Task<IActionResult> LogEvent()
        {
            try
            {
                var rawBody = await new StreamReader(Request.Body).ReadToEndAsync();
                Statics.Log("BBBWebhookRaw", rawBody);

                var formData = HttpUtility.ParseQueryString(rawBody);

                string domain = formData["domain"];
                string eventDataJson = formData["event"];
                string timestampStr = formData["timestamp"];

                if (string.IsNullOrEmpty(eventDataJson))
                {
                    Statics.Log("BBBWebhookError", "No event data received");
                    return Ok(new { status = "received", message = "No event data" });
                }

                var eventsArray = JsonConvert.DeserializeObject<JArray>(eventDataJson);

                if (eventsArray == null || !eventsArray.Any())
                {
                    Statics.Log("BBBWebhookError", "Invalid event data format");
                    return Ok(new { status = "received", message = "Invalid event data" });
                }

                // ⭐ QUEUE EACH EVENT WITH A NEW SCOPE
                foreach (var eventItem in eventsArray)
                {
                    var eventItemCopy = eventItem;
                    var domainCopy = domain;
                    var timestampStrCopy = timestampStr;
                    var headersCopy = HttpContext.GetGeneralHeaders();

                    _taskQueue.QueueBackgroundWorkItem(async (cancellationToken) =>
                    {
                        // ⭐ ایجاد اسکوپ جدید برای هر تسک
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            // ⭐ دریافت سرویس‌ها از اسکوپ جدید
                            var bBBWebhookLogUseCase = scope.ServiceProvider.GetRequiredService<IBBBWebhookLogUseCase>();
                            var bBBWebhookLogResponse = scope.ServiceProvider.GetRequiredService< ApiPresenter<BBBWebhookLogResponse> >();

                            try
                            {
                                var data = eventItemCopy["data"];
                                var eventObj = eventItemCopy["event"];

                                if (data == null) return;

                                var eventId = data["id"]?.ToString();
                                var attributes = data["attributes"];

                                var request = new BBBWebhookLogRequest
                                {
                                    HookID = null,
                                    MeetingID = attributes?["meeting"]?["external-meeting-id"]?.ToString()
                               ?? attributes?["meeting"]?["externalMeetingId"]?.ToString(),
                                    InternalMeetingID = attributes?["meeting"]?["internal-meeting-id"]?.ToString()
                                        ?? attributes?["meeting"]?["internalMeetingId"]?.ToString(),
                                    Event = eventId,
                                    Domain = domainCopy,
                                    Timestamp = eventObj?["ts"] != null
                                        ? DateTimeOffset.FromUnixTimeMilliseconds(eventObj["ts"].Value<long>()).UtcDateTime
                                        : (timestampStrCopy != null ? DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timestampStrCopy)).UtcDateTime : DateTime.UtcNow),
                                    Data = new Dictionary<string, object>()
                                };

                                request.Data["rawData"] = eventItemCopy.ToString(Formatting.None);
                                request.Data["domain"] = domainCopy;
                                request.Data["timestamp"] = timestampStrCopy;

                                if (attributes?["meeting"] != null)
                                {
                                    var meeting = attributes["meeting"];
                                    request.Data["meeting"] = new
                                    {
                                        internalMeetingId = meeting["internal-meeting-id"]?.ToString(),
                                        externalMeetingId = meeting["external-meeting-id"]?.ToString(),
                                        name = meeting["name"]?.ToString(),
                                        isBreakout = meeting["is-breakout"]?.Value<bool>(),
                                        duration = meeting["duration"]?.Value<int>(),
                                        createTime = meeting["create-time"]?.Value<long>(),
                                        record = meeting["record"]?.Value<bool>(),
                                        metadata = meeting["metadata"]
                                    };
                                }

                                if (attributes?["user"] != null && (eventId == "user-joined" || eventId == "user-left" || eventId == "user-presenter-assigned"))
                                {
                                    var user = attributes["user"];
                                    request.Data["user"] = new
                                    {
                                        internalUserId = user["internal-user-id"]?.ToString(),
                                        externalUserId = user["external-user-id"]?.ToString(),
                                        name = user["name"]?.ToString(),
                                        role = user["role"]?.ToString(),
                                        presenter = user["presenter"]?.Value<bool>(),
                                        userdata = user["userdata"]
                                    };

                                    if (user["userdata"] != null)
                                    {
                                        var userdata = user["userdata"];
                                        request.NationalCode = userdata["nationalCode"]?.ToString();
                                        request.Phone = userdata["phone"]?.ToString();
                                        request.UserId = userdata["userID5"]?.ToString();
                                        request.Vc2AuthNorm = userdata["vc2AuthNorm"]?.ToString();
                                    }
                                }


                                request.GeneralHeaders = headersCopy;

                                // ⭐ استفاده از سرویس‌های اسکوپ جدید
                                await bBBWebhookLogUseCase.handleAsync(request, bBBWebhookLogResponse);
                            }
                            catch (Exception ex)
                            {
                                Statics.Log($"BBBWebhookQueueError_{eventItemCopy?["data"]?["id"]?.ToString() ?? "unknown"}", ex.Message + "\n" + ex.StackTrace);
                            }
                        }
                    });
                }

                return Ok(new { status = "success", processed = eventsArray.Count, queued = true });
            }
            catch (Exception ex)
            {
                Statics.Log("BBBWebhookError", ex.Message + "\n" + ex.StackTrace);
                return Ok(new { status = "received", message = "Event received but processing failed", error = ex.Message });
            }
        }

        // Simple version for just logging

    }
}