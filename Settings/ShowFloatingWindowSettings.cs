using System.Text.Json.Serialization;

namespace SystemTools.Settings;

public class ShowFloatingWindowSettings
{
    [JsonPropertyName("showFloatingWindow")]
    public bool ShowFloatingWindow { get; set; } = true;
}
