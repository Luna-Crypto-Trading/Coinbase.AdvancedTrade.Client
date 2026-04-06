# Security Policy

`Coinbase.AdvancedTrade.Client` handles API credentials and signed authentication for a financial exchange. We take security reports seriously.

## Reporting a Vulnerability

**Do not open a public GitHub issue for security vulnerabilities.**

Use GitHub's private vulnerability reporting:

[**Report a vulnerability →**](https://github.com/Luna-Crypto-Trading/Coinbase.AdvancedTrade.Client/security/advisories/new)

When you report, please include:

- A description of the vulnerability and its potential impact
- Steps to reproduce (or a proof-of-concept)
- The affected version(s) of the package
- Any suggested mitigation, if you have one

## Scope

### In scope

- Authentication bypass or signature forgery in `CoinbaseJwtGenerator` / `CoinbaseAuthenticator`
- Credential exposure (API keys, private keys, JWTs leaking via logs, exceptions, or serialization)
- Vulnerabilities in this package's direct dependencies that affect the client
- Insecure defaults in configuration or DI registration
- Memory-safety or cryptographic issues

### Out of scope

- Vulnerabilities in the Coinbase Advanced Trade API itself — report those to [Coinbase](https://www.coinbase.com/legal/security)
- Vulnerabilities in transitive dependencies — report those upstream
- General bugs, missing features, or incorrect API mappings — open a regular [issue](https://github.com/Luna-Crypto-Trading/Coinbase.AdvancedTrade.Client/issues) instead

## Response Expectations

- We will acknowledge your report within a reasonable window after receipt.
- We will investigate, confirm or refute the issue, and keep you updated on progress.
- If confirmed, we will work on a fix and coordinate disclosure with you.
- We do not currently offer a paid bug bounty.

## Supported Versions

The package is currently in the `0.x` alpha series. Only the latest published version is actively supported. Once `1.0.0` ships, the support window will be documented here.

| Version | Supported |
|---------|:---------:|
| 0.4.x   | Yes (latest) |
| < 0.4   | No        |

## Best Practices for Users

If you depend on this package, please:

- **Never commit API keys or private keys** to version control. Use environment variables, [`dotnet user-secrets`](https://learn.microsoft.com/aspnet/core/security/app-secrets), or a secrets manager.
- **Use sandbox mode** (`UseSandbox = true`) for development and testing.
- **Restrict API key permissions** in the Coinbase dashboard to the minimum required scopes for your application.
- **Keep the package up to date** — security fixes are published as new releases.
- **Rotate keys** if you suspect they have been exposed.
- **Audit logs** for unexpected requests, especially around order placement.

## Disclaimer

This library is not officially affiliated with Coinbase. Use at your own risk. Always test thoroughly in sandbox environment before using in production.
