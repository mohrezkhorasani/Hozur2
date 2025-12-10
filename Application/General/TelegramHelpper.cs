using Application.Interface.IInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Application.General
{
    public static class TelegramHelper
    {
        private static IServiceScopeFactory? _scopeFactory;

        public static void Initialize(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        private static string BuildFullExceptionMessage(Exception? e, string? extraInfo = null)
        {
            if (e == null) return "❌ Exception نال بود!";

            var sb = new StringBuilder();

            sb.AppendLine("🚨 *خطای جدید در سیستم* 🚨");
            sb.AppendLine("```");
            sb.AppendLine($"زمان وقوع: {DateTime.Now:yyyy-MM-dd HH:mm:ss} (UTC+3:30)");

            if (!string.IsNullOrWhiteSpace(extraInfo))
                sb.AppendLine($"توضیح اضافه: {extraInfo}");

            sb.AppendLine($"نوع خطا: {e.GetType().FullName}");
            sb.AppendLine($"پیام: {e.Message}");

            if (e.TargetSite != null)
                sb.AppendLine($"متد: {e.TargetSite.DeclaringType?.FullName}.{e.TargetSite.Name}");

            // اطلاعات دقیق خط کد (فقط در Debug mode یا وقتی PDB باشه)
            var stackTrace = new StackTrace(e, true);
            var frame = stackTrace.GetFrames()?.FirstOrDefault(f => f.GetFileName() != null);
            if (frame != null)
            {
                sb.AppendLine($"فایل: {Path.GetFileName(frame.GetFileName())}");
                sb.AppendLine($"خط کد: {frame.GetFileLineNumber()}");
                sb.AppendLine($"ستون: {frame.GetFileColumnNumber()}");
            }

            sb.AppendLine($"منبع: {e.Source}");
            sb.AppendLine("```");

            // استک تریس کامل (فقط ۱۰ خط اول برای جلوگیری از طولانی شدن)
            sb.AppendLine("*استک تریس:*");
            sb.AppendLine("```csharp");
            var stackLines = e.StackTrace?.Split('\n')?.Take(15) ?? Array.Empty<string>();
            foreach (var line in stackLines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    sb.AppendLine(trimmed.Length > 100 ? trimmed.Substring(0, 97) + "..." : trimmed);
            }
            sb.AppendLine("```");

            // Inner Exception (اگر داشت)
            if (e.InnerException != null)
            {
                sb.AppendLine("*Inner Exception:*");
                sb.AppendLine("```");
                sb.AppendLine($"نوع: {e.InnerException.GetType().Name}");
                sb.AppendLine($"پیام: {e.InnerException.Message}");
                sb.AppendLine("```");
            }

            // Data (مثل اطلاعات اضافی که خودت به Exception.Data اضافه کردی)
            if (e.Data.Count > 0)
            {
                sb.AppendLine("*داده‌های اضافی:*");
                sb.AppendLine("```json");
                foreach (DictionaryEntry de in e.Data)
                    sb.AppendLine($"  \"{de.Key}\": \"{de.Value}\"");
                sb.AppendLine("```");
            }

            return sb.ToString();
        }
        public static async Task LogExceptionAsync(Exception e, int topic = 60, string? extraInfo = null)
        {
            try
            {
                using var scope = _scopeFactory!.CreateScope();
                var telegram = scope.ServiceProvider.GetRequiredService<ITelegramService>();

                string message = BuildFullExceptionMessage(e, extraInfo);
                await telegram.SendTelegramMessageAsync($"خطایی داریم {e.Message} {e.InnerException}", topic);
                await telegram.SendTelegramMessageAsync(message, topic);
            }
            catch (Exception ex)
            {

            }
            // هر بار یک Scope جدید می‌سازیم → Scoped سرویس‌ها درست کار می‌کنن

        }
        public static async Task SendMessage(string msg, int topic = 60, string? extraInfo = null)
        {
            // هر بار یک Scope جدید می‌سازیم → Scoped سرویس‌ها درست کار می‌کنن
            using var scope = _scopeFactory!.CreateScope();
            var telegram = scope.ServiceProvider.GetRequiredService<ITelegramService>();

            await telegram.SendTelegramMessageAsync(msg.EscapeTelegramMarkdown(), topic);
        }
        public static string EscapeTelegramMarkdown(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var chars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var c in chars)
                text = text.Replace(c, $"\\{c}");
            return text;
        }
    }
}
