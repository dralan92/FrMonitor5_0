using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class RsiDivergenceService : IRsiDivergenceService
    {
        IRsiService _rsiService;
        IEmaService _emaService;
        IUtilityService _utilityService;
        ITradingService _tradingService;
        public RsiDivergenceService(IRsiService rsiService, IEmaService emaService, IUtilityService utilityService, ITradingService tradingService)
        {
            _rsiService = rsiService;
            _emaService = emaService;
            _utilityService = utilityService;
            _tradingService = tradingService;
        }
        public void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles)
        {
            var rsiList = _rsiService.CalculateRawRsi2(candles, 14);
            var last3rsi = rsiList.Skip(Math.Max(0, rsiList.Count() - 3)).ToList();
            var peakAndLows = AlternatePeaksAndLaws(CleanedPeakAndLow(candles));

            if (last3rsi[2] < 50 &&  last3rsi[1] > 50)
            {
                var lastPeak = peakAndLows[0];
                var secondLastPeak = peakAndLows[2];

                if (lastPeak.PV == "PEAK")
                {
                    var lastPeakRsi = rsiList[candles.FindIndex(c => c.Time == lastPeak.Candle.Time)];
                    var secondLastPeakRsi = rsiList[candles.FindIndex(c => c.Time == secondLastPeak.Candle.Time)];
                    if(lastPeakRsi >= 70)
                    {
                        var stick = _utilityService.GetMeasuringStick(candles);
                        var sl = lastPeak.Candle.Mid.H;
                        _tradingService.PlaceTradeWithRisk4(instrument, candles.Last(), candles.Last().Mid.C, sl, false, 5, timeframe, stick);

                        File.AppendAllText("AlanLog/RsiDivergence.txt",
                       "SELL "+ timeframe +" - " + instrument.Name + " | " + DateTime.Now + "\n" +
                       "Last Peak--> " + DateTime.Parse(lastPeak.Candle.Time) + "\n" +
                       "Last Peak Rsi--> " + lastPeakRsi + "\n" +
                       "Second last Peak--> " + DateTime.Parse(secondLastPeak.Candle.Time) + "\n" +
                       "Second Last Peak Rsi--> " + secondLastPeakRsi + "\n" +
                       "============================================\n"
                       );
                    }
                   

                }
            }

            if (last3rsi[2] > 50 && last3rsi[1]< 50)
            {
                var lastLow = peakAndLows[0];
                var secondLastLow = peakAndLows[2];

                if (lastLow.PV == "LOW")
                {
                    var lastLowRsi = rsiList[candles.FindIndex(c => c.Time == lastLow.Candle.Time)];
                    var secondLastLowRsi = rsiList[candles.FindIndex(c => c.Time == secondLastLow.Candle.Time)];
                    if(lastLowRsi <= 30)
                    {
                        var stick = _utilityService.GetMeasuringStick(candles);
                        var sl = lastLow.Candle.Mid.L;
                        _tradingService.PlaceTradeWithRisk4(instrument, candles.Last(), candles.Last().Mid.C, sl, true, 5, timeframe, stick);

                        File.AppendAllText("AlanLog/RsiDivergence.txt",
                       "BUY " + timeframe + " - " + instrument.Name + " | " + DateTime.Now + "\n" +
                       "Last Low--> " + DateTime.Parse(lastLow.Candle.Time) + "\n" +
                       "Last Low Rsi--> " + lastLowRsi + "\n" +
                       "Second last Low --> " + DateTime.Parse(secondLastLow.Candle.Time) + "\n" +
                       "Second Last Low Rsi--> " + secondLastLowRsi + "\n" +
                       "============================================\n"
                       );
                    }
                   

                }
            }
        }
        public List<CandlePlus> AlternatePeaksAndLaws(List<CandlePlus> cpList)
        {
            var resultList = new List<CandlePlus>();
            for (var i = cpList.Count() - 1; i >= 1;)
            {
                if (resultList.Count() >= 8) break;
                if (cpList[i].PV != cpList[i - 1].PV)
                {
                    resultList.Add(cpList[i]);
                    i--;
                }
                else
                {
                    var j = i;
                    var tempList = new List<CandlePlus>();
                    while (cpList[j].PV == cpList[j - 1].PV)
                    {
                        tempList.Add(cpList[j]);
                        j--;
                    }
                    tempList.Add(cpList[j]);

                    var result = cpList[j].PV == "PEAK" ?
                        tempList.OrderBy(l => l.Candle.Mid.H).Last() :
                        tempList.OrderBy(l => l.Candle.Mid.L).First();
                    resultList.Add(result);
                    i -= tempList.Count();

                }
            }
            return resultList;
        }
        public List<CandlePlus> CleanUpLows(List<CandlePlus> cpPeakList)
        {
            return LowCandleTimeStamps(cpPeakList.Select(cp => cp.Candle).ToList());
        }
        public List<CandlePlus> LowCandleTimeStamps(List<Candle> candles)
        {
            var peakTs = new List<DateTime>();
            var valleyTs = new List<CandlePlus>();

            var lowList = candles.Select(c => c.Mid.L).ToList();
            var ema_8 = _emaService.CalculateEma(lowList, 8);

            bool directionUp = candles[0].Mid.L <= candles[1].Mid.L;
            for (int i = 1; i < candles.Count() - 1; i++)
            {
                if (directionUp && candles[i + 1].Mid.L < candles[i].Mid.L)
                {
                    peakTs.Add(DateTime.Parse(candles[i].Time));
                    directionUp = false;
                }
                else if (!directionUp && candles[i + 1].Mid.L > candles[i].Mid.L)//&& candles[i].Mid.L < ema_8[i]
                {
                    valleyTs.Add(
                        new CandlePlus
                        {
                            Candle = candles[i],
                            PV = "LOW"
                        }
                        );
                    directionUp = true;

                }
            }
            return valleyTs;

        }

        public List<CandlePlus> CleanedPeakAndLow(List<Candle> candles)
        {
            return CleanUpPeaks(PeakCandleTimeStamps(candles)).Concat(CleanUpLows(LowCandleTimeStamps(candles))).OrderBy(cp => cp.Candle.Time).ToList();
        }
        public List<CandlePlus> CleanUpPeaks(List<CandlePlus> cpPeakList)
        {
            return PeakCandleTimeStamps(cpPeakList.Select(cp => cp.Candle).ToList());
        }
        public List<CandlePlus> PeakCandleTimeStamps(List<Candle> candles)
        {
            var peakTs = new List<CandlePlus>();
            var valleyTs = new List<DateTime>();

            var highList = candles.Select(c => c.Mid.H).ToList();
            var ema_8 = _emaService.CalculateEma(highList, 8);


            bool directionUp = candles[0].Mid.H <= candles[1].Mid.H;


            for (int i = 1; i < candles.Count() - 1; i++)
            {
                if (directionUp && candles[i + 1].Mid.H < candles[i].Mid.H)// && candles[i].Mid.H > ema_8[i]
                {
                    peakTs.Add(
                        new CandlePlus
                        {
                            Candle = candles[i],
                            PV = "PEAK"
                        }
                        );
                    directionUp = false;
                }
                else if (!directionUp && candles[i + 1].Mid.H > candles[i].Mid.H)
                {
                    valleyTs.Add(DateTime.Parse(candles[i].Time));
                    directionUp = true;

                }
            }
            return peakTs;
        }
    }
}
