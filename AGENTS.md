# AGENTS.md

Guidance for AI coding agents (Claude Code, GitHub Copilot, Cursor, Codex, Aider, etc.) working on this repository.

This file plus [`CLAUDE.md`](CLAUDE.md) is the contract. `CLAUDE.md` is the architecture reference; this file is the rulebook for behavior.

## Read First

Before making changes, read:

1. [`CLAUDE.md`](CLAUDE.md) — project overview, request flow, layer breakdown, all 48 API endpoints, testing strategy, CI/CD pipeline.
2. [`CONTRIBUTING.md`](CONTRIBUTING.md) — human-facing setup, test commands, PR guidelines.
3. [`.editorconfig`](.editorconfig) — code style rules CI enforces.
4. [`.github/workflows/ci.yml`](.github/workflows/ci.yml) — what CI will validate on your PR.

## Hard Rules

These are non-negotiable. Violating them will get your PR rejected.

### Build & Format
- Run `dotnet format` before claiming work is complete. CI runs `dotnet format --verify-no-changes` and will fail otherwise.
- Run `dotnet build --configuration Release` to match CI's analyzer warnings-as-errors strictness.
- Run `dotnet test` and confirm all unit + integration tests pass before finishing.

### Code Patterns
- **Never throw from client methods.** All methods on `ICoinbaseAdvancedTradeClient` return `ApiResponse<T>` with `IsSuccess`, `Data`, `ErrorMessage`, and `Exception` properties. Errors are returned, not thrown.
- **Currency values are `string`, not `decimal`.** This matches the Coinbase API convention. Use the conversion helpers in `Coinbase.AdvancedTrade.Client/Extensions/CoinbaseModelExtensions.cs` when consumers need a `decimal`.
- **Use `[JsonPropertyName]` on every model property.** The repo uses `System.Text.Json` and serialization is contract-driven.
- **Follow the partial-class-by-domain pattern** for `CoinbaseAdvancedTradeClient`. Domain code lives in `CoinbaseAdvancedTradeClient.Futures.cs`, `CoinbaseAdvancedTradeClient.Perpetuals.cs`, `CoinbaseAdvancedTradeClient.Converts.cs`, etc. Don't dump everything into the root partial.
- **File-scoped namespaces.** No exceptions.
- **Nullable reference types are enabled.** Handle nullability explicitly — don't suppress with `!` unless you can justify it.

### Testing
- Use **NSubstitute** to mock `ICoinbaseApi` in unit tests (`Coinbase.AdvancedTrade.Client.Tests/`).
- Use **WireMock.Net** to test HTTP behavior end-to-end in integration tests (`Coinbase.AdvancedTrade.Client.IntegrationTests/`).
- Use **FluentAssertions** (`result.Should().Be(...)`) for assertions.
- New public surface area requires tests. New endpoint mappings on `ICoinbaseApi` need at least one unit test for the client wrapper.

### Security
- **Never commit secrets.** This includes API keys, JWTs, `appsettings.*.local.json`, `secrets.json`, `api-keys.json`. The `.gitignore` already blocks the common patterns — don't bypass it.
- Treat any code touching `Authentication/CoinbaseJwtGenerator.cs` or `Authentication/CoinbaseAuthenticator.cs` as security-sensitive. Ask for review.
- Don't log full Authorization headers, JWT payloads, or API secrets.

## Validation Matching CI

Run this sequence before declaring work complete. It mirrors what `.github/workflows/ci.yml` does:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet format --verify-no-changes
```

If any of these fail, fix the underlying issue before continuing. Don't suppress, don't `--no-verify`, don't disable analyzers.

## Scope Discipline

- **Don't add features that weren't asked for.** A bug fix doesn't need a refactor pass. A new endpoint doesn't need additional configurability "for the future."
- **Don't restructure files you don't need to touch.** Stay focused on the change.
- **Don't add backwards-compat shims** unless the change is part of a tagged release migration. The package is pre-1.0 alpha — breaking changes are acceptable when justified.
- **Don't create helpers or abstractions for one-time operations.** Inline first; extract only when the third caller appears.

## When in Doubt

- Check `CLAUDE.md` for the architectural answer.
- Check existing similar code for the pattern (e.g., to add a new endpoint, copy the shape of an existing partial-class method).
- Ask the human reviewer instead of guessing.
