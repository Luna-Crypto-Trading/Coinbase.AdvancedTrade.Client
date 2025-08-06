using Coinbase.AdvancedTrade.Client;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCoinbaseAdvancedTradeClient();

var serviceProvider = services.BuildServiceProvider();
var helloWorldService = serviceProvider.GetRequiredService<IHelloWorldService>();

helloWorldService.SayHello();
