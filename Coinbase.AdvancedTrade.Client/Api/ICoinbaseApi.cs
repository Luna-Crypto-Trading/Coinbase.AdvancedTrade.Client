using Coinbase.AdvancedTrade.Client.Models;
using Refit;

namespace Coinbase.AdvancedTrade.Client.Api;

/// <summary>
/// Interface for Coinbase Advanced Trade API
/// This interface is used with Refit to generate the API client
/// </summary>
public interface ICoinbaseApi
{
    #region Accounts

    /// <summary>
    /// List all accounts available to the user
    /// </summary>
    [Get("/accounts")]
    Task<AccountsResponse> ListAccounts(
        [Query] int? limit = null,
        [Query] string? cursor = null,
        [Query] string? retail_portfolio_id = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get account details by ID
    /// </summary>
    [Get("/accounts/{id}")]
    Task<CoinbaseAccount> GetAccount(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Orders

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
    /// Get order fills (trades)
    /// </summary>
    [Get("/orders/historical/fills")]
    Task<FillsResponse> GetFills(
        [Query] string? order_id = null,
        [Query] string? product_id = null,
        [Query] string? cursor = null,
        [Query] int? limit = null,
        [Query] string? sort_by = null,
        CancellationToken cancellationToken = default
    );

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

    #endregion

    #region Products

    /// <summary>
    /// List all available products
    /// </summary>
    [Get("/products")]
    Task<ListProductsResponse> ListProducts(
        [Query] int? limit = null,
        [Query] int? offset = null,
        [Query] string? product_type = null,
        [Query(CollectionFormat.Multi)] List<string>? product_ids = null,
        [Query] string? contract_expiry_type = null,
        [Query] string? expiring_contract_status = null,
        [Query] bool? get_all_products = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get details for a specific product
    /// </summary>
    [Get("/products/{productId}")]
    Task<AdvancedTradeProduct> GetProduct(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the product book (order book depth) for a product
    /// </summary>
    [Get("/product_book")]
    Task<ProductBookResponse> GetProductBook(
        [Query] string product_id,
        [Query] int? limit = null,
        [Query] string? aggregation_price_increment = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get candlestick data for a product
    /// </summary>
    [Get("/products/{productId}/candles")]
    Task<CandleResponse> GetProductCandles(
        string productId,
        [Query] long start,
        [Query] long end,
        [Query] string granularity,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get market trades for a product
    /// </summary>
    [Get("/products/{productId}/ticker")]
    Task<MarketTradesResponse> GetMarketTrades(
        string productId,
        [Query] int limit,
        [Query] string? start = null,
        [Query] string? end = null,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Market Data

    /// <summary>
    /// Get the best bid/ask prices for specified products
    /// </summary>
    [Get("/best_bid_ask")]
    Task<BestBidAskResponse> GetBestBidAsk(
        [Query(CollectionFormat.Multi)] List<string>? product_ids = null,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Fees

    /// <summary>
    /// Get transaction summary (fees and volume)
    /// </summary>
    [Get("/transaction_summary")]
    Task<TransactionSummaryResponse> GetTransactionSummary(
        [Query] string? product_type = null,
        [Query] string? contract_expiry_type = null,
        [Query] string? product_venue = null,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Portfolios

    /// <summary>
    /// Get all portfolios for the user
    /// </summary>
    [Get("/portfolios")]
    Task<AdvancedTradePortfolioResponse> GetPortfolios(
        [Query] string? portfolio_type = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get detailed breakdown of a specific portfolio
    /// </summary>
    [Get("/portfolios/{portfolio_uuid}")]
    Task<AdvancedTradePortfolioBreakdownResponse> GetPortfolioBreakdown(string portfolio_uuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new portfolio
    /// </summary>
    [Post("/portfolios")]
    Task<AdvancedTradePortfolioResponse> CreatePortfolio([Body] CreatePortfolioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Edit a portfolio name
    /// </summary>
    [Put("/portfolios/{portfolio_uuid}")]
    Task<AdvancedTradePortfolioResponse> EditPortfolio(string portfolio_uuid, [Body] EditPortfolioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a portfolio
    /// </summary>
    [Delete("/portfolios/{portfolio_uuid}")]
    Task<HttpResponseMessage> DeletePortfolio(string portfolio_uuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move funds between portfolios
    /// </summary>
    [Post("/portfolios/move_funds")]
    Task<MoveFundsResponse> MoveFunds([Body] MoveFundsRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Futures / CFM

    /// <summary>
    /// Get futures balance summary
    /// </summary>
    [Get("/cfm/balance_summary")]
    Task<FuturesBalanceSummaryResponse> GetFuturesBalanceSummary(CancellationToken cancellationToken = default);

    /// <summary>
    /// List all futures positions
    /// </summary>
    [Get("/cfm/positions")]
    Task<FuturesPositionsResponse> ListFuturesPositions(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific futures position
    /// </summary>
    [Get("/cfm/positions/{product_id}")]
    Task<FuturesPositionResponse> GetFuturesPosition(string product_id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule a futures sweep
    /// </summary>
    [Post("/cfm/sweeps/schedule")]
    Task<FuturesSweepResponse> ScheduleFuturesSweep([Body] ScheduleSweepRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// List futures sweeps
    /// </summary>
    [Get("/cfm/sweeps")]
    Task<FuturesSweepResponse> ListFuturesSweeps(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel pending futures sweep
    /// </summary>
    [Delete("/cfm/sweeps")]
    Task<FuturesSweepResponse> CancelFuturesSweep(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get intraday margin setting
    /// </summary>
    [Get("/cfm/intraday/margin_setting")]
    Task<IntradayMarginSettingResponse> GetIntradayMarginSetting(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current margin window
    /// </summary>
    [Get("/cfm/intraday/current_margin_window")]
    Task<CurrentMarginWindowResponse> GetCurrentMarginWindow(
        [Query] string? margin_profile_type = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Set intraday margin setting
    /// </summary>
    [Post("/cfm/intraday/margin_setting")]
    Task<IntradayMarginSettingResponse> SetIntradayMarginSetting([Body] SetIntradayMarginSettingRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Perpetuals / INTX

    /// <summary>
    /// Allocate portfolio funds for perpetual trading
    /// </summary>
    [Post("/intx/allocate")]
    Task<HttpResponseMessage> AllocatePortfolio([Body] AllocatePortfolioRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get perpetuals portfolio summary
    /// </summary>
    [Get("/intx/portfolio/{portfolio_uuid}")]
    Task<IntxPortfolioResponse> GetPerpsPortfolioSummary(string portfolio_uuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// List perpetuals positions for a portfolio
    /// </summary>
    [Get("/intx/positions/{portfolio_uuid}")]
    Task<IntxPositionsResponse> ListPerpsPositions(string portfolio_uuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific perpetuals position
    /// </summary>
    [Get("/intx/positions/{portfolio_uuid}/{symbol}")]
    Task<IntxPositionResponse> GetPerpsPosition(string portfolio_uuid, string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get perpetuals portfolio balances
    /// </summary>
    [Get("/intx/balances/{portfolio_uuid}")]
    Task<IntxBalancesResponse> GetPerpsPortfolioBalances(string portfolio_uuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opt in or out of multi-asset collateral
    /// </summary>
    [Post("/intx/multi_asset_collateral")]
    Task<MultiAssetCollateralResponse> SetMultiAssetCollateral([Body] MultiAssetCollateralRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Converts

    /// <summary>
    /// Create a convert quote
    /// </summary>
    [Post("/convert/quote")]
    Task<ConvertQuoteResponse> CreateConvertQuote([Body] ConvertQuoteRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a convert trade by ID
    /// </summary>
    [Get("/convert/trade/{trade_id}")]
    Task<ConvertTradeResponse> GetConvertTrade(
        string trade_id,
        [Query] string from_account,
        [Query] string to_account,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Commit a convert trade
    /// </summary>
    [Post("/convert/trade/{trade_id}")]
    Task<ConvertTradeResponse> CommitConvertTrade(
        string trade_id,
        [Body] ConvertQuoteRequest request,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Public

    /// <summary>
    /// Get server time
    /// </summary>
    [Get("/time")]
    Task<ServerTimeResponse> GetServerTime(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get public product book (no auth required)
    /// </summary>
    [Get("/market/product_book")]
    Task<ProductBookResponse> GetPublicProductBook(
        [Query] string product_id,
        [Query] int? limit = null,
        [Query] string? aggregation_price_increment = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// List all public products (no auth required)
    /// </summary>
    [Get("/market/products")]
    Task<ListProductsResponse> GetPublicProducts(
        [Query] int? limit = null,
        [Query] int? offset = null,
        [Query] string? product_type = null,
        [Query(CollectionFormat.Multi)] List<string>? product_ids = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get public product details (no auth required)
    /// </summary>
    [Get("/market/products/{productId}")]
    Task<AdvancedTradeProduct> GetPublicProduct(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get public candlestick data for a product (no auth required)
    /// </summary>
    [Get("/market/products/{productId}/candles")]
    Task<CandleResponse> GetPublicProductCandles(
        string productId,
        [Query] long start,
        [Query] long end,
        [Query] string granularity,
        [Query] int? limit = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get public market trades for a product (no auth required)
    /// </summary>
    [Get("/market/products/{productId}/ticker")]
    Task<MarketTradesResponse> GetPublicMarketTrades(
        string productId,
        [Query] int limit,
        [Query] string? start = null,
        [Query] string? end = null,
        CancellationToken cancellationToken = default
    );

    #endregion

    #region Payments

    /// <summary>
    /// List all payment methods
    /// </summary>
    [Get("/payment_methods")]
    Task<PaymentMethodsResponse> GetPaymentMethods(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific payment method by ID
    /// </summary>
    [Get("/payment_methods/{payment_method_id}")]
    Task<PaymentMethod> GetPaymentMethod(string payment_method_id, CancellationToken cancellationToken = default);

    #endregion

    #region Data

    /// <summary>
    /// Get API key permissions
    /// </summary>
    [Get("/key_permissions")]
    Task<KeyPermissionsResponse> GetKeyPermissions(CancellationToken cancellationToken = default);

    #endregion
}
