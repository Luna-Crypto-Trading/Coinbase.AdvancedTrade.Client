using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class PreviewOrderResponse
{
    [JsonPropertyName("order_total")]
    public string? OrderTotal { get; set; }

    [JsonPropertyName("commission_total")]
    public string? CommissionTotal { get; set; }

    [JsonPropertyName("errs")]
    public List<string>? Errors { get; set; }

    [JsonPropertyName("warning")]
    public List<string>? Warnings { get; set; }

    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }

    [JsonPropertyName("best_bid")]
    public string? BestBid { get; set; }

    [JsonPropertyName("best_ask")]
    public string? BestAsk { get; set; }

    [JsonPropertyName("is_max")]
    public bool IsMax { get; set; }

    [JsonPropertyName("average_filled_price")]
    public string? AverageFilledPrice { get; set; }

    [JsonPropertyName("order_margin_total")]
    public string? OrderMarginTotal { get; set; }
}
