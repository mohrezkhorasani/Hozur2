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
        [DefaultValue("نظری")]
        public string Teacher { get; set; }
        [DefaultValue("علوم فلسفی")]
        public string Title { get; set; }
        [DefaultValue("1403/2/2")]
        public string JalaliDate { get; set; }
        public string? Explain { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // بررسی تاریخ جلالی معتبر
            if (!string.IsNullOrWhiteSpace(JalaliDate))
            {
                if (!IsValidJalaliDate(JalaliDate))
                {
                    yield return new ValidationResult(
                        "تاریخ وارد شده معتبر نیست. فرمت صحیح مثلاً 1403/07/11",
                        new[] { nameof(JalaliDate) }
                    );
                }
            }

            // بررسی فارسی بودن Teacher
            if (!string.IsNullOrWhiteSpace(Teacher) && !IsPersianText(Teacher))
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
                    "فیلد Title باید فقط شامل حروف فارسی باشد.",
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

        private bool IsPersianText(string input)
        {
            // محدوده یونیکد حروف فارسی \u0600 تا \u06FF
            return Regex.IsMatch(input, @"^[\u0600-\u06FF\s]+$");
        }
    }


    public class NewUserMettingsViewModel : UserMainVM
    {
        public string MeetID { get; set; }
        public string Role { get; set; }
        public string NickName { get; set; }
    }


    public class NewTeacherViewModel : UserMainVM
    {

        public string FullName { get; set; }
        public string? Username { get; set; }
    }

    public class NewMeetingViewModel : UserMainVM
    {

        public string MeetID { get; set; }
        public string RoomID { get; set; }
    }

    public class NewRoomViewModel : UserMainVM
    {
        public string FriendlyID { get; set; }
        public string? Room { get; set; }
    }
    public class NewUserMeetingLogViewModel : UserMainVM
    {
        public string KeyLog { get; set; } //left,join, activity
        public string? ValueLog { get; set; } //raised hand, microphone opened, microphone closed,file share,screen share open,screen share close,file share close,
        public string Role { get; set; }
    }

}