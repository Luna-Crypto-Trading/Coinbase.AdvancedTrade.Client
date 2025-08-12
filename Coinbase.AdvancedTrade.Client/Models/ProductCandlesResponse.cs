namespace Coinbase.AdvancedTrade.Client.Models;

public class Candle
{
    public required string Start { get; set; }
    public required string Low { get; set; }
    public required string High { get; set; }
    public required string Open { get; set; }
    public required string Close { get; set; }
    public required string Volume { get; set; }
}

public class CandleResponse
{
    public required List<Candle> Candles { get; set; }
}