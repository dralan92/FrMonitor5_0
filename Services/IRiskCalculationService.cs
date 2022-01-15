using FrMonitor4_0.Models;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public interface IRiskCalculationService
    {
        double CalculateUnits(Instrument instrument);
    }
}