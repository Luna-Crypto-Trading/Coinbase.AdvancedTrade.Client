using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class CreatePortfolioRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
