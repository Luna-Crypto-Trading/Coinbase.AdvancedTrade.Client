# Basic Dependency Injection Example

This example demonstrates the simplest way to integrate the Coinbase Advanced Trade client into your .NET application using dependency injection.

## Setup

1. **Configure your API credentials** in `appsettings.json`:
   ```json
   {
     "Coinbase": {
       "ApiKey": "your-coinbase-api-key-here",
       "ApiSecret": "your-coinbase-api-secret-here", 
       "Sandbox": true
     }
   }
   ```

2. **Get your Coinbase API credentials**:
   - Go to [Coinbase Advanced Trade](https://advanced.coinbase.com/)
   - Navigate to Settings → API
   - Create a new API key with appropriate permissions
   - Copy the API key and secret

## Running the Example

```bash
cd Examples/BasicDI
dotnet run
```

## Key Features Demonstrated

- **Configuration binding**: API credentials loaded from `appsettings.json`
- **Dependency injection**: Client automatically registered with `AddCoinbaseAdvancedTradeClient()`
- **Service integration**: Client injected into your services via constructor
- **Error handling**: Graceful handling of API errors
- **Multiple operations**: Accounts, products, and portfolio data

## Code Breakdown

### Program.cs Setup
```csharp
// Configure Coinbase settings from configuration
builder.Services.Configure<CoinbaseSettings>(
    builder.Configuration.GetSection("Coinbase"));

// Add the Coinbase Advanced Trade client to DI container  
builder.Services.AddCoinbaseAdvancedTradeClient();
```

### Service Usage
```csharp
public class ExampleService
{
    private readonly ICoinbaseAdvancedTradeClient _coinbaseClient;

    public ExampleService(ICoinbaseAdvancedTradeClient coinbaseClient)
    {
        _coinbaseClient = coinbaseClient;
    }

    public async Task RunExampleAsync()
    {
        var accounts = await _coinbaseClient.GetAccountsAsync();
        // Use the data...
    }
}
```

This pattern works great when your API credentials are static and can be stored in configuration files.