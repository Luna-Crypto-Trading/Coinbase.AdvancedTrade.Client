using System.Net;
using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Authentication;
using Coinbase.AdvancedTrade.Client.Configuration;
using Coinbase.AdvancedTrade.Client.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Coinbase.AdvancedTrade.Client.IntegrationTests;

public class ResilienceTests : IDisposable
{
    private readonly WireMockServer _server;
    private readonly ICoinbaseAdvancedTradeClient _client;
    private readonly IServiceProvider _serviceProvider;

    public ResilienceTests()
    {
        _server = WireMockServer.Start();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        // Configure settings to use WireMock server
        services.AddSingleton(new CoinbaseSettings
        {
            BaseUrl = _server.Urls[0]
        });

        // Mock JWT generator
        services.AddSingleton<ICoinbaseJwtGenerator>(new MockJwtGenerator());

        // Add authenticator
        services.AddTransient<CoinbaseAuthenticator>();

        // Configure HttpClient with our mock server
        services.AddHttpClient("CoinbaseApi", client =>
        {
            client.BaseAddress = new Uri(_server.Urls[0]);
        })
        .AddHttpMessageHandler<CoinbaseAuthenticator>();

        // Add Refit client
        services.AddRefitClient<ICoinbaseApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(_server.Urls[0]))
            .AddHttpMessageHandler<CoinbaseAuthenticator>();

        // Add the main client
        services.AddSingleton<ICoinbaseAdvancedTradeClient, CoinbaseAdvancedTradeClient>();

        _serviceProvider = services.BuildServiceProvider();
        _client = _serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
    }

    [Fact]
    public async Task PlaceOrder_WithRetryableError_RetriesAndSucceeds()
    {
        // Arrange
        var orderRequest = new OrderRequest
        {
            ClientOrderId = "test-order",
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc { QuoteSize = "100" }
            }
        };

        var successResponse = @"{
            ""success"": true,
            ""success_response"": {
                ""order_id"": ""success-after-retry"",
                ""product_id"": ""BTC-USD""
            }
        }";

        // First set up to fail twice then succeed
        _server
            .Given(Request.Create()
                .WithPath("/orders")
                .UsingPost())
            .InScenario("PlaceOrder")
            .WillSetStateTo("FirstAttempt")
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.TooManyRequests)
                .WithBody("Rate limit exceeded"));

        _server
            .Given(Request.Create()
                .WithPath("/orders")
                .UsingPost())
            .InScenario("PlaceOrder")
            .WhenStateIs("FirstAttempt")
            .WillSetStateTo("SecondAttempt")
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.TooManyRequests)
                .WithBody("Rate limit exceeded"));

        _server
            .Given(Request.Create()
                .WithPath("/orders")
                .UsingPost())
            .InScenario("PlaceOrder")
            .WhenStateIs("SecondAttempt")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(successResponse)
                .WithHeader("Content-Type", "application/json"));

        // Act
        var result = await _client.PlaceOrderAsync(orderRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.SuccessResponse.OrderId.Should().Be("success-after-retry");
        // Should have eventually succeeded after retries
    }

    [Fact]
    public async Task GetOrders_WithTransientFailure_RetriesSuccessfully()
    {
        // Arrange
        var successResponse = @"{
            ""orders"": [
                {
                    ""order_id"": ""test-order-1"",
                    ""product_id"": ""BTC-USD"",
                    ""status"": ""FILLED""
                }
            ],
            ""has_next"": false
        }";

        // First fail then succeed
        _server
            .Given(Request.Create()
                .WithPath("/orders/historical/batch")
                .UsingGet())
            .InScenario("GetOrders")
            .WillSetStateTo("FirstAttempt")
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.ServiceUnavailable)
                .WithBody("Service temporarily unavailable"));

        _server
            .Given(Request.Create()
                .WithPath("/orders/historical/batch")
                .UsingGet())
            .InScenario("GetOrders")
            .WhenStateIs("FirstAttempt")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(successResponse)
                .WithHeader("Content-Type", "application/json"));

        // Act
        var result = await _client.GetOrdersAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Orders.Should().HaveCount(1);
        // Should have eventually succeeded after retry
    }

    [Fact]
    public async Task ListProducts_WithSuccessfulResponse_ReturnsData()
    {
        // Arrange
        var successResponse = @"{
            ""products"": [
                {
                    ""product_id"": ""BTC-USD"",
                    ""base_currency"": ""BTC"",
                    ""quote_currency"": ""USD"",
                    ""price"": ""50000.00"",
                    ""status"": ""online""
                },
                {
                    ""product_id"": ""ETH-USD"",
                    ""base_currency"": ""ETH"",
                    ""quote_currency"": ""USD"",
                    ""price"": ""3000.00"",
                    ""status"": ""online""
                }
            ]
        }";

        _server
            .Given(Request.Create()
                .WithPath("/products")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(successResponse)
                .WithHeader("Content-Type", "application/json"));

        // Act
        var result = await _client.ListProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.Products.Should().HaveCount(2);
        result.Data.Products.Should().Contain(p => p.ProductId == "BTC-USD");
        result.Data.Products.Should().Contain(p => p.ProductId == "ETH-USD");
    }

    [Fact]
    public async Task GetBestBidAsk_WithValidProducts_ReturnsMarketData()
    {
        // Arrange
        var productIds = new List<string> { "BTC-USD" };
        var successResponse = @"{
            ""pricebooks"": [
                {
                    ""product_id"": ""BTC-USD"",
                    ""bids"": [
                        {
                            ""price"": ""49900.00"",
                            ""size"": ""0.5""
                        }
                    ],
                    ""asks"": [
                        {
                            ""price"": ""50100.00"",
                            ""size"": ""0.3""
                        }
                    ],
                    ""time"": ""2024-01-01T12:00:00Z""
                }
            ]
        }";

        _server
            .Given(Request.Create()
                .WithPath("/best_bid_ask")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(successResponse)
                .WithHeader("Content-Type", "application/json"));

        // Act
        var result = await _client.GetBestBidAskAsync(productIds);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.PriceBooks.Should().HaveCount(1);

        var pricebook = result.Data.PriceBooks.First();
        pricebook.ProductId.Should().Be("BTC-USD");
        pricebook.Bids.First().Price.Should().Be("49900.00");
        pricebook.Asks.First().Price.Should().Be("50100.00");
    }

    [Fact]
    public async Task CircuitBreaker_AfterMultipleFailures_OpensCircuit()
    {
        // Arrange
        _server
            .Given(Request.Create()
                .WithPath("/accounts")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.InternalServerError)
                .WithBody("Internal Server Error"));

        // Act - Trigger circuit breaker with 5 consecutive failures
        var tasks = new List<Task<ApiResponse<AccountsResponse>>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(_client.ListAccountsAsync());
        }

        var results = await Task.WhenAll(tasks);

        // All should fail
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeFalse());

        // Next call should fail immediately due to open circuit
        var finalResult = await _client.ListAccountsAsync();
        finalResult.IsSuccess.Should().BeFalse();
        finalResult.ErrorMessage.Should().Contain("currently unavailable");
    }

    [Fact]
    public async Task Authentication_WithUnauthorized_ReturnsAuthenticationError()
    {
        // Arrange
        _server
            .Given(Request.Create()
                .WithPath("/accounts/*")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.Unauthorized)
                .WithBody(@"{""error"": ""Invalid credentials""}"));

        // Act
        var result = await _client.GetAccountAsync(Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Authentication failed");
    }

    [Fact]
    public async Task RateLimiting_WithTooManyRequests_HandlesGracefully()
    {
        // Arrange
        var errorResponse = @"{
            ""error"": ""RATE_LIMIT_EXCEEDED"",
            ""message"": ""Too many requests""
        }";

        _server
            .Given(Request.Create()
                .WithPath("/orders")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode((int)HttpStatusCode.TooManyRequests)
                .WithBody(errorResponse)
                .WithHeader("Content-Type", "application/json"));

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

        // Act - Should retry 3 times then fail
        var result = await _client.PlaceOrderAsync(orderRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Rate limit exceeded");
    }

    public void Dispose()
    {
        _server?.Stop();
        _server?.Dispose();
    }

    private class MockJwtGenerator : ICoinbaseJwtGenerator
    {
        public string GenerateJwt(string uri, string apiKey, string apiSecret)
        {
            return "mock-jwt-token";
        }
    }
}