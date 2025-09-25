using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.General
{
    public static class Statics
    {
        public static string URL = "https://domain.com"; //

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
        public static string GetDifferentPrettyDate(DateTime date)
        {
            var dif = (date - DateTime.Now).TotalSeconds;
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
        public static string GenerateJWTDoerToWork(int workid, string keyword, string guid)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("34383f87-02fe-4484-9663-5f3975d9d0e9"));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
              //_configuration.GetSection("Appsettings:ValIssuerID").Value,
              //_configuration.GetSection("Appsettings:ValIudienceID").Value,
              issuer: "etier.com",
              audience: "etier.com",
              notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(1),
                claims: new List<Claim>
                {
                    new Claim("date",DateTime.Now.ToString()),
                    new Claim("UserID", guid),
                    new Claim("keyword", keyword),
                    new Claim("workid",workid.ToString()),

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
    }

}
