using System.Text.Json.Serialization;

namespace Panel.Data
{
    public class AlertCounter
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
