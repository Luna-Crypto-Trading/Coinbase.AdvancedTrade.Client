using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class PaymentMethodsResponse
{
    [JsonPropertyName("payment_methods")]
    public required List<PaymentMethod> PaymentMethods { get; set; }
}

public class PaymentMethod
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }

    [JsonPropertyName("allow_buy")]
    public bool? AllowBuy { get; set; }

    [JsonPropertyName("allow_sell")]
    public bool? AllowSell { get; set; }

    [JsonPropertyName("allow_deposit")]
    public bool? AllowDeposit { get; set; }

    [JsonPropertyName("allow_withdraw")]
    public bool? AllowWithdraw { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }
}
