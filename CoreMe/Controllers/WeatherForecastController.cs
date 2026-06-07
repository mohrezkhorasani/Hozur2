using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;

namespace CoreMe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet("GetCredit")]
        public async Task<IActionResult> GetCredit()
        {
            var r = await GetCreditWithHttpClient("09127482529", "6pK-]EMXr#*}TaU");
            return Ok(r);

        }
        [HttpPost("AddScedul")]
        public async Task<IActionResult> AddScedul()
        {
            // محاسبه زمان نیم ساعت بعد از الان با فرمت مورد نیاز API
            string scheduleDateTime = DateTime.Now.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss");

            var result = await AddScheduleWithHttpClient(
                username: "09127482529",
                password: "6pK-]EMXr#*}TaU",
                from: "500019900091390",
                to: "09129382449",  // برای چند شماره از ; استفاده کنید: "09129382449;09121111111"
                scheduleDateTime: scheduleDateTime,
                text: "پیامک آزمایشی",
                isflash: false,
                period: "Once"
            );

            // بررسی نتیجه (طبق مستندات: کدهای >100 موفق هستند)
            if (result > 100)
            {
                return Ok(new { success = true, scheduleId = result, message = $"پیامک با کد {result} با موفقیت ثبت شد" });
            }
            else
            {
                return BadRequest(new { success = false, errorCode = result, message = GetErrorMessage(result) });
            }
        }

        // تابع کمکی برای نمایش پیام خطا
        [NonAction]
        private string GetErrorMessage(int code)
        {
            return code switch
            {
                0 => "نام کاربری یا رمز عبور صحیح نمی‌باشد",
                100 => "شما مجاز به استفاده از وب سرویس نمی‌باشید",
                _ => "خطای ناشناخته"
            };
        }




        [NonAction]
        public async Task<double> GetCreditWithHttpClient(string username, string password)
        {
            // ساخت درخواست SOAP برای متد GetCredit
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                       xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                       xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
          <soap:Body>
            <GetCredit xmlns=""http://tempuri.org/"">
              <username>{username}</username>
              <password>{password}</password>
            </GetCredit>
          </soap:Body>
        </soap:Envelope>";

            using var client = new HttpClient();
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");

            // هدر SOAPAction مخصوص این متد (طبق مستندات وب‌سرویس)
            client.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/GetCredit");

            // آدرس وب‌سرویس برای متدهای اعتبار و ارسال عادی
            var response = await client.PostAsync("https://linepayamak.ir/Post/Send.asmx", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            // استخراج مقدار اعتبار (از نوع double) از پاسخ XML
            var xmlDoc = XDocument.Parse(responseBody);
            XNamespace ns = "http://tempuri.org/";
            double credit = (double)xmlDoc.Descendants(ns + "GetCreditResult").First();

            return credit;
        }

        [NonAction]
        public async Task<int> AddScheduleWithHttpClient(
            string username, string password, string from,
            string to, string scheduleDateTime, string text,
            bool isflash, string period)
        {
            string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                       xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                       xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
          <soap:Body>
            <AddSchedule xmlns=""http://tempuri.org/"">
              <username>{username}</username>
              <password>{password}</password>
              <from>{from}</from>
              <to>{to}</to>
              <scheduleDateTime>{scheduleDateTime}</scheduleDateTime>
              <text>{text}</text>
              <isflash>{isflash.ToString().ToLower()}</isflash>
              <period>{period}</period>
            </AddSchedule>
          </soap:Body>
        </soap:Envelope>";

            using var client = new HttpClient();
            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            client.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/AddSchedule");

            var response = await client.PostAsync("https://linepayamak.ir/Post/Schedule.asmx", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            // استخراج کد بازگشتی (اگر >100 باشد موفق است)
            var xmlDoc = XDocument.Parse(responseBody);
            XNamespace ns = "http://tempuri.org/";
            int resultCode = (int)xmlDoc.Descendants(ns + "AddScheduleResult").First();

            return resultCode;
        }

    }
}