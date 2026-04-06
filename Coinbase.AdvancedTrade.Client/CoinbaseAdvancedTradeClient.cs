using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Refit;

namespace Coinbase.AdvancedTrade.Client;

public interface ICoinbaseAdvancedTradeClient
{
    Task<ApiResponse<OrderInformation>> PlaceOrderAsync(OrderRequest order, CancellationToken cancellationToken = default);
    Task<ApiResponse<EditOrderResponse>> EditOrderAsync(EditOrderRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreviewOrderResponse>> PreviewOrderAsync(OrderRequest order, CancellationToken cancellationToken = default);
    Task<ApiResponse<PreviewOrderResponse>> PreviewEditOrderAsync(EditOrderRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CancelOrdersResponse>> CancelOrdersAsync(List<string> orderIds, CancellationToken cancellationToken = default);
    Task<ApiResponse<OrderInformation>> ClosePositionAsync(ClosePositionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetOrdersResponse>> GetOrdersAsync(OrderSearchRequest? request = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetOrderResponse>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);
    Task<ApiResponse<ListProductsResponse>> ListProductsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AdvancedTradeProduct>> GetProductAsync(string productId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CandleResponse>> GetProductCandlesAsync(string productId, long start, long end, string granularity, CancellationToken cancellationToken = default);
    Task<ApiResponse<MarketTradesResponse>> GetMarketTradesAsync(string productId, int limit = 100, CancellationToken cancellationToken = default);
    Task<ApiResponse<BestBidAskResponse>> GetBestBidAskAsync(List<string>? productIds = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<TransactionSummaryResponse>> GetTransactionSummaryAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AccountsResponse>> ListAccountsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CoinbaseAccount>> GetAccountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<AdvancedTradePortfolioResponse>> GetPortfoliosAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AdvancedTradePortfolioBreakdownResponse>> GetPortfolioBreakdownAsync(string portfolioUuid, CancellationToken cancellationToken = default);
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private ApiResponse(bool isSuccess, T? data, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static ApiResponse<T> Success(T data) => new(true, data, null, null);
    public static ApiResponse<T> Failure(string errorMessage, Exception? exception = null) => new(false, default, errorMessage, exception);
}

public class CoinbaseAdvancedTradeClient : ICoinbaseAdvancedTradeClient
{
    private readonly ICoinbaseApi _coinbaseApi;
    private readonly ILogger<CoinbaseAdvancedTradeClient>? _logger;
    private readonly ResiliencePipeline _resiliencePipeline;

    public CoinbaseAdvancedTradeClient(
        ICoinbaseApi coinbaseApi,
        ILogger<CoinbaseAdvancedTradeClient>? logger = null)
    {
        _coinbaseApi = coinbaseApi;
        _logger = logger;

        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ApiException>()
                    .Handle<HttpRequestException>(),
                FailureRatio = 1.0,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromMinutes(2),
                OnOpened = args =>
                {
                    _logger?.LogError(
                        "Circuit breaker tripped for Coinbase API. Breaking for {BreakDuration}s. Exception: {Exception}",
                        args.BreakDuration.TotalSeconds,
                        args.Outcome.Exception?.Message);
                    return default;
                },
                OnClosed = args =>
                {
                    _logger?.LogInformation("Circuit breaker reset for Coinbase API");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    _logger?.LogInformation("Circuit breaker half-open for Coinbase API");
                    return default;
                }
            })
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<ApiException>(ex => IsTransientError(ex))
                    .Handle<HttpRequestException>(),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    _logger?.LogWarning(
                        "Retrying Coinbase API call after {Delay}s. Attempt: {AttemptNumber}. Exception: {Exception}",
                        args.RetryDelay.TotalSeconds,
                        args.AttemptNumber,
                        args.Outcome.Exception?.Message);
                    return default;
                }
            })
            .Build();
    }

    private static bool IsTransientError(ApiException ex)
    {
        return ex.StatusCode is
            System.Net.HttpStatusCode.TooManyRequests or
            System.Net.HttpStatusCode.BadGateway or
            System.Net.HttpStatusCode.ServiceUnavailable or
            System.Net.HttpStatusCode.GatewayTimeout or
            System.Net.HttpStatusCode.RequestTimeout;
    }

    public async Task<ApiResponse<OrderInformation>> PlaceOrderAsync(OrderRequest order, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Placing {Side} order for product {ProductId}", order.Side, order.ProductId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.PlaceOrder(order, ct),
                cancellationToken);

            if (!response.Success)
            {
                var errorMessage = response.ErrorResponse?.Message
                                ?? response.ErrorResponse?.NewOrderFailureReason
                                ?? "Unknown error";
                _logger?.LogWarning("Order placement failed: {ErrorMessage}", errorMessage);
                return ApiResponse<OrderInformation>.Failure($"Failed to place order: {errorMessage}");
            }

            _logger?.LogInformation("Successfully placed order, ID: {OrderId}", response.SuccessResponse?.OrderId);
            return ApiResponse<OrderInformation>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<OrderInformation>(ex, "placing order");
        }
    }

    public async Task<ApiResponse<EditOrderResponse>> EditOrderAsync(EditOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Editing order {OrderId}", request.OrderId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.EditOrder(request, ct),
                cancellationToken);

            if (!response.Success)
            {
                var reasons = response.Errors?.Select(e => e.EditFailureReason).Where(r => r != null);
                var errorMessage = reasons?.Any() == true ? string.Join(", ", reasons) : "Unknown error";
                _logger?.LogWarning("Edit order failed: {ErrorMessage}", errorMessage);
                return ApiResponse<EditOrderResponse>.Failure($"Failed to edit order: {errorMessage}");
            }

            return ApiResponse<EditOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<EditOrderResponse>(ex, $"editing order {request.OrderId}");
        }
    }

    public async Task<ApiResponse<PreviewOrderResponse>> PreviewOrderAsync(OrderRequest order, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Previewing {Side} order for product {ProductId}", order.Side, order.ProductId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.PreviewOrder(order, ct),
                cancellationToken);

            if (response.Errors?.Any() == true)
            {
                var errorMessage = string.Join(", ", response.Errors);
                _logger?.LogWarning("Order preview returned errors: {Errors}", errorMessage);
            }

            return ApiResponse<PreviewOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<PreviewOrderResponse>(ex, "previewing order");
        }
    }

    public async Task<ApiResponse<PreviewOrderResponse>> PreviewEditOrderAsync(EditOrderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Previewing edit for order {OrderId}", request.OrderId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.PreviewEditOrder(request, ct),
                cancellationToken);

            return ApiResponse<PreviewOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<PreviewOrderResponse>(ex, $"previewing edit for order {request.OrderId}");
        }
    }

    public async Task<ApiResponse<CancelOrdersResponse>> CancelOrdersAsync(List<string> orderIds, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Cancelling {Count} order(s)", orderIds.Count);

            var request = new CancelOrdersRequest { OrderIds = orderIds };
            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.CancelOrders(request, ct),
                cancellationToken);

            return ApiResponse<CancelOrdersResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<CancelOrdersResponse>(ex, "cancelling orders");
        }
    }

    public async Task<ApiResponse<OrderInformation>> ClosePositionAsync(ClosePositionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Closing position for product {ProductId}", request.ProductId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ClosePosition(request, ct),
                cancellationToken);

            if (!response.Success)
            {
                var errorMessage = response.ErrorResponse?.Message
                                ?? response.ErrorResponse?.NewOrderFailureReason
                                ?? "Unknown error";
                _logger?.LogWarning("Position close failed: {ErrorMessage}", errorMessage);
                return ApiResponse<OrderInformation>.Failure($"Failed to close position: {errorMessage}");
            }

            _logger?.LogInformation("Successfully closed position for product {ProductId}", request.ProductId);
            return ApiResponse<OrderInformation>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<OrderInformation>(ex, "closing position");
        }
    }

    public async Task<ApiResponse<GetOrdersResponse>> GetOrdersAsync(OrderSearchRequest? request = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving orders with applied filters");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetOrders(request, ct),
                cancellationToken);

            return ApiResponse<GetOrdersResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<GetOrdersResponse>(ex, "retrieving orders");
        }
    }

    public async Task<ApiResponse<GetOrderResponse>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving order {OrderId}", orderId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetOrder(orderId, ct),
                cancellationToken);

            return ApiResponse<GetOrderResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<GetOrderResponse>(ex, $"retrieving order {orderId}");
        }
    }

    public async Task<ApiResponse<ListProductsResponse>> ListProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving available products");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ListProducts(ct),
                cancellationToken);

            return ApiResponse<ListProductsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<ListProductsResponse>(ex, "retrieving products");
        }
    }

    public async Task<ApiResponse<AdvancedTradeProduct>> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving product {ProductId}", productId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetProduct(productId, ct),
                cancellationToken);

            return ApiResponse<AdvancedTradeProduct>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<AdvancedTradeProduct>(ex, $"retrieving product {productId}");
        }
    }

    public async Task<ApiResponse<CandleResponse>> GetProductCandlesAsync(string productId, long start, long end, string granularity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving historical rates for product {ProductId} with granularity {Granularity}", productId, granularity);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetProductCandles(productId, start, end, granularity, ct),
                cancellationToken);

            _logger?.LogInformation("Successfully retrieved historical rates for product {ProductId}", productId);
            return ApiResponse<CandleResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<CandleResponse>(ex, $"retrieving historical rates for {productId}");
        }
    }

    public async Task<ApiResponse<MarketTradesResponse>> GetMarketTradesAsync(string productId, int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving market trades for product {ProductId}", productId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetMarketTrades(productId, limit, ct),
                cancellationToken);

            return ApiResponse<MarketTradesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<MarketTradesResponse>(ex, $"retrieving market trades for {productId}");
        }
    }

    public async Task<ApiResponse<BestBidAskResponse>> GetBestBidAskAsync(List<string>? productIds = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving best bid/ask prices");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetBestBidAsk(productIds, ct),
                cancellationToken);

            return ApiResponse<BestBidAskResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<BestBidAskResponse>(ex, "retrieving best bid/ask prices");
        }
    }

    public async Task<ApiResponse<TransactionSummaryResponse>> GetTransactionSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving transaction summary");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetTransactionSummary(ct),
                cancellationToken);

            return ApiResponse<TransactionSummaryResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<TransactionSummaryResponse>(ex, "retrieving transaction summary");
        }
    }

    public async Task<ApiResponse<AccountsResponse>> ListAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving user accounts");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ListAccounts(ct),
                cancellationToken);

            return ApiResponse<AccountsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<AccountsResponse>(ex, "retrieving accounts");
        }
    }

    public async Task<ApiResponse<CoinbaseAccount>> GetAccountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving account with ID {AccountId}", id);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetAccount(id, ct),
                cancellationToken);

            return ApiResponse<CoinbaseAccount>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<CoinbaseAccount>(ex, $"retrieving account {id}");
        }
    }

    public async Task<ApiResponse<AdvancedTradePortfolioResponse>> GetPortfoliosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving portfolios");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetPortfolios(ct),
                cancellationToken);

            return ApiResponse<AdvancedTradePortfolioResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<AdvancedTradePortfolioResponse>(ex, "retrieving portfolios");
        }
    }

    public async Task<ApiResponse<AdvancedTradePortfolioBreakdownResponse>> GetPortfolioBreakdownAsync(string portfolioUuid, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving portfolio breakdown for {PortfolioUuid}", portfolioUuid);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetPortfolioBreakdown(portfolioUuid, ct),
                cancellationToken);

            return ApiResponse<AdvancedTradePortfolioBreakdownResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<AdvancedTradePortfolioBreakdownResponse>(ex, $"retrieving portfolio breakdown for {portfolioUuid}");
        }
    }

    private ApiResponse<T> HandleException<T>(Exception ex, string operation)
    {
        string errorMessage = ex switch
        {
            ApiException apiEx => apiEx.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Authentication failed. Please check your API credentials.",
                System.Net.HttpStatusCode.Forbidden => "Your API key does not have permission for this operation.",
                System.Net.HttpStatusCode.NotFound => "The requested resource was not found.",
                System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded. Please try again later.",
                System.Net.HttpStatusCode.BadGateway or
                System.Net.HttpStatusCode.ServiceUnavailable or
                System.Net.HttpStatusCode.GatewayTimeout => "Coinbase API is currently unavailable. Please try again later.",
                _ => $"Error when {operation}: {apiEx.Message}"
            },
            BrokenCircuitException => "Coinbase API is currently unavailable. Please try again later.",
            _ => $"An unexpected error occurred when {operation}: {ex.Message}"
        };

        _logger?.LogError(ex, "Error when {Operation}: {Message}", operation, ex.Message);
        return ApiResponse<T>.Failure(errorMessage, ex);
    }
}