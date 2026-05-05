using Application.DTO.Generals;
using Domain.Entity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Application.General
{
    public static class Statics
    {
        public static BBBConfig bbb { get; set; }

        public static string URL = "https://core.hekmateislami.com/";
        public static string URLFiles = "https://vcfile.hekmateislami.com/";

        private static readonly Regex ChromeRegex = new Regex(@"Chrome/(\d+)", RegexOptions.Compiled);
        private static readonly Regex FirefoxRegex = new Regex(@"Firefox/(\d+)", RegexOptions.Compiled);
        private static readonly Regex SafariRegex = new Regex(@"Safari/(\d+)", RegexOptions.Compiled);
        public static string DEF_AVATAR_PATH = "img/rooms/icons//Apple/bust-in-silhouette_1f464.png";

        public static readonly List<MainEntity_Status> MeetingRunning =
        [
            MainEntity_Status.MEETING_STARTED_LATER,
    MainEntity_Status.MEETING_LIVE
        ];


        public static bool ParseLine(string line, out string device, out string browser, out string os)
        {
            device = "";
            browser = "Unknown";
            os = "Unknown";

            if (string.IsNullOrWhiteSpace(line))
                return false;

            // جدا کردن بر اساس /$$
            var parts = line.Split(new[] { "/$$" }, StringSplitOptions.None);

            if (parts.Length < 2)
                return false;

            // بخش اول: device id
            if (!string.IsNullOrEmpty(parts[0].Trim()))
            {
                device = parts[0].TrimEnd('/').Trim();
            }

            // بخش دوم: User-Agent
            string userAgent = parts[1];

            // تشخیص مرورگر
            if (userAgent.Contains("Firefox"))
            {
                var match = FirefoxRegex.Match(userAgent);
                browser = match.Success ? $"Firefox {match.Groups[1].Value}" : "Firefox";
            }
            else if (userAgent.Contains("Chrome"))
            {
                var match = ChromeRegex.Match(userAgent);
                browser = match.Success ? $"Chrome {match.Groups[1].Value}" : "Chrome";
            }
            else if (userAgent.Contains("Safari"))
            {
                var match = SafariRegex.Match(userAgent);
                browser = match.Success ? $"Safari {match.Groups[1].Value}" : "Safari";
            }

            // تشخیص OS
            if (userAgent.Contains("Windows NT 10.0"))
                os = "Windows 10";
            else if (userAgent.Contains("Windows NT"))
                os = "Windows";
            else if (userAgent.Contains("Android"))
            {
                var androidMatch = Regex.Match(userAgent, @"Android\s+(\d+)");
                os = androidMatch.Success ? $"Android {androidMatch.Groups[1].Value}" : "Android";
            }
            else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                os = "iOS";
            else if (userAgent.Contains("Macintosh"))
                os = "macOS";
            else if (userAgent.Contains("Linux"))
                os = "Linux";

            // اگر بخش سوم وجود داشت و quoted بود، از اون هم استفاده کن (مثل "Windows")
            if (parts.Length > 3 && parts[3 - 1] != null) // parts[2] یعنی بخش سوم
            {
                string quotedOs = parts[2].Trim();
                if (quotedOs.StartsWith("\"") && quotedOs.EndsWith("\""))
                {
                    quotedOs = quotedOs.Trim('"');
                    if (!string.IsNullOrEmpty(quotedOs))
                        os = quotedOs;
                }
            }

            return true;
        }
        #region Accounting
        public static int TYPE_BILL_OF_INCREASE = 1;
        public static int TYPE_BILL_OF_DECREASE = 2;
        public static int TYPE_BILL_OF_INCREASE_UNKNOWN = 3;
        #endregion
        public enum TokenStatus
        {
            Invaid = 1,
            Alive = 2,
            Expired = 3
        }

        public static long CalculatePrice(long price, int decentPercent)
        {
            return price - ((price * decentPercent) / 100);
        }
        public static long CalculatePriceDoer(long price, int decentPercent)
        {
            return ((price * decentPercent) / 100);
        }
        private static readonly Dictionary<string, string> MonthMap = new Dictionary<string, string>
    {
        {"فروردین", "۰۱"},
        {"اردیبهشت", "۰۲"},
        {"خرداد", "۰۳"},
        {"تیر", "۰۴"},
        {"مرداد", "۰۵"},
        {"شهریور", "۰۶"},
        {"مهر", "۰۷"},
        {"آبان", "۰۸"},
        {"آذر", "۰۹"},
        {"دی", "۱۰"},
        {"بهمن", "۱۱"},
        {"اسفند", "۱۲"}
    };
        public static string CombinePath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            // Start with the first part
            string result = paths[0] ?? string.Empty;

            for (int i = 1; i < paths.Length; i++)
            {
                string next = paths[i];
                if (string.IsNullOrEmpty(next))
                    continue;

                // Determine separator based on the current result (if any)
                char separator = result.Contains('/') ? '/' : '\\';

                // Trim trailing separator from result and leading from next
                result = result.TrimEnd('/', '\\');
                next = next.TrimStart('/', '\\');

                result = $"{result}{separator}{next}";
            }

            return result;
        }

        public static string ConvertPersianMonthToNumber(string persianDate)
        {
            foreach (var month in MonthMap)
            {
                if (persianDate.Contains(month.Key))
                {
                    return persianDate.Replace(month.Key, month.Value);
                }
            }
            throw new ArgumentException("ماه نامعتبر است.");
        }
        public static string ToPersianNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] persianDigits = { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
            char[] result = input.ToCharArray();

            for (int i = 0; i < result.Length; i++)
            {
                if (persianDigits.Contains(result[i]))
                {
                    continue;
                }
                if (char.IsDigit(result[i]))
                {
                    // '0' در یونیکد 48 است
                    result[i] = persianDigits[result[i] - '0'];
                }
            }

            return new string(result);
        }

        public static string DecodeUrlString(string url)
        {
            try
            {

                string newUrl;
                while ((newUrl = Uri.UnescapeDataString(url)) != url)
                    url = newUrl;
                return newUrl;
            }
            catch (Exception e)
            {
                return url;
            }
        }
        public static string ComputeSha1(string input)
        {
            using SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
        public static string GetDifferentPrettyDate(DateTime date)
        {
            var timeSpan = (date - DateTime.Now).Duration(); // استفاده از Duration برای گرفتن قدرمطلق اختلاف
            var totalDays = (int)timeSpan.TotalDays;
            var totalHours = timeSpan.Hours;
            var totalMinutes = timeSpan.Minutes;

            // اگر بیشتر از یک روز بود
            if (totalDays > 0)
            {
                return $"{totalDays} روز";
            }

            // اگر ساعت داشت
            if (totalHours > 0)
            {
                string result = $"{totalHours} ساعت";
                if (totalMinutes > 0)
                {
                    result += $" و {totalMinutes} دقیقه";
                }
                return result;
            }

            // اگر فقط دقیقه بود
            if (totalMinutes > 0)
            {
                return $"{totalMinutes} دقیقه";
            }

            return "کمتر از یک دقیقه";
        }
        private static readonly Random _random = new Random();
        private const string Chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        public static string GenerateRandomRoomFriendlyID()
        {
            var parts = new string[4];
            for (int i = 0; i < 4; i++)
            {
                parts[i] = GenerateSegment(3);
            }
            return string.Join("-", parts);
        }

        public static string GenerateSegment(int length)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(Chars[_random.Next(Chars.Length)]);
            }
            return sb.ToString();
        }

        public static string GenerateRoomMeetID()
        {
            // Generate 20 random bytes (SHA-1 length)
            byte[] randomBytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Convert bytes to hex string
            StringBuilder hex = new StringBuilder(40);
            foreach (var b in randomBytes)
            {
                hex.Append(b.ToString("x2"));
            }

            return hex.ToString();
        }
        public class NormalizedQuery
        {
            public string Persian { get; set; }  // نسخه فارسی استاندارد
            public string Arabic { get; set; }   // نسخه عربی معادل (برای پوشش داده‌های قدیمی)
        }

        // تابع کمکی برای نرمالایز کردن کوئری به هر دو شکل ممکن
        public static NormalizedQuery NormalizeQuery(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new NormalizedQuery { Persian = "", Arabic = "" };

            string trimmed = input.Trim();

            // نسخه فارسی استاندارد
            string persian = trimmed
                .Replace("ي", "ی")   // ی عربی → ی فارسی
                .Replace("ي", "ی")   // تکرار برای اطمینان
                .Replace("ى", "ی")   // الف مقصوره → ی
                .Replace("ك", "ک")   // ک عربی → ک فارسی
                .Replace("ة", "ه")   // ته مربوطه → ه
                .Replace("ؤ", "و")   // واو معجم → واو ساده (در برخی موارد)
                .Replace("إ", "ا")   // الف با همزه زیر → الف
                .Replace("أ", "ا")   // الف با همزه بالا
                .Replace("آ", "ا");  // آ → ا (اختیاری، بسته به نیاز)

            // نسخه عربی (برای پوشش داده‌هایی که با کیبورد عربی ذخیره شدن)
            string arabic = trimmed
                .Replace("ی", "ي")   // ی فارسی → ی عربی
                .Replace("ک", "ك")   // ک فارسی → ک عربی
                .Replace("ه", "ة");  // ه → ته مربوطه (در انتهای کلمه معموله)

            return new NormalizedQuery
            {
                Persian = persian,
                Arabic = arabic
            };
        }
        public static string GetPrettyDateBySeconds(long seconds)
        {
            var dif = seconds;
            dif = Math.Abs(dif);
            if (dif > 86400)
            {
                var day = (int)(dif / 86400);
                return day.ToString() + " روز";
            }
            else if (dif > 3600)
            {
                var h = ((int)(dif / 3600));
                return h.ToString() + " ساعت";
            }
            else if (dif > 60)
            {
                var m = ((int)(dif / 60));
                return m.ToString() + " دقیقه";
            }
            else
            {

                return dif.ToString("#.##") + " ثانیه.";
            }

        }
        public static string GenerateSecretKey(int size = 32) // default 32 bytes = 256-bit key
        {
            var randomBytes = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Return as Base64 string (safe to store in appsettings.json)
            return Convert.ToBase64String(randomBytes);
        }
        public static double GetDifferentPrettyDateHour(DateTime date)
        {
            return (date - DateTime.Now).TotalHours;
        }
        public static int GetDifferentPrettyDateDays(DateTime date)
        {
            return (int)(date - DateTime.Now).TotalDays;
        }

        public static string CheckBankExist(string txt)
        {
            int[] pishes = { 603799, 589210, 627648, 627961, 603770, 628023, 627760, 502908, 627412, 622106, 639347, 627488, 621986, 639346, 639607, 621986, 502806, 502938, 603769, 610433, 627353, 589463, 627381 };
            if (pishes.Contains(int.Parse(txt.Substring(0, 6))))
            {
                return txt.Substring(0, 6);
            }
            else
            {
                return "unknown.png";
            }
        }
        public static string GenerateJWTLoginVC2(UserTBL user, IGeneralHeaders headers)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a1b32c5e-8f1d-5a3c-8d5e-2f6a5b4c9d0e"));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
              //_configuration.GetSection("Appsettings:ValIssuerID").Value,
              //_configuration.GetSection("Appsettings:ValIudienceID").Value,
              issuer: "core.hekmateislami.com",
              audience: "vc2.hekmateislami.com",
              notBefore: DateTime.Now.AddSeconds(-1),
                expires: DateTime.Now.AddDays(10),
                claims: new List<Claim>
                {
                    new Claim("UserID", user.UUID),
                    new Claim("Role", user.GetRole()),
                    new Claim("IP", headers.RequestedIP),
                    new Claim("Device", headers.RequestedDevice),
                    new Claim("Agent", headers.Agent),
                    new Claim("Guid",headers.IdentificationCode==null? "": headers.IdentificationCode.ToString()),
                    new Claim("JMT",headers.JWTMeetingToken==null? "": headers.JWTMeetingToken),
                    new Claim("JUT",headers.JWTMeetingUserToken==null? "": headers.JWTMeetingUserToken),
                    new Claim("MIC",headers.MeetIdentificationCode==null? "": headers.MeetIdentificationCode),
                    new Claim("Referer",headers.Referer==null? "": headers.Referer),
                },
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static double ConvertToMili(DateTime date)
        {
            DateTime epochTime = DateTime.Parse("Thu Jan 01 1970 03:30:00");
            var milliseconds = date.Subtract(epochTime).TotalMilliseconds;
            return milliseconds;
        }
        public static string GetPersianDateYear(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetYear(date).ToString();
        }
        public static string GetPersianDateMonth(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            string res = "";
            int d = pc.GetMonth(date);
            if (d < 10) res = "0" + d;
            else res = d + "";
            return res;
        }
        public static string GetPersianDateDay(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            string res = "";
            int d = pc.GetDayOfMonth(date);
            if (d < 10) res = "0" + d;
            else res = d + "";
            return res;
        }
        public static string GetPersianDate(DateTime date)
        {
            PersianCalendar pc = new PersianCalendar();
            return GetPersianDateYear(date) + "/" + GetPersianDateMonth(date) + "/" + GetPersianDateDay(date);
        }

        public static string GetTodayPersianDateString()
        {
            PersianCalendar pc = new PersianCalendar();
            string week = "";
            if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Saturday) week = "شنبه";
            else if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Sunday) week = "یک شنبه";
            else if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Monday) week = "دو شنبه";
            else if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Tuesday) week = "سه شنبه";
            else if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Wednesday) week = "چهار شنبه";
            else if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Thursday) week = "پنج شنبه";
            else if (pc.GetDayOfWeek(DateTime.Now) == DayOfWeek.Friday) week = "جمعه";
            string month = "";
            if (pc.GetMonth(DateTime.Now) == 1) month = "فروردین";
            else if (pc.GetMonth(DateTime.Now) == 2) month = "اردیبهشت";
            else if (pc.GetMonth(DateTime.Now) == 3) month = "خرداد";
            else if (pc.GetMonth(DateTime.Now) == 4) month = "تیر";
            else if (pc.GetMonth(DateTime.Now) == 5) month = "مرداد";
            else if (pc.GetMonth(DateTime.Now) == 6) month = "شهریور";
            else if (pc.GetMonth(DateTime.Now) == 7) month = "مهر";
            else if (pc.GetMonth(DateTime.Now) == 8) month = "آبان";
            else if (pc.GetMonth(DateTime.Now) == 9) month = "آذر";
            else if (pc.GetMonth(DateTime.Now) == 10) month = "دی";
            else if (pc.GetMonth(DateTime.Now) == 11) month = "بهمن";
            else if (pc.GetMonth(DateTime.Now) == 12) month = "اسفند";
            return week + " " + pc.GetDayOfMonth(DateTime.Now) + " " + month + " " + pc.GetYear(DateTime.Now);

        }

        public static DateTime ConvertToDate(double v)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMilliseconds(v)
                .ToLocalTime();
            return dateTime;
        }

        public static string GetTodayAsStringFileName()
        {
            return DateTime.Now.Year + "\\" + DateTime.Now.Month + "\\" + DateTime.Now.Day + "\\";
        }

        public static string GenerateJWTUser(UserTBL user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes("34383f87-02fe-4484-9663-5f3975d9d0e9"));
            string uuid = user.ID.ToString();
            if (user.UUID != null)
                uuid = user.UUID;
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
              //_configuration.GetSection("Appsettings:ValIssuerID").Value,
              //_configuration.GetSection("Appsettings:ValIudienceID").Value,
              issuer: "vc.hekmateislami.com",
              audience: "vc.hekmateislami.com",
              notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(5),
                claims: new List<Claim>
                {
                    new Claim("date",DateTime.Now.ToString()),
                    new Claim("guid", user.AllowedGUID.Last().ToString()),
                    new Claim("uuid", uuid),
                    new Claim("secret", GenerateSecretKey(128)),
                    new Claim("api-key", GenerateSecretKey(32)),
                },
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // یه متد ساده برای لاگ زدن (کپی کن بذار توی Program.cs یا یه کلاس جدا)
        public static void Log(string level, string message)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                Directory.CreateDirectory("logs");
                File.AppendAllText($"logs/std-{DateTime.Today:yyyy-MM-dd}.log", line);
            }
            catch (Exception e)
            {

            }
        }
        public static void Log(string level, Exception message)
        {
            try
            {
                var line = @$"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message.Message} {message.Source} 
{message.InnerException}{message.StackTrace} {Environment.NewLine}";

                Directory.CreateDirectory("logs");
                File.AppendAllText($"logs/std-{DateTime.Today:yyyy-MM-dd}.log", line);
            }
            catch (Exception e)
            {

            }
        }
        public static string GenerateJWTMeet(MeetingTBL tbl)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes("9c2a7d64-47ac-4dd6-9af0-5d313faad181"));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
              //_configuration.GetSection("Appsettings:ValIssuerID").Value,
              //_configuration.GetSection("Appsettings:ValIudienceID").Value,
              issuer: "vc.hekmateislami.com",
              audience: "vc.hekmateislami.com",
              notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(5),
                claims: new List<Claim>
                {
                    new Claim("date",DateTime.Now.ToString()),
                    new Claim("guid", tbl.MeetID),
                    new Claim("room", tbl.RoomFriendlyID),
                    new Claim("secret", GenerateSecretKey(128)),
                    new Claim("api-key", GenerateSecretKey(32)),
                },
                signingCredentials: cred);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string ToJalaliDate(DateTime date)
        {
            var pc = new PersianCalendar();

            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);

            string monthName = GetPersianMonthName(month);

            return $"{year}-{monthName}-{day:D2}";
        }

        private static string GetPersianMonthName(int month)
        {
            return month switch
            {
                1 => "فروردین",
                2 => "اردیبهشت",
                3 => "خرداد",
                4 => "تیر",
                5 => "مرداد",
                6 => "شهریور",
                7 => "مهر",
                8 => "آبان",
                9 => "آذر",
                10 => "دی",
                11 => "بهمن",
                12 => "اسفند",
                _ => ""
            };
        }
    }
    public static class PdfCompressor
    {
        //public static string CompressPdf(string inputPath)
        //{
        //    try
        //    {
        //        if (!File.Exists(inputPath))
        //            throw new FileNotFoundException("File not found.", inputPath);

        //        string dir = Path.GetDirectoryName(inputPath);
        //        string name = Path.GetFileNameWithoutExtension(inputPath);
        //        string outputPath = Path.Combine(dir, $"{name}_compressed.pdf");

        //        using (var reader = new PdfReader(inputPath))
        //        {
        //            reader.RemoveUnusedObjects(); // حذف اشیاء غیرضروری

        //            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        //            using (var document = new Document())
        //            using (var writer = PdfWriter.GetInstance(document, fs))
        //            {
        //                writer.SetFullCompression();
        //                document.Open();

        //                PdfContentByte cb = writer.DirectContent;

        //                for (int i = 1; i <= reader.NumberOfPages; i++)
        //                {
        //                    document.NewPage();
        //                    PdfImportedPage page = writer.GetImportedPage(reader, i);
        //                    cb.AddTemplate(page, 0, 0);
        //                }

        //                document.Close();
        //            }
        //        }

        //        return outputPath;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return null;
        //    }
        //}
    }
    public static class DateExtensions
    {
        public static string ToPersianDate(this DateTime date)
        {
            try
            {
                var pc = new System.Globalization.PersianCalendar();
                return $"{pc.GetYear(date)}/{pc.GetMonth(date):D2}/{pc.GetDayOfMonth(date):D2} {date:HH:mm}";
            }
            catch
            {
                return date.ToString("yyyy/MM/dd HH:mm");
            }
        }
    }
    public static class PersianDateHelper
    {
        private static readonly string[] PersianDayNames = new[]
        {
        "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنج‌شنبه", "جمعه", "شنبه"
    };

        /// <summary>
        /// تاریخ میلادی رو می‌گیره و نام روز هفته به فارسی برمی‌گردونه (مثل: سه‌شنبه)
        /// </summary>
        public static string GetPersianDayName(this DateTime date)
        {
            int dayIndex = (int)date.DayOfWeek; // یکشنبه = 0, ..., شنبه = 6

            // در .NET یکشنبه اولین روز هفته است، اما در تقویم پارسی شنبه آخرین روز و یکشنبه اولین روز است
            // پس جابجایی می‌کنیم:
            return dayIndex == 6 ? PersianDayNames[6] : PersianDayNames[dayIndex + 1];
        }
    }

    public class BBBConfig
    {
        public string Url { get; set; }
        public string UrlAPI { get; set; }
        public string Secret { get; set; }
        public string ModeratorPassword { get; set; }
        public string AttendPassword { get; set; }
        public string Last { get; set; }
    }
}
