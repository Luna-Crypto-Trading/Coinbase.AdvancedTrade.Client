using Coinbase.AdvancedTrade.Client.Extensions;
using Coinbase.AdvancedTrade.Client.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
services.AddCoinbaseAdvancedTradeClient(configuration);

var serviceProvider = services.BuildServiceProvider();

// Get the client and make some test calls
var client = serviceProvider.GetRequiredService<ICoinbaseAdvancedTradeClient>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("Testing Coinbase Advanced Trade Client...");

    // Test listing products
    logger.LogInformation("Fetching available products...");
    var products = await client.ListProductsAsync();
    logger.LogInformation("Found {ProductCount} products", products.ProductCount);

    // Display first few products
    foreach (var product in products.Products.Take(5))
    {
        logger.LogInformation("Product: {ProductId} - Price: {Price} ({Change}%)", 
            product.ProductId, product.Price, product.PricePercentageChange24h);
    }

    // Test getting best bid/ask for BTC-USD
    logger.LogInformation("Getting best bid/ask for BTC-USD...");
    var bidAsk = await client.GetBestBidAskAsync(new List<string> { "BTC-USD" });
    
    if (bidAsk.PriceBooks.Any())
    {
        var btcBook = bidAsk.PriceBooks.First();
        if (btcBook.Bids.Any() && btcBook.Asks.Any())
        {
            logger.LogInformation("BTC-USD - Best Bid: {BestBid}, Best Ask: {BestAsk}", 
                btcBook.Bids.First().Price, btcBook.Asks.First().Price);
        }
    }

    logger.LogInformation("Test completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during testing: {ErrorMessage}", ex.Message);
}