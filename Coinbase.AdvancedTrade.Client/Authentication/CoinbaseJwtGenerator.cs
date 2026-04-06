using System.Security.Cryptography;
using Jose;

namespace Coinbase.AdvancedTrade.Client.Authentication;

public interface ICoinbaseJwtGenerator
{
    string GenerateJwt(string uri, string apiKey, string apiSecret);
}

public class CoinbaseJwtGenerator : ICoinbaseJwtGenerator
{
    public string GenerateJwt(string uri, string apiKey, string apiSecret)
    {
        var secret = ParseKey(apiSecret);
        var privateKeyBytes = Convert.FromBase64String(secret);

        using var key = ECDsa.Create();
        key.ImportECPrivateKey(privateKeyBytes, out _);

        var timestamp = Convert.ToInt64(
            (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
        );
        var payload = new Dictionary<string, object>
        {
            { "sub", apiKey },
            { "iss", "coinbase-cloud" },
            { "nbf", timestamp },
            {
                "exp",
                Convert.ToInt64(
                    (
                        DateTime.UtcNow.AddMinutes(1)
                        - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    ).TotalSeconds
                )
            },
            { "uri", uri },
        };

        var extraHeaders = new Dictionary<string, object>
        {
            { "kid", apiKey },
            { "nonce", RandomHex(10) },
            { "typ", "JWT" },
        };

        return JWT.Encode(payload, key, JwsAlgorithm.ES256, extraHeaders);
    }

    private static string RandomHex(int digits)
    {
        byte[] buffer = new byte[(digits + 1) / 2];
        RandomNumberGenerator.Fill(buffer);
        string result = Convert.ToHexString(buffer);
        return result[..digits];
    }

    static string ParseKey(string key)
    {
        key = key.Replace("\\n", "\n");

        // Handle multiple input formats
        var lines = key.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        // If it's already base64 encoded (no header/footer), return as-is
        if (lines.Count == 1 && !lines[0].StartsWith("-----"))
        {
            return lines[0];
        }

        // Remove PEM header and footer
        var filteredLines = lines
            .Where(line => !line.StartsWith("-----BEGIN") && !line.StartsWith("-----END"))
            .ToList();

        // Join all the remaining lines to form the base64 encoded key
        return string.Join("", filteredLines);
    }
}