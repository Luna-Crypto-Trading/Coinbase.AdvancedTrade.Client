using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class ProductBookResponse
{
    [JsonPropertyName("pricebook")]
    public required PriceBook PriceBook { get; set; }
}
