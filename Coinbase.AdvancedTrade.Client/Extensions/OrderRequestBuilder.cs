using System.Globalization;
using Coinbase.AdvancedTrade.Client.Models;

namespace Coinbase.AdvancedTrade.Client.Extensions;

/// <summary>
/// Builder class for creating common order types
/// </summary>
public class OrderRequestBuilder
{
    private readonly OrderRequest _order;

    private OrderRequestBuilder(string productId, string side)
    {
        _order = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = productId,
            Side = side,
            OrderConfiguration = new OrderConfiguration()
        };
    }

    /// <summary>
    /// Creates a new buy order builder
    /// </summary>
    public static OrderRequestBuilder Buy(string productId) => new(productId, "BUY");

    /// <summary>
    /// Creates a new sell order builder
    /// </summary>
    public static OrderRequestBuilder Sell(string productId) => new(productId, "SELL");

    /// <summary>
    /// Sets a custom client order ID
    /// </summary>
    public OrderRequestBuilder WithClientOrderId(string clientOrderId)
    {
        _order.ClientOrderId = clientOrderId;
        return this;
    }

    /// <summary>
    /// Creates a market order with quote size (amount in quote currency, e.g., USD)
    /// </summary>
    public OrderRequestBuilder MarketOrderByQuote(decimal quoteSize)
    {
        _order.OrderConfiguration.MarketMarketIoc = new MarketMarketIoc
        {
            QuoteSize = quoteSize.ToString(CultureInfo.InvariantCulture)
        };
        return this;
    }

    /// <summary>
    /// Creates a market order with base size (amount in base currency, e.g., BTC)
    /// </summary>
    public OrderRequestBuilder MarketOrderByBase(decimal baseSize)
    {
        _order.OrderConfiguration.MarketMarketIoc = new MarketMarketIoc
        {
            BaseSize = baseSize.ToString(CultureInfo.InvariantCulture)
        };
        return this;
    }

    /// <summary>
    /// Creates a limit order that stays on the book (Good Till Cancelled)
    /// </summary>
    public OrderRequestBuilder LimitOrder(decimal baseSize, decimal limitPrice, bool postOnly = false)
    {
        _order.OrderConfiguration.LimitLimitGtc = new LimitLimitGtcV3
        {
            BaseSize = baseSize.ToString(CultureInfo.InvariantCulture),
            LimitPrice = limitPrice.ToString(CultureInfo.InvariantCulture),
            PostOnly = postOnly
        };
        return this;
    }

    /// <summary>
    /// Creates a limit order with expiration time (Good Till Date)
    /// </summary>
    public OrderRequestBuilder LimitOrderWithExpiry(decimal baseSize, decimal limitPrice, DateTime endTime, bool postOnly = false)
    {
        _order.OrderConfiguration.LimitLimitGtd = new LimitLimitGtdV3
        {
            BaseSize = baseSize.ToString(CultureInfo.InvariantCulture),
            LimitPrice = limitPrice.ToString(CultureInfo.InvariantCulture),
            EndTime = endTime,
            PostOnly = postOnly
        };
        return this;
    }

    /// <summary>
    /// Creates a limit order that must fill completely or be cancelled (Fill Or Kill)
    /// </summary>
    public OrderRequestBuilder LimitOrderFillOrKill(decimal baseSize, decimal limitPrice)
    {
        _order.OrderConfiguration.LimitLimitFok = new LimitLimitFokV3
        {
            BaseSize = baseSize.ToString(CultureInfo.InvariantCulture),
            LimitPrice = limitPrice.ToString(CultureInfo.InvariantCulture)
        };
        return this;
    }

    /// <summary>
    /// Creates a stop-limit order (Good Till Cancelled)
    /// </summary>
    public OrderRequestBuilder StopLimitOrder(decimal baseSize, decimal limitPrice, decimal stopPrice, StopDirection direction)
    {
        var stopDirectionString = direction == StopDirection.Up ? "STOP_DIRECTION_STOP_UP" : "STOP_DIRECTION_STOP_DOWN";

        _order.OrderConfiguration.StopLimitStopLimitGtc = new StopLimitStopLimitGtcV3
        {
            BaseSize = baseSize.ToString(CultureInfo.InvariantCulture),
            LimitPrice = limitPrice.ToString(CultureInfo.InvariantCulture),
            StopPrice = stopPrice.ToString(CultureInfo.InvariantCulture),
            StopDirection = stopDirectionString
        };
        return this;
    }

    /// <summary>
    /// Creates a stop-limit order with expiration (Good Till Date)
    /// </summary>
    public OrderRequestBuilder StopLimitOrderWithExpiry(decimal baseSize, decimal limitPrice, decimal stopPrice, StopDirection direction, DateTime endTime)
    {
        var stopDirectionString = direction == StopDirection.Up ? "STOP_DIRECTION_STOP_UP" : "STOP_DIRECTION_STOP_DOWN";

        _order.OrderConfiguration.StopLimitStopLimitGtd = new StopLimitStopLimitGtdV3
        {
            BaseSize = baseSize,
            LimitPrice = limitPrice.ToString(CultureInfo.InvariantCulture),
            StopPrice = stopPrice.ToString(CultureInfo.InvariantCulture),
            StopDirection = stopDirectionString,
            EndTime = endTime
        };
        return this;
    }

    /// <summary>
    /// Sets leverage for futures trading
    /// </summary>
    public OrderRequestBuilder WithLeverage(decimal leverage)
    {
        _order.Leverage = leverage.ToString(CultureInfo.InvariantCulture);
        return this;
    }

    /// <summary>
    /// Sets margin type
    /// </summary>
    public OrderRequestBuilder WithMarginType(MarginType marginType)
    {
        _order.MarginType = marginType == MarginType.Cross ? "CROSS" : "ISOLATED";
        return this;
    }

    /// <summary>
    /// Sets a preview ID to associate with a preview request
    /// </summary>
    public OrderRequestBuilder WithPreviewId(string previewId)
    {
        _order.PreviewId = previewId;
        return this;
    }

    /// <summary>
    /// Builds the final order request
    /// </summary>
    public OrderRequest Build() => _order;
}

/// <summary>
/// Stop direction for stop-limit orders
/// </summary>
public enum StopDirection
{
    Up,   // Trigger when price goes above stop price
    Down  // Trigger when price goes below stop price
}

/// <summary>
/// Margin type for futures trading
/// </summary>
public enum MarginType
{
    Cross,
    Isolated
}

/// <summary>
/// Extension methods for easier order creation
/// </summary>
public static class OrderBuilderExtensions
{
    /// <summary>
    /// Quick method to create a market buy order
    /// </summary>
    public static OrderRequest CreateMarketBuyOrder(string productId, decimal quoteAmount)
    {
        return OrderRequestBuilder.Buy(productId).MarketOrderByQuote(quoteAmount).Build();
    }

    /// <summary>
    /// Quick method to create a market sell order
    /// </summary>
    public static OrderRequest CreateMarketSellOrder(string productId, decimal baseAmount)
    {
        return OrderRequestBuilder.Sell(productId).MarketOrderByBase(baseAmount).Build();
    }

    /// <summary>
    /// Quick method to create a limit buy order
    /// </summary>
    public static OrderRequest CreateLimitBuyOrder(string productId, decimal baseAmount, decimal limitPrice, bool postOnly = false)
    {
        return OrderRequestBuilder.Buy(productId).LimitOrder(baseAmount, limitPrice, postOnly).Build();
    }

    /// <summary>
    /// Quick method to create a limit sell order
    /// </summary>
    public static OrderRequest CreateLimitSellOrder(string productId, decimal baseAmount, decimal limitPrice, bool postOnly = false)
    {
        return OrderRequestBuilder.Sell(productId).LimitOrder(baseAmount, limitPrice, postOnly).Build();
    }
}