using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface ITelegramService
    {
        public Task SendTelegramMessageAsync(string message, int topicID = 60, long ChatID = -1003492895671);
        public Task SendTelegramMessageAsync(Exception message, int topicID = 60, long ChatID = -1003492895671);
    }
}
