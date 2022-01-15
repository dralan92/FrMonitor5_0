using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class BollingerBandService : IBollingerBandService
    {
        public static IMovingAverageService _movingAverageService;
        public static readonly object _object = new object();
        public BollingerBandService(IMovingAverageService movingAverageService)
        {
            _movingAverageService = movingAverageService;
        }


        public List<BollingerBand> GenerateBolingerData_Raw(List<double> values, int period, double factor)
        {
            var smaList = GenerateSmaList(values, period);
            var sdList = GenerateSdList(values, smaList, period);
            var bbList = GenerateBBList(smaList, sdList, period, factor);
            return bbList;

        }

        List<double> GenerateSmaList(List<double> closeList, int period)
        {
            var smaList = new List<double>();
            for (var i = 0; i < period - 1; i++)
            {
                smaList.Add(0.0);
            }

            for (var i = period - 1; i < closeList.Count; i++)
            {
                var sma = closeList.GetRange((i + 1) - period, period).Average();
                smaList.Add(sma);
            }

            return smaList;
        }

        List<double> GenerateSdList(List<double> closeList, List<double> smaList, int period)
        {
            var sdList = new List<double>();
            try
            {
                for (var i = 0; i < period - 1; i++)
                {
                    sdList.Add(0.0);
                }

                for (var i = 0; i < closeList.Count; i++)
                {
                    #region Sub list with first "period" entries and SMA for the sublist
                    var tempList = closeList.GetRange(i, period);
                    var tempSma = smaList[i + period - 1];
                    #endregion
                    var sum = 0.0;
                    for (var j = 0; j < tempList.Count; j++)
                    {
                        sum += Math.Pow((tempList[j] - tempSma), 2);
                    }
                    var variance = sum / period;
                    var sd = Math.Sqrt(variance);
                    sdList.Add(sd);
                }

            }
            catch (Exception ex)
            {

            }

            return sdList;
        }

        List<BollingerBand> GenerateBBList(List<double> smaList, List<double> sdList, int period, double factor)
        {
            var bbList = new List<BollingerBand>();
            try
            {
                for (var i = 0; i < period - 1; i++)
                {
                    var bb = new BollingerBand()
                    {
                        LowerBolingerBand = 0,
                        SimpleMovingAverage = 0,
                        StandardDeviation = 0,
                        UpperBolingerBand = 0
                    };
                    bbList.Add(bb);
                }

                for (var i = period - 1; i < sdList.Count; i++)
                {
                    var lowerBB = smaList[i] - sdList[i] * factor;
                    var upperBB = smaList[i] + sdList[i] * factor;
                    var sma = smaList[i];
                    var sd = sdList[i];

                    var bb = new BollingerBand()
                    {
                        LowerBolingerBand = lowerBB,
                        SimpleMovingAverage = sma,
                        StandardDeviation = sd,
                        UpperBolingerBand = upperBB
                    };
                    bbList.Add(bb);
                }
            }
            catch (Exception ex)
            {

            }

            return bbList;
        }
    }
}
