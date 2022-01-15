using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class UtilityService : IUtilityService
    {
        IEmaService _emaService;
        public UtilityService(IEmaService emaService)
        {
            _emaService = emaService;
        }
        public double GetMeasuringStick(List<Candle> candles)
        {
            return _emaService.CalculateEma(GetBodyLengthList(candles), 50).Last();
        }

        List<double> GetBodyLengthList(List<Candle> candles)
        {
            return candles.Select(c => GetBodyLength(c)).ToList();
        }

        public double GetBodyLength(Candle candle)
        {
            return Math.Abs(candle.Mid.O - candle.Mid.C);
        }

        public int GetDecimalPrecision2(double x)
        {
            var precision = 0;

            while (x * Math.Pow(10, precision) !=
                     Math.Round(x * Math.Pow(10, precision)))
                precision++;

            return precision;

        }

        public bool IsBullish(Candle candle)
        {
            if (candle.Mid.C > candle.Mid.O) return true;
            else return false;
        }
        public bool IsBearish(Candle candle)
        {
            if (candle.Mid.C < candle.Mid.O) return true;
            else return false;
        }

        public bool IsCut(Candle candle, double indicator)
        {
            if (IsBullish(candle))
            {
                if (indicator < candle.Mid.C && indicator > candle.Mid.O)
                {
                    return true;
                }
            }

            if (IsBearish(candle))
            {
                if (indicator > candle.Mid.C && indicator < candle.Mid.O)
                {
                    return true;
                }
            }
            return false;
        }

        public double GetBodyRatio(Candle candle1, Candle candle2)
        {
            return Math.Round((GetBodyLength(candle2) / GetBodyLength(candle1)), 2);
        }


    }
}
