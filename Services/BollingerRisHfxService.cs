using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class BollingerRisHfxService : IBollingerRisHfxService
    {
        IRsiService _rsiService;
        IBollingerBandService _bandService;
        IUtilityService _utilityService;
        public BollingerRisHfxService(IRsiService rsiService, IBollingerBandService bollingerBandService, IUtilityService utilityService)
        {
            _rsiService = rsiService;
            _bandService = bollingerBandService;
            _utilityService = utilityService;
        }

        public void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles)
        {
            var rsiList = _rsiService.CalculateRawRsi2(candles, 13);
            if (rsiList.Last() < 20) //check for buy
            {
                var bb13 = _bandService.GenerateBolingerData_Raw(candles.Select(c => c.Mid.C).ToList(), 13, 2.2);
                var bb6 = _bandService.GenerateBolingerData_Raw(candles.Select(c => c.Mid.C).ToList(), 6, 2);
                var levelToBreak = Math.Min(bb13.Last().LowerBolingerBand, bb6.Last().LowerBolingerBand);
                if (candles.Last().Mid.L < levelToBreak)
                {
                    var wbr = GetWickToBodyRatio(candles.Last(), true);
                    File.AppendAllText("AlanLog/BollingerRisHfx.txt", "BUY " + instrument.Name + " WBR: " + wbr + " | " + DateTime.Now + "\n");
                }

            }
            if (rsiList.Last() > 80) //check for sell
            {
                var bb13 = _bandService.GenerateBolingerData_Raw(candles.Select(c => c.Mid.C).ToList(), 13, 2.2);
                var bb6 = _bandService.GenerateBolingerData_Raw(candles.Select(c => c.Mid.C).ToList(), 6, 2);
                var levelToBreak = Math.Max(bb13.Last().UpperBolingerBand, bb6.Last().UpperBolingerBand);
                if (candles.Last().Mid.H > levelToBreak)
                {
                    var wbr = GetWickToBodyRatio(candles.Last(), false);
                    File.AppendAllText("AlanLog/BollingerRisHfx.txt", "SELL " + instrument.Name + " WBR: " + wbr + " | " + DateTime.Now + "\n");

                }
            }
        }

        double GetWickToBodyRatio(Candle candle, bool isLowerWick)
        {
            var bodyLength = _utilityService.GetBodyLength(candle);
            double wickLength;
            if (isLowerWick)
            {
                if (_utilityService.IsBearish(candle))
                {
                    wickLength = Math.Abs(candle.Mid.C - candle.Mid.L);
                }
                else
                {
                    wickLength = Math.Abs(candle.Mid.O - candle.Mid.L);
                }
            }
            else
            {
                if (_utilityService.IsBearish(candle))
                {
                    wickLength = Math.Abs(candle.Mid.H - candle.Mid.O);
                }
                else
                {
                    wickLength = Math.Abs(candle.Mid.H - candle.Mid.C);
                }
            }
            return wickLength / bodyLength;
        }
    }
}
