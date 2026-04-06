using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class IntradayMarginSettingResponse
{
    [JsonPropertyName("setting")]
    public string? Setting { get; set; }
}

public class CurrentMarginWindowResponse
{
    [JsonPropertyName("margin_window")]
    public required MarginWindow MarginWindow { get; set; }
}

public class MarginWindow
{
    [JsonPropertyName("margin_window_type")]
    public string? MarginWindowType { get; set; }

    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }

    [JsonPropertyName("is_intraday_margin_killswitch_enabled")]
    public bool? IsIntradayMarginKillswitchEnabled { get; set; }

    [JsonPropertyName("is_intraday_margin_enrollment_killswitch_enabled")]
    public bool? IsIntradayMarginEnrollmentKillswitchEnabled { get; set; }
}

public class SetIntradayMarginSettingRequest
{
    [JsonPropertyName("setting")]
    public required string Setting { get; set; }
}
