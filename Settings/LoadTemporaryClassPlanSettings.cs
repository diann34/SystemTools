using System.Text.Json.Serialization;

namespace SystemTools.Settings;

public class LoadTemporaryClassPlanSettings
{
    [JsonPropertyName("classPlanId")]
    public string ClassPlanId { get; set; } = string.Empty;
}
