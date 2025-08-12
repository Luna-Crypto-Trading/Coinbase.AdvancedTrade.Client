using Coinbase.AdvancedTrade.Client.Configuration;

namespace Coinbase.AdvancedTrade.Client.Authentication;

public interface IAuthenticatedClientFactory
{
    HttpClient CreateAuthenticatedClient(string baseUrl, string apiKey, string apiSecret);
}

public class CoinbaseAuthenticatedClientFactory : IAuthenticatedClientFactory
{
    private readonly ICoinbaseJwtGenerator _jwtGenerator;
    private readonly CoinbaseSettings _coinbaseSettings;

    public CoinbaseAuthenticatedClientFactory(
        ICoinbaseJwtGenerator jwtGenerator,
        CoinbaseSettings coinbaseSettings)
    {
        _jwtGenerator = jwtGenerator;
        _coinbaseSettings = coinbaseSettings;
    }

    public HttpClient CreateAuthenticatedClient(string baseUrl, string apiKey, string apiSecret)
    {
        var innerHandler = new HttpClientHandler();

        // Create the authenticator with the inner handler
        var authenticator = new CoinbaseAuthenticator(
            _jwtGenerator,
            apiKey,
            apiSecret,
            _coinbaseSettings
        )
        {
            InnerHandler = innerHandler,
        };

        // Create the HttpClient with the authenticator
        var client = new HttpClient(authenticator) { BaseAddress = new Uri(baseUrl) };

        return client;
    }
}