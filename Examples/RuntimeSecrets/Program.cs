using Coinbase.AdvancedTrade.Client;
using Coinbase.AdvancedTrade.Client.Configuration;
using Coinbase.AdvancedTrade.Client.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Configure empty Coinbase settings initially - we'll update at runtime
builder.Services.Configure<CoinbaseSettings>(settings =>
{
    settings.Sandbox = true; // Default to sandbox for safety
});

// Add the Coinbase Advanced Trade client to DI container
builder.Services.AddCoinbaseAdvancedTradeClient();

// Add our example service that handles runtime credential injection
builder.Services.AddScoped<RuntimeCredentialsService>();

var host = builder.Build();

// Run the example
var credentialsService = host.Services.GetRequiredService<RuntimeCredentialsService>();
await credentialsService.RunExampleAsync();

public class RuntimeCredentialsService
{
    private readonly ICoinbaseAdvancedTradeClient _coinbaseClient;
    private readonly IOptionsMonitor<CoinbaseSettings> _settingsMonitor;
    private readonly CoinbaseCredentialValidator _validator;

    public RuntimeCredentialsService(
        ICoinbaseAdvancedTradeClient coinbaseClient,
        IOptionsMonitor<CoinbaseSettings> settingsMonitor,
        CoinbaseCredentialValidator validator)
    {
        _coinbaseClient = coinbaseClient;
        _settingsMonitor = settingsMonitor;
        _validator = validator;
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

            // Method 3: Get credentials from external service (simulated)
            Console.WriteLine("Method 3: External Service/Key Vault (Simulated)");
            await TryExternalService();
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
            Console.WriteLine("❌ Environment variables COINBASE_API_KEY and COINBASE_API_SECRET not found");
            Console.WriteLine("   Set them with:");
            Console.WriteLine("   export COINBASE_API_KEY=\"your-api-key\"");
            Console.WriteLine("   export COINBASE_API_SECRET=\"your-api-secret\"");
            return;
        }

        Console.WriteLine("✅ Found credentials in environment variables");
        await UpdateSettingsAndTest(apiKey, apiSecret, "Environment Variables");
    }

    private async Task TryUserInput()
    {
        Console.Write("Enter your Coinbase API Key (or press Enter to skip): ");
        var apiKey = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("❌ Skipped user input");
            return;
        }

        Console.Write("Enter your Coinbase API Secret: ");
        var apiSecret = ReadPassword();

        Console.WriteLine("✅ Got credentials from user input");
        await UpdateSettingsAndTest(apiKey, apiSecret, "User Input");
    }

    private async Task TryExternalService()
    {
        Console.WriteLine("📡 Simulating retrieval from external key vault/service...");
        
        // Simulate calling an external service
        await Task.Delay(1000);
        
        var (apiKey, apiSecret) = await GetCredentialsFromExternalService();
        
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ External service returned no credentials");
            return;
        }

        Console.WriteLine("✅ Retrieved credentials from external service");
        await UpdateSettingsAndTest(apiKey, apiSecret, "External Service");
    }

    private async Task UpdateSettingsAndTest(string apiKey, string apiSecret, string source)
    {
        try
        {
            // Validate credentials first
            Console.WriteLine($"🔍 Validating credentials from {source}...");
            var validationResult = await _validator.ValidateCredentialsAsync(apiKey, apiSecret);
            
            if (!validationResult.IsValid)
            {
                Console.WriteLine($"❌ Credential validation failed: {validationResult.ErrorMessage}");
                return;
            }

            Console.WriteLine("✅ Credentials validated successfully");

            // Update settings at runtime
            Microsoft.Extensions.Options.OptionsMonitor<CoinbaseSettings>.ChangeToken = Microsoft.Extensions.Primitives.ChangeToken.FromCancellationToken(new CancellationToken());
            
            // For demonstration, we'll create a new settings object
            // In real apps, you might use IOptionsSnapshot or recreate the client
            var settings = new CoinbaseSettings
            {
                ApiKey = apiKey,
                ApiSecret = apiSecret,
                Sandbox = true
            };

            // Test the client with new credentials
            Console.WriteLine($"🚀 Testing API call with credentials from {source}...");
            
            // Create a new client instance with runtime settings
            using var serviceScope = CreateScopeWithSettings(settings);
            var client = serviceScope.ServiceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
            
            var accounts = await client.GetAccountsAsync();
            Console.WriteLine($"✅ Success! Retrieved {accounts.Accounts?.Length ?? 0} accounts");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error testing credentials from {source}: {ex.Message}");
        }
    }

    private IServiceScope CreateScopeWithSettings(CoinbaseSettings settings)
    {
        var services = new ServiceCollection();
        services.Configure<CoinbaseSettings>(_ => 
        {
            _.ApiKey = settings.ApiKey;
            _.ApiSecret = settings.ApiSecret; 
            _.Sandbox = settings.Sandbox;
        });
        services.AddCoinbaseAdvancedTradeClient();
        
        var provider = services.BuildServiceProvider();
        return provider.CreateScope();
    }

    private static string ReadPassword()
    {
        var password = "";
        ConsoleKeyInfo keyInfo;

        do
        {
            keyInfo = Console.ReadKey(true);
            if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace)
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
            else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        } while (keyInfo.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    private static async Task<(string apiKey, string apiSecret)> GetCredentialsFromExternalService()
    {
        // Simulate calling external service (Azure Key Vault, AWS Secrets Manager, etc.)
        await Task.Delay(500);
        
        // In a real implementation, this would call:
        // - Azure Key Vault: var secret = await keyVaultClient.GetSecretAsync("coinbase-api-key");
        // - AWS Secrets Manager: var secret = await secretsClient.GetSecretValueAsync(request);
        // - HashiCorp Vault: var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path);
        
        // For demo purposes, we'll try to read from environment variables with different names
        var apiKey = Environment.GetEnvironmentVariable("EXTERNAL_COINBASE_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("EXTERNAL_COINBASE_SECRET");
        
        return (apiKey ?? "", apiSecret ?? "");
    }
}