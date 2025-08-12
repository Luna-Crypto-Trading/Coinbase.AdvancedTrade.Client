using Coinbase.AdvancedTrade.Client.Authentication;
using Coinbase.AdvancedTrade.Client.Configuration;
using Microsoft.Extensions.Logging;

namespace Coinbase.AdvancedTrade.Client.Validation;

public interface ICoinbaseCredentialValidator
{
    Task<ValidationResult> ValidateCredentialsAsync(string apiKey, string apiSecret, CancellationToken cancellationToken = default);
}

public class ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private ValidationResult(bool isValid, string? errorMessage, Exception? exception)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static ValidationResult Success() => new(true, null, null);
    public static ValidationResult Failure(string errorMessage, Exception? exception = null) => new(false, errorMessage, exception);
}

public class CoinbaseCredentialValidator : ICoinbaseCredentialValidator
{
    private readonly IAuthenticatedClientFactory _clientFactory;
    private readonly CoinbaseSettings _settings;
    private readonly ILogger<CoinbaseCredentialValidator>? _logger;

    public CoinbaseCredentialValidator(
        IAuthenticatedClientFactory clientFactory,
        CoinbaseSettings settings,
        ILogger<CoinbaseCredentialValidator>? logger = null)
    {
        _clientFactory = clientFactory;
        _settings = settings;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateCredentialsAsync(string apiKey, string apiSecret, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
                return ValidationResult.Failure("API Key is required");

            if (string.IsNullOrEmpty(apiSecret))
                return ValidationResult.Failure("API Secret is required");

            var baseUrl = _settings.GetActiveBaseUrl();
            using var client = _clientFactory.CreateAuthenticatedClient(baseUrl, apiKey, apiSecret);

            // Make a simple API call to validate credentials
            // Using accounts endpoint as it's typically available and low-cost
            var response = await client.GetAsync("/accounts", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger?.LogInformation("Coinbase API credentials validated successfully");
                return ValidationResult.Success();
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var errorMessage = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Invalid API credentials. Please check your API key and secret.",
                System.Net.HttpStatusCode.Forbidden => "API credentials do not have sufficient permissions.",
                System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded during validation.",
                _ => $"API validation failed: {response.StatusCode} - {errorContent}"
            };

            _logger?.LogWarning("Credential validation failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
            return ValidationResult.Failure(errorMessage);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error validating Coinbase credentials");
            return ValidationResult.Failure($"Validation error: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Extension methods for easy credential validation
/// </summary>
public static class CoinbaseCredentialValidatorExtensions
{
    /// <summary>
    /// Validates credentials and throws an exception if invalid
    /// </summary>
    public static async Task ValidateOrThrowAsync(this ICoinbaseCredentialValidator validator, string apiKey, string apiSecret, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateCredentialsAsync(apiKey, apiSecret, cancellationToken);
        if (!result.IsValid)
        {
            throw new InvalidOperationException($"Credential validation failed: {result.ErrorMessage}", result.Exception);
        }
    }
}