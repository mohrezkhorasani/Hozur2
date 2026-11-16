using Newtonsoft.Json;
using System;
using System.Globalization;

namespace Application.General
{
    /// <summary>
    /// Ensures that all DateTime values serialized via Newtonsoft.Json are emitted in ISO 8601 UTC with 'Z' suffix.
    /// Unspecified Kind values (e.g. from SQL Server) are treated as UTC as per database design.
    /// </summary>
    public class NewtonsoftUtcDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            if (value is not DateTime dt)
            {
                return;
            }

            // If Unspecified, treat it as UTC since database stores all dates in UTC
            DateTime utc = dt.Kind == DateTimeKind.Utc 
                ? dt 
                : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

            writer.WriteValue(utc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (reader.TokenType == JsonToken.Date)
            {
                var dt = Convert.ToDateTime(reader.Value);
                return dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            if (reader.TokenType == JsonToken.String)
            {
                var str = reader.Value?.ToString();
                if (string.IsNullOrWhiteSpace(str)) return null;
                if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dto))
                {
                    return dto.UtcDateTime;
                }
                if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtParsed))
                {
                    return DateTime.SpecifyKind(dtParsed, DateTimeKind.Utc);
                }
            }
            return reader.Value;
        }
    }

    /// <summary>
    /// System.Text.Json converter for non-nullable DateTime ensuring UTC ISO 8601 with 'Z'.
    /// </summary>
    public class SystemTextJsonUtcDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
    {
        public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TryGetDateTimeOffset(out var dto))
            {
                return dto.UtcDateTime;
            }
            var str = reader.GetString();
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            return reader.GetDateTime();
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
        {
            DateTime utc = value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);

            writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// System.Text.Json converter for nullable DateTime? ensuring UTC ISO 8601 with 'Z'.
    /// </summary>
    public class SystemTextJsonNullableUtcDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.Null) return null;
            if (reader.TryGetDateTimeOffset(out var dto))
            {
                return dto.UtcDateTime;
            }
            var str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return null;
            if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            return reader.GetDateTime();
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime? value, System.Text.Json.JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            DateTime utc = value.Value.Kind == DateTimeKind.Utc
                ? value.Value
                : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);

            writer.WriteStringValue(utc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
        }
    }
}
