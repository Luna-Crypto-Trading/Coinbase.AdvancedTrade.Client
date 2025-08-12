using System.Net.Http.Headers;
using Coinbase.AdvancedTrade.Client.Configuration;
using Microsoft.Extensions.Options;

namespace Coinbase.AdvancedTrade.Client.Authentication;

public class CoinbaseAuthenticator : DelegatingHandler
{
    private readonly ICoinbaseJwtGenerator _jwtGenerator;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly CoinbaseSettings _settings;

    public CoinbaseAuthenticator(
        ICoinbaseJwtGenerator jwtGenerator,
        string apiKey,
        string apiSecret,
        CoinbaseSettings settings)
        : base(new HttpClientHandler())
    {
        _jwtGenerator = jwtGenerator;
        _apiKey = apiKey;
        _apiSecret = apiSecret;
        _settings = settings;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string requestMethod = request.Method.ToString();
        string absoluteUri = request.RequestUri?.AbsolutePath ?? String.Empty;

        var uriHost = new Uri(_settings.GetActiveBaseUrl(), UriKind.Absolute).Host;
        string uri = $"{requestMethod} {uriHost}{absoluteUri}";

        var token = _jwtGenerator.GenerateJwt(uri, _apiKey, _apiSecret);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException($"Coinbase API request failed: {response.StatusCode} - {errorContent}");
    }
}