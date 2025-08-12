using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class ListProductsResponse
{
    [JsonPropertyName("products")]
    public required List<AdvancedTradeProduct> Products { get; set; }

    [JsonPropertyName("num_products")]
    public int ProductCount { get; set; }
}

public class AdvancedTradeProduct
{
    [JsonPropertyName("product_id")]
    public required string ProductId { get; set; }

    [JsonPropertyName("price")]
    public required string Price { get; set; }

    [JsonPropertyName("price_percentage_change_24h")]
    public required string PricePercentageChange24h { get; set; }

    [JsonPropertyName("volume_24h")]
    public required string Volume24h { get; set; }

    [JsonPropertyName("volume_percentage_change_24h")]
    public required string VolumePercentageChange24h { get; set; }

    [JsonPropertyName("base_increment")]
    public required string BaseIncrement { get; set; }

    [JsonPropertyName("quote_increment")]
    public required string QuoteIncrement { get; set; }

    [JsonPropertyName("quote_min_size")]
    public required string QuoteMinSize { get; set; }

    [JsonPropertyName("quote_max_size")]
    public required string QuoteMaxSize { get; set; }

    [JsonPropertyName("base_min_size")]
    public required string BaseMinSize { get; set; }

    [JsonPropertyName("base_max_size")]
    public required string BaseMaxSize { get; set; }

    [JsonPropertyName("base_name")]
    public required string BaseName { get; set; }

    [JsonPropertyName("quote_name")]
    public required string QuoteName { get; set; }

    [JsonPropertyName("watched")]
    public bool Watched { get; set; }

    [JsonPropertyName("is_disabled")]
    public bool IsDisabled { get; set; }

    [JsonPropertyName("new")]
    public bool New { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("cancel_only")]
    public bool CancelOnly { get; set; }

    [JsonPropertyName("limit_only")]
    public bool LimitOnly { get; set; }

    [JsonPropertyName("post_only")]
    public bool PostOnly { get; set; }

    [JsonPropertyName("trading_disabled")]
    public bool TradingDisabled { get; set; }

    [JsonPropertyName("auction_mode")]
    public bool AuctionMode { get; set; }

    [JsonPropertyName("product_type")]
    public required string ProductType { get; set; }

    [JsonPropertyName("quote_currency_id")]
    public required string QuoteCurrencyId { get; set; }

    [JsonPropertyName("base_currency_id")]
    public required string BaseCurrencyId { get; set; }

    [JsonPropertyName("fcm_trading_session_details")]
    public required SessionDetails FcmTradingSessionDetails { get; set; }

    [JsonPropertyName("mid_market_price")]
    public required string MidMarketPrice { get; set; }

    [JsonPropertyName("alias")]
    public required string Alias { get; set; }

    [JsonPropertyName("alias_to")]
    public required List<string> AliasTo { get; set; }

    [JsonPropertyName("base_display_symbol")]
    public required string BaseDisplaySymbol { get; set; }

    [JsonPropertyName("quote_display_symbol")]
    public required string QuoteDisplaySymbol { get; set; }

    [JsonPropertyName("view_only")]
    public bool ViewOnly { get; set; }

    [JsonPropertyName("price_increment")]
    public required string PriceIncrement { get; set; }

    [JsonPropertyName("display_name")]
    public required string DisplayName { get; set; }

    [JsonPropertyName("product_venue")]
    public required string ProductVenue { get; set; }
}

public class SessionDetails
{
    [JsonPropertyName("is_open")]
    public required bool IsOpen { get; set; }
    
    [JsonPropertyName("open_time")]
    public required DateTimeOffset OpenTime { get; set; }
    
    [JsonPropertyName("close_time")]
    public required DateTimeOffset CloseTime { get; set; }
}