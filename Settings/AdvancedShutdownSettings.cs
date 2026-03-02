using System.Text.Json.Serialization;

namespace SystemTools.Settings;

public class AdvancedShutdownSettings
{
    [JsonPropertyName("minutes")] public int Minutes { get; set; } = 2;
}
