using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class EditOrderRequest
{
    [JsonPropertyName("order_id")]
    public required string OrderId { get; set; }

    [JsonPropertyName("price")]
    public required string Price { get; set; }

    [JsonPropertyName("size")]
    public required string Size { get; set; }
}

public class EditOrderResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errors")]
    public List<EditOrderError>? Errors { get; set; }
}

public class EditOrderError
{
    [JsonPropertyName("edit_failure_reason")]
    public string? EditFailureReason { get; set; }

    [JsonPropertyName("preview_failure_reason")]
    public string? PreviewFailureReason { get; set; }
}
