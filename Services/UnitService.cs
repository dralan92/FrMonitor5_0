using FrMonitor4_0.Models;
using System;

namespace FrMonitor4_0.Services
{
    public class UnitService : IUnitService
    {
        IPriceService _priceService;
        public UnitService(IPriceService priceService)
        {
            _priceService = priceService;
        }
        double GetPipValue(Instrument instrument)
        {
            var parsed = int.TryParse(instrument.PipLocation, out int pipLocation);
            if (parsed)
            {
                if (pipLocation < 0)
                {
                    return Math.Pow(10, pipLocation);
                }
                else
                {
                    return Math.Pow(10, pipLocation - 1);
                }
            }
            return 0.0;
        }
        public double CalculateUnitsUniversal(Instrument instrument, double riskAmount, double slLength, AccountDetail accountDetail)
        {
            var quoteItem = instrument.Name.Split("_")[1];// What we are gonna spend
            var baseItem = instrument.Name.Split("_")[0];//What we are gonna buy
            var valueOfPipFor1Lot = 0.0;//magic value1 (USD)

            var askPrice = _priceService.GetPriceListRaw(instrument.Name, accountDetail).Prices[0].CloseOutAsk;
            var aLotOfBaseItemCosts = askPrice * 100000;// this much quoteItem
            var pv = GetPipValue(instrument);
            var aLotOfBaseItemCosts_After1PipRise = (askPrice + pv) * 100000;// this much quoteItem
            var priceDiff_1Pip1Lot = aLotOfBaseItemCosts_After1PipRise - aLotOfBaseItemCosts; //this much quoteItem

            //Since our account is in USD, convert "priceDiff_1Pip1Lot" to USD
            if (quoteItem == "USD")
            {
                valueOfPipFor1Lot = priceDiff_1Pip1Lot;
            }
            else // convert quote item to USD
            {
                var quoteUsdCr = 0.0;
                var quoteUsdPair = string.Join("_", quoteItem, "USD");
                var price1 = _priceService.GetPriceListRaw(quoteUsdPair, accountDetail);
                if (price1.Prices != null)
                {
                    quoteUsdCr = price1.Prices[0].CloseOutAsk;
                }
                else
                {
                    var usdQuotePair = string.Join("_", "USD", quoteItem);
                    price1 = _priceService.GetPriceListRaw(usdQuotePair, accountDetail);
                    if (price1.Prices != null)
                    {
                        quoteUsdCr = 1 / price1.Prices[0].CloseOutAsk;
                    }
                }

                valueOfPipFor1Lot = priceDiff_1Pip1Lot * quoteUsdCr;

            }

            var riskAmountFor1Lot = slLength * valueOfPipFor1Lot;
            var riskAmountWeWant = riskAmount;

            var reductionFactor = riskAmountWeWant / riskAmountFor1Lot;//magic value2

            var unitsWeWant = Math.Round(100000 * reductionFactor, 1);

            Console.WriteLine(instrument.Name + " --> " + unitsWeWant);

            return unitsWeWant;
        }
    }
}
