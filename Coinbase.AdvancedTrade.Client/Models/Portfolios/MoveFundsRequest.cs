using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class MoveFundsRequest
{
    [JsonPropertyName("funds")]
    public required MoveFundsAmount Funds { get; set; }

    [JsonPropertyName("source_portfolio_uuid")]
    public required string SourcePortfolioUuid { get; set; }

    [JsonPropertyName("target_portfolio_uuid")]
    public required string TargetPortfolioUuid { get; set; }
}

public class MoveFundsAmount
{
    [JsonPropertyName("value")]
    public required string Value { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
}

public class MoveFundsResponse
{
    [JsonPropertyName("source_portfolio_uuid")]
    public string? SourcePortfolioUuid { get; set; }

    [JsonPropertyName("target_portfolio_uuid")]
    public string? TargetPortfolioUuid { get; set; }
}
