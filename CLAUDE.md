# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET client library for the Coinbase Advanced Trade API. Currently contains a proof-of-concept DI extension method that demonstrates basic dependency injection integration.

## Development Commands

- `dotnet build` - Build the solution
- `dotnet run --project TestApp` - Run the test console application
- `dotnet pack Coinbase.AdvancedTrade.Client/Coinbase.AdvancedTrade.Client.csproj --configuration Release` - Create NuGet package
- `dotnet restore` - Restore dependencies

## Project Structure

- `Coinbase.AdvancedTrade.Client/` - Main library project
  - `ServiceCollectionExtensions.cs` - DI extension methods
  - `IHelloWorldService.cs` - Service interface
  - `HelloWorldService.cs` - Service implementation
- `TestApp/` - Console application for testing the library
- `.github/workflows/publish.yml` - GitHub Actions workflow for publishing to GitHub Packages

## Publishing to GitHub Packages

1. Update version in `.csproj` file
2. Update repository URL and author information in `.csproj`
3. Create a tag: `git tag v1.0.0`
4. Push the tag: `git push origin v1.0.0`
5. GitHub Actions will automatically build and publish the package

## Using the Package

To consume this package in other projects:
1. Add the GitHub Packages source to your `nuget.config`
2. Install the package: `dotnet add package Coinbase.AdvancedTrade.Client`
3. Register services: `services.AddCoinbaseAdvancedTradeClient()`
4. Inject `IHelloWorldService` and call `SayHello()`

## Architecture Guidelines

- Use HttpClient with proper configuration for API calls
- Implement strongly-typed models for all API endpoints  
- Support dependency injection for easy integration
- Follow async/await patterns for API calls
- Implement proper error handling and logging