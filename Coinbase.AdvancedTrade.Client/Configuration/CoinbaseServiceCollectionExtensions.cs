using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Authentication;
using Coinbase.AdvancedTrade.Client.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Refit;

namespace Coinbase.AdvancedTrade.Client.Configuration;

public static class CoinbaseServiceCollectionExtensions
{
    /// <summary>
    /// Adds Coinbase Advanced Trade client services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoinbaseAdvancedTradeClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure settings
        services.Configure<CoinbaseSettings>(configuration.GetSection("Coinbase"));
        
        // Also register the settings directly for easier access with validation
        services.AddSingleton<CoinbaseSettings>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CoinbaseSettings>>();
            var settings = options.Value;
            
            // Validate configuration - API credentials are required for production use
            if (string.IsNullOrEmpty(settings.ApiKey) || string.IsNullOrEmpty(settings.ApiSecret))
            {
                throw new InvalidOperationException("Coinbase API credentials (ApiKey and ApiSecret) are required");
            }
            
            return settings;
        });

        // Register core services
        services.AddScoped<ICoinbaseJwtGenerator, CoinbaseJwtGenerator>();
        services.AddScoped<IAuthenticatedClientFactory, CoinbaseAuthenticatedClientFactory>();
        services.AddScoped<ICoinbaseCredentialValidator, CoinbaseCredentialValidator>();
        
        // Register authenticator for HTTP message handler scenarios
        services.AddScoped<CoinbaseAuthenticator>(sp =>
        {
            var jwtGenerator = sp.GetRequiredService<ICoinbaseJwtGenerator>();
            var settings = sp.GetRequiredService<CoinbaseSettings>();
            // Note: For integration tests, API credentials might be test values
            return new CoinbaseAuthenticator(jwtGenerator, settings.ApiKey ?? "", settings.ApiSecret ?? "", settings);
        });

        // Register Coinbase API client with Refit
        services.AddRefitClient<ICoinbaseApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var settings = sp.GetRequiredService<CoinbaseSettings>(); // This will trigger validation
                client.BaseAddress = new Uri(settings.GetActiveBaseUrl());
            });
            
        // Register named HttpClient for tests
        services.AddHttpClient("CoinbaseApi", (sp, client) =>
        {
            var settings = sp.GetRequiredService<CoinbaseSettings>();
            client.BaseAddress = new Uri(settings.GetActiveBaseUrl());
        });

        // Register high-level client
        services.AddScoped<ICoinbaseAdvancedTradeClient, CoinbaseAdvancedTradeClient>();

        return services;
    }

    /// <summary>
    /// Adds Coinbase Advanced Trade client services with explicit settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="settings">The Coinbase settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoinbaseAdvancedTradeClient(
        this IServiceCollection services,
        CoinbaseSettings settings)
    {
        // Configure settings
        services.AddSingleton(settings);

        // Register core services
        services.AddScoped<ICoinbaseJwtGenerator, CoinbaseJwtGenerator>();
        services.AddScoped<IAuthenticatedClientFactory, CoinbaseAuthenticatedClientFactory>();
        services.AddScoped<ICoinbaseCredentialValidator, CoinbaseCredentialValidator>();
        
        // Register authenticator for HTTP message handler scenarios
        services.AddScoped<CoinbaseAuthenticator>(sp =>
        {
            var jwtGenerator = sp.GetRequiredService<ICoinbaseJwtGenerator>();
            // Use the passed-in settings directly
            return new CoinbaseAuthenticator(jwtGenerator, settings.ApiKey ?? "", settings.ApiSecret ?? "", settings);
        });

        // Register Coinbase API client with Refit
        services.AddRefitClient<ICoinbaseApi>()
            .ConfigureHttpClient(client =>
            {
                client.BaseAddress = new Uri(settings.GetActiveBaseUrl());
            });
            
        // Register named HttpClient for tests
        services.AddHttpClient("CoinbaseApi", client =>
        {
            client.BaseAddress = new Uri(settings.GetActiveBaseUrl());
        });

        // Register high-level client
        services.AddScoped<ICoinbaseAdvancedTradeClient, CoinbaseAdvancedTradeClient>();

        return services;
    }

    /// <summary>
    /// Adds Coinbase Advanced Trade client services with configuration action
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the settings</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoinbaseAdvancedTradeClient(
        this IServiceCollection services,
        Action<CoinbaseSettings> configureOptions)
    {
        var settings = new CoinbaseSettings();
        configureOptions(settings);

        return services.AddCoinbaseAdvancedTradeClient(settings);
    }
}