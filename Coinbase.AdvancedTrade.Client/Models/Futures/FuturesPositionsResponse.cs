using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class FuturesPositionsResponse
{
    [JsonPropertyName("positions")]
    public required List<FuturesPositionDetail> Positions { get; set; }
}

public class FuturesPositionResponse
{
    [JsonPropertyName("position")]
    public required FuturesPositionDetail Position { get; set; }
}

public class FuturesPositionDetail
{
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("expiration_time")]
    public string? ExpirationTime { get; set; }

    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("number_of_contracts")]
    public string? NumberOfContracts { get; set; }

    [JsonPropertyName("current_price")]
    public string? CurrentPrice { get; set; }

    [JsonPropertyName("avg_entry_price")]
    public string? AvgEntryPrice { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public string? UnrealizedPnl { get; set; }

    [JsonPropertyName("daily_realized_pnl")]
    public string? DailyRealizedPnl { get; set; }
}
