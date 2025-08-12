# Coinbase.AdvancedTrade.Client

A comprehensive .NET client library for the Coinbase Advanced Trade API. This package provides a complete, production-ready solution with JWT authentication, built-in resilience patterns, strongly-typed models, and dependency injection support.

## Features

- ✅ **Complete API Coverage**: All major Coinbase Advanced Trade endpoints
- ✅ **JWT Authentication**: Secure ES256 JWT token generation with automatic header management
- ✅ **Resilience Patterns**: Built-in retry policies and circuit breaker patterns using Polly
- ✅ **Strongly Typed**: Comprehensive models for all API requests and responses
- ✅ **Dependency Injection**: Easy integration with .NET DI container
- ✅ **Configuration Support**: Supports both appsettings.json and programmatic configuration
- ✅ **Sandbox Support**: Easy switching between production and sandbox environments
- ✅ **Async/Await**: Full async/await support with cancellation tokens

## Installation

```bash
dotnet add package Coinbase.AdvancedTrade.Client --prerelease
```

## Quick Start

### 1. Configuration

Add your Coinbase credentials to `appsettings.json`:

```json
{
  "Coinbase": {
    "ApiKey": "your-api-key",
    "ApiSecret": "your-private-key",
    "UseSandbox": true,
    "MaxRetryAttempts": 3,
    "EnableCircuitBreaker": true
  }
}
```

### 2. Service Registration

Register the client in your DI container:

```csharp
using Coinbase.AdvancedTrade.Client.Extensions;

// From configuration
services.AddCoinbaseAdvancedTradeClient(configuration);

// Or programmatically
services.AddCoinbaseAdvancedTradeClient(options =>
{
    options.ApiKey = "your-api-key";
    options.ApiSecret = "your-private-key";
    options.UseSandbox = true;
});
```

### 3. Usage

Inject and use the client:

```csharp
using Coinbase.AdvancedTrade.Client.Client;

public class TradingService
{
    private readonly ICoinbaseAdvancedTradeClient _client;

    public TradingService(ICoinbaseAdvancedTradeClient client)
    {
        _client = client;
    }

    public async Task<decimal> GetBitcoinPriceAsync()
    {
        var products = await _client.ListProductsAsync();
        var btc = products.Products.FirstOrDefault(p => p.ProductId == "BTC-USD");
        return decimal.Parse(btc?.Price ?? "0");
    }

    public async Task PlaceOrderAsync()
    {
        var order = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc
                {
                    QuoteSize = "10.00" // $10 worth of BTC
                }
            }
        };

        var result = await _client.PlaceOrderAsync(order);
        if (result.Success)
        {
            Console.WriteLine($"Order placed: {result.SuccessResponse.OrderId}");
        }
    }
}
```

## Available Methods

### Account Management
- `ListAccountsAsync()` - List all accounts
- `GetAccountAsync(Guid id)` - Get specific account details

### Market Data
- `ListProductsAsync()` - List all available trading pairs
- `GetProductAsync(string productId)` - Get details for specific product
- `GetBestBidAskAsync(List<string>? productIds)` - Get best bid/ask prices

### Order Management
- `PlaceOrderAsync(OrderRequest request)` - Place a new order

## Configuration Options

| Property | Description | Default |
|----------|-------------|---------|
| `ApiKey` | Your Coinbase API key | Required |
| `ApiSecret` | Your private key (PEM or base64) | Required |
| `UseSandbox` | Use sandbox environment | `false` |
| `BaseUrl` | Production API URL | `https://api.coinbase.com/api/v3/brokerage` |
| `SandboxBaseUrl` | Sandbox API URL | `https://api-sandbox.coinbase.com/api/v3/brokerage` |
| `Timeout` | HTTP request timeout | `30 seconds` |
| `MaxRetryAttempts` | Number of retry attempts | `3` |
| `EnableCircuitBreaker` | Enable circuit breaker pattern | `true` |
| `CircuitBreakerFailuresBeforeBreaking` | Failures before breaking | `5` |
| `CircuitBreakerDurationOfBreak` | Break duration | `2 minutes` |

## Order Types Supported

- **Market Orders**: `MarketMarketIoc`
- **Limit Orders**: `LimitLimitGtc`, `LimitLimitGtd`, `LimitLimitFok`
- **Stop Orders**: `StopLimitStopLimitGtc`, `StopLimitStopLimitGtd`
- **Bracket Orders**: `TriggerBracketGtc`, `TriggerBracketGtd`
- **Smart Order Routing**: `SorLimitIoc`

## Error Handling

The client throws `CoinbaseApiException` for API-related errors:

```csharp
try
{
    var accounts = await client.ListAccountsAsync();
}
catch (CoinbaseApiException ex)
{
    Console.WriteLine($"API Error: {ex.StatusCode} - {ex.Message}");
    Console.WriteLine($"Details: {ex.ErrorDetails}");
}
```

## Resilience Features

### Retry Policy
- Automatic retries for transient failures
- Exponential backoff strategy
- Configurable retry attempts

### Circuit Breaker
- Protects against cascading failures
- Configurable failure threshold
- Automatic recovery

### Rate Limiting
- Handles 429 (Too Many Requests) responses
- Automatic retry with appropriate delays

## Getting API Credentials

1. Log in to [Coinbase Advanced Trade](https://advanced.coinbase.com/)
2. Go to Settings → API Keys
3. Create a new API key with appropriate permissions
4. Download your private key and store it securely

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.

## Disclaimer

This library is not officially affiliated with Coinbase. Use at your own risk. Always test thoroughly in sandbox environment before using in production.