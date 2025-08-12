namespace Coinbase.AdvancedTrade.Client.Constants;

/// <summary>
/// Common constants for Coinbase Advanced Trade API
/// </summary>
public static class CoinbaseConstants
{
    /// <summary>
    /// Popular trading pairs
    /// </summary>
    public static class TradingPairs
    {
        public const string BtcUsd = "BTC-USD";
        public const string EthUsd = "ETH-USD";
        public const string AdaUsd = "ADA-USD";
        public const string SolUsd = "SOL-USD";
        public const string DogeUsd = "DOGE-USD";
        public const string MaticUsd = "MATIC-USD";
        public const string AvaxUsd = "AVAX-USD";
        public const string LinkUsd = "LINK-USD";
        public const string DotUsd = "DOT-USD";
        public const string UniUsd = "UNI-USD";
        public const string LtcUsd = "LTC-USD";
        public const string BchUsd = "BCH-USD";
        public const string XlmUsd = "XLM-USD";
        public const string AlgoUsd = "ALGO-USD";
        public const string AtomUsd = "ATOM-USD";
        
        // EUR pairs
        public const string BtcEur = "BTC-EUR";
        public const string EthEur = "ETH-EUR";
        
        // GBP pairs
        public const string BtcGbp = "BTC-GBP";
        public const string EthGbp = "ETH-GBP";
        
        // BTC pairs
        public const string EthBtc = "ETH-BTC";
        public const string AdaBtc = "ADA-BTC";
        public const string LinkBtc = "LINK-BTC";
    }

    /// <summary>
    /// Order sides
    /// </summary>
    public static class OrderSides
    {
        public const string Buy = "BUY";
        public const string Sell = "SELL";
    }

    /// <summary>
    /// Order statuses
    /// </summary>
    public static class OrderStatuses
    {
        public const string Open = "OPEN";
        public const string Filled = "FILLED";
        public const string Cancelled = "CANCELED";
        public const string Expired = "EXPIRED";
        public const string Rejected = "REJECTED";
        public const string Pending = "PENDING";
        public const string PartiallyFilled = "PARTIALLY_FILLED";
    }

    /// <summary>
    /// Candlestick granularities
    /// </summary>
    public static class Granularities
    {
        public const string OneMinute = "ONE_MINUTE";
        public const string FiveMinute = "FIVE_MINUTE";
        public const string FifteenMinute = "FIFTEEN_MINUTE";
        public const string OneHour = "ONE_HOUR";
        public const string SixHour = "SIX_HOUR";
        public const string OneDay = "ONE_DAY";
    }

    /// <summary>
    /// Stop directions
    /// </summary>
    public static class StopDirections
    {
        public const string StopUp = "STOP_DIRECTION_STOP_UP";
        public const string StopDown = "STOP_DIRECTION_STOP_DOWN";
    }

    /// <summary>
    /// Margin types
    /// </summary>
    public static class MarginTypes
    {
        public const string Cross = "CROSS";
        public const string Isolated = "ISOLATED";
    }

    /// <summary>
    /// Product types
    /// </summary>
    public static class ProductTypes
    {
        public const string Spot = "SPOT";
        public const string Future = "FUTURE";
        public const string Unknown = "UNKNOWN_PRODUCT_TYPE";
    }

    /// <summary>
    /// Account types
    /// </summary>
    public static class AccountTypes
    {
        public const string Crypto = "ACCOUNT_TYPE_CRYPTO";
        public const string Fiat = "ACCOUNT_TYPE_FIAT";
        public const string Vault = "ACCOUNT_TYPE_VAULT";
        public const string PerpFutures = "ACCOUNT_TYPE_PERP_FUTURES";
        public const string Unspecified = "ACCOUNT_TYPE_UNSPECIFIED";
    }

    /// <summary>
    /// Common currencies
    /// </summary>
    public static class Currencies
    {
        // Fiat
        public const string Usd = "USD";
        public const string Eur = "EUR";
        public const string Gbp = "GBP";
        
        // Major cryptocurrencies
        public const string Btc = "BTC";
        public const string Eth = "ETH";
        public const string Ada = "ADA";
        public const string Sol = "SOL";
        public const string Doge = "DOGE";
        public const string Matic = "MATIC";
        public const string Avax = "AVAX";
        public const string Link = "LINK";
        public const string Dot = "DOT";
        public const string Uni = "UNI";
        public const string Ltc = "LTC";
        public const string Bch = "BCH";
        public const string Xlm = "XLM";
        public const string Algo = "ALGO";
        public const string Atom = "ATOM";
    }

    /// <summary>
    /// API rate limits (approximate)
    /// </summary>
    public static class RateLimits
    {
        public const int RequestsPerSecond = 10;
        public const int RequestsPerMinute = 600;
        public const int OrdersPerSecond = 5;
        public const int OrdersPerMinute = 300;
    }

    /// <summary>
    /// Common time periods for historical data
    /// </summary>
    public static class TimePeriods
    {
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);
        public static readonly TimeSpan SixHours = TimeSpan.FromHours(6);
        public static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
        public static readonly TimeSpan OneWeek = TimeSpan.FromDays(7);
        public static readonly TimeSpan OneMonth = TimeSpan.FromDays(30);
        public static readonly TimeSpan ThreeMonths = TimeSpan.FromDays(90);
        public static readonly TimeSpan SixMonths = TimeSpan.FromDays(180);
        public static readonly TimeSpan OneYear = TimeSpan.FromDays(365);
    }
}

/// <summary>
/// Enums for type-safe parameter passing
/// </summary>
public enum CandleGranularity
{
    OneMinute,
    FiveMinute,
    FifteenMinute,
    OneHour,
    SixHour,
    OneDay
}

/// <summary>
/// Extension methods for enums
/// </summary>
public static class CoinbaseEnumExtensions
{
    /// <summary>
    /// Converts CandleGranularity enum to API string
    /// </summary>
    public static string ToApiString(this CandleGranularity granularity)
    {
        return granularity switch
        {
            CandleGranularity.OneMinute => CoinbaseConstants.Granularities.OneMinute,
            CandleGranularity.FiveMinute => CoinbaseConstants.Granularities.FiveMinute,
            CandleGranularity.FifteenMinute => CoinbaseConstants.Granularities.FifteenMinute,
            CandleGranularity.OneHour => CoinbaseConstants.Granularities.OneHour,
            CandleGranularity.SixHour => CoinbaseConstants.Granularities.SixHour,
            CandleGranularity.OneDay => CoinbaseConstants.Granularities.OneDay,
            _ => CoinbaseConstants.Granularities.OneHour
        };
    }
}