using Coinbase.AdvancedTrade.Client.Models;
using Refit;

namespace Coinbase.AdvancedTrade.Client.Api;

/// <summary>
/// Interface for Coinbase Advanced Trade API
/// This interface is used with Refit to generate the API client
/// </summary>
public interface ICoinbaseApi
{
    /// <summary>
    /// List all accounts available to the user
    /// </summary>
    [Get("/accounts")]
    Task<AccountsResponse> ListAccounts();

    /// <summary>
    /// Get account details by ID
    /// </summary>
    [Get("/accounts/{id}")]
    Task<CoinbaseAccount> GetAccount(Guid id);

    /// <summary>
    /// Get historical orders
    /// </summary>
    [Get("/orders/historical/batch")]
    Task<GetOrdersResponse> GetOrders(OrderSearchRequest? request = null);

    /// <summary>
    /// Place a new order
    /// </summary>
    [Post("/orders")]
    Task<OrderInformation> PlaceOrder([Body] OrderRequest request);

    /// <summary>
    /// Close an existing position/order
    /// </summary>
    [Post("/orders/close_position")]
    Task<OrderInformation> ClosePosition([Body] ClosePositionRequest request);

    /// <summary>
    /// Get the best bid/ask prices for specified products
    /// </summary>
    [Get("/best_bid_ask")]
    Task<BestBidAskResponse> GetBestBidAsk(
        [Query(CollectionFormat.Multi)] List<string>? product_ids = null
    );

    /// <summary>
    /// List all available products
    /// </summary>
    [Get("/products")]
    Task<ListProductsResponse> ListProducts();

    /// <summary>
    /// Get candlestick data for a product
    /// </summary>
    [Get("/products/{productId}/candles")]
    Task<CandleResponse> GetProductCandles(
        string productId,
        [Query] long start,
        [Query] long end,
        [Query] string granularity
    );

    /// <summary>
    /// Get details for a specific product
    /// </summary>
    [Get("/products/{productId}")]
    Task<string> GetProduct(string productId);

    /// <summary>
    /// Get all portfolios for the user
    /// </summary>
    [Get("/portfolios")]
    Task<AdvancedTradePortfolioResponse> GetPortfolios();

    /// <summary>
    /// Get detailed breakdown of a specific portfolio
    /// </summary>
    [Get("/portfolios/{portfolio_uuid}")]
    Task<AdvancedTradePortfolioBreakdownResponse> GetPortfolioBreakdown(string portfolio_uuid);
}