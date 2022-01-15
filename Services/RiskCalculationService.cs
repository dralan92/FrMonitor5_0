using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;

namespace FrMonitor4_0.Services
{
    public class RiskCalculationService : IRiskCalculationService
    {
        IUnitService _unitService;
        IPriceService _priceService;
        IAccountService _accountService;
        public RiskCalculationService(IUnitService unitService, IPriceService priceService, IAccountService accountService)
        {
            _unitService = unitService;
            _priceService = priceService;
            _accountService = accountService;

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

        double GetAccurateSpreadPips(double ask, double bid, Instrument instrument)
        {

            var spread = ask - bid;
            var pipValue = GetPipValue(instrument);
            return Math.Round((spread / pipValue), 2);
        }

        double StringToDouble(string input)
        {
            double result;
            bool inputParsed = Double.TryParse(input, out result);
            if (!inputParsed)
            {
                return -1;
            }
            return result;
        }
        public double CalculateUnits(Instrument instrument)
        {
            double units = -1;
            Console.WriteLine("Instrument Name: \n");
            var iname = Console.ReadLine();
            if (instrument.Name == iname)
            {

                Console.WriteLine("Stop Loss: \n");
                var sl = Console.ReadLine();
                var stopLoss = StringToDouble(sl);

                Console.WriteLine("Entry: \n");
                var e = Console.ReadLine();
                var entry = StringToDouble(e);

                Console.WriteLine("Amount to risk: \n");
                var a = Console.ReadLine();
                var amount = StringToDouble(a);

                var accountDetail = _accountService.GetAccountDetail();
                if (accountDetail != null)
                {
                    var price = _priceService.GetPriceList(instrument, accountDetail).Prices[0];
                    if (price.Instrument != null)
                    {
                        var ask = price.CloseOutAsk;
                        var bid = price.CloseOutBid;
                        var spread = ask - bid;

                        var sp = GetAccurateSpreadPips(ask, bid, instrument);
                        var pv = GetPipValue(instrument);

                        var risk = (Math.Abs(entry - stopLoss)) / pv;
                        var fullSpreadRisk = (Math.Abs(entry - stopLoss) + (spread)) / pv;
                        units = _unitService.CalculateUnitsUniversal(instrument, amount, fullSpreadRisk, accountDetail);
                    }


                }


            }
            else
            {
                return -1;
            }

            return units;

        }
    }
}
