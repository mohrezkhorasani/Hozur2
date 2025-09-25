using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Generals
{
    public interface IGeneralHeaders
    {
        public string Agent { get; set; }
        public string Token { get; set; }//JWT
        public string RequestedPage { get; set; }
        public string RequestedIP { get; set; }
        public string RequestedDevice { get; set; }
        public Guid IdentificationCode { get; set; }
        public string Referer { get; set; }
        public string RefererGoogle { get; set; }
    }
}
