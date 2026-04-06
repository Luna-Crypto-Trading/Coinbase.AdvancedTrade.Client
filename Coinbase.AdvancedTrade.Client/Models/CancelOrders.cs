using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class CancelOrdersRequest
{
    [JsonPropertyName("order_ids")]
    public required List<string> OrderIds { get; set; }
}

public class CancelOrdersResponse
{
    [JsonPropertyName("results")]
    public required List<CancelOrderResult> Results { get; set; }
}

public class CancelOrderResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; set; }

    [JsonPropertyName("order_id")]
    public required string OrderId { get; set; }
}
