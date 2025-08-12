using Coinbase.AdvancedTrade.Client;
using Coinbase.AdvancedTrade.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure Coinbase settings from configuration
builder.Services.Configure<CoinbaseSettings>(
    builder.Configuration.GetSection("Coinbase"));

// Add the Coinbase Advanced Trade client to DI container
builder.Services.AddCoinbaseAdvancedTradeClient();

// Add our example service
builder.Services.AddScoped<ExampleService>();

var host = builder.Build();

// Run the example
var exampleService = host.Services.GetRequiredService<ExampleService>();
await exampleService.RunExampleAsync();

public class ExampleService
{
    private readonly ICoinbaseAdvancedTradeClient _coinbaseClient;

    public ExampleService(ICoinbaseAdvancedTradeClient coinbaseClient)
    {
        _coinbaseClient = coinbaseClient;
    }

    public async Task RunExampleAsync()
    {
        try
        {
            Console.WriteLine("=== Coinbase Advanced Trade Client - Basic DI Example ===");
            Console.WriteLine();

            // Example 1: Get all accounts
            Console.WriteLine("1. Fetching accounts...");
            var accounts = await _coinbaseClient.GetAccountsAsync();
            Console.WriteLine($"Found {accounts.Accounts?.Length ?? 0} accounts");
            
            if (accounts.Accounts?.Any() == true)
            {
                foreach (var account in accounts.Accounts.Take(3))
                {
                    Console.WriteLine($"  - {account.Name}: {account.AvailableBalance?.Value} {account.AvailableBalance?.Currency}");
                }
            }
            Console.WriteLine();

            // Example 2: Get products (trading pairs)
            Console.WriteLine("2. Fetching first 5 products...");
            var products = await _coinbaseClient.GetProductsAsync(limit: 5);
            Console.WriteLine($"Found {products.Products?.Length ?? 0} products");
            
            if (products.Products?.Any() == true)
            {
                foreach (var product in products.Products)
                {
                    Console.WriteLine($"  - {product.ProductId}: {product.DisplayName} (Status: {product.Status})");
                }
            }
            Console.WriteLine();

            // Example 3: Get portfolio breakdown
            Console.WriteLine("3. Fetching portfolio breakdown...");
            var portfolio = await _coinbaseClient.GetPortfolioBreakdownAsync();
            Console.WriteLine($"Portfolio breakdown:");
            Console.WriteLine($"  - Total balance: {portfolio.Breakdown?.TotalBalance?.Value} {portfolio.Breakdown?.TotalBalance?.Currency}");
            
            if (portfolio.Breakdown?.SpotPositions?.Any() == true)
            {
                Console.WriteLine("  - Top holdings:");
                foreach (var position in portfolio.Breakdown.SpotPositions.Take(3))
                {
                    Console.WriteLine($"    * {position.Asset}: {position.TotalBalance?.Value} (${position.AssetImgUrl})");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Example completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure your API credentials are configured in appsettings.json");
        }
    }
}