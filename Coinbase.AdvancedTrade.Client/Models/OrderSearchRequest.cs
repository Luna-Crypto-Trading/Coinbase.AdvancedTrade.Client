namespace Coinbase.AdvancedTrade.Client.Models;

/// <summary>
/// Represents the parameters for fetching and filtering orders.
/// </summary>
public sealed record OrderSearchRequest
{
    /// <summary>
    /// ID(s) of the order(s).
    /// </summary>
    public List<string>? OrderIds { get; set; }

    /// <summary>
    /// Optional string of the product ID(s). Defaults to null, or fetches for all products.
    /// </summary>
    public List<string>? ProductIds { get; set; } = null;

    /// <summary>
    /// Possible values: UNKNOWN_PRODUCT_TYPE, SPOT, FUTURE . Returns orders matching this product type. By default, returns all product types.
    /// </summary>
    public string? ProductType { get; set; }

    /// <summary>
    /// Only returns orders matching the specified order statuses.
    /// </summary>
    public List<string>? OrderStatus { get; set; }

    /// <summary>
    /// Only orders matching this time in force(s) are returned. Default is to return all time in forces.
    /// </summary>
    public List<string>? TimeInForces { get; set; }

    /// <summary>
    /// Only returns orders matching the specified order types (e.g. MARKET). By default, returns all order types.
    /// </summary>
    public List<string>? OrderTypes { get; set; }

    /// <summary>
    /// Only returns the orders matching the specified side (e.g. 'BUY', 'SELL'). By default, returns all sides.
    /// </summary>
    public string? OrderSide { get; set; }

    /// <summary>
    /// The start date to fetch orders from (inclusive). If provided, only orders created after this date will be returned.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The end date to fetch orders from (exclusive). If provided, only orders with creation time before this date will be returned.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Possible values: UNKNOWN_PLACEMENT_SOURCE, RETAIL_SIMPLE, RETAIL_ADVANCED. Only returns the orders matching this placement source. By default, returns RETAIL_ADVANCED placement source.
    /// </summary>
    public string? OrderPlacementSource { get; set; }

    /// <summary>
    /// Possible values: UNKNOWN_CONTRACT_EXPIRY_TYPE, EXPIRING, PERPETUAL. Only returns the orders matching the contract expiry type. Only applicable if product_type is set to FUTURE.
    /// </summary>
    public string? ContractExpiryType { get; set; } = "UNKNOWN_CONTRACT_EXPIRY_TYPE";

    /// <summary>
    /// Only returns the orders where the quote, base, or underlying asset matches the provided asset filter(s) (e.g. 'BTC').
    /// </summary>
    public List<string>? AssetFilters { get; set; }

    /// <summary>
    /// (Deprecated) Only orders matching this retail portfolio ID are returned. Only applicable for legacy keys. CDP keys will default to the key's permissioned portfolio.
    /// </summary>
    public string? RetailPortfolioId { get; set; }

    /// <summary>
    /// The number of orders to display per page (no default amount). If has_next is true, additional pages of orders are available to be fetched. Use the cursor parameter to start on a specified page.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// For paginated responses, returns all responses that come after this value.
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// Possible values: UNKNOWN_SORT_BY, LIMIT_PRICE, LAST_FILL_TIME. Sort results by a field; results use unstable pagination. Default is to sort by creation time.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// (Deprecated) Native currency to fetch orders with. Default is USD.
    /// </summary>
    public string? UserNativeCurrency { get; set; }
}