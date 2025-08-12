using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class AdvancedTradePortfolioBreakdownResponse
{
    [JsonPropertyName("breakdown")]
    public required AdvancedTradePortfolioBreakdown Breakdown { get; set; }
}

public class AdvancedTradePortfolioBreakdown
{
    [JsonPropertyName("portfolio")]
    public required AdvancedTradePortfolio Portfolio { get; set; }

    [JsonPropertyName("portfolio_balances")]
    public required AdvancedTradePortfolioBalances PortfolioBalances { get; set; }

    [JsonPropertyName("spot_positions")]
    public required List<AdvancedTradeSpotPosition> SpotPositions { get; set; }

    [JsonPropertyName("perp_positions")]
    public required List<AdvancedTradePerpPosition> PerpPositions { get; set; }

    [JsonPropertyName("futures_positions")]
    public required List<AdvancedTradeFuturesPosition> FuturesPositions { get; set; }
}

public class AdvancedTradePortfolioBalances
{
    [JsonPropertyName("total_balance")]
    public required AdvancedTradeBalance TotalBalance { get; set; }

    [JsonPropertyName("total_futures_balance")]
    public required AdvancedTradeBalance TotalFuturesBalance { get; set; }

    [JsonPropertyName("total_cash_equivalent_balance")]
    public required AdvancedTradeBalance TotalCashEquivalentBalance { get; set; }

    [JsonPropertyName("total_crypto_balance")]
    public required AdvancedTradeBalance TotalCryptoBalance { get; set; }

    [JsonPropertyName("futures_unrealized_pnl")]
    public required AdvancedTradeBalance FuturesUnrealizedPnl { get; set; }

    [JsonPropertyName("perp_unrealized_pnl")]
    public required AdvancedTradeBalance PerpUnrealizedPnl { get; set; }
}

public class AdvancedTradeBalance
{
    [JsonPropertyName("value")]
    public required string Value { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
}

public class AdvancedTradeSpotPosition
{
    [JsonPropertyName("asset")]
    public required string Asset { get; set; }

    [JsonPropertyName("account_uuid")]
    public required string AccountUuid { get; set; }

    [JsonPropertyName("total_balance_fiat")]
    public required decimal TotalBalanceFiat { get; set; }

    [JsonPropertyName("total_balance_crypto")]
    public required decimal TotalBalanceCrypto { get; set; }

    [JsonPropertyName("available_to_trade_fiat")]
    public required decimal AvailableToTradeFiat { get; set; }

    [JsonPropertyName("allocation")]
    public required decimal Allocation { get; set; }

    [JsonPropertyName("one_day_change")]
    public decimal OneDayChange { get; set; }

    [JsonPropertyName("cost_basis")]
    public required AdvancedTradeBalance CostBasis { get; set; }

    [JsonPropertyName("asset_img_url")]
    public required string AssetImgUrl { get; set; }

    [JsonPropertyName("is_cash")]
    public required bool IsCash { get; set; }
}

public class AdvancedTradePerpPosition
{
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("product_uuid")]
    public required string ProductUuid { get; set; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; set; }

    [JsonPropertyName("asset_image_url")]
    public required string AssetImageUrl { get; set; }

    [JsonPropertyName("vwap")]
    public required AdvancedTradeVwapPrice Vwap { get; set; }

    [JsonPropertyName("position_side")]
    public required string PositionSide { get; set; }

    [JsonPropertyName("net_size")]
    public required string NetSize { get; set; }

    [JsonPropertyName("buy_order_size")]
    public required string BuyOrderSize { get; set; }

    [JsonPropertyName("sell_order_size")]
    public required string SellOrderSize { get; set; }

    [JsonPropertyName("im_contribution")]
    public required string ImContribution { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public required AdvancedTradeVwapPrice UnrealizedPnl { get; set; }

    [JsonPropertyName("mark_price")]
    public required AdvancedTradeVwapPrice MarkPrice { get; set; }

    [JsonPropertyName("liquidation_price")]
    public required AdvancedTradeVwapPrice LiquidationPrice { get; set; }

    [JsonPropertyName("leverage")]
    public required string Leverage { get; set; }

    [JsonPropertyName("im_notional")]
    public required AdvancedTradeVwapPrice ImNotional { get; set; }

    [JsonPropertyName("mm_notional")]
    public required AdvancedTradeVwapPrice MmNotional { get; set; }

    [JsonPropertyName("position_notional")]
    public required AdvancedTradeVwapPrice PositionNotional { get; set; }

    [JsonPropertyName("margin_type")]
    public required string MarginType { get; set; }

    [JsonPropertyName("liquidation_buffer")]
    public required string LiquidationBuffer { get; set; }

    [JsonPropertyName("liquidation_percentage")]
    public required string LiquidationPercentage { get; set; }
}

public class AdvancedTradeVwapPrice
{
    [JsonPropertyName("userNativeCurrency")]
    public required AdvancedTradeBalance UserNativeCurrency { get; set; }

    [JsonPropertyName("rawCurrency")]
    public required AdvancedTradeBalance RawCurrency { get; set; }
}

public class AdvancedTradeFuturesPosition
{
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("contract_size")]
    public required string ContractSize { get; set; }

    [JsonPropertyName("side")]
    public required string Side { get; set; }

    [JsonPropertyName("amount")]
    public required string Amount { get; set; }

    [JsonPropertyName("avg_entry_price")]
    public required string AvgEntryPrice { get; set; }

    [JsonPropertyName("current_price")]
    public required string CurrentPrice { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public required string UnrealizedPnl { get; set; }

    [JsonPropertyName("expiry")]
    public required string Expiry { get; set; }

    [JsonPropertyName("underlying_asset")]
    public required string UnderlyingAsset { get; set; }

    [JsonPropertyName("asset_img_url")]
    public required string AssetImgUrl { get; set; }

    [JsonPropertyName("product_name")]
    public required string ProductName { get; set; }

    [JsonPropertyName("venue")]
    public required string Venue { get; set; }

    [JsonPropertyName("notional_value")]
    public required string NotionalValue { get; set; }
}