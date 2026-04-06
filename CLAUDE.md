# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

.NET 10 client library for the Coinbase Advanced Trade API, published as a NuGet package (`Coinbase.AdvancedTrade.Client`, currently `0.3.1-alpha`). Wraps the REST API with ES256 JWT authentication, Polly resilience policies, Refit-generated HTTP clients, and Microsoft DI integration.

## Development Commands

```bash
dotnet build                          # Build entire solution
dotnet test                           # Run all unit + integration tests
dotnet format --verify-no-changes     # Check formatting (CI enforces this)

# Run specific test projects
dotnet test Coinbase.AdvancedTrade.Client.Tests/
dotnet test Coinbase.AdvancedTrade.Client.IntegrationTests/

# Run a single test by name
dotnet test --filter "FullyQualifiedName~PlaceOrderAsync_ReturnsSuccess"

# Pack for NuGet
dotnet pack Coinbase.AdvancedTrade.Client/Coinbase.AdvancedTrade.Client.csproj --configuration Release

# Run the manual test console app (requires API credentials in appsettings.json)
dotnet run --project TestApp
```

## Architecture

### Request Flow

```
Consumer → ICoinbaseAdvancedTradeClient (resilience + error mapping)
             → Polly CircuitBreaker → Polly Retry
                → ICoinbaseApi (Refit-generated)
                   → CoinbaseAuthenticator (DelegatingHandler, adds JWT)
                      → Coinbase REST API
```

### Key Layers

- **`Api/ICoinbaseApi`** — Refit interface defining all REST endpoints. Refit generates the HTTP implementation at registration time.
- **`CoinbaseAdvancedTradeClient`** — High-level client wrapping `ICoinbaseApi`. Every method returns `ApiResponse<T>` (success/failure with error details) instead of throwing. Contains Polly retry (3 attempts, exponential backoff on transient HTTP errors) and circuit breaker (5 failures, 2-min break) policies.
- **`Authentication/`** — `CoinbaseJwtGenerator` creates ES256 JWTs (1-min expiry) using `jose-jwt`. `CoinbaseAuthenticator` is a `DelegatingHandler` that generates a fresh JWT per request and sets the `Authorization: Bearer` header.
- **`Configuration/`** — `CoinbaseSettings` (Options pattern) holds `ApiKey`, `ApiSecret`, `BaseUrl`, `UseSandbox`. Three `AddCoinbaseAdvancedTradeClient()` overloads register everything into DI (from `IConfiguration`, direct settings, or lambda).
- **`Models/`** — Strongly-typed request/response models using `System.Text.Json` with `[JsonPropertyName]`. Currency values are `string` (not `decimal`) per Coinbase API convention; extension methods provide decimal conversion.
- **`Extensions/`** — `OrderRequestBuilder` (fluent builder for orders), `CoinbaseModelExtensions` (decimal parsing, status checks, spread calculations).
- **`Validation/`** — `CoinbaseCredentialValidator` verifies API keys by calling `GET /accounts`.

### Testing

- **Unit tests** (`Coinbase.AdvancedTrade.Client.Tests/`): xUnit + NSubstitute + FluentAssertions. Mocks `ICoinbaseApi` to test client logic, error handling, retry behavior, and circuit breaker activation.
- **Integration tests** (`Coinbase.AdvancedTrade.Client.IntegrationTests/`): xUnit + WireMock.Net + FluentAssertions. Tests DI registration, service resolution, and resilience policies against a local mock HTTP server.

## CI/CD

CI runs on push to `main`/`develop` and PRs to `main` (`.github/workflows/ci.yml`):
- Matrix: ubuntu/windows/macos + .NET 10
- Stages: build, unit tests, integration tests, `dotnet format` check, analyzer warnings-as-errors, security scan
- Publishing: version tags (`v*`) trigger NuGet.org publish + GitHub Release

## Publishing

1. Update `<Version>` in `Coinbase.AdvancedTrade.Client.csproj`
2. Commit, tag (`git tag v0.4.0-alpha`), push tag
3. CI builds, tests, publishes to NuGet.org (requires `NUGET_API_KEY` secret)

Note: v1.0.0 was accidentally published and unlisted. Use `--prerelease` flag for current alpha versions.
