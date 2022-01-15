using FrMonitor4_0.Models;
using System.Collections.Generic;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class RsiService : IRsiService
    {
        public List<double> CalculateRawRsi2(List<Candle> candles, int period)
        {
            var rsiList = new List<double>();
            var closeList = candles.Select(c => c.Mid.C).ToList();
            var upMoveList = new List<double>();
            var downMoveList = new List<double>();
            var avgUpMove = 0.0;
            var avgDownMove = 0.0;
            var avgUpMoveList = new List<double>();
            var avgDownMoveList = new List<double>();
            var rsList = new List<double>();
            for (var i = 1; i < candles.Count; i++)
            {
                if (closeList[i] >= closeList[i - 1])
                {
                    upMoveList.Add(closeList[i] - closeList[i - 1]);
                    downMoveList.Add(0);
                }
                else if (closeList[i] < closeList[i - 1])
                {
                    downMoveList.Add(closeList[i - 1] - closeList[i]);
                    upMoveList.Add(0);
                }
            }

            avgUpMoveList.Add(upMoveList.GetRange(0, period).Average());
            avgDownMoveList.Add(downMoveList.GetRange(0, period).Average());

            for (var i = period; i < closeList.Count - 1; i++)
            {
                avgUpMoveList.Add(((avgUpMoveList[avgUpMoveList.Count - 1] * (period - 1)) + upMoveList[i]) / period);
                avgDownMoveList.Add(((avgDownMoveList[avgDownMoveList.Count - 1] * (period - 1)) + downMoveList[i]) / period);
            }
            for (var i = 0; i < avgUpMoveList.Count; i++)
            {
                rsList.Add(avgUpMoveList[i] / avgDownMoveList[i]);

            }
            for (var i = 0; i < rsList.Count; i++)
            {
                var x = 100 / (1 + rsList[i]);
                rsiList.Add(100 - x);
            }

            for (var i = 0; i < period; i++)
            {
                rsiList.Insert(0, 0);
            }

            return rsiList;
        }
    }
}
