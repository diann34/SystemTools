using System.Text.Json.Serialization;

namespace SystemTools.Settings;

public class ImeStateSettings
{
    [JsonPropertyName("enableIme")] public bool EnableIme { get; set; }
}
