using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IUtilityService
    {
        double GetMeasuringStick(List<Candle> candles);
        double GetBodyLength(Candle candle);

        int GetDecimalPrecision2(double x);

        bool IsCut(Candle candle, double indicator);

        bool IsBullish(Candle candle);

        bool IsBearish(Candle candle);

        double GetBodyRatio(Candle candle1, Candle candle2);


    }
}