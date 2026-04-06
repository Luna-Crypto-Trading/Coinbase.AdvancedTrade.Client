using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Models;
using Microsoft.Extensions.Logging;

namespace Coinbase.AdvancedTrade.Client;

public partial class CoinbaseAdvancedTradeClient
{
    public async Task<ApiResponse<EmptyResponse>> AllocatePortfolioAsync(AllocatePortfolioRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Allocating portfolio {PortfolioUuid} for {Symbol}", request.PortfolioUuid, request.Symbol);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.AllocatePortfolio(request, ct),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to allocate portfolio: {StatusCode}", response.StatusCode);
                return ApiResponse<EmptyResponse>.Failure($"Failed to allocate portfolio: {response.StatusCode}");
            }

            return ApiResponse<EmptyResponse>.Success(new EmptyResponse());
        }
        catch (Exception ex)
        {
            return HandleException<EmptyResponse>(ex, "allocating portfolio");
        }
    }

    public async Task<ApiResponse<IntxPortfolioResponse>> GetPerpsPortfolioSummaryAsync(string portfolioUuid, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving perps portfolio summary for {PortfolioUuid}", portfolioUuid);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetPerpsPortfolioSummary(portfolioUuid, ct),
                cancellationToken);

            return ApiResponse<IntxPortfolioResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<IntxPortfolioResponse>(ex, $"retrieving perps portfolio summary for {portfolioUuid}");
        }
    }

    public async Task<ApiResponse<IntxPositionsResponse>> ListPerpsPositionsAsync(string portfolioUuid, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving perps positions for {PortfolioUuid}", portfolioUuid);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ListPerpsPositions(portfolioUuid, ct),
                cancellationToken);

            return ApiResponse<IntxPositionsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<IntxPositionsResponse>(ex, $"retrieving perps positions for {portfolioUuid}");
        }
    }

    public async Task<ApiResponse<IntxPositionResponse>> GetPerpsPositionAsync(string portfolioUuid, string symbol, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving perps position {Symbol} for {PortfolioUuid}", symbol, portfolioUuid);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetPerpsPosition(portfolioUuid, symbol, ct),
                cancellationToken);

            return ApiResponse<IntxPositionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<IntxPositionResponse>(ex, $"retrieving perps position {symbol}");
        }
    }

    public async Task<ApiResponse<IntxBalancesResponse>> GetPerpsPortfolioBalancesAsync(string portfolioUuid, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving perps portfolio balances for {PortfolioUuid}", portfolioUuid);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetPerpsPortfolioBalances(portfolioUuid, ct),
                cancellationToken);

            return ApiResponse<IntxBalancesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<IntxBalancesResponse>(ex, $"retrieving perps portfolio balances for {portfolioUuid}");
        }
    }

    public async Task<ApiResponse<MultiAssetCollateralResponse>> SetMultiAssetCollateralAsync(MultiAssetCollateralRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Setting multi-asset collateral for {PortfolioUuid} to {Enabled}", request.PortfolioUuid, request.MultiAssetCollateralEnabled);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.SetMultiAssetCollateral(request, ct),
                cancellationToken);

            return ApiResponse<MultiAssetCollateralResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<MultiAssetCollateralResponse>(ex, "setting multi-asset collateral");
        }
    }
}
