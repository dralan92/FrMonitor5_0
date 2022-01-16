using System.Collections.Generic;

namespace FrMonitor4_0.Models
{
    

    public class MetaConfig
    {
        public string AccountNumber { get; set; }
        public string AuthHeader { get; set; }
        public string BaseUrl { get; set; }
        public string InstrumentsToMonitor { get; set; }
        public string TimeFrames { get; set; }
        public bool TargetReached { get; set; }
        public bool OneTrade { get; set; }
        public bool RiskCalculationMode { get; set; }
        public double RiskAmount { get; set; }
        public double OneTradeGain { get; set; }
        public double RequiredRr { get; set; }
        public double Target { get; set; }
        public sbyte StrategyNumber { get; set; }
    }

    public class InstrumentList
    {
        public List<Instrument> Instruments { get; set; }

    }

    public class HarmonicPatternDPoint
    {
        public string Name { get; set; }
        public string dTime { get; set; }
        public string Look4 { get; set; }


    }

    public class BollingerBand
    {
        public double SimpleMovingAverage { get; set; }
        public double StandardDeviation { get; set; }
        public double UpperBolingerBand { get; set; }
        public double LowerBolingerBand { get; set; }
    }

    public class Instrument
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string PipLocation { get; set; }
        public double MinimumTradeSize { get; set; }
    }

    public class CandlePlus
    {
        public Candle Candle { get; set; }

        public string PV { get; set; }

    }

    public class Candle
    {
        public double Volume { get; set; }
        public string Time { get; set; }
        public bool Complete { get; set; }
        public Mid Mid { get; set; }
    }

    public class Mid
    {
        public double O { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double C { get; set; }
    }

    public class CandleWindow
    {
        public List<Candle> Candles { get; set; }

    }

    public class AccountDetail
    {
        public string AccountId { get; set; }

        public string InstrumentPriceUri { get; set; }
        public string AuthHeader { get; set; }
        public string OrderUri { get; set; }

        public double Balance { get; set; }
        public int OpenTradeCount { get; set; }
        public int OpenPositionCount { get; set; }

    }

    public class PriceList
    {
        public List<Price> Prices { get; set; }

    }

    public class Price
    {
        public double CloseOutBid { get; set; }
        public double CloseOutAsk { get; set; }
        public UnitsAvailable UnitsAvailable { get; set; }
        public string Instrument { get; set; }
    }

    public class UnitsAvailable
    {
        public Default Default { get; set; }

    }

    public class Default
    {
        public double Long { get; set; }
        public double Short { get; set; }
    }

     public class PlacedTrade
    {
        public string InstrumentName { get; set; }
        public int Timeframe { get; set; }
        public sbyte StrategyNo { get; set; }
        public string TradeCandleTime { get; set; }
        public string TradeId { get; set; }
        public string Action { get; set; }

    }

    public class Position
    {
        public string instrument { get; set; }

    }

    public class OpenPositions
    {
        public List<Position> Positions { get; set; }
        public string LastTransactionId { get; set; }


    }
}
