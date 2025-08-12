# Coinbase Advanced Trade Client Examples

This directory contains practical examples demonstrating how to integrate and use the Coinbase Advanced Trade client library in different scenarios.

## Examples Overview

### 1. [Basic Dependency Injection](./BasicDI/) 
**Perfect for: Simple applications with static API credentials**

- Configuration-based setup using `appsettings.json`
- Standard DI registration with `AddCoinbaseAdvancedTradeClient()`
- Basic API operations (accounts, products, portfolio)
- Error handling and logging integration

**Run it:**
```bash
cd BasicDI
# Update appsettings.json with your API credentials
dotnet run
```

### 2. [Runtime Secrets Injection](./RuntimeSecrets/)
**Perfect for: Production applications requiring secure credential management**

- Multiple credential injection methods
- Environment variables, user input, external key vaults
- Credential validation before use
- Security best practices demonstration

**Run it:**
```bash
cd RuntimeSecrets
# Set environment variables or use interactive input
dotnet run
```

## Quick Start

1. **Get your Coinbase API credentials:**
   - Visit [Coinbase Advanced Trade](https://advanced.coinbase.com/)
   - Go to Settings → API → Create New API Key
   - Grant necessary permissions (view accounts, trade, etc.)
   - Copy your API Key and Secret

2. **Choose an example based on your needs:**
   - Use **BasicDI** for development/testing with config files
   - Use **RuntimeSecrets** for production with secure credential management

3. **Configure and run:**
   ```bash
   cd Examples/[ExampleName]
   dotnet run
   ```

## Production Considerations

### Security 🔐
- **Never commit API secrets** to version control
- Use secure secret management (Azure Key Vault, AWS Secrets Manager, etc.)
- Implement credential rotation strategies
- Use least-privilege API permissions

### Resilience 🛡️
- The client includes built-in retry policies with exponential backoff
- Circuit breaker patterns prevent cascade failures
- Proper error handling and logging throughout

### Performance ⚡
- HTTP client pooling and connection reuse
- Async/await throughout for non-blocking operations
- Efficient JSON serialization with System.Text.Json

### Monitoring 📊
- Structured logging integration
- HTTP client telemetry support
- Custom metrics and health checks capability

## Available Operations

The client supports all major Coinbase Advanced Trade endpoints:

| Category | Operations | Example Usage |
|----------|------------|---------------|
| **Accounts** | List accounts, get account details | `GetAccountsAsync()`, `GetAccountAsync(uuid)` |
| **Products** | List trading pairs, get product details | `GetProductsAsync()`, `GetProductAsync(productId)` |
| **Orders** | Create, cancel, list orders | `PlaceOrderAsync()`, `CancelOrderAsync()`, `GetOrdersAsync()` |
| **Portfolio** | Get portfolio breakdown, positions | `GetPortfolioBreakdownAsync()` |
| **Market Data** | Get candles, best bid/ask | `GetProductCandlesAsync()`, `GetBestBidAskAsync()` |

## Common Patterns

### Order Placement with Fluent Builder
```csharp
var orderRequest = OrderRequestBuilder
    .Buy("BTC-USD")
    .LimitOrder(baseSize: 0.01m, limitPrice: 50000m, postOnly: true)
    .WithClientOrderId("my-unique-id-123")
    .Build();

var result = await client.PlaceOrderAsync(orderRequest);
```

### Error Handling
```csharp
try
{
    var accounts = await client.GetAccountsAsync();
    // Process accounts...
}
catch (HttpRequestException ex) when (ex.Message.Contains("401"))
{
    // Handle authentication errors
    logger.LogError("Invalid API credentials: {Error}", ex.Message);
}
catch (HttpRequestException ex) when (ex.Message.Contains("429"))
{
    // Handle rate limiting
    logger.LogWarning("Rate limited, retrying...");
    await Task.Delay(TimeSpan.FromSeconds(60));
}
```

### Dependency Injection Setup
```csharp
// Startup.cs or Program.cs
services.Configure<CoinbaseSettings>(configuration.GetSection("Coinbase"));
services.AddCoinbaseAdvancedTradeClient();

// Your service
public class TradingService
{
    private readonly ICoinbaseAdvancedTradeClient _client;
    
    public TradingService(ICoinbaseAdvancedTradeClient client)
    {
        _client = client;
    }
}
```

## Need Help?

- Check the [main README](../README.md) for installation and basic setup
- Review the client library source code for advanced usage
- Coinbase API documentation: https://docs.cloud.coinbase.com/advanced-trade-api/docs
- Report issues: [GitHub Issues](https://github.com/Luna-Crypto-Trading/Coinbase.AdvancedTrade.Client/issues)