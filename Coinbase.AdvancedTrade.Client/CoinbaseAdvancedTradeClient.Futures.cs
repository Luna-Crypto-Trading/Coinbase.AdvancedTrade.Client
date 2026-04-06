using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Models;
using Microsoft.Extensions.Logging;

namespace Coinbase.AdvancedTrade.Client;

public partial class CoinbaseAdvancedTradeClient
{
    public async Task<ApiResponse<FuturesBalanceSummaryResponse>> GetFuturesBalanceSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving futures balance summary");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetFuturesBalanceSummary(ct),
                cancellationToken);

            return ApiResponse<FuturesBalanceSummaryResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<FuturesBalanceSummaryResponse>(ex, "retrieving futures balance summary");
        }
    }

    public async Task<ApiResponse<FuturesPositionsResponse>> ListFuturesPositionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving futures positions");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ListFuturesPositions(ct),
                cancellationToken);

            return ApiResponse<FuturesPositionsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<FuturesPositionsResponse>(ex, "retrieving futures positions");
        }
    }

    public async Task<ApiResponse<FuturesPositionResponse>> GetFuturesPositionAsync(string productId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving futures position for {ProductId}", productId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetFuturesPosition(productId, ct),
                cancellationToken);

            return ApiResponse<FuturesPositionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<FuturesPositionResponse>(ex, $"retrieving futures position for {productId}");
        }
    }

    public async Task<ApiResponse<FuturesSweepResponse>> ScheduleFuturesSweepAsync(string usdAmount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Scheduling futures sweep for {Amount} USD", usdAmount);

            var request = new ScheduleSweepRequest { UsdAmount = usdAmount };
            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ScheduleFuturesSweep(request, ct),
                cancellationToken);

            return ApiResponse<FuturesSweepResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<FuturesSweepResponse>(ex, "scheduling futures sweep");
        }
    }

    public async Task<ApiResponse<FuturesSweepResponse>> ListFuturesSweepsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving futures sweeps");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.ListFuturesSweeps(ct),
                cancellationToken);

            return ApiResponse<FuturesSweepResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<FuturesSweepResponse>(ex, "retrieving futures sweeps");
        }
    }

    public async Task<ApiResponse<FuturesSweepResponse>> CancelFuturesSweepAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Cancelling pending futures sweep");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.CancelFuturesSweep(ct),
                cancellationToken);

            return ApiResponse<FuturesSweepResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<FuturesSweepResponse>(ex, "cancelling futures sweep");
        }
    }

    public async Task<ApiResponse<IntradayMarginSettingResponse>> GetIntradayMarginSettingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving intraday margin setting");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetIntradayMarginSetting(ct),
                cancellationToken);

            return ApiResponse<IntradayMarginSettingResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<IntradayMarginSettingResponse>(ex, "retrieving intraday margin setting");
        }
    }

    public async Task<ApiResponse<CurrentMarginWindowResponse>> GetCurrentMarginWindowAsync(string? marginProfileType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving current margin window");

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetCurrentMarginWindow(marginProfileType, ct),
                cancellationToken);

            return ApiResponse<CurrentMarginWindowResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<CurrentMarginWindowResponse>(ex, "retrieving current margin window");
        }
    }

    public async Task<ApiResponse<IntradayMarginSettingResponse>> SetIntradayMarginSettingAsync(string setting, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Setting intraday margin setting to {Setting}", setting);

            var request = new SetIntradayMarginSettingRequest { Setting = setting };
            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.SetIntradayMarginSetting(request, ct),
                cancellationToken);

            return ApiResponse<IntradayMarginSettingResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<IntradayMarginSettingResponse>(ex, "setting intraday margin setting");
        }
    }
}
