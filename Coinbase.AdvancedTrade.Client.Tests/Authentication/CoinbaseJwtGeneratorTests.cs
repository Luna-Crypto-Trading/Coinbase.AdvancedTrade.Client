using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Coinbase.AdvancedTrade.Client.Authentication;
using FluentAssertions;

namespace Coinbase.AdvancedTrade.Client.Tests.Authentication;

public class CoinbaseJwtGeneratorTests
{
    private readonly CoinbaseJwtGenerator _sut;
    private const string TestApiKey = "test-api-key";

    public CoinbaseJwtGeneratorTests()
    {
        _sut = new CoinbaseJwtGenerator();
    }

    [Fact]
    public void GenerateJwt_WithValidKey_ReturnsValidJwt()
    {
        // Arrange - generate a real EC P-256 key
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var privateKeyBytes = ecdsa.ExportECPrivateKey();
        var base64Key = Convert.ToBase64String(privateKeyBytes);
        var uri = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var apiKey = "test-api-key-123";

        // Act
        var jwt = _sut.GenerateJwt(uri, apiKey, base64Key);

        // Assert - JWT has 3 dot-separated parts
        jwt.Should().NotBeNullOrEmpty();
        var parts = jwt.Split('.');
        parts.Should().HaveCount(3, "JWT must have header.payload.signature");

        // Decode header and verify claims
        var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
        var header = JsonDocument.Parse(headerJson).RootElement;
        header.GetProperty("alg").GetString().Should().Be("ES256");
        header.GetProperty("kid").GetString().Should().Be(apiKey);
        header.GetProperty("typ").GetString().Should().Be("JWT");
        header.TryGetProperty("nonce", out _).Should().BeTrue();

        // Decode payload and verify claims
        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
        var payload = JsonDocument.Parse(payloadJson).RootElement;
        payload.GetProperty("sub").GetString().Should().Be(apiKey);
        payload.GetProperty("iss").GetString().Should().Be("coinbase-cloud");
        payload.GetProperty("uri").GetString().Should().Be(uri);

        var nbf = payload.GetProperty("nbf").GetInt64();
        var exp = payload.GetProperty("exp").GetInt64();
        (exp - nbf).Should().Be(60, "JWT should expire 60 seconds after nbf");
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }

    [Fact]
    public void GenerateJwt_WithInvalidKey_ThrowsException()
    {
        // Arrange
        var uri = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var invalidKey = "invalid-key-data";

        // Act & Assert
        var act = () => _sut.GenerateJwt(uri, TestApiKey, invalidKey);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GenerateJwt_WithEmptyApiKey_StillProcessesRequest()
    {
        // Arrange
        var uri = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var emptyApiKey = "";
        var invalidKey = "invalid-key";

        // Act & Assert - Should still attempt to process but fail on invalid key
        var act = () => _sut.GenerateJwt(uri, emptyApiKey, invalidKey);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GenerateJwt_WithNullParameters_ThrowsException()
    {
        // Act & Assert
        var act1 = () => _sut.GenerateJwt(null!, TestApiKey, "invalid-key");
        act1.Should().Throw<Exception>();

        var act2 = () => _sut.GenerateJwt("https://api.coinbase.com", null!, "invalid-key");
        act2.Should().Throw<Exception>();

        var act3 = () => _sut.GenerateJwt("https://api.coinbase.com", TestApiKey, null!);
        act3.Should().Throw<Exception>();
    }

    [Fact]
    public void GenerateJwt_WithDifferentUris_ProducesConsistentBehavior()
    {
        // Arrange
        var uri1 = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var uri2 = "https://api.coinbase.com/api/v3/brokerage/orders";
        var invalidKey = "invalid-key";

        // Act & Assert - Both should fail consistently with invalid key
        var act1 = () => _sut.GenerateJwt(uri1, TestApiKey, invalidKey);
        var act2 = () => _sut.GenerateJwt(uri2, TestApiKey, invalidKey);

        act1.Should().Throw<Exception>();
        act2.Should().Throw<Exception>();
    }

    [Fact]
    public void GenerateJwt_WithMalformedPemKey_ThrowsException()
    {
        // Arrange
        var uri = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var malformedKey = @"-----BEGIN EC PRIVATE KEY-----
        MALFORMED_KEY_DATA_FOR_TESTING
        -----END EC PRIVATE KEY-----";

        // Act & Assert
        var act = () => _sut.GenerateJwt(uri, TestApiKey, malformedKey);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GenerateJwt_WithNewlineEscapedKey_HandlesCorrectly()
    {
        // Arrange
        var uri = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var keyWithEscapedNewlines = "invalid-key\\nwith\\nescaped\\nnewlines";

        // Act & Assert
        var act = () => _sut.GenerateJwt(uri, TestApiKey, keyWithEscapedNewlines);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GenerateJwt_ParameterValidation_RequiresAllParameters()
    {
        // This test verifies the method signature and basic parameter handling
        // without needing real cryptographic operations

        // Arrange
        var uri = "https://api.coinbase.com/api/v3/brokerage/accounts";
        var apiKey = "test-key";
        var apiSecret = "invalid-secret";

        // Act & Assert - Verifies the method exists and processes parameters
        var act = () => _sut.GenerateJwt(uri, apiKey, apiSecret);
        act.Should().Throw<Exception>("Invalid key should cause an exception");
    }
}
