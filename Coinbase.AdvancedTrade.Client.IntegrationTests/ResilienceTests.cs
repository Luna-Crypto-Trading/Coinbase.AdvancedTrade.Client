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

        // Add mock API credentials
        var mockApiKey = "test-api-key";
        var mockApiSecret = "test-api-secret";

        // Create authenticator factory
        services.AddSingleton<IAuthenticatedClientFactory>(provider =>
        {
            var jwtGenerator = provider.GetRequiredService<ICoinbaseJwtGenerator>();
            var settings = provider.GetRequiredService<CoinbaseSettings>();
            return new MockAuthenticatedClientFactory(jwtGenerator, settings, mockApiKey, mockApiSecret);
        });

        // Add Refit client without authentication (we'll handle it in the factory)
        services.AddRefitClient<ICoinbaseApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(_server.Urls[0]));

        // Add the main client
        services.AddTransient<ICoinbaseAdvancedTradeClient, CoinbaseAdvancedTradeClient>();

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
        result.Data!.SuccessResponse!.OrderId.Should().Be("success-after-retry");
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
                    ""user_id"": ""test-user-123"",
                    ""order_configuration"": {
                        ""market_market_ioc"": {
                            ""quote_size"": ""5000.00""
                        }
                    },
                    ""side"": ""BUY"",
                    ""client_order_id"": ""client-order-123"",
                    ""status"": ""FILLED"",
                    ""time_in_force"": ""IMMEDIATE_OR_CANCEL"",
                    ""created_time"": ""2024-01-01T00:00:00Z"",
                    ""completion_percentage"": ""100"",
                    ""filled_size"": ""0.1"",
                    ""average_filled_price"": ""50000.00"",
                    ""number_of_fills"": ""1""
                }
            ],
            ""has_next"": false,
            ""cursor"": """",
            ""order"": ""DESC""
        }";

        // First fail with 503 (which should trigger a retry)
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
                    ""status"": ""online"",
                    ""price_percentage_change_24h"": ""5.0"",
                    ""volume_24h"": ""1000000"",
                    ""volume_percentage_change_24h"": ""10.0"",
                    ""base_increment"": ""0.00000001"",
                    ""quote_increment"": ""0.01"",
                    ""quote_min_size"": ""10"",
                    ""quote_max_size"": ""1000000"",
                    ""base_min_size"": ""0.001"",
                    ""base_max_size"": ""100"",
                    ""base_currency_id"": ""BTC"",
                    ""quote_currency_id"": ""USD"",
                    ""display_name"": ""BTC-USD"",
                    ""is_disabled"": false,
                    ""new"": false,
                    ""auction_mode"": false
                },
                {
                    ""product_id"": ""ETH-USD"",
                    ""base_currency"": ""ETH"",
                    ""quote_currency"": ""USD"",
                    ""price"": ""3000.00"",
                    ""status"": ""online"",
                    ""price_percentage_change_24h"": ""3.0"",
                    ""volume_24h"": ""500000"",
                    ""volume_percentage_change_24h"": ""7.0"",
                    ""base_increment"": ""0.00000001"",
                    ""quote_increment"": ""0.01"",
                    ""quote_min_size"": ""10"",
                    ""quote_max_size"": ""1000000"",
                    ""base_min_size"": ""0.01"",
                    ""base_max_size"": ""1000"",
                    ""base_currency_id"": ""ETH"",
                    ""quote_currency_id"": ""USD"",
                    ""display_name"": ""ETH-USD"",
                    ""is_disabled"": false,
                    ""new"": false,
                    ""auction_mode"": false
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

    private class MockAuthenticatedClientFactory : IAuthenticatedClientFactory
    {
        private readonly ICoinbaseJwtGenerator _jwtGenerator;
        private readonly CoinbaseSettings _settings;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public MockAuthenticatedClientFactory(
            ICoinbaseJwtGenerator jwtGenerator,
            CoinbaseSettings settings,
            string apiKey,
            string apiSecret)
        {
            _jwtGenerator = jwtGenerator;
            _settings = settings;
            _apiKey = apiKey;
            _apiSecret = apiSecret;
        }

        public HttpClient CreateAuthenticatedClient(string baseUrl, string apiKey, string apiSecret)
        {
            var innerHandler = new HttpClientHandler();
            var authenticator = new CoinbaseAuthenticator(_jwtGenerator, _apiKey, _apiSecret, _settings)
            {
                InnerHandler = innerHandler
            };
            
            return new HttpClient(authenticator) { BaseAddress = new Uri(baseUrl) };
        }
    }
}