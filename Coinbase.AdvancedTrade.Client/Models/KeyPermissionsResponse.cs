using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class KeyPermissionsResponse
{
    [JsonPropertyName("can_view")]
    public bool CanView { get; set; }

    [JsonPropertyName("can_trade")]
    public bool CanTrade { get; set; }

    [JsonPropertyName("can_transfer")]
    public bool CanTransfer { get; set; }

    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    [JsonPropertyName("portfolio_type")]
    public string? PortfolioType { get; set; }
}
