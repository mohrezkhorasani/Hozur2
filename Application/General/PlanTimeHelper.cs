using Application.DTO.Models;
using Domain.Entity;
using System;
using System.Globalization;

namespace Application.General
{
    public static class PlanTimeHelper
    {
        private static readonly PersianCalendar _persianCalendar = new PersianCalendar();
        public static readonly TimeSpan IranOffset = TimeSpan.FromHours(3.5);

        /// <summary>
        /// Converts a UTC DateTime to Tehran local time (+03:30).
        /// </summary>
        public static DateTime ToTehranDateTime(DateTime utcDateTime)
        {
            var utc = utcDateTime.Kind == DateTimeKind.Utc 
                ? utcDateTime 
                : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            return utc.Add(IranOffset);
        }

        /// <summary>
        /// Converts a Tehran local DateTime to UTC (-03:30).
        /// </summary>
        public static DateTime TehranToUtc(DateTime tehranDateTime)
        {
            return DateTime.SpecifyKind(tehranDateTime.Subtract(IranOffset), DateTimeKind.Utc);
        }

        /// <summary>
        /// Formats UTC DateTime into Tehran Jalali date (e.g. "1405/08/17") and time (e.g. "18:30").
        /// </summary>
        public static (string Date, string Time, string Formatted) ToTehranJalaliStrings(DateTime utcDateTime)
        {
            var tehran = ToTehranDateTime(utcDateTime);
            int y = _persianCalendar.GetYear(tehran);
            int m = _persianCalendar.GetMonth(tehran);
            int d = _persianCalendar.GetDayOfMonth(tehran);
            string dateStr = $"{y:D4}/{m:D2}/{d:D2}";
            string timeStr = $"{tehran.Hour:D2}:{tehran.Minute:D2}";
            return (dateStr, timeStr, $"{dateStr} {timeStr}");
        }

        /// <summary>
        /// Maps a RoomTBLPlanTBL to MiniPlanJSON with complete Tehran and UTC date/time fields.
        /// </summary>
        public static MiniPlanJSON MapToMiniPlanJSON(RoomTBLPlanTBL p)
        {
            if (p == null) return null!;

            string? dateStr = null;
            string? timeStr = null;
            string? formatted = null;
            DateTime? tehranDt = null;
            DateTime? utcDt = null;

            if (p.StartDate.HasValue)
            {
                var dt = p.StartDate.Value;
                utcDt = dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                tehranDt = ToTehranDateTime(utcDt.Value);
                var parts = ToTehranJalaliStrings(utcDt.Value);
                dateStr = parts.Date;
                timeStr = parts.Time;
                formatted = parts.Formatted;
            }

            return new MiniPlanJSON
            {
                Id = p.ID,
                StartAt = tehranDt ?? p.StartDate,
                StartAtUtc = utcDt,
                Date = dateStr,
                Time = timeStr,
                StartDateJalali = dateStr,
                StartTimeTehran = timeStr,
                StartAtTehran = formatted,
                Title = p.Title,
                Teacher = p.Teachers,
                Dabir = p.Dabir,
                RoomJSON = p.RoomTBL != null ? new MiniRoomJSON
                {
                    RoomID = p.RoomTBL.FriendlyID,
                    IsSubscribed = p.RoomTBL.IsSubscribed,
                    Title = p.RoomTBL.Title,
                    Avatar = p.RoomTBL.Avatar,
                    Owner = p.RoomTBL.OwnerTBL != null ? new MiniUserJSON
                    {
                        Fullname = p.RoomTBL.OwnerTBL.GetName(),
                        Username = p.RoomTBL.OwnerTBL.Username
                    } : null!
                } : null!
            };
        }
    }
}
