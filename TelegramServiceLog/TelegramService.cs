using Application.Interface.IInfrastructure;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;



namespace TelegramServiceLog
{

    public class TelegramRequest
    {
        public long chat_id { get; set; }
        public int message_id { get; set; }
        public int topic_id { get; set; }
        public string message { get; set; }
    }

    public class TelegramService : ITelegramService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task SendTelegramMessageAsync(string message, int topicid = 60, long ChatID = -1003492895671)
        {
            var url = "https://bot.tel.mt5log.either.ir/api/Telegram/send";

            var payload = new TelegramRequest
            {
                chat_id = ChatID,
                message_id = 0,
                topic_id = topicid,
                message = message
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(errorBody); // این دقیقاً همون JSON خطاست که می‌خوای

                // You said "response no matter" — so no need to check
            }
            catch (Exception ex)
            {
                // Optional: log or ignore
                Console.WriteLine($"⚠️ Failed to send message: {ex.Message}");
            }
        }
        private static string SafeHtml(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")   // این خط خیلی مهمه!
                .Replace("'", "&#39;");
        }
        public async Task SendTelegramMessageAsync(Exception message, int topicid = 60, long ChatID = -1003492895671)
        {



            try
            {
                var url = "https://bot.tel.mt5log.either.ir/api/Telegram/send";
                var payload = new TelegramRequest
                {
                    chat_id = ChatID,
                    message_id = 0,
                    topic_id = topicid,
                    message = $"<b>🐞🐞Exeption</b>\n\n{SafeHtml(message.Message)}\n{SafeHtml(message.InnerException != null ? message.InnerException.ToString() : null)}\n{SafeHtml(message.Source)}\n{SafeHtml(message.StackTrace)}"
                };
                payload.message = payload.message.Replace("\\", "%");
                payload.message = payload.message.Replace("\"", "%");
                payload.message = payload.message.Replace("\\\"\\\"", "%");
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                var errorBody = await response.Content.ReadAsStringAsync();
                // You said "response no matter" — so no need to check
                Console.WriteLine(errorBody); // این دقیقاً همون JSON خطاست که می‌خوای
            }
            catch (Exception ex)
            {
                // Optional: log or ignore
                Console.WriteLine($"⚠️ Failed to send message: {ex.Message}");
            }
        }
    }
}
