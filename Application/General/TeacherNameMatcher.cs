using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Domain.Entity;

namespace Application.General
{
    public class MatchedTeacherResult
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Position { get; set; }
        public bool IsMatchedInDb { get; set; }
        public string FormattedDisplayName => string.IsNullOrWhiteSpace(Position) ? Name : $"{Name}({Position})";
    }

    public static class TeacherNameMatcher
    {
        private static readonly PersianCalendar _persianCalendar = new PersianCalendar();

        /// <summary>
        /// Converts Arabic and Persian digits to ASCII 0-9 digits.
        /// </summary>
        public static string NormalizeDigits(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (c >= '\u0660' && c <= '\u0669') // Arabic-Indic digits ٠-٩
                    sb.Append((char)('0' + (c - '\u0660')));
                else if (c >= '\u06F0' && c <= '\u06F9') // Eastern Arabic / Persian digits ۰-۹
                    sb.Append((char)('0' + (c - '\u06F0')));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Normalizes Arabic characters to Persian, strips tashkeel/diacritics,
        /// removes ZWNJ, and normalizes whitespaces.
        /// </summary>
        public static string NormalizePersianText(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var text = NormalizeDigits(input);

            // Replace Arabic specific characters with standard Persian
            text = text.Replace('ي', 'ی')
                       .Replace('ك', 'ک')
                       .Replace('ة', 'ه')
                       .Replace('ۀ', 'ه')
                       .Replace('ؤ', 'و')
                       .Replace('إ', 'ا')
                       .Replace('أ', 'ا')
                       .Replace('آ', 'ا')
                       .Replace('ٱ', 'ا')
                       .Replace('ئ', 'ی');

            // Strip diacritics / tashkeel / tashdid (\u064B-\u065F, \u0670)
            text = Regex.Replace(text, @"[\u064B-\u065F\u0670]", "");

            // Replace ZWNJ, ZWJ and special spaces with standard space
            text = text.Replace('\u200C', ' ')
                       .Replace('\u200D', ' ')
                       .Replace('\u00A0', ' ')
                       .Replace('\t', ' ')
                       .Replace('\r', ' ')
                       .Replace('\n', ' ');

            // Collapse multiple spaces
            text = Regex.Replace(text, @"\s+", " ").Trim();

            return text;
        }

        /// <summary>
        /// Extracts position specified in parentheses, e.g. "علیرضا حسینی(سردبیر روزنامه حکمت)" -> Name: "علیرضا حسینی", Position: "سردبیر روزنامه حکمت"
        /// </summary>
        public static (string CleanName, string? Position) ExtractPosition(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return (string.Empty, null);
            var trimmed = raw.Trim();

            var match = Regex.Match(trimmed, @"^(.*?)\s*[\(（](.*?)[\)）]\s*$");
            if (match.Success)
            {
                var name = match.Groups[1].Value.Trim();
                var pos = match.Groups[2].Value.Trim();
                return (name, string.IsNullOrWhiteSpace(pos) ? null : pos);
            }

            return (trimmed, null);
        }

        /// <summary>
        /// Strips honorifics and titles from the start of a normalized name iteratively.
        /// Handles: حجت الاسلام والمسلمین، حجت الاسلام، آیت الله العظمی، آیت الله، علامه، دکتر، مهندس، سید، و ...
        /// </summary>
        public static string StripHonorifics(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var current = NormalizePersianText(name);

            // Regex patterns for honorifics to strip from start of string
            // Ordered from longest compound titles to single titles
            var titlePatterns = new[]
            {
                @"^(حضرت\s+)?(ایت\s*الله\s*العظمی|آیت\s*الله\s*العظمی)\b",
                @"^(حضرت\s+)?(ایت\s*الله|آیت\s*الله)\b",
                @"^(حجت\s*الاسلام\s*(و\s*)?المسلمین|حجة\s*الاسلام\s*(و\s*)?المسلمین|حجه\s*الاسلام\s*(و\s*)?المسلمین|حجت\s*اسلام\s*(و\s*)?المسلمین)\b",
                @"^(حجت\s*الاسلام|حجة\s*الاسلام|حجه\s*الاسلام|حجت\s*اسلام|حجة\s*اسلام|حجه\s*اسلام)\b",
                @"^(ثقة\s*الاسلام|ثقه\s*الاسلام)\b",
                @"^(علامه|پروفسور|پرفسور)\b",
                @"^(دکتر|دكتر|dr\.?)\b",
                @"^(مهندس|eng\.?)\b",
                @"^(استاد)\b",
                @"^(حضرت)\b",
                @"^(جناب\s*اقای|جناب\s*آقای|جناب|سرکار\s*خانم|اقای|آقای|خانم)\b",
                @"^(سید|سيد|سیده|سيده)\b"
            };

            bool changed = true;
            int loopGuard = 0;
            while (changed && loopGuard++ < 10)
            {
                changed = false;
                foreach (var pattern in titlePatterns)
                {
                    var m = Regex.Match(current, pattern, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        current = current.Substring(m.Length).Trim();
                        changed = true;
                    }
                }
            }

            return current.Trim();
        }

        /// <summary>
        /// Checks if a user is qualified as a teacher in the database (not an ordinary user).
        /// </summary>
        public static bool IsTeacherUser(UserTBL? user)
        {
            if (user == null) return false;

            if (user.Type == MainEntity_Types.USER_TEACHER) return true;
            if (user.GetRole().Equals("TEACHER", StringComparison.OrdinalIgnoreCase)) return true;
            if (user.TeacherMeetings != null && user.TeacherMeetings.Any()) return true;

            return false;
        }

        /// <summary>
        /// Finds the matching teacher user in the database for a given raw teacher name.
        /// Strips honorifics and matches core name against DB teacher core names.
        /// </summary>
        public static UserTBL? FindMatchingTeacher(string inputName, IEnumerable<UserTBL> dbTeachers)
        {
            if (string.IsNullOrWhiteSpace(inputName)) return null;

            var (cleanName, _) = ExtractPosition(inputName);
            var inputCore = StripHonorifics(cleanName);
            if (string.IsNullOrWhiteSpace(inputCore))
            {
                inputCore = NormalizePersianText(cleanName);
            }

            var inputNormalizedFull = NormalizePersianText(cleanName);

            // Filter only valid teachers
            var validTeachers = dbTeachers.Where(IsTeacherUser).ToList();
            if (!validTeachers.Any()) return null;

            // 1. Direct exact match on full name (with honorifics removed from both)
            foreach (var t in validTeachers)
            {
                var dbFullName = $"{t.Firstname} {t.LastName}".Trim();
                var dbCore = StripHonorifics(dbFullName);

                if (!string.IsNullOrWhiteSpace(dbCore) && string.Equals(inputCore, dbCore, StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }

            // 2. Direct exact match on normalized full text (if DB teacher stored without titles)
            foreach (var t in validTeachers)
            {
                var dbFullName = $"{t.Firstname} {t.LastName}".Trim();
                var dbNorm = NormalizePersianText(dbFullName);

                if (string.Equals(inputNormalizedFull, dbNorm, StringComparison.OrdinalIgnoreCase))
                {
                    return t;
                }
            }

            // 3. Match without spaces or sub-phrase matching (e.g. input has extra middle name or vice versa)
            var inputNoSpaces = inputCore.Replace(" ", "");
            if (inputNoSpaces.Length >= 4)
            {
                var candidates = new List<UserTBL>();
                foreach (var t in validTeachers)
                {
                    var dbFullName = $"{t.Firstname} {t.LastName}".Trim();
                    var dbCore = StripHonorifics(dbFullName);
                    var dbNoSpaces = dbCore.Replace(" ", "");

                    if (dbNoSpaces.Equals(inputNoSpaces, StringComparison.OrdinalIgnoreCase))
                    {
                        return t;
                    }

                    // Check token subset if both have first & last name
                    var inputTokens = inputCore.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var dbTokens = dbCore.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (inputTokens.Length >= 2 && dbTokens.Length >= 2)
                    {
                        if (inputTokens.First() == dbTokens.First() && inputTokens.Last() == dbTokens.Last())
                        {
                            candidates.Add(t);
                        }
                    }
                }

                if (candidates.Count == 1)
                {
                    return candidates[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Splits raw teachers input (single string, comma separated, or list) and matches each against DB teachers.
        /// If found: uses the exact official name from DB and teacher ID.
        /// If not found: keeps the input name and ID 0.
        /// Preserves positions e.g. (سمت).
        /// </summary>
        public static List<MatchedTeacherResult> MatchAndFormatTeachers(string? rawTeachers, IEnumerable<UserTBL> dbTeachers)
        {
            var results = new List<MatchedTeacherResult>();
            if (string.IsNullOrWhiteSpace(rawTeachers)) return results;

            // Handle potential JSON array input e.g. [{"name": "...", "id": ...}] or ["استاد 1", "استاد 2"]
            var trimmed = rawTeachers.Trim();
            if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
            {
                try
                {
                    // Try parsing JSON array of objects
                    var doc = System.Text.Json.JsonDocument.Parse(trimmed);
                    if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            string itemText = "";
                            if (el.ValueKind == System.Text.Json.JsonValueKind.String)
                            {
                                itemText = el.GetString() ?? "";
                            }
                            else if (el.ValueKind == System.Text.Json.JsonValueKind.Object)
                            {
                                if (el.TryGetProperty("name", out var nameProp))
                                {
                                    itemText = nameProp.GetString() ?? "";
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(itemText))
                            {
                                results.Add(ProcessSingleTeacher(itemText, dbTeachers));
                            }
                        }
                        if (results.Any()) return results;
                    }
                }
                catch
                {
                    // Not valid json, proceed to delimiter split
                }
            }

            // Split by common delimiters: comma, Persian comma, semicolon, newline, " و "
            var parts = Regex.Split(trimmed, @"[،,;\r\n]+|\s+و\s+")
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrWhiteSpace(p));

            foreach (var part in parts)
            {
                results.Add(ProcessSingleTeacher(part, dbTeachers));
            }

            return results;
        }

        private static MatchedTeacherResult ProcessSingleTeacher(string rawItem, IEnumerable<UserTBL> dbTeachers)
        {
            var (cleanName, position) = ExtractPosition(rawItem);
            var matchedUser = FindMatchingTeacher(cleanName, dbTeachers);

            if (matchedUser != null)
            {
                var officialName = $"{matchedUser.Firstname} {matchedUser.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(officialName))
                {
                    officialName = matchedUser.Username;
                }

                return new MatchedTeacherResult
                {
                    Id = matchedUser.ID,
                    Name = officialName,
                    Position = position,
                    IsMatchedInDb = true
                };
            }

            return new MatchedTeacherResult
            {
                Id = 0,
                Name = cleanName.Trim(),
                Position = position,
                IsMatchedInDb = false
            };
        }

        /// <summary>
        /// Parses a Jalali/Shamsi date string (e.g. "1405/08/13", "1405-08-13", "۱۴۰۵/۰۸/۱۳")
        /// and time string (e.g. "10:30", "10:30:00", "۱۰:۳۰") into UTC DateTime.
        /// </summary>
        public static (bool Success, DateTime UtcDateTime, string ErrorMessage) ParseJalaliDateTime(string? dateStr, string? timeStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
            {
                return (false, DateTime.MinValue, "تاریخ برگزاری الزامی است");
            }

            var normDate = NormalizeDigits(dateStr).Trim();
            var normTime = string.IsNullOrWhiteSpace(timeStr) ? "10:00" : NormalizeDigits(timeStr).Trim();

            // Extract date parts
            var dateMatch = Regex.Match(normDate, @"^(\d{4})[\/\-\.](\d{1,2})[\/\-\.](\d{1,2})$");
            if (!dateMatch.Success)
            {
                return (false, DateTime.MinValue, "فرمت تاریخ نامعتبر است. نمونه صحیح: 1405/08/13");
            }

            int year = int.Parse(dateMatch.Groups[1].Value);
            int month = int.Parse(dateMatch.Groups[2].Value);
            int day = int.Parse(dateMatch.Groups[3].Value);

            if (year < 1300 || year > 1500 || month < 1 || month > 12 || day < 1 || day > 31)
            {
                return (false, DateTime.MinValue, "تاریخ شمسی وارد شده در محدوده معتبر نیست");
            }

            // Extract time parts
            int hour = 0;
            int minute = 0;
            int second = 0;

            var timeMatch = Regex.Match(normTime, @"^(\d{1,2})(?::(\d{1,2}))?(?::(\d{1,2}))?$");
            if (timeMatch.Success)
            {
                hour = int.Parse(timeMatch.Groups[1].Value);
                if (timeMatch.Groups[2].Success)
                {
                    minute = int.Parse(timeMatch.Groups[2].Value);
                }
                if (timeMatch.Groups[3].Success)
                {
                    second = int.Parse(timeMatch.Groups[3].Value);
                }
            }
            else
            {
                return (false, DateTime.MinValue, "فرمت ساعت نامعتبر است. نمونه صحیح: 10:30");
            }

            if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
            {
                return (false, DateTime.MinValue, "ساعت وارد شده نامعتبر است");
            }

            try
            {
                // Convert Jalali to Gregorian DateTime
                var gregorian = _persianCalendar.ToDateTime(year, month, day, hour, minute, second, 0);

                // Iran standard time is UTC+03:30 (Iran abolished DST)
                var iranOffset = TimeSpan.FromHours(3.5);
                var iranDateTimeOffset = new DateTimeOffset(gregorian, iranOffset);
                var utcDateTime = iranDateTimeOffset.UtcDateTime;

                return (true, utcDateTime, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, DateTime.MinValue, $"خطا در تبدیل تاریخ: {ex.Message}");
            }
        }
    }
}
