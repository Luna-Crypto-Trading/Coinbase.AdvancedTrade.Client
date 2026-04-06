using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class FuturesBalanceSummaryResponse
{
    [JsonPropertyName("balance_summary")]
    public required FuturesBalanceSummary BalanceSummary { get; set; }
}

public class FuturesBalanceSummary
{
    [JsonPropertyName("futures_buying_power")]
    public string? FuturesBuyingPower { get; set; }

    [JsonPropertyName("total_usd_balance")]
    public string? TotalUsdBalance { get; set; }

    [JsonPropertyName("cbi_usd_balance")]
    public string? CbiUsdBalance { get; set; }

    [JsonPropertyName("cfm_usd_balance")]
    public string? CfmUsdBalance { get; set; }

    [JsonPropertyName("total_open_orders_hold_amount")]
    public string? TotalOpenOrdersHoldAmount { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public string? UnrealizedPnl { get; set; }

    [JsonPropertyName("daily_realized_pnl")]
    public string? DailyRealizedPnl { get; set; }

    [JsonPropertyName("initial_margin")]
    public string? InitialMargin { get; set; }

    [JsonPropertyName("available_margin")]
    public string? AvailableMargin { get; set; }

    [JsonPropertyName("liquidation_threshold")]
    public string? LiquidationThreshold { get; set; }

    [JsonPropertyName("liquidation_buffer_amount")]
    public string? LiquidationBufferAmount { get; set; }

    [JsonPropertyName("liquidation_buffer_percentage")]
    public string? LiquidationBufferPercentage { get; set; }

    [JsonPropertyName("intraday_margin_window_measure")]
    public string? IntradayMarginWindowMeasure { get; set; }
}
