using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCP.Shared.Events
{
    public class DovecotEvent
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("hostname")]
        public string Hostname { get; set; } = string.Empty;

        [JsonPropertyName("start_time")]
        public DateTimeOffset StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTimeOffset EndTime { get; set; }

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = [];

        [JsonPropertyName("fields")]
        public Dictionary<string, JsonElement> Fields { get; set; } = [];

        public List<string> ReasonCode => GetStringList("reason_code");
        public string? User => GetString("user");
        public long? Duration => GetLong("duration");
        public string? Protocol => GetString("protocol");
        public string? RemoteIp => GetString("remote_ip");
        public long? RemotePort => GetLong("remote_port");
        public string? ConnectionId => GetString("connection_id");
        public string? Session => GetString("session");
        public string? TransactionId => GetString("transaction_id");
        public string? MailFrom => GetString("mail_from");
        public string? MailFromRaw => GetString("mail_from_raw");
        public string? RcptTo => GetString("rcpt_to");
        public string? MessageId => GetString("message_id");
        public string? Subject => GetString("message_subject");
        public string? MessageFrom => GetString("message_from");
        public static DovecotEvent FromJson(string json)
            => JsonSerializer.Deserialize<DovecotEvent>(json)!;
        public string? GetString(string key)
            => Fields.TryGetValue(key, out var el) ? el.GetString() : null;
        public long? GetLong(string key)
            => Fields.TryGetValue(key, out var el) ? el.GetInt64() : null;
        public List<string> GetStringList(string key)
            => Fields.TryGetValue(key, out var el) && el.ValueKind == JsonValueKind.Array
                ? el.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToList()
                : [];
    }
}
