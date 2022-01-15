using FrMonitor4_0.Models;

namespace FrMonitor4_0.Services
{
    public interface IUnitService
    {
        double CalculateUnitsUniversal(Instrument instrument, double riskAmount, double slLength, AccountDetail accountDetail);
    }
}