using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class AllocatePortfolioRequest
{
    [JsonPropertyName("portfolio_uuid")]
    public required string PortfolioUuid { get; set; }

    [JsonPropertyName("symbol")]
    public required string Symbol { get; set; }

    [JsonPropertyName("amount")]
    public required string Amount { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
}

public class IntxPortfolioResponse
{
    [JsonPropertyName("summary")]
    public required IntxPortfolioSummary Summary { get; set; }
}

public class IntxPortfolioSummary
{
    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    [JsonPropertyName("collateral")]
    public string? Collateral { get; set; }

    [JsonPropertyName("position_notional")]
    public string? PositionNotional { get; set; }

    [JsonPropertyName("open_position_notional")]
    public string? OpenPositionNotional { get; set; }

    [JsonPropertyName("pending_fees")]
    public string? PendingFees { get; set; }

    [JsonPropertyName("borrow")]
    public string? Borrow { get; set; }

    [JsonPropertyName("accrued_interest")]
    public string? AccruedInterest { get; set; }

    [JsonPropertyName("rolling_debt")]
    public string? RollingDebt { get; set; }

    [JsonPropertyName("portfolio_initial_margin")]
    public string? PortfolioInitialMargin { get; set; }

    [JsonPropertyName("portfolio_current_margin")]
    public string? PortfolioCurrentMargin { get; set; }

    [JsonPropertyName("in_liquidation")]
    public bool? InLiquidation { get; set; }
}

public class IntxPositionsResponse
{
    [JsonPropertyName("positions")]
    public required List<IntxPosition> Positions { get; set; }
}

public class IntxPositionResponse
{
    [JsonPropertyName("position")]
    public required IntxPosition Position { get; set; }
}

public class IntxPosition
{
    [JsonPropertyName("symbol")]
    public required string Symbol { get; set; }

    [JsonPropertyName("portfolio_uuid")]
    public string? PortfolioUuid { get; set; }

    [JsonPropertyName("net_size")]
    public string? NetSize { get; set; }

    [JsonPropertyName("vwap")]
    public string? Vwap { get; set; }

    [JsonPropertyName("entry_vwap")]
    public string? EntryVwap { get; set; }

    [JsonPropertyName("unrealized_pnl")]
    public string? UnrealizedPnl { get; set; }

    [JsonPropertyName("mark_price")]
    public string? MarkPrice { get; set; }

    [JsonPropertyName("liquidation_price")]
    public string? LiquidationPrice { get; set; }

    [JsonPropertyName("leverage")]
    public string? Leverage { get; set; }

    [JsonPropertyName("im_notional")]
    public string? ImNotional { get; set; }

    [JsonPropertyName("mm_notional")]
    public string? MmNotional { get; set; }

    [JsonPropertyName("position_side")]
    public string? PositionSide { get; set; }
}

public class IntxBalancesResponse
{
    [JsonPropertyName("portfolio_balances")]
    public required List<IntxBalance> PortfolioBalances { get; set; }
}

public class IntxBalance
{
    [JsonPropertyName("asset_id")]
    public required string AssetId { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("hold")]
    public string? Hold { get; set; }

    [JsonPropertyName("transfer_hold")]
    public string? TransferHold { get; set; }

    [JsonPropertyName("collateral_value")]
    public string? CollateralValue { get; set; }

    [JsonPropertyName("max_withdraw_amount")]
    public string? MaxWithdrawAmount { get; set; }
}

public class MultiAssetCollateralRequest
{
    [JsonPropertyName("portfolio_uuid")]
    public required string PortfolioUuid { get; set; }

    [JsonPropertyName("multi_asset_collateral_enabled")]
    public required bool MultiAssetCollateralEnabled { get; set; }
}

public class MultiAssetCollateralResponse
{
    [JsonPropertyName("multi_asset_collateral_enabled")]
    public bool MultiAssetCollateralEnabled { get; set; }
}
