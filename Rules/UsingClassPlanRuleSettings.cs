using System.Text.Json.Serialization;

namespace SystemTools.Rules;

public class UsingClassPlanRuleSettings
{
    [JsonPropertyName("classPlanId")]
    public string ClassPlanId { get; set; } = string.Empty;
}
