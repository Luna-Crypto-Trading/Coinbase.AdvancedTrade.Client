using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public sealed record ClosePositionRequest
{
    /// <summary>
    /// The unique ID provided for the order (used for identification purposes).
    /// </summary>
    [JsonPropertyName("client_order_id")]
    public required string ClientOrderId { get; set; }

    /// <summary>
    /// The trading pair (e.g. 'BTC-USD').
    /// </summary>
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    /// <summary>
    /// The amount of contracts that should be closed.
    /// </summary>
    [JsonPropertyName("size")]
    public string? Size { get; set; }
}