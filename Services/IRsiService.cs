using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IRsiService
    {
        List<double> CalculateRawRsi2(List<Candle> candles, int period);
    }
}