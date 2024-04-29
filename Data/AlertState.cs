using Panel.Helpers;
using System.Text.Json.Serialization;

namespace Panel.Data
{
    public class AlertState
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("info")]
        public string Info { get; set; } = string.Empty;
        [JsonPropertyName("current")]
        public bool IsCurrent { get; set; }
        [JsonPropertyName("ingame")]
        public bool InGame { get; set; }
        [JsonPropertyName("threshold")]
        public int Value { get; set; } = 1234;
        [JsonPropertyName("order")]
        public int Order { get; set; }
    }
}
