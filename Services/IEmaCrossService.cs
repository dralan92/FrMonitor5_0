using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IEmaCrossService
    {
        void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles);
        void CheckForExit(Instrument instrument, int timeframe, List<Candle> candles);
    }
}