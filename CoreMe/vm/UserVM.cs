using Application.DTO;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CoreMe.VM
{
    public class UserMainVM
    {
        [DefaultValue("gl-guest-00ba42207eec54b9ffd56aae1")]
        public string? UUID { get; set; }
        [DefaultValue(null)]
        public string? Firstname { get; set; }
        [DefaultValue(null)]
        public string? Lastname { get; set; }
        [DefaultValue(null)]
        public string? NationalCode { get; set; }
        [DefaultValue(null)]
        public string? PhoneNumber { get; set; }
        [DefaultValue(null)]
        public string? ExtId { get; set; }
        [DefaultValue(null)]
        public string? Email { get; set; }
        [DefaultValue(null)]
        public string? AllowedGUID { get; set; }
        public string? Role { get; set; }
        [DefaultValue(null)]
        public string? RoomName { get; set; }
    }
    public class NewUserViewModel : UserMainVM, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UUID) &&
                string.IsNullOrWhiteSpace(NationalCode) &&
                string.IsNullOrWhiteSpace(PhoneNumber) &&
                string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "At least one of UUID,  NationalCode, PhoneNumber or Email must be provided.",
                    new[] { nameof(UUID), nameof(Firstname), nameof(Lastname), nameof(NationalCode), nameof(PhoneNumber), nameof(ExtId), nameof(Email) }
                );
            }
        }
    }

    public class UpdateMeetingViewModel : UserMainVM, IValidatableObject
    {
        [DefaultValue("[\"نظری\"]")]
        public string[] Teacher { get; set; }
        [DefaultValue("علوم فلسفی")]
        public string Title { get; set; }
        [DefaultValue("1404-مهر-12")]
        public string JalaliDate { get; set; }
        public string? Explain { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // بررسی تاریخ جلالی معتبر
            if (!string.IsNullOrWhiteSpace(JalaliDate))
            {
                if (!IsValidPersianDate(JalaliDate))
                {
                    yield return new ValidationResult(
                        "تاریخ وارد شده معتبر نیست. فرمت صحیح مثلاً 1404-مهر-12",
                        new[] { nameof(JalaliDate) }
                    );
                }
            }

            // بررسی فارسی بودن Teacher
            if (Teacher!=null && Teacher.Length>0 && !string.IsNullOrWhiteSpace(Teacher[0]) && !IsPersianText(Teacher[0]))
            {
                yield return new ValidationResult(
                    "فیلد Teacher باید فقط شامل حروف فارسی باشد.",
                    new[] { nameof(Teacher) }
                );
            }

            // بررسی فارسی بودن Title
            if (!string.IsNullOrWhiteSpace(Title) && !IsPersianText(Title))
            {
                yield return new ValidationResult(
                    "فیلد Title باید فقط شامل حروف فارسی و عدد باشد.",
                    new[] { nameof(Title) }
                );
            }

        }

        private bool IsValidJalaliDate(string input)
        {
            try
            {
                var parts = input.Split('/');
                if (parts.Length != 3) return false;

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int day = int.Parse(parts[2]);

                PersianCalendar pc = new PersianCalendar();
                DateTime dt = pc.ToDateTime(year, month, day, 0, 0, 0, 0);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPersianDate(string input)
        {
            try
            {
                // جدا کردن بخش‌ها
                var parts = input.Split('-');
                if (parts.Length != 3) return false;

                int year = int.Parse(parts[0]);
                string monthName = parts[1];
                int day = int.Parse(parts[2]);

                // لیست ماه‌های فارسی
                string[] months = {
                "فروردین","اردیبهشت","خرداد",
                "تیر","مرداد","شهریور",
                "مهر","آبان","آذر",
                "دی","بهمن","اسفند"
            };

                int month = Array.IndexOf(months, monthName) + 1;
                if (month == 0) return false; // ماه نامعتبر

                // استفاده از PersianCalendar برای اعتبارسنجی
                PersianCalendar pc = new PersianCalendar();
                DateTime dt = pc.ToDateTime(year, month, day, 0, 0, 0, 0);

                // اگه به خطا نخورد یعنی تاریخ معتبره
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool IsPersianText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // فقط حروف فارسی، فاصله، عدد فارسی و عدد انگلیسی
            return Regex.IsMatch(text, @"^[\u0600-\u06FF\s0-9۰-۹]+$");
        }

    }
    public class GetMeetingURLViewModel : UserMainVM
    {
        [DefaultValue("u65-al5-bnp-tgm")]
        public string MeetID { get; set; }
        [DefaultValue(false)]
        public bool IsModerator { get; set; }
        public string Password { get; set; }
        [DefaultValue("مجمع عالی حکمت اسلامی")]
        public string NickName { get; set; }
    }

    public class NewUserMettingsViewModel : UserMainVM
    {
        public string MeetID { get; set; }
        public string Role { get; set; }
        public string NickName { get; set; }
        public string MeetUserID { get; set; }
    }


    public class NewTeacherViewModel : UserMainVM
    {

        public string FullName { get; set; }
        public string? Username { get; set; }
    }

    public class NewMeetingViewModel : UserMainVM
    {

        public string MeetID { get; set; }
        [DefaultValue("u65-al5-bnp-tgm")]
        public string RoomID { get; set; }
    }

    public class NewRoomViewModel : UserMainVM
    {
        public string FriendlyID { get; set; }
        public string? Room { get; set; }
    }
    public class GenerateCertificateViewModel : UserMainVM
    {
        [DefaultValue(true)]
        public bool Sex { get; set; }
    }
    public class GuarantCertificateViewModel : UserMainVM
    {
        public string NationalCode { get; set; }

    }
    public class EndTheMeetingViewModel : UserMainVM
    {
    }
    public class EndTheMeetingWithAPIViewModel
    {
        public string FriendlyID { get; set; }
    }

    public class NewUserMeetingLogViewModel : UserMainVM
    {
        public string KeyLog { get; set; } //left,join, activity
        public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
        public string? OS { get; set; } 
        public string Role { get; set; }
        public string MeetUserID { get; set; }
    }
    public class NewUserMeetingLogFromModeratorViewModel : UserMainVM
    {
        public string KeyLog { get; set; } //left,join, activity
        public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
        public string MeetUserID { get; set; }
        public string UserExtID { get; set; }

    }
    public class NewQuestionViewModel : UserMainVM
    {
        public string Content { get; set; } 

    }
    public class AnswerPollListViewModel : UserMainVM
    {
        public List<AnswerItemRequestJSON> AnswerItemRequestJSONs { get; set; }
    }
    public class StartMeetingViewModel
    {
        [DefaultValue("jtb-a8e-ydl-05b")]
        [FromRoute]
        public string IDRoom { get; set; }
        public bool RequiredToEnterName { get; set; } = false;
        public string? MeetTitle { get; set; }
        public string? Name { get; set; } = "مجمع عالی حکمت اسلامی";
        public long? IDPlan{ get; set; }
        public List<string> Teachers { get; set; } = new List<string>();
        public bool HasCertificate { get; set; } = false;
        public string? Dabir { get; set; }

    }

    public class SignInUserPanelViewModel
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Explain { get; set; }
    }
    public class LoginUserPanelViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class VerifyTokenViewModel
    {
        public string JWT { get; set; }
    }
    public class NewPlanRoomViewModel
    {
        public string FriendlyID { get; set; }
        public string? Title { get; set; }
        public string? Teacher { get; set; }
        public string? Dabir { get; set; }
        public bool HasCert { get; set; } = true;
        public DateTime StartDate { get; set; }
    }
    public class UpdatePresentMinToCertOfMeetViewModel
    {
        public int Minutes { get; set; }
    }


}