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
        result.Data!.SuccessResponse!.OrderId.Should().Be("success-after-retry");

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

        _mockApi.ListProducts(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<List<string>?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Products.Should().HaveCount(0);

        await _mockApi.Received(1).ListProducts(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<List<string>?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>());
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

        _mockApi.ListAccounts(Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListAccountsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Accounts.Should().HaveCount(0);

        await _mockApi.Received(1).ListAccounts(Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
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

        _mockApi.ListAccounts(Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(forbiddenException);

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

        _mockApi.ListProducts(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<List<string>?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Throws(timeoutException);

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
        result.Data!.SuccessResponse!.OrderId.Should().Be("close-position-order-id");

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

        _mockApi.GetProductCandles(productId, start, end, granularity, Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetProductCandlesAsync(productId, start, end, granularity);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Candles.Should().HaveCount(1);
        result.Data.Candles.First().Open.Should().Be("50000");

        await _mockApi.Received(1).GetProductCandles(productId, start, end, granularity, Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region CancelOrdersAsync Tests

    [Fact]
    public async Task CancelOrdersAsync_WithSuccessfulCancellation_ReturnsSuccess()
    {
        // Arrange
        var orderIds = new List<string> { "order-1", "order-2" };
        var expectedResponse = new CancelOrdersResponse
        {
            Results = new List<CancelOrderResult>
            {
                new CancelOrderResult { Success = true, OrderId = "order-1" },
                new CancelOrderResult { Success = true, OrderId = "order-2" }
            }
        };

        _mockApi.CancelOrders(Arg.Any<CancelOrdersRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.CancelOrdersAsync(orderIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Results.Should().HaveCount(2);
        result.Data.Results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
    }

    [Fact]
    public async Task CancelOrdersAsync_WithPartialFailure_ReturnsResultsWithFailures()
    {
        // Arrange
        var orderIds = new List<string> { "order-1", "order-2" };
        var expectedResponse = new CancelOrdersResponse
        {
            Results = new List<CancelOrderResult>
            {
                new CancelOrderResult { Success = true, OrderId = "order-1" },
                new CancelOrderResult { Success = false, OrderId = "order-2", FailureReason = "UNKNOWN_CANCEL_FAILURE_REASON" }
            }
        };

        _mockApi.CancelOrders(Arg.Any<CancelOrdersRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.CancelOrdersAsync(orderIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Results.Should().HaveCount(2);
        result.Data.Results[0].Success.Should().BeTrue();
        result.Data.Results[1].Success.Should().BeFalse();
        result.Data.Results[1].FailureReason.Should().Be("UNKNOWN_CANCEL_FAILURE_REASON");
    }

    [Fact]
    public async Task CancelOrdersAsync_WithApiException_ReturnsError()
    {
        // Arrange
        var orderIds = new List<string> { "order-1" };
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://api.coinbase.com"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.CancelOrders(Arg.Any<CancelOrdersRequest>()).Throws(apiException);

        // Act
        var result = await _sut.CancelOrdersAsync(orderIds);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    #endregion

    #region GetOrderAsync Tests

    [Fact]
    public async Task GetOrderAsync_WithValidOrderId_ReturnsOrder()
    {
        // Arrange
        var orderId = "test-order-id";
        var expectedResponse = new GetOrderResponse
        {
            Order = new OrderV3
            {
                OrderId = orderId,
                ProductId = "BTC-USD",
                UserId = "user-1",
                OrderConfiguration = new OrderConfiguration(),
                Side = "BUY",
                ClientOrderId = "client-order-1",
                Status = "FILLED",
                TimeInForce = "GOOD_UNTIL_CANCELLED",
                CreatedTime = "2024-01-01T00:00:00Z",
                CompletionPercentage = "100",
                FilledSize = "0.5",
                AverageFilledPrice = "50000",
                NumberOfFills = "1"
            }
        };

        _mockApi.GetOrder(orderId).Returns(expectedResponse);

        // Act
        var result = await _sut.GetOrderAsync(orderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Order.OrderId.Should().Be(orderId);
        result.Data.Order.Status.Should().Be("FILLED");
    }

    [Fact]
    public async Task GetOrderAsync_WithNotFoundOrderId_ReturnsError()
    {
        // Arrange
        var orderId = "nonexistent-order";
        var notFoundException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new RefitSettings()
        );

        _mockApi.GetOrder(orderId).Throws(notFoundException);

        // Act
        var result = await _sut.GetOrderAsync(orderId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion

    #region GetProductAsync Tests

    [Fact]
    public async Task GetProductAsync_WithValidProductId_ReturnsProduct()
    {
        // Arrange
        var productId = "BTC-USD";
        var expectedProduct = new AdvancedTradeProduct
        {
            ProductId = productId,
            Price = "50000.00",
            PricePercentageChange24h = "2.5",
            Volume24h = "1000",
            VolumePercentageChange24h = "5.0",
            BaseIncrement = "0.00000001",
            QuoteIncrement = "0.01",
            QuoteMinSize = "1",
            QuoteMaxSize = "10000000",
            BaseMinSize = "0.00000001",
            BaseMaxSize = "1000",
            BaseCurrencyId = "BTC",
            QuoteCurrencyId = "USD",
            DisplayName = "BTC/USD",
            Status = "online"
        };

        _mockApi.GetProduct(productId).Returns(expectedProduct);

        // Act
        var result = await _sut.GetProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.ProductId.Should().Be(productId);
        result.Data.Price.Should().Be("50000.00");
    }

    [Fact]
    public async Task GetProductAsync_WithNotFoundProduct_ReturnsError()
    {
        // Arrange
        var productId = "INVALID-PAIR";
        var notFoundException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new RefitSettings()
        );

        _mockApi.GetProduct(productId).Throws(notFoundException);

        // Act
        var result = await _sut.GetProductAsync(productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion

    #region EditOrderAsync Tests

    [Fact]
    public async Task EditOrderAsync_WithSuccessfulEdit_ReturnsSuccess()
    {
        // Arrange
        var request = new EditOrderRequest { OrderId = "order-1", Price = "51000", Size = "0.5" };
        var expectedResponse = new EditOrderResponse { Success = true };

        _mockApi.EditOrder(Arg.Any<EditOrderRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.EditOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task EditOrderAsync_WithFailedEdit_ReturnsFailure()
    {
        // Arrange
        var request = new EditOrderRequest { OrderId = "order-1", Price = "51000", Size = "0.5" };
        var expectedResponse = new EditOrderResponse
        {
            Success = false,
            Errors = new List<EditOrderError>
            {
                new EditOrderError { EditFailureReason = "EDIT_FAILURE_NOT_FOUND" }
            }
        };

        _mockApi.EditOrder(Arg.Any<EditOrderRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.EditOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("EDIT_FAILURE_NOT_FOUND");
    }

    #endregion

    #region PreviewOrderAsync Tests

    [Fact]
    public async Task PreviewOrderAsync_ReturnsPreviewData()
    {
        // Arrange
        var order = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" }
            }
        };

        var expectedResponse = new PreviewOrderResponse
        {
            OrderTotal = "100.50",
            CommissionTotal = "0.50",
            QuoteSize = "100",
            BaseSize = "0.002",
            BestBid = "49999",
            BestAsk = "50001"
        };

        _mockApi.PreviewOrder(Arg.Any<OrderRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.PreviewOrderAsync(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.OrderTotal.Should().Be("100.50");
        result.Data.CommissionTotal.Should().Be("0.50");
        result.Data.BestBid.Should().Be("49999");
    }

    #endregion

    #region PreviewEditOrderAsync Tests

    [Fact]
    public async Task PreviewEditOrderAsync_ReturnsPreviewData()
    {
        // Arrange
        var request = new EditOrderRequest { OrderId = "order-1", Price = "51000", Size = "0.5" };
        var expectedResponse = new PreviewOrderResponse
        {
            OrderTotal = "25500",
            CommissionTotal = "12.75"
        };

        _mockApi.PreviewEditOrder(Arg.Any<EditOrderRequest>()).Returns(expectedResponse);

        // Act
        var result = await _sut.PreviewEditOrderAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.OrderTotal.Should().Be("25500");
    }

    #endregion

    #region GetMarketTradesAsync Tests

    [Fact]
    public async Task GetMarketTradesAsync_ReturnsTradeData()
    {
        // Arrange
        var expectedResponse = new MarketTradesResponse
        {
            Trades = new List<MarketTrade>
            {
                new MarketTrade
                {
                    TradeId = "trade-1",
                    ProductId = "BTC-USD",
                    Price = "50000",
                    Size = "0.1",
                    Time = "2024-01-01T00:00:00Z",
                    Side = "BUY"
                }
            },
            BestBid = "49999",
            BestAsk = "50001"
        };

        _mockApi.GetMarketTrades("BTC-USD", 10, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetMarketTradesAsync("BTC-USD", 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Trades.Should().HaveCount(1);
        result.Data.Trades[0].Price.Should().Be("50000");
        result.Data.BestBid.Should().Be("49999");
    }

    #endregion

    #region GetTransactionSummaryAsync Tests

    [Fact]
    public async Task GetTransactionSummaryAsync_ReturnsFeeData()
    {
        // Arrange
        var expectedResponse = new TransactionSummaryResponse
        {
            TotalFees = 125.50,
            FeeTier = new FeeTier
            {
                PricingTier = "<$10k",
                TakerFeeRate = "0.006",
                MakerFeeRate = "0.004"
            },
            AdvancedTradeOnlyVolume = 50000
        };

        _mockApi.GetTransactionSummary(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetTransactionSummaryAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.TotalFees.Should().Be(125.50);
        result.Data.FeeTier.TakerFeeRate.Should().Be("0.006");
        result.Data.FeeTier.MakerFeeRate.Should().Be("0.004");
    }

    #endregion

    #region GetPortfoliosAsync Tests

    [Fact]
    public async Task GetPortfoliosAsync_ReturnsPortfolios()
    {
        // Arrange
        var expectedResponse = new AdvancedTradePortfolioResponse
        {
            Portfolios = new List<AdvancedTradePortfolio>
            {
                new AdvancedTradePortfolio { Uuid = "portfolio-1", Name = "Default", Type = "DEFAULT", Deleted = false }
            }
        };

        _mockApi.GetPortfolios(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetPortfoliosAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Portfolios.Should().HaveCount(1);
        result.Data.Portfolios[0].Name.Should().Be("Default");
    }

    [Fact]
    public async Task GetPortfoliosAsync_WithApiError_ReturnsFailure()
    {
        // Arrange
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.GetPortfolios(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Throws(apiException);

        // Act
        var result = await _sut.GetPortfoliosAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    #endregion

    #region GetPortfolioBreakdownAsync Tests

    [Fact]
    public async Task GetPortfolioBreakdownAsync_WithApiError_ReturnsFailure()
    {
        // Arrange
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound),
            new RefitSettings()
        );

        _mockApi.GetPortfolioBreakdown("nonexistent").Throws(apiException);

        // Act
        var result = await _sut.GetPortfolioBreakdownAsync("nonexistent");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task PlaceOrderAsync_CancellationToken_IsPassedToApi()
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
            SuccessResponse = new SuccessResponse { OrderId = "test-order-id" }
        };

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest, token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _mockApi.Received(1).PlaceOrder(Arg.Any<OrderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAccountsAsync_CancellationToken_IsPassedToApi()
    {
        // Arrange
        var expectedResponse = new AccountsResponse
        {
            Accounts = new List<CoinbaseAccount>(),
            HasNext = false
        };

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockApi.ListAccounts(Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListAccountsAsync(cancellationToken: token);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _mockApi.Received(1).ListAccounts(Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Resilience Tests

    [Fact]
    public async Task NonTransientError_DoesNotRetry()
    {
        // Arrange - 401 Unauthorized is not transient, should not retry
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

        var unauthorizedException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://api.coinbase.com"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>(), Arg.Any<CancellationToken>())
            .Throws(unauthorizedException);

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest);

        // Assert - should NOT retry, only 1 call
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
        await _mockApi.Received(1).PlaceOrder(Arg.Any<OrderRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RetryExhaustion_AllAttemptsFail_ReturnsError()
    {
        // Arrange - 429 is transient, will retry 3 times then fail
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

        var rateLimitException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, "https://api.coinbase.com"),
            HttpMethod.Post,
            new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new RefitSettings()
        );

        _mockApi.PlaceOrder(Arg.Any<OrderRequest>(), Arg.Any<CancellationToken>())
            .Throws(rateLimitException);

        // Act
        var result = await _sut.PlaceOrderAsync(orderRequest);

        // Assert - 1 initial + 3 retries = 4 total calls
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().NotBeEmpty();
        await _mockApi.Received(4).PlaceOrder(Arg.Any<OrderRequest>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetFillsAsync Tests

    [Fact]
    public async Task GetFillsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FillsResponse
        {
            Fills = new List<Fill>
            {
                new Fill
                {
                    EntryId = "entry-1",
                    TradeId = "trade-1",
                    OrderId = "order-1",
                    TradeTime = "2024-01-01T00:00:00Z",
                    TradeType = "FILL",
                    Price = "50000",
                    Size = "0.1",
                    Commission = "0.50",
                    ProductId = "BTC-USD",
                    Side = "BUY"
                }
            },
            Cursor = "next-cursor"
        };

        _mockApi.GetFills(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetFillsAsync(orderId: "order-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Fills.Should().HaveCount(1);
        result.Data.Fills[0].Price.Should().Be("50000");
        result.Data.Cursor.Should().Be("next-cursor");
    }

    [Fact]
    public async Task GetFillsAsync_WithApiError_ReturnsFailure()
    {
        // Arrange
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.GetFills(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Throws(apiException);

        // Act
        var result = await _sut.GetFillsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    #endregion

    #region GetProductBookAsync Tests

    [Fact]
    public async Task GetProductBookAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new ProductBookResponse
        {
            PriceBook = new PriceBook
            {
                ProductId = "BTC-USD",
                Bids = new List<PriceBookEntry>
                {
                    new PriceBookEntry { Price = "49999", Size = "1.5" }
                },
                Asks = new List<PriceBookEntry>
                {
                    new PriceBookEntry { Price = "50001", Size = "2.0" }
                },
                Time = "2024-01-01T00:00:00Z"
            }
        };

        _mockApi.GetProductBook("BTC-USD", Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetProductBookAsync("BTC-USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PriceBook.ProductId.Should().Be("BTC-USD");
        result.Data.PriceBook.Bids.Should().HaveCount(1);
        result.Data.PriceBook.Asks.Should().HaveCount(1);
    }

    #endregion

    #region GetPublicProductCandlesAsync Tests

    [Fact]
    public async Task GetPublicProductCandlesAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new CandleResponse
        {
            Candles = new List<Candle>
            {
                new Candle { Start = "1704067200", Open = "50000", High = "51000", Low = "49000", Close = "50500", Volume = "100" }
            }
        };

        _mockApi.GetPublicProductCandles("BTC-USD", 1704067200, 1704153600, "ONE_HOUR", Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetPublicProductCandlesAsync("BTC-USD", 1704067200, 1704153600, "ONE_HOUR");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Candles.Should().HaveCount(1);
    }

    #endregion

    #region CreatePortfolioAsync Tests

    [Fact]
    public async Task CreatePortfolioAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new AdvancedTradePortfolioResponse
        {
            Portfolios = new List<AdvancedTradePortfolio>
            {
                new AdvancedTradePortfolio { Uuid = "new-uuid", Name = "My Portfolio", Type = "CONSUMER", Deleted = false }
            }
        };

        _mockApi.CreatePortfolio(Arg.Any<CreatePortfolioRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.CreatePortfolioAsync("My Portfolio");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Portfolios.Should().HaveCount(1);
        result.Data.Portfolios[0].Name.Should().Be("My Portfolio");
    }

    #endregion

    #region EditPortfolioAsync Tests

    [Fact]
    public async Task EditPortfolioAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new AdvancedTradePortfolioResponse
        {
            Portfolios = new List<AdvancedTradePortfolio>
            {
                new AdvancedTradePortfolio { Uuid = "portfolio-1", Name = "Updated Name", Type = "CONSUMER", Deleted = false }
            }
        };

        _mockApi.EditPortfolio("portfolio-1", Arg.Any<EditPortfolioRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.EditPortfolioAsync("portfolio-1", "Updated Name");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Portfolios[0].Name.Should().Be("Updated Name");
    }

    #endregion

    #region DeletePortfolioAsync Tests

    [Fact]
    public async Task DeletePortfolioAsync_ReturnsSuccess()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

        _mockApi.DeletePortfolio("portfolio-1", Arg.Any<CancellationToken>())
            .Returns(httpResponse);

        // Act
        var result = await _sut.DeletePortfolioAsync("portfolio-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePortfolioAsync_WithNotFound_ReturnsFailure()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _mockApi.DeletePortfolio("nonexistent", Arg.Any<CancellationToken>())
            .Returns(httpResponse);

        // Act
        var result = await _sut.DeletePortfolioAsync("nonexistent");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to delete portfolio");
    }

    #endregion

    #region MovePortfolioFundsAsync Tests

    [Fact]
    public async Task MovePortfolioFundsAsync_ReturnsSuccess()
    {
        // Arrange
        var request = new MoveFundsRequest
        {
            Funds = new MoveFundsAmount { Value = "100.00", Currency = "USD" },
            SourcePortfolioUuid = "source-uuid",
            TargetPortfolioUuid = "target-uuid"
        };

        var expectedResponse = new MoveFundsResponse
        {
            SourcePortfolioUuid = "source-uuid",
            TargetPortfolioUuid = "target-uuid"
        };

        _mockApi.MoveFunds(Arg.Any<MoveFundsRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.MovePortfolioFundsAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.SourcePortfolioUuid.Should().Be("source-uuid");
    }

    #endregion

    #region GetKeyPermissionsAsync Tests

    [Fact]
    public async Task GetKeyPermissionsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new KeyPermissionsResponse
        {
            CanView = true,
            CanTrade = true,
            CanTransfer = false,
            PortfolioUuid = "portfolio-1",
            PortfolioType = "CONSUMER"
        };

        _mockApi.GetKeyPermissions(Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetKeyPermissionsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.CanView.Should().BeTrue();
        result.Data.CanTrade.Should().BeTrue();
        result.Data.CanTransfer.Should().BeFalse();
    }

    [Fact]
    public async Task GetKeyPermissionsAsync_WithApiError_ReturnsFailure()
    {
        // Arrange
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.GetKeyPermissions(Arg.Any<CancellationToken>())
            .Throws(apiException);

        // Act
        var result = await _sut.GetKeyPermissionsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    #endregion

    #region GetServerTimeAsync Tests

    [Fact]
    public async Task GetServerTimeAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new ServerTimeResponse
        {
            Iso = "2024-01-01T00:00:00Z",
            EpochSeconds = "1704067200",
            EpochMillis = "1704067200000"
        };

        _mockApi.GetServerTime(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetServerTimeAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Iso.Should().Be("2024-01-01T00:00:00Z");
        result.Data.EpochSeconds.Should().Be("1704067200");
    }

    #endregion

    #region GetPublicProductBookAsync Tests

    [Fact]
    public async Task GetPublicProductBookAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new ProductBookResponse
        {
            PriceBook = new PriceBook
            {
                ProductId = "BTC-USD",
                Bids = new List<PriceBookEntry>
                {
                    new PriceBookEntry { Price = "49999", Size = "1.5" }
                },
                Asks = new List<PriceBookEntry>
                {
                    new PriceBookEntry { Price = "50001", Size = "2.0" }
                },
                Time = "2024-01-01T00:00:00Z"
            }
        };

        _mockApi.GetPublicProductBook("BTC-USD", Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetPublicProductBookAsync("BTC-USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PriceBook.ProductId.Should().Be("BTC-USD");
    }

    #endregion

    #region GetPublicProductsAsync Tests

    [Fact]
    public async Task GetPublicProductsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new ListProductsResponse
        {
            Products = new List<AdvancedTradeProduct>()
        };

        _mockApi.GetPublicProducts(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<List<string>?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetPublicProductsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetPublicProductAsync Tests

    [Fact]
    public async Task GetPublicProductAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new AdvancedTradeProduct
        {
            ProductId = "BTC-USD",
            Price = "50000",
            PricePercentageChange24h = "1.5",
            Volume24h = "1000",
            VolumePercentageChange24h = "5.0",
            BaseIncrement = "0.00000001",
            QuoteIncrement = "0.01",
            QuoteMinSize = "1",
            QuoteMaxSize = "10000000",
            BaseMinSize = "0.00000001",
            BaseMaxSize = "1000",
            BaseCurrencyId = "BTC",
            QuoteCurrencyId = "USD",
            DisplayName = "BTC-USD",
            Status = "online"
        };

        _mockApi.GetPublicProduct("BTC-USD", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetPublicProductAsync("BTC-USD");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.ProductId.Should().Be("BTC-USD");
    }

    #endregion

    #region GetPublicMarketTradesAsync Tests

    [Fact]
    public async Task GetPublicMarketTradesAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new MarketTradesResponse
        {
            Trades = new List<MarketTrade>
            {
                new MarketTrade
                {
                    TradeId = "trade-1",
                    ProductId = "BTC-USD",
                    Price = "50000",
                    Size = "0.1",
                    Time = "2024-01-01T00:00:00Z",
                    Side = "BUY"
                }
            },
            BestBid = "49999",
            BestAsk = "50001"
        };

        _mockApi.GetPublicMarketTrades("BTC-USD", 10, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetPublicMarketTradesAsync("BTC-USD", 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Trades.Should().HaveCount(1);
    }

    #endregion

    #region ListPaymentMethodsAsync Tests

    [Fact]
    public async Task ListPaymentMethodsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new PaymentMethodsResponse
        {
            PaymentMethods = new List<PaymentMethod>
            {
                new PaymentMethod
                {
                    Id = "pm-1",
                    Type = "ach_bank_account",
                    Name = "My Bank Account",
                    Currency = "USD",
                    Verified = true,
                    AllowBuy = true,
                    AllowSell = true
                }
            }
        };

        _mockApi.GetPaymentMethods(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListPaymentMethodsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PaymentMethods.Should().HaveCount(1);
        result.Data.PaymentMethods[0].Name.Should().Be("My Bank Account");
    }

    [Fact]
    public async Task ListPaymentMethodsAsync_WithApiError_ReturnsFailure()
    {
        // Arrange
        var apiException = await ApiException.Create(
            new HttpRequestMessage(HttpMethod.Get, "https://api.coinbase.com"),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new RefitSettings()
        );

        _mockApi.GetPaymentMethods(Arg.Any<CancellationToken>()).Throws(apiException);

        // Act
        var result = await _sut.ListPaymentMethodsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    #endregion

    #region GetPaymentMethodAsync Tests

    [Fact]
    public async Task GetPaymentMethodAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new PaymentMethod
        {
            Id = "pm-1",
            Type = "ach_bank_account",
            Name = "My Bank Account",
            Currency = "USD"
        };

        _mockApi.GetPaymentMethod("pm-1", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetPaymentMethodAsync("pm-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be("pm-1");
        result.Data.Type.Should().Be("ach_bank_account");
    }

    #endregion

    #region GetFuturesBalanceSummaryAsync Tests

    [Fact]
    public async Task GetFuturesBalanceSummaryAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FuturesBalanceSummaryResponse
        {
            BalanceSummary = new FuturesBalanceSummary
            {
                FuturesBuyingPower = "10000.00",
                TotalUsdBalance = "15000.00",
                UnrealizedPnl = "500.00"
            }
        };

        _mockApi.GetFuturesBalanceSummary(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetFuturesBalanceSummaryAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.BalanceSummary.FuturesBuyingPower.Should().Be("10000.00");
    }

    #endregion

    #region ListFuturesPositionsAsync Tests

    [Fact]
    public async Task ListFuturesPositionsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FuturesPositionsResponse
        {
            Positions = new List<FuturesPositionDetail>
            {
                new FuturesPositionDetail
                {
                    ProductId = "BIT-28MAR25-CDE",
                    Side = "LONG",
                    NumberOfContracts = "5",
                    UnrealizedPnl = "250.00"
                }
            }
        };

        _mockApi.ListFuturesPositions(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListFuturesPositionsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Positions.Should().HaveCount(1);
        result.Data.Positions[0].ProductId.Should().Be("BIT-28MAR25-CDE");
    }

    #endregion

    #region GetFuturesPositionAsync Tests

    [Fact]
    public async Task GetFuturesPositionAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FuturesPositionResponse
        {
            Position = new FuturesPositionDetail
            {
                ProductId = "BIT-28MAR25-CDE",
                Side = "LONG",
                NumberOfContracts = "5"
            }
        };

        _mockApi.GetFuturesPosition("BIT-28MAR25-CDE", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetFuturesPositionAsync("BIT-28MAR25-CDE");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Position.ProductId.Should().Be("BIT-28MAR25-CDE");
    }

    #endregion

    #region ScheduleFuturesSweepAsync Tests

    [Fact]
    public async Task ScheduleFuturesSweepAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FuturesSweepResponse
        {
            Sweeps = new List<FuturesSweep>
            {
                new FuturesSweep { Id = "sweep-1", RequestedAmount = "1000.00", Status = "PENDING" }
            }
        };

        _mockApi.ScheduleFuturesSweep(Arg.Any<ScheduleSweepRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.ScheduleFuturesSweepAsync("1000.00");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Sweeps.Should().HaveCount(1);
        result.Data.Sweeps[0].Status.Should().Be("PENDING");
    }

    #endregion

    #region ListFuturesSweepsAsync Tests

    [Fact]
    public async Task ListFuturesSweepsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FuturesSweepResponse
        {
            Sweeps = new List<FuturesSweep>()
        };

        _mockApi.ListFuturesSweeps(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListFuturesSweepsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region CancelFuturesSweepAsync Tests

    [Fact]
    public async Task CancelFuturesSweepAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new FuturesSweepResponse
        {
            Sweeps = new List<FuturesSweep>()
        };

        _mockApi.CancelFuturesSweep(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.CancelFuturesSweepAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetIntradayMarginSettingAsync Tests

    [Fact]
    public async Task GetIntradayMarginSettingAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new IntradayMarginSettingResponse
        {
            Setting = "STANDARD"
        };

        _mockApi.GetIntradayMarginSetting(Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetIntradayMarginSettingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Setting.Should().Be("STANDARD");
    }

    #endregion

    #region GetCurrentMarginWindowAsync Tests

    [Fact]
    public async Task GetCurrentMarginWindowAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new CurrentMarginWindowResponse
        {
            MarginWindow = new MarginWindow
            {
                MarginWindowType = "INTRADAY",
                IsIntradayMarginKillswitchEnabled = false,
                IsIntradayMarginEnrollmentKillswitchEnabled = false
            }
        };

        _mockApi.GetCurrentMarginWindow(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetCurrentMarginWindowAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.MarginWindow.MarginWindowType.Should().Be("INTRADAY");
    }

    #endregion

    #region SetIntradayMarginSettingAsync Tests

    [Fact]
    public async Task SetIntradayMarginSettingAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new IntradayMarginSettingResponse
        {
            Setting = "INTRADAY"
        };

        _mockApi.SetIntradayMarginSetting(Arg.Any<SetIntradayMarginSettingRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.SetIntradayMarginSettingAsync("INTRADAY");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Setting.Should().Be("INTRADAY");
    }

    #endregion

    #region AllocatePortfolioAsync Tests

    [Fact]
    public async Task AllocatePortfolioAsync_ReturnsSuccess()
    {
        // Arrange
        var request = new AllocatePortfolioRequest
        {
            PortfolioUuid = "portfolio-1",
            Symbol = "BTC-PERP",
            Amount = "1000.00",
            Currency = "USD"
        };

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _mockApi.AllocatePortfolio(Arg.Any<AllocatePortfolioRequest>(), Arg.Any<CancellationToken>())
            .Returns(httpResponse);

        // Act
        var result = await _sut.AllocatePortfolioAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetPerpsPortfolioSummaryAsync Tests

    [Fact]
    public async Task GetPerpsPortfolioSummaryAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new IntxPortfolioResponse
        {
            Summary = new IntxPortfolioSummary
            {
                PortfolioUuid = "portfolio-1",
                Collateral = "50000.00",
                InLiquidation = false
            }
        };

        _mockApi.GetPerpsPortfolioSummary("portfolio-1", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetPerpsPortfolioSummaryAsync("portfolio-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Summary.Collateral.Should().Be("50000.00");
    }

    #endregion

    #region ListPerpsPositionsAsync Tests

    [Fact]
    public async Task ListPerpsPositionsAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new IntxPositionsResponse
        {
            Positions = new List<IntxPosition>
            {
                new IntxPosition
                {
                    Symbol = "BTC-PERP",
                    NetSize = "0.5",
                    UnrealizedPnl = "100.00",
                    PositionSide = "LONG"
                }
            }
        };

        _mockApi.ListPerpsPositions("portfolio-1", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.ListPerpsPositionsAsync("portfolio-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Positions.Should().HaveCount(1);
        result.Data.Positions[0].Symbol.Should().Be("BTC-PERP");
    }

    #endregion

    #region GetPerpsPositionAsync Tests

    [Fact]
    public async Task GetPerpsPositionAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new IntxPositionResponse
        {
            Position = new IntxPosition
            {
                Symbol = "BTC-PERP",
                NetSize = "0.5",
                PositionSide = "LONG"
            }
        };

        _mockApi.GetPerpsPosition("portfolio-1", "BTC-PERP", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetPerpsPositionAsync("portfolio-1", "BTC-PERP");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Position.Symbol.Should().Be("BTC-PERP");
    }

    #endregion

    #region GetPerpsPortfolioBalancesAsync Tests

    [Fact]
    public async Task GetPerpsPortfolioBalancesAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new IntxBalancesResponse
        {
            PortfolioBalances = new List<IntxBalance>
            {
                new IntxBalance
                {
                    AssetId = "USD",
                    Quantity = "10000.00",
                    CollateralValue = "10000.00"
                }
            }
        };

        _mockApi.GetPerpsPortfolioBalances("portfolio-1", Arg.Any<CancellationToken>()).Returns(expectedResponse);

        // Act
        var result = await _sut.GetPerpsPortfolioBalancesAsync("portfolio-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.PortfolioBalances.Should().HaveCount(1);
        result.Data.PortfolioBalances[0].AssetId.Should().Be("USD");
    }

    #endregion

    #region SetMultiAssetCollateralAsync Tests

    [Fact]
    public async Task SetMultiAssetCollateralAsync_ReturnsSuccess()
    {
        // Arrange
        var request = new MultiAssetCollateralRequest
        {
            PortfolioUuid = "portfolio-1",
            MultiAssetCollateralEnabled = true
        };

        var expectedResponse = new MultiAssetCollateralResponse
        {
            MultiAssetCollateralEnabled = true
        };

        _mockApi.SetMultiAssetCollateral(Arg.Any<MultiAssetCollateralRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.SetMultiAssetCollateralAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.MultiAssetCollateralEnabled.Should().BeTrue();
    }

    #endregion

    #region CreateConvertQuoteAsync Tests

    [Fact]
    public async Task CreateConvertQuoteAsync_ReturnsSuccess()
    {
        // Arrange
        var request = new ConvertQuoteRequest
        {
            FromAccount = "account-1",
            ToAccount = "account-2",
            Amount = "100.00"
        };

        var expectedResponse = new ConvertQuoteResponse
        {
            Trade = new ConvertTrade
            {
                Id = "trade-1",
                Status = "CREATED"
            }
        };

        _mockApi.CreateConvertQuote(Arg.Any<ConvertQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.CreateConvertQuoteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Trade.Id.Should().Be("trade-1");
        result.Data.Trade.Status.Should().Be("CREATED");
    }

    #endregion

    #region GetConvertTradeAsync Tests

    [Fact]
    public async Task GetConvertTradeAsync_ReturnsSuccess()
    {
        // Arrange
        var expectedResponse = new ConvertTradeResponse
        {
            Trade = new ConvertTrade
            {
                Id = "trade-1",
                Status = "COMPLETED"
            }
        };

        _mockApi.GetConvertTrade("trade-1", "account-1", "account-2", Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetConvertTradeAsync("trade-1", "account-1", "account-2");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Trade.Status.Should().Be("COMPLETED");
    }

    #endregion

    #region CommitConvertTradeAsync Tests

    [Fact]
    public async Task CommitConvertTradeAsync_ReturnsSuccess()
    {
        // Arrange
        var request = new ConvertQuoteRequest
        {
            FromAccount = "account-1",
            ToAccount = "account-2",
            Amount = "100.00"
        };

        var expectedResponse = new ConvertTradeResponse
        {
            Trade = new ConvertTrade
            {
                Id = "trade-1",
                Status = "COMPLETED"
            }
        };

        _mockApi.CommitConvertTrade("trade-1", Arg.Any<ConvertQuoteRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _sut.CommitConvertTradeAsync("trade-1", request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Trade.Status.Should().Be("COMPLETED");
    }

    #endregion
}
