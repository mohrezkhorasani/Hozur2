using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface ISendSMS 
    {
        public Task<bool> SendReservation(string phone, string name, string topic, string meeting, string time, string date, string timeSend);
    }
}
