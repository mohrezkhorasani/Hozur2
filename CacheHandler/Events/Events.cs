using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheHandler.Events
{
    public class MeetingUpdatedEvent : INotification
    {
        public long MeetingId { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.Now;
        public string Action { get; set; }
    }
    public class RoomUpdatedEvent : INotification
    {
        public long RoomId { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.Now;
        public string Action { get; set; }
    }
    public class UserUpdatedEvent : INotification
    {
        public long UserId { get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.Now;
        public string Action { get; set; } // "Create", "Update", "Delete"
    }

}
