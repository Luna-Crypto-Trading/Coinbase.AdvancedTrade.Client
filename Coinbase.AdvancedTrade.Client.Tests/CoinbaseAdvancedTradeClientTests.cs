using System.Net;
using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly.CircuitBreaker;
using Refit;

namespace Coinbase.AdvancedTrade.Client.Tests;

public class CoinbaseAdvancedTradeClientTests
{
    private readonly ICoinbaseApi _mockApi;
    private readonly ILogger<CoinbaseAdvancedTradeClient> _mockLogger;
    private readonly CoinbaseAdvancedTradeClient _sut;

    public CoinbaseAdvancedTradeClientTests()
    {
        _mockApi = Substitute.For<ICoinbaseApi>();
        _mockLogger = Substitute.For<ILogger<CoinbaseAdvancedTradeClient>>();
        _sut = new CoinbaseAdvancedTradeClient(_mockApi, _mockLogger);
    }

    #region PlaceOrderAsync Tests

    [Fact]
    public async Task PlaceOrderAsync_WithSuccessfulOrder_ReturnsSuccessResponse()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" }
            }
        };

        var expectedResponse = new OrderInformation
        {
            Success = true,
            SuccessResponse = new SuccessResponse
            {
                OrderId = "test-order-id",
                ProductId = "BTC-USD"
            }
        };

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedResponse);
        result.ErrorMessage.Should().BeNull();
        result.Exception.Should().BeNull();

        await _mockApi.Received(1).PlaceOrder(orderRequest);
    }

    [Fact]
    public async Task PlaceOrderAsync_WithFailedOrder_ReturnsFailureResponse()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" }
            }
        };

        var failedResponse = new OrderInformation
        {
            Success = false,
            SuccessResponse = new SuccessResponse { OrderId = "dummy" }, // Required even for failed orders
            ErrorResponse = new ErrorResponse
            {
                Message = "Insufficient funds",
                NewOrderFailureReason = "INSUFFICIENT_FUNDS"
            }
        };

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>()).Returns(failedResponse);

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Contain("Insufficient funds");

        await _mockApi.Received(1).PlaceOrder(orderRequest);
    }

    [Fact]
    public async Task PlaceOrderAsync_WithApiException_ReturnsAppropriateErrorMessage()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" }
            }
        };

        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://api.coinbase.com"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>()).Throws(apiException);

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
        result.Exception.Should().Be(apiException);
    }

    [Fact]
    public async Task PlaceOrderAsync_WithRateLimitException_RetriesAndSucceeds()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = "SELL",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" }
            }
        };

        var rateLimitException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://api.coinbase.com"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new RefitSettings()
        );

        var successResponse = new OrderInformation
        {
            Success = true,
            SuccessResponse = new SuccessResponse { OrderId = "success-after-retry" }
        };

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>())
            .Returns(Task.FromException<OrderInformation>(rateLimitException),
                     Task.FromException<OrderInformation>(rateLimitException),
                     Task.FromResult(successResponse));

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.SuccessResponse.OrderId.Should().Be("success-after-retry");

        await _mockApi.Received(3).PlaceOrder(orderRequest);
    }

    #endregion

    #region GetOrdersAsync Tests

    [Fact]
    public async Task GetOrdersAsync_WithNoFilters_ReturnsAllOrders()
    {
        // Arrange
        var expectedResponse = new GetOrdersResponse
        {
            Orders = new List<OrderV3>(),  // Simplify - empty list for now
            HasNext = false,
            Cursor = ""
        };

        _mockApi.GetOrders(null).Returns(expectedResponse);

        // Act
        var result = await _sut.GetOrdersAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orders.Should().HaveCount(0);

        await _mockApi.Received(1).GetOrders(null);
    }

    [Fact]
    public async Task GetOrdersAsync_WithSearchFilters_AppliesFiltersCorrectly()
    {
        // Arrange
        var searchRequest = new OrderSearchRequest
        {
            ProductIds = new List<string> { "BTC-USD" },
            OrderStatus = new List<string> { "FILLED" },
            Limit = 50
        };

        var expectedResponse = new GetOrdersResponse
        {
            Orders = new List<OrderV3>(),  // Simplify for now
            Cursor = ""
        };

        _mockApi.GetOrders(searchRequest).Returns(expectedResponse);

        // Act
        var result = await _sut.GetOrdersAsync(searchRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orders.Should().HaveCount(0);

        await _mockApi.Received(1).GetOrders(searchRequest);
    }

    #endregion

    #region ListProductsAsync Tests

    [Fact]
    public async Task ListProductsAsync_ReturnsAvailableProducts()
    {
        // Arrange
        var expectedResponse = new ListProductsResponse
        {
            Products = new List<AdvancedTradeProduct>()  // Simplify for now
        };

        _mockApi.ListProducts().Returns(expectedResponse);

        // Act
        var result = await _sut.ListProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Products.Should().HaveCount(0);

        await _mockApi.Received(1).ListProducts();
    }

    #endregion

    #region GetBestBidAskAsync Tests

    [Fact]
    public async Task GetBestBidAskAsync_WithProductIds_ReturnsBidAskData()
    {
        // Arrange
        var productIds = new List<string> { "BTC-USD", "ETH-USD" };
        var expectedResponse = new BestBidAskResponse
        {
            PriceBooks = new List<PriceBook>
            {
                new PriceBook
                {
                    ProductId = "BTC-USD",
                    Bids = new List<PriceBookEntry> { new PriceBookEntry { Price = "50000", Size = "0.1" } },
                    Asks = new List<PriceBookEntry> { new PriceBookEntry { Price = "50100", Size = "0.2" } },
                    Time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                }
            }
        };

        _mockApi.GetBestBidAsk(productIds).Returns(expectedResponse);

        // Act
        var result = await _sut.GetBestBidAskAsync(productIds);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.PriceBooks.Should().HaveCount(1);
        result.Data.PriceBooks.First().ProductId.Should().Be("BTC-USD");

        await _mockApi.Received(1).GetBestBidAsk(productIds);
    }

    [Fact]
    public async Task GetBestBidAskAsync_WithNullProductIds_ReturnsAllProducts()
    {
        // Arrange
        var expectedResponse = new BestBidAskResponse
        {
            PriceBooks = new List<PriceBook>()
        };

        _mockApi.GetBestBidAsk(null).Returns(expectedResponse);

        // Act
        var result = await _sut.GetBestBidAskAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        await _mockApi.Received(1).GetBestBidAsk(null);
    }

    #endregion

    #region ListAccountsAsync Tests

    [Fact]
    public async Task ListAccountsAsync_ReturnsUserAccounts()
    {
        // Arrange
        var expectedResponse = new AccountsResponse
        {
            Accounts = new List<CoinbaseAccount>(),  // Simplify for now
            HasNext = false
        };

        _mockApi.ListAccounts().Returns(expectedResponse);

        // Act
        var result = await _sut.ListAccountsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Accounts.Should().HaveCount(0);

        await _mockApi.Received(1).ListAccounts();
    }

    #endregion

    #region GetAccountAsync Tests

    [Fact]
    public async Task GetAccountAsync_WithValidId_ReturnsAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var expectedAccount = new CoinbaseAccount
        {
            Uuid = accountId.ToString(),
            Currency = "BTC",
            AvailableBalance = new Balance { Value = "2.5", Currency = "BTC" },
            Hold = new Balance { Value = "0", Currency = "BTC" }
        };

        _mockApi.GetAccount(accountId).Returns(expectedAccount);

        // Act
        var result = await _sut.GetAccountAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Uuid.Should().Be(accountId.ToString());
        result.Data.Currency.Should().Be("BTC");

        await _mockApi.Received(1).GetAccount(accountId);
    }

    [Fact]
    public async Task GetAccountAsync_WithNotFoundId_ReturnsErrorResponse()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var notFoundException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new RefitSettings()
        );

        _mockApi.GetAccount(accountId).Throws(notFoundException);

        // Act
        var result = await _sut.GetAccountAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");

        await _mockApi.Received(1).GetAccount(accountId);
    }

    #endregion

    #region Circuit Breaker Tests

    [Fact]
    public async Task CircuitBreaker_AfterMultipleFailures_OpensCircuit()
    {
        // Arrange
        var orderRequest = new OrderRequest { ClientOrderId = Guid.NewGuid().ToString(), ProductId = "BTC-USD", Side = "BUY", OrderConfiguration = new OrderConfiguration { MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" } } };
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://api.coinbase.com"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.InternalServerError),
            new RefitSettings()
        );

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>()).Throws(apiException);

        // Act - Trigger circuit breaker (5 consecutive failures)
        for (int i = 0; i < 5; i++)
        {
            var result = await _sut.PlaceOrderAsync(orderRequest);
            result.IsSuccess.Should().BeFalse();
        }

        // Next call should fail immediately with BrokenCircuitException
        var finalResult = await _sut.PlaceOrderAsync(orderRequest);

        // Assert
        finalResult.IsSuccess.Should().BeFalse();
        finalResult.ErrorMessage.Should().Contain("currently unavailable");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task HandleException_WithForbiddenError_ReturnsPermissionError()
    {
        // Arrange
        var forbiddenException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.Forbidden),
            new RefitSettings()
        );

        _mockApi.ListAccounts().Throws(forbiddenException);

        // Act
        var result = await _sut.ListAccountsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not have permission");
    }

    [Fact]
    public async Task HandleException_WithGatewayTimeout_ReturnsServiceUnavailable()
    {
        // Arrange
        var timeoutException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.GatewayTimeout),
            new RefitSettings()
        );

        _mockApi.ListProducts().Throws(timeoutException);

        // Act
        var result = await _sut.ListProductsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("currently unavailable");
    }

    #endregion

    #region ClosePositionAsync Tests

    [Fact]
    public async Task ClosePositionAsync_WithValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new ClosePositionRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Size = "0.5"
        };

        var expectedResponse = new OrderInformation
        {
            Success = true,
            SuccessResponse = new SuccessResponse
            {
                OrderId = "close-position-order-id",
                ProductId = "BTC-USD"
            }
        };

        _mockApi.ClosePosition(request).Returns(expectedResponse);

        // Act
        var result = await _sut.ClosePositionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.SuccessResponse.OrderId.Should().Be("close-position-order-id");

        await _mockApi.Received(1).ClosePosition(request);
    }

    #endregion

    #region GetProductCandlesAsync Tests

    [Fact]
    public async Task GetProductCandlesAsync_WithValidParameters_ReturnsCandles()
    {
        // Arrange
        var productId = "BTC-USD";
        var start = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
        var end = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var granularity = "ONE_HOUR";

        var expectedResponse = new CandleResponse
        {
            Candles = new List<Candle>
            {
                new Candle
                {
                    Start = start.ToString(),
                    Low = "49000",
                    High = "51000",
                    Open = "50000",
                    Close = "50500",
                    Volume = "100"
                }
            }
        };

        _mockApi.GetProductCandles(productId, start, end, granularity).Returns(expectedResponse);

        // Act
        var result = await _sut.GetProductCandlesAsync(productId, start, end, granularity);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Candles.Should().HaveCount(1);
        result.Data.Candles.First().Open.Should().Be("50000");

        await _mockApi.Received(1).GetProductCandles(productId, start, end, granularity);
    }

    #endregion
}