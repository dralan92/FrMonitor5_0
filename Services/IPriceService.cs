using FrMonitor4_0.Models;

namespace FrMonitor4_0.Services
{
    public interface IPriceService
    {
        PriceList GetPriceList(Instrument instrument, AccountDetail accountDetail);

        PriceList GetPriceListRaw(string instrumentName, AccountDetail accountDetail);
    }
}