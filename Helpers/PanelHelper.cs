using System.Text.Json.Serialization;
using System.Text.Json;

namespace Panel.Helpers
{
    public static class PanelHelper
    {
        public static class JsonSerializerHelper
        {
            public static string Serialize<T>(T obj)
            {
                return JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                });
            }

            public static T? Deserialize<T>(string json)
            {
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                });
            }
        }
    }
}
