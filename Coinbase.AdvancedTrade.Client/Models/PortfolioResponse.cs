using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class AdvancedTradePortfolio
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("uuid")]
    public required string Uuid { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("deleted")]
    public required bool Deleted { get; set; }
}

public class AdvancedTradePortfolioResponse
{
    [JsonPropertyName("portfolios")]
    public required List<AdvancedTradePortfolio> Portfolios { get; set; }
}