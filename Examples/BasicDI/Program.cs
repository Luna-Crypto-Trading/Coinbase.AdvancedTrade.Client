using Coinbase.AdvancedTrade.Client;
using Coinbase.AdvancedTrade.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add the Coinbase Advanced Trade client to DI container
builder.Services.AddCoinbaseAdvancedTradeClient(builder.Configuration);

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
            var accountsResponse = await _coinbaseClient.ListAccountsAsync();
            if (accountsResponse.IsSuccess)
            {
                var accounts = accountsResponse.Data!;
                Console.WriteLine($"Found {accounts.Accounts?.Length ?? 0} accounts");

                if (accounts.Accounts?.Any() == true)
                {
                    foreach (var account in accounts.Accounts.Take(3))
                    {
                        Console.WriteLine($"  - {account.Name}: {account.AvailableBalance?.Value} {account.AvailableBalance?.Currency}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: {accountsResponse.ErrorMessage}");
            }
            Console.WriteLine();

            // Example 2: Get products (trading pairs)
            Console.WriteLine("2. Fetching products...");
            var productsResponse = await _coinbaseClient.ListProductsAsync();
            if (productsResponse.IsSuccess)
            {
                var products = productsResponse.Data!;
                Console.WriteLine($"Found {products.Products?.Length ?? 0} products");

                if (products.Products?.Any() == true)
                {
                    foreach (var product in products.Products.Take(5))
                    {
                        Console.WriteLine($"  - {product.ProductId}: {product.DisplayName} (Status: {product.Status})");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: {productsResponse.ErrorMessage}");
            }
            Console.WriteLine();

            // Example 3: Get portfolios and breakdown
            Console.WriteLine("3. Fetching portfolios...");
            var portfoliosResponse = await _coinbaseClient.GetPortfoliosAsync();
            if (portfoliosResponse.IsSuccess && portfoliosResponse.Data!.Portfolios?.Any() == true)
            {
                var firstPortfolio = portfoliosResponse.Data.Portfolios.First();
                Console.WriteLine($"Portfolio: {firstPortfolio.Name} ({firstPortfolio.Uuid})");

                var breakdownResponse = await _coinbaseClient.GetPortfolioBreakdownAsync(firstPortfolio.Uuid);
                if (breakdownResponse.IsSuccess)
                {
                    var breakdown = breakdownResponse.Data!;
                    Console.WriteLine($"  - Total balance: {breakdown.Breakdown?.TotalBalance?.Value} {breakdown.Breakdown?.TotalBalance?.Currency}");
                }
            }
            else
            {
                Console.WriteLine($"Error: {portfoliosResponse.ErrorMessage}");
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
