using System.Text.Json;

namespace ArgusCloud.Infrastructure.Shared
{
    public static class JsonConfig
    {
        public static readonly JsonSerializerOptions camelCaseOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
}
