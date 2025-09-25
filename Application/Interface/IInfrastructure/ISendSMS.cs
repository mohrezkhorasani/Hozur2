using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface.IInfrastructure
{
    public interface ISendSMS 
    {
        public Task<int> SendAsync(int type,string cellphone, string Token1,string Token2, string Token3);
    }
}
