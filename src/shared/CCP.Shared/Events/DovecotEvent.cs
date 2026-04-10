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

        public static DovecotEvent FromJson(string json)
            => JsonSerializer.Deserialize<DovecotEvent>(json)!;

        public string? GetString(string key)
            => Fields.TryGetValue(key, out var el) ? el.GetString() : null;

        public long? GetLong(string key)
            => Fields.TryGetValue(key, out var el) ? el.GetInt64() : null;
    }
}
