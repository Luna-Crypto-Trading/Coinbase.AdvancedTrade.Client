# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

.NET 10 client library for the Coinbase Advanced Trade API, published as a NuGet package (`Coinbase.AdvancedTrade.Client`). Provides full REST API coverage (48 endpoints) with ES256 JWT authentication, Polly resilience policies, Refit-generated HTTP clients, and Microsoft DI integration.

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
- **`CoinbaseAdvancedTradeClient`** — High-level partial class wrapping `ICoinbaseApi`. Every method returns `ApiResponse<T>` (success/failure with error details) instead of throwing. Contains Polly retry (3 attempts, exponential backoff on transient HTTP errors) and circuit breaker (5 failures, 2-min break) policies. Split into domain files: `*.Futures.cs`, `*.Perpetuals.cs`, `*.Converts.cs`.
- **`Authentication/`** — `CoinbaseJwtGenerator` creates ES256 JWTs (1-min expiry) using `jose-jwt`. `CoinbaseAuthenticator` is a `DelegatingHandler` that generates a fresh JWT per request and sets the `Authorization: Bearer` header.
- **`Configuration/`** — `CoinbaseSettings` (Options pattern) holds `ApiKey`, `ApiSecret`, `BaseUrl`, `UseSandbox`. Three `AddCoinbaseAdvancedTradeClient()` overloads register everything into DI (from `IConfiguration`, direct settings, or lambda).
- **`Models/`** — Strongly-typed request/response models using `System.Text.Json` with `[JsonPropertyName]`. Currency values are `string` (not `decimal`) per Coinbase API convention; extension methods provide decimal conversion. Organized into subdirectories by domain: `Portfolios/`, `Futures/`, `Perpetuals/`, `Converts/`, `Payments/`, `Public/`.
- **`Extensions/`** — `OrderRequestBuilder` (fluent builder for orders), `CoinbaseModelExtensions` (decimal parsing for products, orders, fills, futures positions, perps balances, convert amounts; status checks; spread calculations).
- **`Validation/`** — `CoinbaseCredentialValidator` verifies API keys by calling `GET /accounts`.

### API Coverage (48 endpoints)

| Domain | Endpoints | Notes |
|--------|:---------:|-------|
| Accounts | 2 | List + Get, with pagination params |
| Orders | 9 | Place, Edit, Preview, Cancel, Close, Get, List, Fills |
| Products | 3 | List, Get, ProductBook |
| Market Data | 4 | BestBidAsk, Candles, MarketTrades, ProductBook |
| Fees | 1 | TransactionSummary with filters |
| Portfolios | 6 | List, Get, Create, Edit, Delete, MoveFunds |
| Futures/CFM | 9 | BalanceSummary, Positions, Sweeps, Margin settings |
| Perpetuals/INTX | 6 | Allocate, Portfolio, Positions, Balances, Collateral |
| Converts | 3 | Quote, GetTrade, CommitTrade |
| Payments | 2 | List + Get payment methods |
| Public | 6 | ServerTime, Products, Candles, Trades, ProductBook |
| Data | 1 | KeyPermissions |

### Testing

- **Unit tests** (`Coinbase.AdvancedTrade.Client.Tests/`): xUnit + NSubstitute + FluentAssertions. Mocks `ICoinbaseApi` to test client logic, error handling, retry behavior, and circuit breaker activation.
- **Integration tests** (`Coinbase.AdvancedTrade.Client.IntegrationTests/`): xUnit + WireMock.Net + FluentAssertions. Tests DI registration, service resolution, and resilience policies against a local mock HTTP server.

## CI/CD

CI runs on push to `main` and PRs to `main` (`.github/workflows/ci.yml`):
- Matrix: ubuntu/windows/macos + .NET 10
- Stages: build, unit tests, integration tests, `dotnet format` check, analyzer warnings-as-errors, security scan

## Releases

Releases are driven by [release-please](https://github.com/googleapis/release-please) on Conventional Commits — there is no manual version bump or tag step.

- Version source of truth: `.release-please-manifest.json` at the repo root.
- On every push to `main`, `.github/workflows/release-please.yml` opens or updates a "Release PR" that bumps the version and updates `CHANGELOG.md` based on Conventional Commits since the last release.
- Merging the Release PR creates the git tag and GitHub Release, and the same workflow's `publish` job builds with `/p:Version=$VERSION`, packs `Coinbase.AdvancedTrade.Client.csproj`, and pushes to NuGet.org (requires the `NUGET_API_KEY` secret).
- Pre-1.0, `feat:` bumps patch and `feat!:` / `BREAKING CHANGE:` bumps minor (per `release-please-config.json`).
- See [`CONTRIBUTING.md`](CONTRIBUTING.md#commit-messages) for the Conventional Commits rules contributors must follow.
