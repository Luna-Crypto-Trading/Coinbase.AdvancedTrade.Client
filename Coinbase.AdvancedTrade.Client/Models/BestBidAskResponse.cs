using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class BestBidAskResponse
{
    public required List<PriceBook> PriceBooks { get; set; }
}

public class PriceBook
{
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("bids")]
    public required List<PriceBookEntry> Bids { get; set; }

    [JsonPropertyName("asks")]
    public required List<PriceBookEntry> Asks { get; set; }

    [JsonPropertyName("time")]
    public required string Time { get; set; }
}

public class PriceBookEntry
{
    [JsonPropertyName("price")]
    public required string Price { get; set; }

    [JsonPropertyName("size")]
    public required string Size { get; set; }
}