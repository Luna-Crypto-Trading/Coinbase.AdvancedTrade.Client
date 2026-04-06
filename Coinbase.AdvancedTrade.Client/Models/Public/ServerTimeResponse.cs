using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class ServerTimeResponse
{
    [JsonPropertyName("iso")]
    public string? Iso { get; set; }

    [JsonPropertyName("epochSeconds")]
    public string? EpochSeconds { get; set; }

    [JsonPropertyName("epochMillis")]
    public string? EpochMillis { get; set; }
}
