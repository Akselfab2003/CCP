using System.Text.Json.Serialization;

namespace MessagingService.Sdk.Dtos
{
    public class AttachmentDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;
    }
}
