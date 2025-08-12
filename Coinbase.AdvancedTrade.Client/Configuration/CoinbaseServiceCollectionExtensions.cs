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

        // Register core services
        services.AddScoped<ICoinbaseJwtGenerator, CoinbaseJwtGenerator>();
        services.AddScoped<IAuthenticatedClientFactory, CoinbaseAuthenticatedClientFactory>();
        services.AddScoped<ICoinbaseCredentialValidator, CoinbaseCredentialValidator>();

        // Register Coinbase API client with Refit
        services.AddRefitClient<ICoinbaseApi>()
            .ConfigureHttpClient((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CoinbaseSettings>>().Value;
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

        // Register Coinbase API client with Refit
        services.AddRefitClient<ICoinbaseApi>()
            .ConfigureHttpClient(client =>
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