# Contributing to Coinbase.AdvancedTrade.Client

Thanks for your interest in contributing! This document explains how to set up your environment, run the tests, and submit changes.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git
- A GitHub account

The library targets `net10.0` exclusively.

## Getting Started

```bash
# 1. Fork the repo on GitHub, then clone your fork
git clone https://github.com/<your-username>/Coinbase.AdvancedTrade.Client.git
cd Coinbase.AdvancedTrade.Client

# 2. Add the upstream remote
git remote add upstream https://github.com/Luna-Crypto-Trading/Coinbase.AdvancedTrade.Client.git

# 3. Restore and build
dotnet restore
dotnet build
```

Create a feature branch off `main` for your changes:

```bash
git checkout -b feature/short-description
```

## Running Tests

The repo has two test projects: unit tests (xUnit + NSubstitute + FluentAssertions) and integration tests (xUnit + WireMock.Net + FluentAssertions).

```bash
# All tests
dotnet test

# Unit tests only
dotnet test Coinbase.AdvancedTrade.Client.Tests/

# Integration tests only (uses a local WireMock HTTP server — no live API calls)
dotnet test Coinbase.AdvancedTrade.Client.IntegrationTests/

# A single test by name
dotnet test --filter "FullyQualifiedName~PlaceOrderAsync_ReturnsSuccess"
```

Integration tests do **not** hit the real Coinbase API — they spin up a local WireMock.Net server to verify DI registration, request shape, retry behavior, and circuit breaker activation.

There's also a manual test console app in `TestApp/` that talks to the real Coinbase API. It needs valid credentials in `TestApp/appsettings.json` (which is git-ignored) and is not run by CI:

```bash
dotnet run --project TestApp
```

## Code Style

Code style is enforced by `dotnet format` and validated in CI. The rules live in `.editorconfig` at the repo root.

```bash
# Format your changes before committing
dotnet format

# Verify nothing is wrong (this is what CI runs)
dotnet format --verify-no-changes
```

Additionally:

- Analyzer warnings are treated as errors in Release builds. Run `dotnet build --configuration Release` to match CI strictness before pushing.
- Use file-scoped namespaces.
- Nullable reference types are enabled — handle nullability explicitly.
- Currency values are `string`, not `decimal`, per the Coinbase API convention. Use the helpers in `Extensions/CoinbaseModelExtensions.cs` for decimal conversion.

## Testing Conventions

- All new public surface area requires tests.
- Use **NSubstitute** to mock `ICoinbaseApi` in unit tests.
- Use **WireMock.Net** to test HTTP behavior end-to-end in integration tests.
- Use **FluentAssertions** for assertions (`result.Should().Be(...)`).
- Client methods return `ApiResponse<T>` and never throw — your tests should assert on `IsSuccess`/`ErrorMessage`, not `try/catch`.

## Commit Messages

This repository uses [Conventional Commits](https://www.conventionalcommits.org/) to drive automated versioning and changelog generation via [release-please](https://github.com/googleapis/release-please).

**Format:**

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types that trigger releases (pre-1.0):**

- `fix:` — patch bump (e.g., 0.1.3 → 0.1.4)
- `feat:` — patch bump pre-1.0 (e.g., 0.1.3 → 0.1.4); becomes minor post-1.0
- `feat!:` or footer `BREAKING CHANGE:` — minor bump pre-1.0 (e.g., 0.1.3 → 0.2.0); becomes major post-1.0

**Types that appear in the changelog but do not trigger releases:**

- `docs:`, `perf:`, `refactor:`, `build:`, `ci:`, `chore:`, `test:`, `style:`

**Cutting 1.0.0:** Include the footer `Release-As: 1.0.0` on any commit merged to `main`. release-please will open a Release PR for `1.0.0` on its next run.

Commits that do not follow this convention will not produce a release but will still be merged normally.

## Pull Request Guidelines

1. Branch from `main`.
2. Keep PRs focused — one concern per PR.
3. Run `dotnet format`, `dotnet build`, and `dotnet test` locally before pushing.
4. Update `README.md` and `CLAUDE.md` if your change affects user-facing API or architecture.
5. Open the PR against `Luna-Crypto-Trading/Coinbase.AdvancedTrade.Client:main`.
6. Ensure all CI checks pass (build, tests on ubuntu/windows/macOS, format check, analyzers, security scan).
7. Be responsive to review feedback.

## Reporting Issues

Use the GitHub issue templates:

- **Bug report** — include .NET SDK version, OS, package version, minimal reproduction, and expected vs actual behavior.
- **Feature request** — describe the use case and the proposed API.

For security vulnerabilities, **do not open a public issue**. Follow [SECURITY.md](SECURITY.md) instead.

## Project Layout

For a detailed walk-through of the architecture (request flow, layers, API coverage, partial-class organization), see [`CLAUDE.md`](CLAUDE.md). It's the source of truth for the codebase shape.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE) that covers this project.
