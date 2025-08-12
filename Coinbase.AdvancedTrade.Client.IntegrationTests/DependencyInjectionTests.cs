using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Authentication;
using Coinbase.AdvancedTrade.Client.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Coinbase.AdvancedTrade.Client.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddCoinbaseAdvancedTradeClient_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:ApiKey"] = "test-api-key",
                ["Coinbase:ApiSecret"] = "test-api-secret",
                ["Coinbase:BaseUrl"] = "https://api.coinbase.com/api/v3/brokerage"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Act
        services.AddCoinbaseAdvancedTradeClient(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ICoinbaseAdvancedTradeClient>()
            .Should().NotBeNull("ICoinbaseAdvancedTradeClient should be registered");

        serviceProvider.GetService<ICoinbaseJwtGenerator>()
            .Should().NotBeNull("ICoinbaseJwtGenerator should be registered");

        serviceProvider.GetService<ICoinbaseApi>()
            .Should().NotBeNull("ICoinbaseApi should be registered");

        serviceProvider.GetService<CoinbaseSettings>()
            .Should().NotBeNull("CoinbaseSettings should be registered");
    }

    [Fact]
    public void AddCoinbaseAdvancedTradeClient_WithConfiguration_ConfiguresSettingsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedApiKey = "my-test-api-key";
        var expectedApiSecret = "my-test-api-secret";
        var expectedBaseUrl = "https://api.coinbase.com/test";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:ApiKey"] = expectedApiKey,
                ["Coinbase:ApiSecret"] = expectedApiSecret,
                ["Coinbase:BaseUrl"] = expectedBaseUrl
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Act
        services.AddCoinbaseAdvancedTradeClient(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var settings = serviceProvider.GetRequiredService<CoinbaseSettings>();

        // Assert
        settings.Should().NotBeNull();
        // Note: CoinbaseSettings doesn't have ApiKey/ApiSecret properties
        settings.BaseUrl.Should().Be(expectedBaseUrl);
    }

    [Fact]
    public void AddCoinbaseAdvancedTradeClient_RegistersAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:ApiKey"] = "test-api-key",
                ["Coinbase:ApiSecret"] = "test-api-secret"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Act
        services.AddCoinbaseAdvancedTradeClient(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Get two instances and verify they are the same
        var instance1 = serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
        var instance2 = serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();

        instance1.Should().BeSameAs(instance2, "Service should be registered as singleton");
    }

    [Fact]
    public void AddCoinbaseAdvancedTradeClient_HttpClientIsConfiguredWithCorrectBaseAddress()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedBaseUrl = "https://api.coinbase.com/api/v3/brokerage";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:ApiKey"] = "test-api-key",
                ["Coinbase:ApiSecret"] = "test-api-secret",
                ["Coinbase:BaseUrl"] = expectedBaseUrl
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();

        // Act
        services.AddCoinbaseAdvancedTradeClient(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient("CoinbaseApi");

        httpClient.BaseAddress.Should().NotBeNull();
        httpClient.BaseAddress!.ToString().Should().StartWith(expectedBaseUrl);
    }

    [Fact]
    public void AddCoinbaseAdvancedTradeClient_WithMissingConfiguration_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Act & Assert
        var act = () =>
        {
            services.AddCoinbaseAdvancedTradeClient(configuration);
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
        };

        act.Should().Throw<Exception>("Should throw when required configuration is missing");
    }

    [Fact]
    public void AddCoinbaseAdvancedTradeClient_CanResolveAllDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:ApiKey"] = "test-api-key",
                ["Coinbase:ApiSecret"] = "test-api-secret",
                ["Coinbase:BaseUrl"] = "https://api.coinbase.com/api/v3/brokerage"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddConsole());

        // Act
        services.AddCoinbaseAdvancedTradeClient(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Try to create the client, which will resolve all dependencies
        var act = () => serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
        act.Should().NotThrow("All dependencies should be resolvable");
    }

    [Fact]
    public void AddCoinbaseAdvancedTradeClient_LoggerIsOptional()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Coinbase:ApiKey"] = "test-api-key",
                ["Coinbase:ApiSecret"] = "test-api-secret",
                ["Coinbase:BaseUrl"] = "https://api.coinbase.com/api/v3/brokerage"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);
        // Note: Not adding logging services

        // Act
        services.AddCoinbaseAdvancedTradeClient(configuration);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var act = () => serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
        act.Should().NotThrow("Client should work without logger");
    }
}