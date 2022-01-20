using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IRsiDivergenceService
    {
        List<CandlePlus> AlternatePeaksAndLaws(List<CandlePlus> cpList);
        void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles);
        List<CandlePlus> CleanedPeakAndLow(List<Candle> candles);
        List<CandlePlus> CleanUpLows(List<CandlePlus> cpPeakList);
        List<CandlePlus> CleanUpPeaks(List<CandlePlus> cpPeakList);
        List<CandlePlus> LowCandleTimeStamps(List<Candle> candles);
        List<CandlePlus> PeakCandleTimeStamps(List<Candle> candles);
    }
}