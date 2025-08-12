namespace Coinbase.AdvancedTrade.Client.Configuration;

public class CoinbaseSettings
{
    public string BaseUrl { get; set; } = "https://api.coinbase.com/api/v3/brokerage";
    public string SandboxBaseUrl { get; set; } = "http://localhost:5226/api/v3/brokerage";
    public bool UseSandbox { get; set; } = false;

    public string GetActiveBaseUrl() => UseSandbox ? SandboxBaseUrl : BaseUrl;
}