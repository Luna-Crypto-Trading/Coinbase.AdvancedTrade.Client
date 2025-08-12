using System.Text.Json.Serialization;

namespace Coinbase.AdvancedTrade.Client.Models;

public record AccountsResponse
{
    [JsonPropertyName("accounts")]
    public List<CoinbaseAccount>? Accounts { get; set; }

    /// <summary>
    /// Whether there are additional pages for this query.
    /// </summary>
    [JsonPropertyName("has_next")]
    public required bool HasNext { get; set; }

    /// <summary>
    /// For paginated responses, returns all responses that come after this value.
    /// </summary>
    [JsonPropertyName("cursor")]
    public string? Cursor { get; set; }

    /// <summary>
    /// Number of accounts returned
    /// </summary>
    [JsonPropertyName("size")]
    public int? Size { get; set; }
}

public record CoinbaseAccount
{
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("available_balance")]
    public required Balance AvailableBalance { get; set; }

    /// <summary>
    /// Whether or not this account is the user's primary account
    /// </summary>
    [JsonPropertyName("default")]
    public bool? Default { get; set; }

    /// <summary>
    /// Whether or not this account is active and okay to use.
    /// </summary>
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// What type the account is. Possible values: [ACCOUNT_TYPE_UNSPECIFIED, ACCOUNT_TYPE_CRYPTO, ACCOUNT_TYPE_FIAT, ACCOUNT_TYPE_VAULT, ACCOUNT_TYPE_PERP_FUTURES]
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Whether or not this account is ready to trade.
    /// </summary>
    [JsonPropertyName("ready")]
    public bool? Ready { get; set; }

    /// <summary>
    /// Amount that is being held for pending transfers against the available balance.
    /// </summary>
    [JsonPropertyName("hold")]
    public required Balance Hold { get; set; }

    /// <summary>
    /// The ID of the portfolio this account is associated with.
    /// </summary>
    [JsonPropertyName("retail_portfolio_id")]
    public string? RetailPortfolioId { get; set; }
}

public record Balance
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}