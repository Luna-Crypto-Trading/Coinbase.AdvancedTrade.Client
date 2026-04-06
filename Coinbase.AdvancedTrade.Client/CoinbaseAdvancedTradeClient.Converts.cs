using Coinbase.AdvancedTrade.Client.Api;
using Coinbase.AdvancedTrade.Client.Models;
using Microsoft.Extensions.Logging;

namespace Coinbase.AdvancedTrade.Client;

public partial class CoinbaseAdvancedTradeClient
{
    public async Task<ApiResponse<ConvertQuoteResponse>> CreateConvertQuoteAsync(ConvertQuoteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Creating convert quote from {From} to {To}", request.FromAccount, request.ToAccount);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.CreateConvertQuote(request, ct),
                cancellationToken);

            return ApiResponse<ConvertQuoteResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<ConvertQuoteResponse>(ex, "creating convert quote");
        }
    }

    public async Task<ApiResponse<ConvertTradeResponse>> GetConvertTradeAsync(string tradeId, string fromAccount, string toAccount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Retrieving convert trade {TradeId}", tradeId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.GetConvertTrade(tradeId, fromAccount, toAccount, ct),
                cancellationToken);

            return ApiResponse<ConvertTradeResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<ConvertTradeResponse>(ex, $"retrieving convert trade {tradeId}");
        }
    }

    public async Task<ApiResponse<ConvertTradeResponse>> CommitConvertTradeAsync(string tradeId, ConvertQuoteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Committing convert trade {TradeId}", tradeId);

            var response = await _resiliencePipeline.ExecuteAsync(
                async ct => await _coinbaseApi.CommitConvertTrade(tradeId, request, ct),
                cancellationToken);

            return ApiResponse<ConvertTradeResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return HandleException<ConvertTradeResponse>(ex, $"committing convert trade {tradeId}");
        }
    }
}
