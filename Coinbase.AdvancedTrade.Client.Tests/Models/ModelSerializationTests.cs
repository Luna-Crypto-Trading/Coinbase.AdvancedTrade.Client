using System.Text.Json;
using Coinbase.AdvancedTrade.Client.Models;
using FluentAssertions;

namespace Coinbase.AdvancedTrade.Client.Tests.Models;

public class ModelSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ModelSerializationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }

    #region OrderRequest Serialization Tests

    [Fact]
    public void OrderRequest_MarketOrder_SerializesCorrectly()
    {
        // Arrange
        var order = new OrderRequest
        {
            ClientOrderId = "test-client-order-123",
            ProductId = "BTC-USD",
            Side = "BUY",
            OrderConfiguration = new OrderConfiguration
            {
                MarketMarketIoc = new MarketMarketIoc
                {
                    QuoteSize = "1000.00"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(order, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<OrderRequest>(json, _jsonOptions);

        // Assert
        json.Should().Contain("\"client_order_id\": \"test-client-order-123\"");
        json.Should().Contain("\"product_id\": \"BTC-USD\"");
        json.Should().Contain("\"side\": \"BUY\"");
        json.Should().Contain("\"quote_size\": \"1000.00\"");

        deserialized.Should().NotBeNull();
        deserialized!.ClientOrderId.Should().Be(order.ClientOrderId);
        deserialized.ProductId.Should().Be(order.ProductId);
        deserialized.Side.Should().Be(order.Side);
        deserialized.OrderConfiguration!.MarketMarketIoc!.QuoteSize.Should().Be("1000.00");
    }

    [Fact]
    public void OrderRequest_LimitOrder_SerializesCorrectly()
    {
        // Arrange
        var order = new OrderRequest
        {
            ClientOrderId = "limit-order-456",
            ProductId = "ETH-USD",
            Side = "SELL",
            OrderConfiguration = new OrderConfiguration
            {
                LimitLimitGtc = new LimitLimitGtcV3
                {
                    BaseSize = "2.5",
                    LimitPrice = "3000.00",
                    PostOnly = true
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(order, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<OrderRequest>(json, _jsonOptions);

        // Assert
        json.Should().Contain("\"base_size\": \"2.5\"");
        json.Should().Contain("\"limit_price\": \"3000.00\"");
        json.Should().Contain("\"post_only\": true");

        deserialized.Should().NotBeNull();
        deserialized!.OrderConfiguration!.LimitLimitGtc!.PostOnly.Should().BeTrue();
        deserialized.OrderConfiguration.LimitLimitGtc.BaseSize.Should().Be("2.5");
        deserialized.OrderConfiguration.LimitLimitGtc.LimitPrice.Should().Be("3000.00");
    }

    #endregion

    #region OrderInformation Serialization Tests

    [Fact]
    public void OrderInformation_Success_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""success"": true,
            ""success_response"": {
                ""order_id"": ""12345-67890"",
                ""product_id"": ""BTC-USD"",
                ""side"": ""BUY"",
                ""client_order_id"": ""client-123""
            }
        }";

        // Act
        var result = JsonSerializer.Deserialize<OrderInformation>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.SuccessResponse.Should().NotBeNull();
        result.SuccessResponse!.OrderId.Should().Be("12345-67890");
        result.SuccessResponse.ProductId.Should().Be("BTC-USD");
        result.SuccessResponse.ClientOrderId.Should().Be("client-123");
    }

    [Fact]
    public void OrderInformation_Error_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""success"": false,
            ""error_response"": {
                ""error"": ""INSUFFICIENT_FUNDS"",
                ""message"": ""Insufficient funds in account"",
                ""error_details"": ""Account balance too low"",
                ""new_order_failure_reason"": ""INSUFFICIENT_FUNDS""
            }
        }";

        // Act
        var result = JsonSerializer.Deserialize<OrderInformation>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorResponse.Should().NotBeNull();
        result.ErrorResponse!.Message.Should().Be("Insufficient funds in account");
        result.ErrorResponse.NewOrderFailureReason.Should().Be("INSUFFICIENT_FUNDS");
    }

    #endregion

    #region Account Model Tests

    [Fact]
    public void CoinbaseAccount_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""uuid"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
            ""name"": ""BTC Wallet"",
            ""currency"": ""BTC"",
            ""available_balance"": {
                ""value"": ""1.23456789"",
                ""currency"": ""BTC""
            },
            ""default"": true,
            ""active"": true,
            ""created_at"": ""2024-01-01T00:00:00Z"",
            ""updated_at"": ""2024-01-02T00:00:00Z"",
            ""type"": ""ACCOUNT_TYPE_CRYPTO"",
            ""ready"": true,
            ""hold"": {
                ""value"": ""0.1"",
                ""currency"": ""BTC""
            }
        }";

        // Act
        var result = JsonSerializer.Deserialize<CoinbaseAccount>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Uuid.Should().Be("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        result.Name.Should().Be("BTC Wallet");
        result.Currency.Should().Be("BTC");
        result.AvailableBalance!.Value.Should().Be("1.23456789");
        result.Default.Should().BeTrue();
        result.Active.Should().BeTrue();
        result.Type.Should().Be("ACCOUNT_TYPE_CRYPTO");
        result.Hold!.Value.Should().Be("0.1");
    }

    #endregion

    #region Product Model Tests

    [Fact]
    public void Product_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""product_id"": ""BTC-USD"",
            ""price"": ""50000.00"",
            ""price_percentage_change_24h"": ""2.5"",
            ""volume_24h"": ""1000000"",
            ""volume_percentage_change_24h"": ""5.2"",
            ""base_increment"": ""0.00000001"",
            ""quote_increment"": ""0.01"",
            ""quote_min_size"": ""1"",
            ""quote_max_size"": ""1000000"",
            ""base_min_size"": ""0.001"",
            ""base_max_size"": ""100"",
            ""base_name"": ""Bitcoin"",
            ""quote_name"": ""US Dollar"",
            ""watched"": true,
            ""is_disabled"": false,
            ""new"": false,
            ""status"": ""online"",
            ""cancel_only"": false,
            ""limit_only"": false,
            ""post_only"": false,
            ""trading_disabled"": false,
            ""auction_mode"": false,
            ""product_type"": ""SPOT"",
            ""quote_currency"": ""USD"",
            ""base_currency"": ""BTC"",
            ""mid_market_price"": ""50000""
        }";

        // Act
        var result = JsonSerializer.Deserialize<AdvancedTradeProduct>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.ProductId.Should().Be("BTC-USD");
        result.Price.Should().Be("50000.00");
        result.PricePercentageChange24h.Should().Be("2.5");
        result.Volume24h.Should().Be("1000000");
        result.BaseIncrement.Should().Be("0.00000001");
        result.QuoteIncrement.Should().Be("0.01");
        result.BaseCurrencyId.Should().Be("BTC");
        result.QuoteCurrencyId.Should().Be("USD");
        result.Status.Should().Be("online");
        result.ProductType.Should().Be("SPOT");
        result.Watched.Should().BeTrue();
        result.IsDisabled.Should().BeFalse();
    }

    #endregion

    #region BestBidAsk Model Tests

    [Fact]
    public void BestBidAskResponse_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""pricebooks"": [
                {
                    ""product_id"": ""BTC-USD"",
                    ""bids"": [
                        {
                            ""price"": ""49900.00"",
                            ""size"": ""0.5""
                        },
                        {
                            ""price"": ""49850.00"",
                            ""size"": ""1.0""
                        }
                    ],
                    ""asks"": [
                        {
                            ""price"": ""50100.00"",
                            ""size"": ""0.3""
                        },
                        {
                            ""price"": ""50150.00"",
                            ""size"": ""0.7""
                        }
                    ],
                    ""time"": ""2024-01-01T12:00:00Z""
                }
            ]
        }";

        // Act
        var result = JsonSerializer.Deserialize<BestBidAskResponse>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.PriceBooks.Should().HaveCount(1);

        var pricebook = result.PriceBooks.First();
        pricebook.ProductId.Should().Be("BTC-USD");
        pricebook.Bids.Should().HaveCount(2);
        pricebook.Asks.Should().HaveCount(2);
        pricebook.Bids.First().Price.Should().Be("49900.00");
        pricebook.Bids.First().Size.Should().Be("0.5");
        pricebook.Asks.First().Price.Should().Be("50100.00");
        pricebook.Asks.First().Size.Should().Be("0.3");
    }

    #endregion

    #region Candle Model Tests

    [Fact]
    public void CandleResponse_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""candles"": [
                {
                    ""start"": ""1704110400"",
                    ""low"": ""49500"",
                    ""high"": ""50500"",
                    ""open"": ""49800"",
                    ""close"": ""50200"",
                    ""volume"": ""150.5""
                },
                {
                    ""start"": ""1704114000"",
                    ""low"": ""50000"",
                    ""high"": ""51000"",
                    ""open"": ""50200"",
                    ""close"": ""50800"",
                    ""volume"": ""200.3""
                }
            ]
        }";

        // Act
        var result = JsonSerializer.Deserialize<CandleResponse>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Candles.Should().HaveCount(2);

        var firstCandle = result.Candles.First();
        firstCandle.Start.Should().Be("1704110400");
        firstCandle.Low.Should().Be("49500");
        firstCandle.High.Should().Be("50500");
        firstCandle.Open.Should().Be("49800");
        firstCandle.Close.Should().Be("50200");
        firstCandle.Volume.Should().Be("150.5");
    }

    #endregion

    #region Portfolio Model Tests

    [Fact]
    public void AdvancedTradePortfolio_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""uuid"": ""portfolio-123"",
            ""name"": ""Default Portfolio"",
            ""type"": ""DEFAULT"",
            ""deleted"": false
        }";

        // Act
        var result = JsonSerializer.Deserialize<AdvancedTradePortfolio>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Uuid.Should().Be("portfolio-123");
        result.Name.Should().Be("Default Portfolio");
        result.Type.Should().Be("DEFAULT");
        result.Deleted.Should().BeFalse();
    }

    [Fact]
    public void PortfolioBreakdown_DeserializesCorrectly()
    {
        // Arrange
        var json = @"{
            ""portfolio_balances"": {
                ""total_balance"": {
                    ""value"": ""10000.00"",
                    ""currency"": ""USD""
                },
                ""total_futures_balance"": {
                    ""value"": ""2000.00"",
                    ""currency"": ""USD""
                },
                ""total_cash_equivalent_balance"": {
                    ""value"": ""8000.00"",
                    ""currency"": ""USD""
                },
                ""total_crypto_balance"": {
                    ""value"": ""7000.00"",
                    ""currency"": ""USD""
                },
                ""futures_unrealized_pnl"": {
                    ""value"": ""100.00"",
                    ""currency"": ""USD""
                },
                ""perp_unrealized_pnl"": {
                    ""value"": ""50.00"",
                    ""currency"": ""USD""
                }
            },
            ""spot_positions"": [
                {
                    ""asset"": ""BTC"",
                    ""account_uuid"": ""account-123"",
                    ""total_balance_fiat"": 5000.00,
                    ""total_balance_crypto"": 0.1,
                    ""available_to_trade_fiat"": 4500.00,
                    ""allocation"": 0.5
                }
            ]
        }";

        // Act
        var result = JsonSerializer.Deserialize<AdvancedTradePortfolioBreakdown>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.PortfolioBalances.Should().NotBeNull();
        result.PortfolioBalances!.TotalBalance!.Value.Should().Be("10000.00");
        result.PortfolioBalances.TotalCryptoBalance!.Value.Should().Be("7000.00");

        result.SpotPositions.Should().HaveCount(1);
        var spotPosition = result.SpotPositions.First();
        spotPosition.Asset.Should().Be("BTC");
        spotPosition.TotalBalanceFiat.Should().Be(5000.00m);
        spotPosition.TotalBalanceCrypto.Should().Be(0.1m);
        spotPosition.Allocation.Should().Be(0.5m);
    }

    #endregion

    #region Edge Cases and Null Handling

    [Fact]
    public void Models_HandleNullValues_Gracefully()
    {
        // Arrange
        var json = @"{
            ""uuid"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
            ""currency"": ""BTC"",
            ""available_balance"": null,
            ""name"": null,
            ""hold"": null
        }";

        // Act
        var result = JsonSerializer.Deserialize<CoinbaseAccount>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.AvailableBalance.Should().BeNull();
        result.Name.Should().BeNull();
        result.Hold.Should().BeNull();
        result.Currency.Should().Be("BTC");
    }

    [Fact]
    public void Models_HandleEmptyArrays_Correctly()
    {
        // Arrange
        var json = @"{
            ""orders"": [],
            ""has_next"": false,
            ""cursor"": null,
            ""sequence"": null
        }";

        // Act
        var result = JsonSerializer.Deserialize<GetOrdersResponse>(json, _jsonOptions);

        // Assert
        result.Should().NotBeNull();
        result!.Orders.Should().NotBeNull();
        result.Orders.Should().BeEmpty();
        result.HasNext.Should().BeFalse();
        result.Cursor.Should().BeNull();
    }

    #endregion
}