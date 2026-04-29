using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CacheHandler.Events
{
    public class PlanUpdateEvent : INotification
    {
        public string FriendlyID{ get; set; }
        public DateTime OccurredOn { get; set; } = DateTime.Now;
        public string Action { get; set; }
    }
 

}
