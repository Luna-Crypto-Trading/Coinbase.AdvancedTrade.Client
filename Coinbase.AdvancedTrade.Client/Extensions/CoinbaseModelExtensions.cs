using System.Globalization;
using Coinbase.AdvancedTrade.Client.Models;

namespace Coinbase.AdvancedTrade.Client.Extensions;

/// <summary>
/// Extension methods for Coinbase API models to provide convenient functionality
/// </summary>
public static class CoinbaseModelExtensions
{
    /// <summary>
    /// Gets the decimal price value from a string price
    /// </summary>
    public static decimal GetPriceAsDecimal(this AdvancedTradeProduct product)
    {
        return decimal.TryParse(product.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m;
    }

    /// <summary>
    /// Gets the decimal price change percentage
    /// </summary>
    public static decimal GetPriceChangeAsDecimal(this AdvancedTradeProduct product)
    {
        return decimal.TryParse(product.PricePercentageChange24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var change) ? change : 0m;
    }

    /// <summary>
    /// Gets the decimal volume
    /// </summary>
    public static decimal GetVolumeAsDecimal(this AdvancedTradeProduct product)
    {
        return decimal.TryParse(product.Volume24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var volume) ? volume : 0m;
    }

    /// <summary>
    /// Checks if the product is actively tradeable
    /// </summary>
    public static bool IsActiveTradeable(this AdvancedTradeProduct product)
    {
        return !product.IsDisabled && !product.TradingDisabled && !product.CancelOnly;
    }

    /// <summary>
    /// Gets the decimal balance value
    /// </summary>
    public static decimal GetBalanceAsDecimal(this Balance balance)
    {
        return decimal.TryParse(balance.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }

    /// <summary>
    /// Gets the decimal balance value
    /// </summary>
    public static decimal GetBalanceAsDecimal(this AdvancedTradeBalance balance)
    {
        return decimal.TryParse(balance.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0m;
    }

    /// <summary>
    /// Checks if the account is ready for trading
    /// </summary>
    public static bool IsReadyForTrading(this CoinbaseAccount account)
    {
        return account.Active == true && account.Ready == true && account.AvailableBalance.GetBalanceAsDecimal() > 0;
    }

    /// <summary>
    /// Gets the mid-market price from bid/ask spread
    /// </summary>
    public static decimal? GetMidMarketPrice(this PriceBook priceBook)
    {
        if (!priceBook.Bids.Any() || !priceBook.Asks.Any())
            return null;

        var bestBid = decimal.TryParse(priceBook.Bids.First().Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var bid) ? bid : 0m;
        var bestAsk = decimal.TryParse(priceBook.Asks.First().Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var ask) ? ask : 0m;

        return bestBid > 0 && bestAsk > 0 ? (bestBid + bestAsk) / 2 : null;
    }

    /// <summary>
    /// Gets the bid-ask spread as a decimal
    /// </summary>
    public static decimal? GetBidAskSpread(this PriceBook priceBook)
    {
        if (!priceBook.Bids.Any() || !priceBook.Asks.Any())
            return null;

        var bestBid = decimal.TryParse(priceBook.Bids.First().Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var bid) ? bid : 0m;
        var bestAsk = decimal.TryParse(priceBook.Asks.First().Price, NumberStyles.Any, CultureInfo.InvariantCulture, out var ask) ? ask : 0m;

        return bestBid > 0 && bestAsk > 0 ? bestAsk - bestBid : null;
    }

    /// <summary>
    /// Gets the bid-ask spread as a percentage
    /// </summary>
    public static decimal? GetBidAskSpreadPercentage(this PriceBook priceBook)
    {
        var midPrice = priceBook.GetMidMarketPrice();
        var spread = priceBook.GetBidAskSpread();

        return midPrice > 0 && spread.HasValue ? (spread.Value / midPrice.Value) * 100 : null;
    }

    /// <summary>
    /// Parses candle data into decimal values
    /// </summary>
    public static CandleData ParseAsDecimals(this Candle candle)
    {
        var start = long.TryParse(candle.Start, out var startTimestamp) ? DateTimeOffset.FromUnixTimeSeconds(startTimestamp).DateTime : DateTime.MinValue;
        var open = decimal.TryParse(candle.Open, NumberStyles.Any, CultureInfo.InvariantCulture, out var openPrice) ? openPrice : 0m;
        var high = decimal.TryParse(candle.High, NumberStyles.Any, CultureInfo.InvariantCulture, out var highPrice) ? highPrice : 0m;
        var low = decimal.TryParse(candle.Low, NumberStyles.Any, CultureInfo.InvariantCulture, out var lowPrice) ? lowPrice : 0m;
        var close = decimal.TryParse(candle.Close, NumberStyles.Any, CultureInfo.InvariantCulture, out var closePrice) ? closePrice : 0m;
        var volume = decimal.TryParse(candle.Volume, NumberStyles.Any, CultureInfo.InvariantCulture, out var volumeValue) ? volumeValue : 0m;

        return new CandleData(start, open, high, low, close, volume);
    }

    /// <summary>
    /// Checks if order is in a final state (completed, failed, or cancelled)
    /// </summary>
    public static bool IsInFinalState(this OrderV3 order)
    {
        return order.Status.ToUpper() is "FILLED" or "CANCELED" or "EXPIRED" or "REJECTED";
    }

    /// <summary>
    /// Gets the total fees as a decimal
    /// </summary>
    public static decimal GetTotalFeesAsDecimal(this OrderV3 order)
    {
        return decimal.TryParse(order.TotalFees, NumberStyles.Any, CultureInfo.InvariantCulture, out var fees) ? fees : 0m;
    }

    /// <summary>
    /// Gets the filled size as a decimal
    /// </summary>
    public static decimal GetFilledSizeAsDecimal(this OrderV3 order)
    {
        return decimal.TryParse(order.FilledSize, NumberStyles.Any, CultureInfo.InvariantCulture, out var size) ? size : 0m;
    }

    /// <summary>
    /// Gets the average filled price as a decimal
    /// </summary>
    public static decimal GetAverageFilledPriceAsDecimal(this OrderV3 order)
    {
        return decimal.TryParse(order.AverageFilledPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0m;
    }
}

/// <summary>
/// Parsed candle data with decimal values
/// </summary>
public record CandleData(DateTime Time, decimal Open, decimal High, decimal Low, decimal Close, decimal Volume);