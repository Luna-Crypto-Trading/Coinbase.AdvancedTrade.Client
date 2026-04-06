using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class MarketTradesResponse
{
    [JsonPropertyName("trades")]
    public required List<MarketTrade> Trades { get; set; }

    [JsonPropertyName("best_bid")]
    public string? BestBid { get; set; }

    [JsonPropertyName("best_ask")]
    public string? BestAsk { get; set; }
}

public class MarketTrade
{
    [JsonPropertyName("trade_id")]
    public required string TradeId { get; set; }

    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("price")]
    public required string Price { get; set; }

    [JsonPropertyName("size")]
    public required string Size { get; set; }

    [JsonPropertyName("time")]
    public required string Time { get; set; }

    [JsonPropertyName("side")]
    public required string Side { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }
}
