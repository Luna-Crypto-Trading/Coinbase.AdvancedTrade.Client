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
    Task<AccountsResponse> ListAccounts(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get account details by ID
    /// </summary>
    [Get("/accounts/{id}")]
    Task<CoinbaseAccount> GetAccount(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get historical orders
    /// </summary>
    [Get("/orders/historical/batch")]
    Task<GetOrdersResponse> GetOrders(OrderSearchRequest? request = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    [Get("/orders/historical/{orderId}")]
    Task<GetOrderResponse> GetOrder(string orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Place a new order
    /// </summary>
    [Post("/orders")]
    Task<OrderInformation> PlaceOrder([Body] OrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel one or more orders by ID
    /// </summary>
    [Post("/orders/batch_cancel")]
    Task<CancelOrdersResponse> CancelOrders([Body] CancelOrdersRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit an existing order
    /// </summary>
    [Post("/orders/edit")]
    Task<EditOrderResponse> EditOrder([Body] EditOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Preview an order before placing it
    /// </summary>
    [Post("/orders/preview")]
    Task<PreviewOrderResponse> PreviewOrder([Body] OrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Preview an order edit before applying it
    /// </summary>
    [Post("/orders/edit/preview")]
    Task<PreviewOrderResponse> PreviewEditOrder([Body] EditOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Close an existing position/order
    /// </summary>
    [Post("/orders/close_position")]
    Task<OrderInformation> ClosePosition([Body] ClosePositionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the best bid/ask prices for specified products
    /// </summary>
    [Get("/best_bid_ask")]
    Task<BestBidAskResponse> GetBestBidAsk(
        [Query(CollectionFormat.Multi)] List<string>? product_ids = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// List all available products
    /// </summary>
    [Get("/products")]
    Task<ListProductsResponse> ListProducts(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get candlestick data for a product
    /// </summary>
    [Get("/products/{productId}/candles")]
    Task<CandleResponse> GetProductCandles(
        string productId,
        [Query] long start,
        [Query] long end,
        [Query] string granularity,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get public candlestick data for a product (no auth required)
    /// </summary>
    [Get("/market/products/{productId}/candles")]
    Task<CandleResponse> GetPublicProductCandles(
        string productId,
        [Query] long start,
        [Query] long end,
        [Query] string granularity,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get market trades for a product
    /// </summary>
    [Get("/products/{productId}/ticker")]
    Task<MarketTradesResponse> GetMarketTrades(string productId, [Query] int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get details for a specific product
    /// </summary>
    [Get("/products/{productId}")]
    Task<AdvancedTradeProduct> GetProduct(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction summary (fees and volume)
    /// </summary>
    [Get("/transaction_summary")]
    Task<TransactionSummaryResponse> GetTransactionSummary(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all portfolios for the user
    /// </summary>
    [Get("/portfolios")]
    Task<AdvancedTradePortfolioResponse> GetPortfolios(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed breakdown of a specific portfolio
    /// </summary>
    [Get("/portfolios/{portfolio_uuid}")]
    Task<AdvancedTradePortfolioBreakdownResponse> GetPortfolioBreakdown(string portfolio_uuid, CancellationToken cancellationToken = default);
}