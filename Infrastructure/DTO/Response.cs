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

        [XmlElement("owner")]
        public long OwnerID { get; set; }
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