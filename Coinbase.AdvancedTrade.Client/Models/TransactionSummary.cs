using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class TransactionSummaryResponse
{
    [JsonPropertyName("total_fees")]
    public double TotalFees { get; set; }

    [JsonPropertyName("fee_tier")]
    public required FeeTier FeeTier { get; set; }

    [JsonPropertyName("margin_rate")]
    public string? MarginRate { get; set; }

    [JsonPropertyName("goods_and_services_tax")]
    public object? GoodsAndServicesTax { get; set; }

    [JsonPropertyName("advanced_trade_only_volume")]
    public double AdvancedTradeOnlyVolume { get; set; }

    [JsonPropertyName("advanced_trade_only_fees")]
    public double AdvancedTradeOnlyFees { get; set; }

    [JsonPropertyName("coinbase_pro_volume")]
    public double CoinbaseProVolume { get; set; }

    [JsonPropertyName("coinbase_pro_fees")]
    public double CoinbaseProFees { get; set; }

    [JsonPropertyName("total_balance")]
    public string? TotalBalance { get; set; }
}

public class FeeTier
{
    [JsonPropertyName("pricing_tier")]
    public string? PricingTier { get; set; }

    [JsonPropertyName("taker_fee_rate")]
    public required string TakerFeeRate { get; set; }

    [JsonPropertyName("maker_fee_rate")]
    public required string MakerFeeRate { get; set; }

    [JsonPropertyName("aop_from")]
    public string? AopFrom { get; set; }

    [JsonPropertyName("aop_to")]
    public string? AopTo { get; set; }
}
