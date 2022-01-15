using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IEmaService
    {
        List<double> CalculateEma(List<double> input, int period);
    }
}