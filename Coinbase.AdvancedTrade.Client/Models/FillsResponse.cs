using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class FillsResponse
{
    [JsonPropertyName("fills")]
    public required List<Fill> Fills { get; set; }

    [JsonPropertyName("cursor")]
    public string? Cursor { get; set; }
}

public class Fill
{
    [JsonPropertyName("entry_id")]
    public required string EntryId { get; set; }

    [JsonPropertyName("trade_id")]
    public required string TradeId { get; set; }

    [JsonPropertyName("order_id")]
    public required string OrderId { get; set; }

    [JsonPropertyName("trade_time")]
    public required string TradeTime { get; set; }

    [JsonPropertyName("trade_type")]
    public required string TradeType { get; set; }

    [JsonPropertyName("price")]
    public required string Price { get; set; }

    [JsonPropertyName("size")]
    public required string Size { get; set; }

    [JsonPropertyName("commission")]
    public required string Commission { get; set; }

    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("sequence_timestamp")]
    public string? SequenceTimestamp { get; set; }

    [JsonPropertyName("liquidity_indicator")]
    public string? LiquidityIndicator { get; set; }

    [JsonPropertyName("size_in_quote")]
    public bool? SizeInQuote { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("side")]
    public required string Side { get; set; }

    [JsonPropertyName("retail_portfolio_id")]
    public string? RetailPortfolioId { get; set; }
}
