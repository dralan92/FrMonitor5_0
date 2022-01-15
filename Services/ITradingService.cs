using FrMonitor4_0.Models;

namespace FrMonitor4_0.Services
{
    public interface ITradingService
    {
        void PlaceTradeWithRisk3Values(Instrument instrument, Candle lastCandle, double entry, double tp, double sl, bool isLong, sbyte strategyNo, int timeFrame, double stick);
        void PlaceTradeWithRisk4(Instrument instrument, Candle lastCandle, double entry, double stopLoss, bool isLong, sbyte strategyNo, int timeFrame, double stick);

        void CloseTrade(string tradeId);
        OpenPositions GetOpenPositions();
    }
}