using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class EditPortfolioRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
