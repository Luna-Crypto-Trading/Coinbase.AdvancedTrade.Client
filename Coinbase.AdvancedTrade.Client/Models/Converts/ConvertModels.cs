using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public class ConvertQuoteRequest
{
    [JsonPropertyName("from_account")]
    public required string FromAccount { get; set; }

    [JsonPropertyName("to_account")]
    public required string ToAccount { get; set; }

    [JsonPropertyName("amount")]
    public required string Amount { get; set; }

    [JsonPropertyName("user_incentive_id")]
    public string? UserIncentiveId { get; set; }

    [JsonPropertyName("code_val")]
    public string? CodeVal { get; set; }
}

public class ConvertQuoteResponse
{
    [JsonPropertyName("trade")]
    public required ConvertTrade Trade { get; set; }
}

public class ConvertTradeResponse
{
    [JsonPropertyName("trade")]
    public required ConvertTrade Trade { get; set; }
}

public class ConvertTrade
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("user_entered_amount")]
    public ConvertAmount? UserEnteredAmount { get; set; }

    [JsonPropertyName("amount")]
    public ConvertAmount? Amount { get; set; }

    [JsonPropertyName("subtotal")]
    public ConvertAmount? Subtotal { get; set; }

    [JsonPropertyName("total")]
    public ConvertAmount? Total { get; set; }

    [JsonPropertyName("fees")]
    public List<ConvertFee>? Fees { get; set; }

    [JsonPropertyName("total_fee")]
    public ConvertAmount? TotalFee { get; set; }

    [JsonPropertyName("source")]
    public ConvertAccount? Source { get; set; }

    [JsonPropertyName("target")]
    public ConvertAccount? Target { get; set; }

    [JsonPropertyName("exchange_rate")]
    public ConvertAmount? ExchangeRate { get; set; }

    [JsonPropertyName("unit_price")]
    public ConvertUnitPrice? UnitPrice { get; set; }
}

public class ConvertAmount
{
    [JsonPropertyName("value")]
    public required string Value { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }
}

public class ConvertFee
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("amount")]
    public ConvertAmount? Amount { get; set; }
}

public class ConvertAccount
{
    [JsonPropertyName("ledger_account")]
    public ConvertLedgerAccount? LedgerAccount { get; set; }
}

public class ConvertLedgerAccount
{
    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("owner")]
    public ConvertOwner? Owner { get; set; }
}

public class ConvertOwner
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("user_uuid")]
    public string? UserUuid { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public class ConvertUnitPrice
{
    [JsonPropertyName("target_to_fiat")]
    public ConvertAmount? TargetToFiat { get; set; }

    [JsonPropertyName("target_to_source")]
    public ConvertAmount? TargetToSource { get; set; }

    [JsonPropertyName("source_to_fiat")]
    public ConvertAmount? SourceToFiat { get; set; }
}
