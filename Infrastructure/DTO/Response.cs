using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [XmlElement("metadata")]
        public List<MetaData> MetaDatas { get; set; }
    }
    public class MetaData
    {
        [XmlElement("bbb-context-name")]
        public string RoomName { get; set; }
        [XmlElement("bbb-context-id")]
        public string RoomID { get; set; }
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
