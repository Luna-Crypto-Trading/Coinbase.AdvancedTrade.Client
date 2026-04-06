using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class FuturesSweepResponse
{
    [JsonPropertyName("sweeps")]
    public required List<FuturesSweep> Sweeps { get; set; }
}

public class FuturesSweep
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("requested_amount")]
    public string? RequestedAmount { get; set; }

    [JsonPropertyName("should_sweep_all")]
    public bool? ShouldSweepAll { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("scheduled_time")]
    public string? ScheduledTime { get; set; }
}

public class ScheduleSweepRequest
{
    [JsonPropertyName("usd_amount")]
    public required string UsdAmount { get; set; }
}
