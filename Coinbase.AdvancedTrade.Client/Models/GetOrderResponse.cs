using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class GetOrderResponse
{
    [JsonPropertyName("order")]
    public required OrderV3 Order { get; set; }
}
