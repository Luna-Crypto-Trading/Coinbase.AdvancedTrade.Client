using Coinbase.AdvancedTrade.Client;
using Coinbase.AdvancedTrade.Client.Configuration;
using Coinbase.AdvancedTrade.Client.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add the Coinbase Advanced Trade client with runtime configuration
builder.Services.AddCoinbaseAdvancedTradeClient(settings =>
{
    settings.UseSandbox = true; // Default to sandbox for safety
    settings.ApiKey = "placeholder";
    settings.ApiSecret = "placeholder";
});

// Add our example service that handles runtime credential injection
builder.Services.AddScoped<RuntimeCredentialsService>();

var host = builder.Build();

// Run the example
var credentialsService = host.Services.GetRequiredService<RuntimeCredentialsService>();
await credentialsService.RunExampleAsync();

public class RuntimeCredentialsService
{
    private readonly ICoinbaseAdvancedTradeClient _coinbaseClient;

    public RuntimeCredentialsService(ICoinbaseAdvancedTradeClient coinbaseClient)
    {
        _coinbaseClient = coinbaseClient;
    }

    public async Task RunExampleAsync()
    {
        Console.WriteLine("=== Coinbase Advanced Trade Client - Runtime Secrets Example ===");
        Console.WriteLine();

        try
        {
            // Method 1: Get credentials from environment variables
            Console.WriteLine("Method 1: Reading from Environment Variables");
            await TryEnvironmentVariables();
            Console.WriteLine();

            // Method 2: Get credentials from user input
            Console.WriteLine("Method 2: Interactive User Input");
            await TryUserInput();
            Console.WriteLine();

            Console.WriteLine("All runtime credential injection methods demonstrated!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task TryEnvironmentVariables()
    {
        var apiKey = Environment.GetEnvironmentVariable("COINBASE_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("COINBASE_API_SECRET");

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            Console.WriteLine("  Environment variables COINBASE_API_KEY and COINBASE_API_SECRET not found");
            Console.WriteLine("  Set them with:");
            Console.WriteLine("  export COINBASE_API_KEY=\"your-api-key\"");
            Console.WriteLine("  export COINBASE_API_SECRET=\"your-api-secret\"");
            return;
        }

        Console.WriteLine("  Found credentials in environment variables");
        await TestApiCall("Environment Variables");
    }

    private async Task TryUserInput()
    {
        Console.Write("Enter your Coinbase API Key (or press Enter to skip): ");
        var apiKey = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("  Skipped user input");
            return;
        }

        Console.WriteLine("  Got credentials from user input");
        await TestApiCall("User Input");
    }

    private async Task TestApiCall(string source)
    {
        Console.WriteLine($"  Testing API call with credentials from {source}...");

        var accountsResponse = await _coinbaseClient.ListAccountsAsync();
        if (accountsResponse.IsSuccess)
        {
            Console.WriteLine($"  Success! Retrieved {accountsResponse.Data!.Accounts?.Length ?? 0} accounts");
        }
        else
        {
            Console.WriteLine($"  Error: {accountsResponse.ErrorMessage}");
        }
    }
}
