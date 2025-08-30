using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class GetOrderConfiguration
{
    [JsonPropertyName("market_market_ioc")]
    public required MarketMarketIoc MarketMarketIoc { get; set; }

    [JsonPropertyName("sor_limit_ioc")]
    public required SorLimitIoc SorLimitIoc { get; set; }

    [JsonPropertyName("limit_limit_gtc")]
    public required LimitLimitGtc LimitLimitGtc { get; set; }

    [JsonPropertyName("limit_limit_gtd")]
    public required LimitLimitGtd LimitLimitGtd { get; set; }

    [JsonPropertyName("limit_limit_fok")]
    public required LimitLimitFok LimitLimitFok { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtc")]
    public required StopLimitStopLimitGtc StopLimitStopLimitGtc { get; set; }

    [JsonPropertyName("stop_limit_stop_limit_gtd")]
    public required StopLimitStopLimitGtd StopLimitStopLimitGtd { get; set; }

    [JsonPropertyName("trigger_bracket_gtc")]
    public required TriggerBracketGtc TriggerBracketGtc { get; set; }

    [JsonPropertyName("trigger_bracket_gtd")]
    public required TriggerBracketGtd TriggerBracketGtd { get; set; }
}

public class LimitLimitGtc
{
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
}

public class LimitLimitGtd
{
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    [JsonPropertyName("end_time")]
    public required string EndTime { get; set; }

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
}

public class LimitLimitFok
{
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }
}

public class StopLimitStopLimitGtc
{
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    [JsonPropertyName("stop_price")]
    public required string StopPrice { get; set; }

    [JsonPropertyName("stop_direction")]
    public required string StopDirection { get; set; }
}

public class StopLimitStopLimitGtd
{
    [JsonPropertyName("base_size")]
    public double BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    [JsonPropertyName("stop_price")]
    public required string StopPrice { get; set; }

    [JsonPropertyName("end_time")]
    public required string EndTime { get; set; }

    [JsonPropertyName("stop_direction")]
    public required string StopDirection { get; set; }
}

public class TriggerBracketGtc
{
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    [JsonPropertyName("stop_trigger_price")]
    public required string StopTriggerPrice { get; set; }
}

public class TriggerBracketGtd
{
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    [JsonPropertyName("stop_trigger_price")]
    public required string StopTriggerPrice { get; set; }

    [JsonPropertyName("end_time")]
    public required string EndTime { get; set; }
}

public class EditHistory
{
    [JsonPropertyName("price")]
    public required string Price { get; set; }

    [JsonPropertyName("size")]
    public required string Size { get; set; }

    [JsonPropertyName("replace_accept_timestamp")]
    public required string ReplaceAcceptTimestamp { get; set; }
}

public class OrderV3
{
    [JsonPropertyName("order_id")]
    public required string OrderId { get; set; }

    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; set; }

    [JsonPropertyName("order_configuration")]
    public required OrderConfiguration OrderConfiguration { get; set; }

    [JsonPropertyName("side")]
    public required string Side { get; set; }

    [JsonPropertyName("client_order_id")]
    public required string ClientOrderId { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("time_in_force")]
    public required string TimeInForce { get; set; }

    [JsonPropertyName("created_time")]
    public required string CreatedTime { get; set; }

    [JsonPropertyName("completion_percentage")]
    public required string CompletionPercentage { get; set; }

    [JsonPropertyName("filled_size")]
    public required string FilledSize { get; set; }

    [JsonPropertyName("average_filled_price")]
    public required string AverageFilledPrice { get; set; }

    [JsonPropertyName("number_of_fills")]
    public required string NumberOfFills { get; set; }

    [JsonPropertyName("filled_value")]
    public string? FilledValue { get; set; }

    [JsonPropertyName("pending_cancel")]
    public bool PendingCancel { get; set; }

    [JsonPropertyName("size_in_quote")]
    public bool SizeInQuote { get; set; }

    [JsonPropertyName("total_fees")]
    public string? TotalFees { get; set; }

    [JsonPropertyName("size_inclusive_of_fees")]
    public bool SizeInclusiveOfFees { get; set; }

    [JsonPropertyName("total_value_after_fees")]
    public string? TotalValueAfterFees { get; set; }

    [JsonPropertyName("trigger_status")]
    public string? TriggerStatus { get; set; }

    [JsonPropertyName("order_type")]
    public string? OrderType { get; set; }

    [JsonPropertyName("reject_reason")]
    public string? RejectReason { get; set; }

    [JsonPropertyName("settled")]
    public bool Settled { get; set; }

    [JsonPropertyName("product_type")]
    public string? ProductType { get; set; }

    [JsonPropertyName("reject_message")]
    public string? RejectMessage { get; set; }

    [JsonPropertyName("cancel_message")]
    public string? CancelMessage { get; set; }

    [JsonPropertyName("order_placement_source")]
    public string? OrderPlacementSource { get; set; }

    [JsonPropertyName("outstanding_hold_amount")]
    public string? OutstandingHoldAmount { get; set; }

    [JsonPropertyName("is_liquidation")]
    public bool IsLiquidation { get; set; }

    [JsonPropertyName("last_fill_time")]
    public string? LastFillTime { get; set; }

    [JsonPropertyName("edit_history")]
    public List<EditHistory>? EditHistory { get; set; }
}

public class GetOrdersResponse
{
    [JsonPropertyName("orders")]
    public required List<OrderV3> Orders { get; set; }

    [JsonPropertyName("sequence")]
    public string? Sequence { get; set; }

    [JsonPropertyName("has_next")]
    public bool HasNext { get; set; }

    [JsonPropertyName("cursor")]
    public required string Cursor { get; set; }
}