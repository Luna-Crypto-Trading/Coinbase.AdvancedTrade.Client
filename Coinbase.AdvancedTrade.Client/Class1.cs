using Microsoft.Extensions.DependencyInjection;

namespace Coinbase.AdvancedTrade.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoinbaseAdvancedTradeClient(this IServiceCollection services)
    {
        services.AddTransient<IHelloWorldService, HelloWorldService>();
        return services;
    }
}
