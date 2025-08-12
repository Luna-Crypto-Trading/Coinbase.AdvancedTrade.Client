using System.Globalization;
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
    Task<ApiResponse<OrderInformation>> ClosePositionAsync(ClosePositionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetOrdersResponse>> GetOrdersAsync(OrderSearchRequest? request = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<ListProductsResponse>> ListProductsAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CandleResponse>> GetProductCandlesAsync(string productId, long start, long end, string granularity, CancellationToken cancellationToken = default);
    Task<ApiResponse<BestBidAskResponse>> GetBestBidAskAsync(List<string>? productIds = null, CancellationToken cancellationToken = default);
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
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

    public CoinbaseAdvancedTradeClient(
        ICoinbaseApi coinbaseApi,
        ILogger<CoinbaseAdvancedTradeClient>? logger = null)
    {
        _coinbaseApi = coinbaseApi;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<ApiException>(ex => ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger?.LogWarning(
                        "Retrying Coinbase API call after {TimeSpan}s. Attempt: {RetryCount}. Exception: {Exception}",
                        timeSpan.TotalSeconds,
                        retryCount,
                        exception.Message
                    );
                }
            );

        _circuitBreakerPolicy = Policy
            .Handle<ApiException>()
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromMinutes(2),
                onBreak: (ex, breakDelay) =>
                {
                    _logger?.LogError(
                        "Circuit breaker tripped for Coinbase API. Breaking for {BreakDelay}s. Exception: {Exception}",
                        breakDelay.TotalSeconds,
                        ex.Message
                    );
                },
                onReset: () =>
                {
                    _logger?.LogInformation("Circuit breaker reset for Coinbase API");
                },
                onHalfOpen: () =>
                {
                    _logger?.LogInformation("Circuit breaker half-open for Coinbase API");
                }
            );
    }

    public async Task<ApiResponse<OrderInformation>> PlaceOrderAsync(OrderRequest order, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Placing {Side} order for product {ProductId}", order.Side, order.ProductId);

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.PlaceOrder(order);
                })
            );

            if (!response.Success)
            {
                var errorMessage = response.ErrorResponse?.Message
                                ?? response.ErrorResponse?.NewOrderFailureReason
                                ?? "Unknown error";
                _logger?.LogWarning("Order placement failed: {ErrorMessage}", errorMessage);
                return ApiResponse<OrderInformation>.Failure($"Failed to place order: {errorMessage}");
            }

            _logger?.LogInformation("Successfully placed order, ID: {OrderId}", response.SuccessResponse.OrderId);
            return ApiResponse<OrderInformation>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<OrderInformation>(ex, "placing order");
        }
    }

    public async Task<ApiResponse<OrderInformation>> ClosePositionAsync(ClosePositionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Closing position for product {ProductId}", request.ProductId);

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.ClosePosition(request);
                })
            );

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

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.GetOrders(request);
                })
            );

            return ApiResponse<GetOrdersResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<GetOrdersResponse>(ex, "retrieving orders");
        }
    }

    public async Task<ApiResponse<ListProductsResponse>> ListProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving available products");

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.ListProducts();
                })
            );

            return ApiResponse<ListProductsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<ListProductsResponse>(ex, "retrieving products");
        }
    }

    public async Task<ApiResponse<CandleResponse>> GetProductCandlesAsync(string productId, long start, long end, string granularity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving historical rates for product {ProductId} with granularity {Granularity}", productId, granularity);

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.GetProductCandles(productId, start, end, granularity);
                })
            );

            _logger?.LogInformation("Successfully retrieved historical rates for product {ProductId}", productId);
            return ApiResponse<CandleResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<CandleResponse>(ex, $"retrieving historical rates for {productId}");
        }
    }

    public async Task<ApiResponse<BestBidAskResponse>> GetBestBidAskAsync(List<string>? productIds = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving best bid/ask prices");

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.GetBestBidAsk(productIds);
                })
            );

            return ApiResponse<BestBidAskResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<BestBidAskResponse>(ex, "retrieving best bid/ask prices");
        }
    }

    public async Task<ApiResponse<AccountsResponse>> ListAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving user accounts");

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.ListAccounts();
                })
            );

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

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.GetAccount(id);
                })
            );

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

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.GetPortfolios();
                })
            );

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

            var response = await _circuitBreakerPolicy.ExecuteAsync(
                async () => await _retryPolicy.ExecuteAsync(async () =>
                {
                    return await _coinbaseApi.GetPortfolioBreakdown(portfolioUuid);
                })
            );

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