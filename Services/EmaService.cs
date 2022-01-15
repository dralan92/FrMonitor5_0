using System.Collections.Generic;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class EmaService : IEmaService
    {
        public List<double> CalculateEma(List<double> input, int period)
        {
            var emaList = new List<double>();
            var multiplier = 2.0 / (period + 1);
            for (var i = 0; i < period - 1; i++)
            {
                emaList.Add(0.0);
            }
            var smaForFirstPeriod = input.GetRange(0, period).Average();
            emaList.Add(smaForFirstPeriod);
            for (var i = period; i < input.Count; i++)
            {
                var prevEma = emaList[i - 1];
                var ema = ((input[i] - prevEma) * multiplier) + prevEma;
                emaList.Add(ema);
            }
            return emaList;
        }
    }
}
