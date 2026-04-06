# Coinbase.AdvancedTrade.Client

A comprehensive .NET client library for the Coinbase Advanced Trade API. This package provides a complete, production-ready solution with JWT authentication, built-in resilience patterns, strongly-typed models, and dependency injection support.

## Features

- **JWT Authentication**: Secure ES256 JWT token generation with automatic header management
- **Resilience Patterns**: Built-in retry policies and circuit breaker patterns using Polly
- **Strongly Typed**: Comprehensive models for all API requests and responses
- **Dependency Injection**: Easy integration with .NET DI container
- **Configuration Support**: Supports both appsettings.json and programmatic configuration
- **Sandbox Support**: Easy switching between production and sandbox environments
- **Async/Await**: Full async/await support with cancellation tokens

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
    "UseSandbox": true
  }
}
```

### 2. Service Registration

Register the client in your DI container:

```csharp
using Coinbase.AdvancedTrade.Client.Configuration;

// From configuration
services.AddCoinbaseAdvancedTradeClient(configuration);

// Or programmatically
services.AddCoinbaseAdvancedTradeClient(settings =>
{
    settings.ApiKey = "your-api-key";
    settings.ApiSecret = "your-private-key";
    settings.UseSandbox = true;
});
```

### 3. Usage

Inject and use the client:

```csharp
using Coinbase.AdvancedTrade.Client;
using Coinbase.AdvancedTrade.Client.Models;

public class TradingService
{
    private readonly ICoinbaseAdvancedTradeClient _client;

    public TradingService(ICoinbaseAdvancedTradeClient client)
    {
        _client = client;
    }

    public async Task<decimal> GetBitcoinPriceAsync()
    {
        var response = await _client.ListProductsAsync();
        if (!response.IsSuccess) return 0;

        var btc = response.Data!.Products?.FirstOrDefault(p => p.ProductId == "BTC-USD");
        return decimal.Parse(btc?.Price ?? "0");
    }

    public async Task PlaceOrderAsync()
    {
        var order = new OrderRequest
        {
            ClientOrderId = Guid.NewGuid().ToString(),
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc
                {
                    QuoteSize = "10.00" // $10 worth of BTC
                }
            }
        };

        var result = await _client.PlaceOrderAsync(order);
        if (result.IsSuccess && result.Data!.Success)
        {
            Console.WriteLine($"Order placed: {result.Data.SuccessResponse?.OrderId}");
        }
        else
        {
            Console.WriteLine($"Order failed: {result.ErrorMessage ?? result.Data?.ErrorResponse?.Message}");
        }
    }
}
```

## Available Methods

### Account Management
- `ListAccountsAsync()` - List all accounts
- `GetAccountAsync(Guid id)` - Get specific account details

### Order Management
- `PlaceOrderAsync(OrderRequest request)` - Place a new order
- `CancelOrdersAsync(List<string> orderIds)` - Cancel one or more orders
- `GetOrdersAsync(OrderSearchRequest? request)` - Search historical orders with filters
- `GetOrderAsync(string orderId)` - Get a specific order by ID
- `ClosePositionAsync(ClosePositionRequest request)` - Close a futures/perp position

### Market Data
- `ListProductsAsync()` - List all available trading pairs
- `GetProductAsync(string productId)` - Get details for a specific product
- `GetBestBidAskAsync(List<string>? productIds)` - Get best bid/ask prices
- `GetProductCandlesAsync(string productId, long start, long end, string granularity)` - Get OHLCV candlestick data

### Portfolio Management
- `GetPortfoliosAsync()` - List all portfolios
- `GetPortfolioBreakdownAsync(string portfolioUuid)` - Get detailed portfolio breakdown

All methods return `ApiResponse<T>` with `IsSuccess`, `Data`, `ErrorMessage`, and `Exception` properties.

## Configuration Options

| Property | Description | Default |
|----------|-------------|---------|
| `ApiKey` | Your Coinbase API key | Required |
| `ApiSecret` | Your private key (PEM or base64) | Required |
| `UseSandbox` | Use sandbox environment | `false` |
| `BaseUrl` | Production API URL | `https://api.coinbase.com/api/v3/brokerage` |
| `SandboxBaseUrl` | Sandbox API URL | `https://api-sandbox.coinbase.com/api/v3/brokerage` |

## Order Types Supported

- **Market Orders**: `MarketMarketIoc`
- **Limit Orders**: `LimitLimitGtcV3`, `LimitLimitGtdV3`, `LimitLimitFokV3`
- **Stop Orders**: `StopLimitStopLimitGtcV3`, `StopLimitStopLimitGtdV3`
- **Bracket Orders**: `TriggerBracketGtcV3`, `TriggerBracketGtdV3`
- **Smart Order Routing**: `SorLimitIoc`

## Error Handling

All methods return `ApiResponse<T>` instead of throwing exceptions:

```csharp
var response = await client.ListAccountsAsync();
if (response.IsSuccess)
{
    var accounts = response.Data!;
    // Use accounts...
}
else
{
    Console.WriteLine($"Error: {response.ErrorMessage}");
    // response.Exception contains the underlying exception if needed
}
```

## Resilience Features

### Retry Policy
- Automatic retries for transient failures (429, 502, 503, 504, 408)
- Exponential backoff strategy (2^n seconds)
- 3 retry attempts

### Circuit Breaker
- Opens after 5 consecutive failures
- 2-minute break duration
- Automatic recovery

## Getting API Credentials

1. Log in to [Coinbase Advanced Trade](https://advanced.coinbase.com/)
2. Go to Settings > API Keys
3. Create a new API key with appropriate permissions
4. Download your private key and store it securely

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License.

## Disclaimer

This library is not officially affiliated with Coinbase. Use at your own risk. Always test thoroughly in sandbox environment before using in production.
