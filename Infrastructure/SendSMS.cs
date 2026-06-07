using Application.Interface.IInfrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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

      
        public async Task<bool> SendReservation(string phone = "09129382449", string name = "محمدرضا خراسانی", string topic = "نشست علمی قم", 
            string meeting = "امتداد صدرایی در نظر حکیم بزرگ ملا صدرا", string time = "18:30",string dateSend= "2026-05-24", string timesend= "22:30:00")
        {
            using var client = new HttpClient();

            // Create the message text
            string messageText = $"سلام {name} عزیز،\n" +
                                $"جلسه {topic} با موضوع \"{meeting}\" \n" +
                                $"ساعت {time} برگزار می‌شود.\n";

            var requestBody = new
            {
                text = messageText,
                line_number = "50002178584000",
                number_format = "english",
                schedule = $"{dateSend} {timesend}",
                recipients=new string[] {phone.Replace("+989","09")}
            };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Add the API key to headers
            client.DefaultRequestHeaders.Add("Api-Key", "iOB6evopI0ht6SVhNWxYoKIXXMiRj8ZaZZmUEVerNvOuYNzvqI");

            // Optional: Add phone number to headers if required by the API
            // client.DefaultRequestHeaders.Add("X-Phone-Number", phone);

            try
            {
                var response = await client.PostAsync("https://api.iranpayamak.com/ws/v1/sms/simple", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ SMS sent successfully to {phone}");
                    Console.WriteLine($"📝 Response: {responseString}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"❌ Failed to send SMS to {phone}");
                    Console.WriteLine($"Status: {response.StatusCode}");
                    Console.WriteLine($"Response: {responseString}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"🌐 Network error: {ex.Message}");
                return false;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"📄 JSON error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Unexpected error: {ex.Message}");
                return false;
            }
        }



    }
}
