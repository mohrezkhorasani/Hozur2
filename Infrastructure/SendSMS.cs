using Application.Interface.IInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Infrastracture
{
    public class SendSMS : ISendSMS
    {
        public class SendSMSModel
        {
            public string token { set; get; }
            public string mobile { set; get; }
            public string message { set; get; }
        }

        public async Task<int> SendAsync(int type, string cellphone, string Token1, string Token2, string Token3)
        {
                string requestJson = "";
                if (type == 1)
                {
                    requestJson = "receptor=" + cellphone + "&token=" + Token1+ "&template=verifykhodkar724";

                }

                string serviceurl = "https://api.kavenegar.com/v1/7A342F786E346167544961464E4E6759504238656934307644596F535A52473479716135504B4A2F6769413D/verify/lookup.json?" + requestJson;
                var c = new HttpClient();
                var r = c.GetAsync(serviceurl).Result;


            //string requestJson = "";
            //if (type == 1)
            //{
            //    requestJson = "receptor=" + cellphone + "&token=" + Token1 + "&template=verifysindokht";

            //}

            //string serviceurl = "https://api.kavenegar.com/v1/4F3149636555626B307736503270446970706E4971633970364B6F7864386153/verify/lookup.json?" + requestJson;
            //var client = new HttpClient();
            //var response = client.GetAsync(serviceurl).Result;

            return 1;
        }

    }
}
