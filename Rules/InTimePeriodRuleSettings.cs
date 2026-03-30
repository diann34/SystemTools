using System.Text.Json.Serialization;

namespace SystemTools.Rules;

public class InTimePeriodRuleSettings
{
    [JsonPropertyName("startTime")]
    public string StartTime { get; set; } = "08:00:00";

    [JsonPropertyName("endTime")]
    public string EndTime { get; set; } = "18:00:00";
}
