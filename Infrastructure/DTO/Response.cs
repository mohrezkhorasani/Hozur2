using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Infrastructure.DTO
{
    [XmlRoot("response")]
    public class BBBResponse
    {
        [XmlElement("returncode")]
        public string ReturnCode { get; set; }

        [XmlElement("meetings")]
        public Meetings Meetings { get; set; }
    }

    public class Meetings
    {
        [XmlElement("meeting")]
        public List<Meeting> MeetingList { get; set; }
    }

    public class Meeting
    {
        [XmlElement("meetingID")]
        public string MeetingID { get; set; }

        [XmlElement("meetingName")]
        public string MeetingName { get; set; }

        [XmlElement("createTime")]
        public string CreateTime { get; set; }

        [XmlElement("attendeePW")]
        public string AttendeePW { get; set; }

        [XmlElement("moderatorPW")]
        public string ModeratorPW { get; set; }

        [XmlElement("internalMeetingID")]
        public string InternalMeetingID { get; set; }

        [XmlElement("running")]
        public string Running { get; set; }

        [XmlElement("participantCount")]
        public int ParticipantCount { get; set; }

        [XmlElement("moderatorCount")]
        public int ModeratorCount { get; set; }

        [XmlElement("hasUserJoined")]
        public string HasUserJoined { get; set; }

        [XmlElement("recording")]
        public string Recording { get; set; }

        [XmlElement("attendees")]
        public Attendees Attendees { get; set; }

        [XmlElement("metadata")]
        public MetaData MetaDatas { get; set; }
    }

    public class Attendees
    {
        [XmlElement("attendee")]
        public List<Attendee> AttendeeList { get; set; }
    }

    public class Attendee
    {
        [XmlElement("userID")]
        public string UserID { get; set; }

        [XmlElement("fullName")]
        public string FullName { get; set; }

        [XmlElement("role")]
        public string Role { get; set; }

        [XmlElement("isPresenter")]
        public string IsPresenter { get; set; }

        [XmlElement("isListeningOnly")]
        public string IsListeningOnly { get; set; }

        [XmlElement("hasJoinedVoice")]
        public string HasJoinedVoice { get; set; }

        [XmlElement("hasVideo")]
        public string HasVideo { get; set; }

        [XmlElement("clientType")]
        public string ClientType { get; set; }

        [XmlElement("customdata")]
        public CustomData CustomData { get; set; }
    }

    public class CustomData
    {
        [XmlElement("nationalCode")]
        public string NationalCode { get; set; }

        [XmlElement("phone")]
        public string Phone { get; set; }

        [XmlElement("userID5")]
        public string UserID5 { get; set; }

        [XmlElement("vc2AuthNorm")]
        public string Vc2AuthNorm { get; set; }

        [XmlAnyElement]
        public List<System.Xml.XmlElement> AnyElements { get; set; }
    }

    public class MetaData
    {
        [XmlElement("roomid")]
        public string RoomID { get; set; }

        [XmlElement("public")]
        public bool RoomPublic { get; set; }

        [XmlElement("supprivate")]
        public bool IsSuperPrivate { get; set; }
        [XmlElement("smvbem")]
        public bool IsShowMemberVIPBeforeEndTheMeeting { get; set; }

        [XmlElement("owner")]
        public long OwnerID { get; set; }

        [XmlElement("admins")]
        public string AdminIDsString { get; set; }

        [XmlIgnore]
        public List<long> AdminIDs
        {
            get
            {
                if (string.IsNullOrEmpty(AdminIDsString))
                    return new List<long>();
                return AdminIDsString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(long.Parse)
                    .ToList();
            }
            set
            {
                AdminIDsString = value != null && value.Any()
                    ? string.Join(",", value)
                    : null;
            }
        }
    }
    // Add these response model classes

    [XmlRoot("response")]
    public class BBBHookResponse
    {
        [XmlElement("returncode")]
        public string ReturnCode { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("messageKey")]
        public string MessageKey { get; set; }

        [XmlElement("hookID")]
        public string HookID { get; set; }

        [XmlElement("callbackURL")]
        public string CallbackURL { get; set; }
    }

    public class BBBHookInfo
    {
        public string HookID { get; set; }
        public string CallbackURL { get; set; }
        public string MeetingID { get; set; }
        public bool Permanent { get; set; }
        public bool RawData { get; set; }
        public List<string> EventIDs { get; set; } = new List<string>();
    }

    // Optional: Add constants for common event types
    public static class BBBWebhookEvents
    {
        public const string MeetingCreated = "meeting-created";
        public const string MeetingDestroyed = "meeting-destroyed";
        public const string MeetingEnded = "meeting-ended";
        public const string UserJoined = "user-joined";
        public const string UserLeft = "user-left";
        public const string UserJoinedVoice = "user-joined-voice";
        public const string UserLeftVoice = "user-left-voice";
        public const string UserSharedCamera = "user-shared-camera";
        public const string UserUnsharedCamera = "user-unshared-camera";
        public const string UserStartedSharingScreen = "user-started-sharing-screen";
        public const string UserStoppedSharingScreen = "user-stopped-sharing-screen";
        public const string RecordingStarted = "recording-started";
        public const string RecordingStopped = "recording-stopped";
        public const string ParticipantJoined = "participant-joined";
        public const string ParticipantLeft = "participant-left";
    }

    [XmlRoot("response")]
    public class BBBCreateResponse
    {
        [XmlElement("returncode")]
        public string ReturnCode { get; set; }

        [XmlElement("meetingID")]
        public string MeetingID { get; set; }

        [XmlElement("messageKey")]
        public string MessageKey { get; set; }

        [XmlElement("internalMeetingID")]
        public string InternalMeetingID { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }
    }

    [XmlRoot("response")]
    public class BBBRunningResponse
    {
        [XmlElement("returncode")]
        public string ReturnCode { get; set; }

        [XmlElement("running")]
        public string Running { get; set; }
    }


}