using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IBollingerBandService
    {
        List<BollingerBand> GenerateBolingerData_Raw(List<double> values, int period, double factor);
    }
}