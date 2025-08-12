using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public sealed record OrderRequest
{
    /// <summary>
    /// A unique ID provided for the order (used for identification purposes). If the ID provided is not unique, the order will not be created and the order corresponding with that ID will be returned instead.
    /// </summary>
    [JsonPropertyName("client_order_id")]
    public required string ClientOrderId { get; set; }

    /// <summary>
    /// The trading pair (e.g. 'BTC-USD').
    /// </summary>
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    /// <summary>
    /// The side of the market that the order is on (e.g. 'BUY', 'SELL'). Possible values: [BUY, SELL]
    /// </summary>
    [JsonPropertyName("side")]
    public required string Side { get; set; }

    /// <summary>
    /// The configuration of the order (e.g. the order type, size, etc).
    /// </summary>
    [JsonPropertyName("order_configuration")]
    public required OrderConfiguration OrderConfiguration { get; set; }

    /// <summary>
    /// The amount of leverage for the order (default is 1.0).
    /// </summary>
    [JsonPropertyName("leverage")]
    public string? Leverage { get; set; }

    /// <summary>
    /// Margin Type for this order (default is CROSS). Possible values: [CROSS, ISOLATED]
    /// </summary>
    [JsonPropertyName("margin_type")]
    public string? MarginType { get; set; }

    /// <summary>
    /// Preview ID for this order, to associate this order with a preview request
    /// </summary>
    [JsonPropertyName("preview_id")]
    public string? PreviewId { get; set; }
}

public record OrderInformation
{
    /// <summary>
    /// Whether the order was created.
    /// </summary>
    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("success_response")]
    public SuccessResponse? SuccessResponse { get; set; }

    [JsonPropertyName("error_response")]
    public ErrorResponse? ErrorResponse { get; set; }

    /// <summary>
    /// The configuration of the order (e.g. the order type, size, etc).
    /// </summary>
    [JsonPropertyName("order_configuration")]
    public OrderConfiguration? OrderConfiguration { get; set; }
}

public record SuccessResponse
{
    /// <summary>
    /// The ID of the order.
    /// </summary>
    [JsonPropertyName("order_id")]
    public required string OrderId { get; set; }

    /// <summary>
    /// The trading pair (e.g. 'BTC-USD').
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; set; }

    /// <summary>
    /// The side of the market that the order is on (e.g. 'BUY', 'SELL'). Possible values: [BUY, SELL]
    /// </summary>
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    /// <summary>
    /// The unique ID provided for the order (used for identification purposes).
    /// </summary>
    [JsonPropertyName("client_order_id")]
    public string? ClientOrderId { get; set; }
}

public record ErrorResponse
{
    /// <summary>
    /// **(Deprecated)** The reason the order failed to be created. Possible values: [UNKNOWN_FAILURE_REASON, UNSUPPORTED_ORDER_CONFIGURATION, INVALID_SIDE, INVALID_PRODUCT_ID, INVALID_SIZE_PRECISION, INVALID_PRICE_PRECISION, INSUFFICIENT_FUND, INVALID_LEDGER_BALANCE, ORDER_ENTRY_DISABLED, INELIGIBLE_PAIR, INVALID_LIMIT_PRICE_POST_ONLY, INVALID_LIMIT_PRICE, INVALID_NO_LIQUIDITY, INVALID_REQUEST, COMMANDER_REJECTED_NEW_ORDER, INSUFFICIENT_FUNDS, IN_LIQUIDATION, INVALID_MARGIN_TYPE, INVALID_LEVERAGE, UNTRADABLE_PRODUCT, INVALID_FCM_TRADING_SESSION, GEOFENCING_RESTRICTION, QUOTE_SIZE_NOT_ALLOWED_FOR_BRACKET, INVALID_BRACKET_PRICES, MISSING_MARKET_TRADE_DATA, INVALID_BRACKET_LIMIT_PRICE, INVALID_BRACKET_STOP_TRIGGER_PRICE, BRACKET_LIMIT_PRICE_OUT_OF_BOUNDS, STOP_TRIGGER_PRICE_OUT_OF_BOUNDS, BRACKET_ORDER_NOT_SUPPORTED, FOK_DISABLED, FOK_ONLY_ALLOWED_ON_LIMIT_ORDERS, POST_ONLY_NOT_ALLOWED_WITH_FOK, UBO_HIGH_LEVERAGE_QUANTITY_BREACHED, END_TIME_TOO_FAR_IN_FUTURE, LIMIT_PRICE_TOO_FAR_FROM_MARKET, OPEN_BRACKET_ORDERS, FUTURES_AFTER_HOUR_INVALID_ORDER_TYPE, FUTURES_AFTER_HOUR_INVALID_TIME_IN_FORCE, INVALID_ATTACHED_TAKE_PROFIT_PRICE, INVALID_ATTACHED_STOP_LOSS_PRICE, INVALID_ATTACHED_TAKE_PROFIT_PRICE_PRECISION, INVALID_ATTACHED_STOP_LOSS_PRICE_PRECISION, INVALID_ATTACHED_TAKE_PROFIT_PRICE_OUT_OF_BOUNDS, INVALID_ATTACHED_STOP_LOSS_PRICE_OUT_OF_BOUNDS]
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Generic error message explaining why the order was not created
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Descriptive error message explaining why the order was not created
    /// </summary>
    [JsonPropertyName("error_details")]
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// **(Deprecated)** The reason the order failed to be created. Possible values: [UNKNOWN_PREVIEW_FAILURE_REASON, PREVIEW_MISSING_COMMISSION_RATE, PREVIEW_INVALID_SIDE, PREVIEW_INVALID_ORDER_CONFIG, PREVIEW_INVALID_PRODUCT_ID, PREVIEW_INVALID_SIZE_PRECISION, PREVIEW_INVALID_PRICE_PRECISION, PREVIEW_MISSING_PRODUCT_PRICE_BOOK, PREVIEW_INVALID_LEDGER_BALANCE, PREVIEW_INSUFFICIENT_LEDGER_BALANCE, PREVIEW_INVALID_LIMIT_PRICE_POST_ONLY, PREVIEW_INVALID_LIMIT_PRICE, PREVIEW_INVALID_NO_LIQUIDITY, PREVIEW_INSUFFICIENT_FUND, PREVIEW_INVALID_COMMISSION_CONFIGURATION, PREVIEW_INVALID_STOP_PRICE, PREVIEW_INVALID_BASE_SIZE_TOO_LARGE, PREVIEW_INVALID_BASE_SIZE_TOO_SMALL, PREVIEW_INVALID_QUOTE_SIZE_PRECISION, PREVIEW_INVALID_QUOTE_SIZE_TOO_LARGE, PREVIEW_INVALID_PRICE_TOO_LARGE, PREVIEW_INVALID_QUOTE_SIZE_TOO_SMALL, PREVIEW_INSUFFICIENT_FUNDS_FOR_FUTURES, PREVIEW_BREACHED_PRICE_LIMIT, PREVIEW_BREACHED_ACCOUNT_POSITION_LIMIT, PREVIEW_BREACHED_COMPANY_POSITION_LIMIT, PREVIEW_INVALID_MARGIN_HEALTH, PREVIEW_RISK_PROXY_FAILURE, PREVIEW_UNTRADABLE_FCM_ACCOUNT_STATUS, PREVIEW_IN_LIQUIDATION, PREVIEW_INVALID_MARGIN_TYPE, PREVIEW_INVALID_LEVERAGE, PREVIEW_UNTRADABLE_PRODUCT, PREVIEW_INVALID_FCM_TRADING_SESSION, PREVIEW_NOT_ALLOWED_BY_MARKET_STATE, PREVIEW_BREACHED_OPEN_INTEREST_LIMIT, PREVIEW_GEOFENCING_RESTRICTION, PREVIEW_INVALID_END_TIME, PREVIEW_OPPOSITE_MARGIN_TYPE_EXISTS, PREVIEW_QUOTE_SIZE_NOT_ALLOWED_FOR_BRACKET, PREVIEW_INVALID_BRACKET_PRICES, PREVIEW_MISSING_MARKET_TRADE_DATA, PREVIEW_INVALID_BRACKET_LIMIT_PRICE, PREVIEW_INVALID_BRACKET_STOP_TRIGGER_PRICE, PREVIEW_BRACKET_LIMIT_PRICE_OUT_OF_BOUNDS, PREVIEW_STOP_TRIGGER_PRICE_OUT_OF_BOUNDS, PREVIEW_BRACKET_ORDER_NOT_SUPPORTED, PREVIEW_INVALID_STOP_PRICE_PRECISION, PREVIEW_STOP_PRICE_ABOVE_LIMIT_PRICE, PREVIEW_STOP_PRICE_BELOW_LIMIT_PRICE, PREVIEW_STOP_PRICE_ABOVE_LAST_TRADE_PRICE, PREVIEW_STOP_PRICE_BELOW_LAST_TRADE_PRICE, PREVIEW_FOK_DISABLED, PREVIEW_FOK_ONLY_ALLOWED_ON_LIMIT_ORDERS, PREVIEW_POST_ONLY_NOT_ALLOWED_WITH_FOK, PREVIEW_UBO_HIGH_LEVERAGE_QUANTITY_BREACHED, PREVIEW_ECOSYSTEM_LEVERAGE_UTILIZATION_BREACHED, PREVIEW_CLOSE_ONLY_FAILURE, PREVIEW_UBO_HIGH_LEVERAGE_NOTIONAL_BREACHED, PREVIEW_END_TIME_TOO_FAR_IN_FUTURE, PREVIEW_LIMIT_PRICE_TOO_FAR_FROM_MARKET, PREVIEW_FUTURES_AFTER_HOUR_INVALID_ORDER_TYPE, PREVIEW_FUTURES_AFTER_HOUR_INVALID_TIME_IN_FORCE, PREVIEW_INVALID_ATTACHED_TAKE_PROFIT_PRICE, PREVIEW_INVALID_ATTACHED_STOP_LOSS_PRICE, PREVIEW_INVALID_ATTACHED_TAKE_PROFIT_PRICE_PRECISION, PREVIEW_INVALID_ATTACHED_STOP_LOSS_PRICE_PRECISION, PREVIEW_INVALID_ATTACHED_TAKE_PROFIT_PRICE_OUT_OF_BOUNDS, PREVIEW_INVALID_ATTACHED_STOP_LOSS_PRICE_OUT_OF_BOUNDS, PREVIEW_INVALID_BRACKET_ORDER_SIDE, PREVIEW_BRACKET_ORDER_SIZE_EXCEEDS_POSITION, PREVIEW_ORDER_SIZE_EXCEEDS_BRACKETED_POSITION, PREVIEW_INVALID_LIMIT_PRICE_PRECISION, PREVIEW_INVALID_STOP_TRIGGER_PRICE_PRECISION]
    /// </summary>
    [JsonPropertyName("preview_failure_reason")]
    public string? PreviewFailureReason { get; set; }

    /// <summary>
    /// The reason the order failed to be created. Possible values: [UNKNOWN_FAILURE_REASON, UNSUPPORTED_ORDER_CONFIGURATION, INVALID_SIDE, INVALID_PRODUCT_ID, INVALID_SIZE_PRECISION, INVALID_PRICE_PRECISION, INSUFFICIENT_FUND, INVALID_LEDGER_BALANCE, ORDER_ENTRY_DISABLED, INELIGIBLE_PAIR, INVALID_LIMIT_PRICE_POST_ONLY, INVALID_LIMIT_PRICE, INVALID_NO_LIQUIDITY, INVALID_REQUEST, COMMANDER_REJECTED_NEW_ORDER, INSUFFICIENT_FUNDS, IN_LIQUIDATION, INVALID_MARGIN_TYPE, INVALID_LEVERAGE, UNTRADABLE_PRODUCT, INVALID_FCM_TRADING_SESSION, GEOFENCING_RESTRICTION, QUOTE_SIZE_NOT_ALLOWED_FOR_BRACKET, INVALID_BRACKET_PRICES, MISSING_MARKET_TRADE_DATA, INVALID_BRACKET_LIMIT_PRICE, INVALID_BRACKET_STOP_TRIGGER_PRICE, BRACKET_LIMIT_PRICE_OUT_OF_BOUNDS, STOP_TRIGGER_PRICE_OUT_OF_BOUNDS, BRACKET_ORDER_NOT_SUPPORTED, FOK_DISABLED, FOK_ONLY_ALLOWED_ON_LIMIT_ORDERS, POST_ONLY_NOT_ALLOWED_WITH_FOK, UBO_HIGH_LEVERAGE_QUANTITY_BREACHED, END_TIME_TOO_FAR_IN_FUTURE, LIMIT_PRICE_TOO_FAR_FROM_MARKET, OPEN_BRACKET_ORDERS, FUTURES_AFTER_HOUR_INVALID_ORDER_TYPE, FUTURES_AFTER_HOUR_INVALID_TIME_IN_FORCE, INVALID_ATTACHED_TAKE_PROFIT_PRICE, INVALID_ATTACHED_STOP_LOSS_PRICE, INVALID_ATTACHED_TAKE_PROFIT_PRICE_PRECISION, INVALID_ATTACHED_STOP_LOSS_PRICE_PRECISION, INVALID_ATTACHED_TAKE_PROFIT_PRICE_OUT_OF_BOUNDS, INVALID_ATTACHED_STOP_LOSS_PRICE_OUT_OF_BOUNDS]
    /// </summary>
    [JsonPropertyName("new_order_failure_reason")]
    public required string NewOrderFailureReason { get; set; }
}

/// <summary>
/// The configuration of the order (e.g. the order type, size, etc).
/// </summary>
public class OrderConfiguration
{
    /// <summary>
    /// Buy or sell a specified quantity of an Asset at the current best available market price. Read more on Market Orders (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#market-order)
    /// </summary>
    [JsonPropertyName("market_market_ioc")]
    public MarketMarketIoc? MarketMarketIoc { get; set; }

    /// <summary>
    /// Buy or sell a specified quantity of an Asset at a specified price.
    /// The Order will only post to the Order Book if it will immediately Fill;
    /// any remaining quantity is canceled. Read more on Limit Orders.
    /// (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#limit-order)
    /// </summary>
    [JsonPropertyName("sor_limit_ioc")]
    public SorLimitIoc? SorLimitIoc { get; set; }

    /// <summary>
    /// Buy or sell a specified quantity of an Asset at a specified price.
    /// If posted, the Order will remain on the Order Book until canceled.
    /// Read more on Limit Orders.
    /// (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#limit-order)
    /// </summary>
    [JsonPropertyName("limit_limit_gtc")]
    public LimitLimitGtcV3? LimitLimitGtc { get; set; }

    /// <summary>
    /// Buy or sell a specified quantity of an Asset at a specified price.
    /// If posted, the Order will remain on the Order Book until a certain
    /// time is reached or the Order is canceled. Read more on Limit Orders.
    /// (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#limit-order)
    /// </summary>
    [JsonPropertyName("limit_limit_gtd")]
    public LimitLimitGtdV3? LimitLimitGtd { get; set; }

    /// <summary>
    /// Buy or sell a specified quantity of an Asset at a specified price.
    /// The Order will only post to the Order Book if it is to
    /// immediately and completely Fill. Read more on Limit Orders.
    /// (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#limit-order)
    /// </summary>
    [JsonPropertyName("limit_limit_fok")]
    public LimitLimitFokV3? LimitLimitFok { get; set; }

    /// <summary>
    /// Posts an Order to buy or sell a specified quantity of an Asset, but only if and when the last trade price on the Order Book equals or surpasses the Stop CurrentPrice. If posted, the Order will remain on the Order Book until canceled. Read more on Stop-Limit Orders. (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#stop-limit-order)
    /// </summary>
    [JsonPropertyName("stop_limit_stop_limit_gtc")]
    public StopLimitStopLimitGtcV3? StopLimitStopLimitGtc { get; set; }

    /// <summary>
    /// Posts an Order to buy or sell a specified quantity of an Asset, but only if and when the last trade price on the Order Book equals or surpasses the Stop CurrentPrice. If posted, the Order will remain on the Order Book until a certain time is reached or the Order. Read more on Stop-Limit Orders. (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#stop-limit-order)
    /// </summary>
    [JsonPropertyName("stop_limit_stop_limit_gtd")]
    public StopLimitStopLimitGtdV3? StopLimitStopLimitGtd { get; set; }

    /// <summary>
    /// A Limit Order to buy or sell a specified quantity of an Asset at a specified price, with stop limit order parameters embedded in the order. If posted, the Order will remain on the Order Book until canceled. Read more on Bracket Orders. (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#bracket-order)
    /// </summary>
    [JsonPropertyName("trigger_bracket_gtc")]
    public TriggerBracketGtcV3? TriggerBracketGtc { get; set; }

    /// <summary>
    /// A Limit Order to buy or sell a specified quantity of an Asset at a specified price, with stop limit order parameters embedded in the order. If posted, the Order will remain on the Order Book until a certain time is reached or the Order is canceled. Read more on Bracket Orders. (https://help.coinbase.com/en/coinbase/trading-and-funding/advanced-trade/order-types#bracket-order)
    /// </summary>
    [JsonPropertyName("trigger_bracket_gtd")]
    public TriggerBracketGtdV3? TriggerBracketGtd { get; set; }
}

public class MarketMarketIoc
{
    /// <summary>
    /// The amount of the second Asset in the Trading Pair. For example,
    /// on the BTC/USD Order Book, USD is the Quote Asset.
    /// </summary>
    [JsonPropertyName("quote_size")]
    public string? QuoteSize { get; set; }

    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example,
    /// on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public string? BaseSize { get; set; }
}

public class SorLimitIoc
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }
}

public class LimitLimitGtcV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    /// <summary>
    /// Enable or disable Post-only Mode. When enabled, only Maker Orders will be posted to the Order Book. Orders that will be posted as a Taker Order will be rejected.
    /// </summary>
    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
}

public class LimitLimitGtdV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    /// <summary>
    /// The time at which the order will be cancelled if it is not Filled.
    /// </summary>
    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Enable or disable Post-only Mode. When enabled, only Maker Orders will be posted to the Order Book. Orders that will be posted as a Taker Order will be rejected.
    /// </summary>
    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }
}

public class LimitLimitFokV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }
}

public class StopLimitStopLimitGtcV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public required string BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    /// <summary>
    /// The specified price that will trigger the placement of the Order.
    /// </summary>
    [JsonPropertyName("stop_price")]
    public required string StopPrice { get; set; }

    /// <summary>
    /// The direction of the stop limit Order. If Up, then the Order will trigger when the last trade price goes above the stop_price. If Down, then the Order will trigger when the last trade price goes below the stop_price. Possible values: [STOP_DIRECTION_STOP_UP, STOP_DIRECTION_STOP_DOWN]
    /// </summary>
    [JsonPropertyName("stop_direction")]
    public required string StopDirection { get; set; }
}

public class StopLimitStopLimitGtdV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public decimal BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    /// <summary>
    /// The specified price that will trigger the placement of the Order.
    /// </summary>
    [JsonPropertyName("stop_price")]
    public required string StopPrice { get; set; }

    /// <summary>
    /// The time at which the order will be cancelled if it is not Filled.
    /// </summary>
    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// The direction of the stop limit Order. If Up, then the Order will trigger when the last trade price goes above the stop_price. If Down, then the Order will trigger when the last trade price goes below the stop_price. Possible values: [STOP_DIRECTION_STOP_UP, STOP_DIRECTION_STOP_DOWN]
    /// </summary>
    [JsonPropertyName("stop_direction")]
    public required string StopDirection { get; set; }
}

public class TriggerBracketGtcV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public decimal BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    /// <summary>
    /// The price level (in quote currency) where the position will be exited. When triggered, a stop limit order is automatically placed with a limit price 5% higher for BUYS and 5% lower for SELLS.
    /// </summary>
    [JsonPropertyName("stop_trigger_price")]
    public required string StopTriggerPrice { get; set; }
}

public class TriggerBracketGtdV3
{
    /// <summary>
    /// The amount of the first Asset in the Trading Pair. For example, on the BTC-USD Order Book, BTC is the Base Asset.
    /// </summary>
    [JsonPropertyName("base_size")]
    public decimal BaseSize { get; set; }

    /// <summary>
    /// The specified price, or better, that the Order should be executed at. A Buy Order will execute at or lower than the limit price. A Sell Order will execute at or higher than the limit price.
    /// </summary>
    [JsonPropertyName("limit_price")]
    public required string LimitPrice { get; set; }

    /// <summary>
    /// The price level (in quote currency) where the position will be exited. When triggered, a stop limit order is automatically placed with a limit price 5% higher for BUYS and 5% lower for SELLS.
    /// </summary>
    [JsonPropertyName("stop_trigger_price")]
    public required string StopTriggerPrice { get; set; }

    /// <summary>
    /// The time at which the order will be cancelled if it is not Filled.
    /// </summary>
    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }
}