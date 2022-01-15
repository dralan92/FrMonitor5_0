using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IDeloreanService
    {
        void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candle);

        void CheckForExit(Instrument instrument, int timeframe, List<Candle> candle);
    }
}