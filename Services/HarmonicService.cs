using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class HarmonicService : IHarmonicService
    {
        IEmaService _emaService;
        IBollingerBandService _bollingerBandService;
        IUtilityService _utilityService;
        IBoxService _boxService;
        ITradingService _tradingService;
        public HarmonicService(IEmaService emaService, IBollingerBandService bollingerBandService, IUtilityService utilityService,
            IBoxService boxService, ITradingService tradingService)
        {
            _emaService = emaService;
            _bollingerBandService = bollingerBandService;
            _utilityService = utilityService;
            _boxService = boxService;
            _tradingService = tradingService;
        }
        string DisplayPattern(CandlePlus x, CandlePlus a, CandlePlus b, CandlePlus c, CandlePlus d)
        {
            return "\nX-->" + DateTime.Parse(x.Candle.Time) + "\n" + "A-->" + DateTime.Parse(a.Candle.Time) + "\n" + "B-->" + DateTime.Parse(b.Candle.Time) + "\n" +
                "C-->" + DateTime.Parse(c.Candle.Time) + "\n" + "D-->" + DateTime.Parse(d.Candle.Time) + "\n";
        }

        sbyte Ema50_100_150_Confirmation(double ema50, double ema100, double ema150)
        {
            if (ema50 > ema100 && ema100 > ema150) return 1;
            if (ema50 < ema100 && ema100 < ema150) return 3;
            return 2;
        }
        public void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles)
        {
            try
            {
                var peakAndLows = AlternatePeaksAndLaws(CleanedPeakAndLow(candles));
                var bb = _bollingerBandService.GenerateBolingerData_Raw(candles.Select(c => c.Mid.C).ToList(), 20, 2);
                var closeList = candles.Select(c => c.Mid.C).ToList();

                //DivergenceDetector(instrument, candles, bb, peakAndLows, lcrs, timespan);

                var point_D = peakAndLows[0];
                var point_C = peakAndLows[1];
                var point_B = peakAndLows[2];
                var point_A = peakAndLows[3];
                var point_X = peakAndLows[4];

                var retrace_XB = 0.0;
                var retrace_XD = 0.0;

                #region Look For Buy pattern
                if (point_C.PV == "PEAK")
                {
                    var XA = point_A.Candle.Mid.H - point_X.Candle.Mid.L;
                    var AB = point_A.Candle.Mid.H - point_B.Candle.Mid.L;
                    retrace_XB = AB / XA;

                    var AD = point_A.Candle.Mid.H - point_D.Candle.Mid.L;
                    retrace_XD = AD / XA;


                    if (retrace_XB > 0.38 && retrace_XB < 0.78 && retrace_XD > 0.78)
                    {
                        var stick = _utilityService.GetMeasuringStick(candles);
                        var lcrs = _utilityService.GetBodyLength(candles.Last()) / stick;
                        var dBB = bb[candles.FindIndex(c => c.Time == point_D.Candle.Time)];
                        var bbp = (dBB.LowerBolingerBand - point_D.Candle.Mid.L) / stick;

                        #region Write Buy pattern to file

                        File.AppendAllText(@"AlanLog\Harmonic.txt", "BUY " + instrument.Name + "[" + lcrs + "]" + DateTime.Now +
                            DisplayPattern(point_X, point_A, point_B, point_C, point_D) + "BBP:" + bbp + "\n" + Environment.NewLine);

                        if (bbp > 0)
                        {
                            File.AppendAllText(@"AlanLog\Harmonic.txt", "Checking if already in file: " + instrument.Name
                             + "||" + DateTime.Now + Environment.NewLine);
                            if (_boxService.GetHarmonicPatternDPoint(instrument.Name).Name == null)
                            {
                                File.AppendAllText(@"AlanLog\Harmonic.txt", "Not Prsent, new entry: " + instrument.Name
                            + "||" + DateTime.Now + Environment.NewLine
                                    + "\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n");
                                _boxService.AddHarmonicPatternDPoint(
                                    new HarmonicPatternDPoint
                                    {
                                        dTime = point_D.Candle.Time,
                                        Look4 = "buy",
                                        Name = instrument.Name
                                    });
                            }
                        }



                        #endregion
                    }
                }
                #endregion

                #region Look For Sell pattern
                if (point_C.PV == "LOW")
                {
                    var XA = point_X.Candle.Mid.H - point_A.Candle.Mid.L;
                    var AB = point_B.Candle.Mid.H - point_A.Candle.Mid.L;
                    retrace_XB = AB / XA;

                    var AD = point_D.Candle.Mid.H - point_A.Candle.Mid.L;
                    retrace_XD = AD / XA;


                    if (retrace_XB > 0.38 && retrace_XB < 0.78 && retrace_XD > 0.78)
                    {
                        var stick = _utilityService.GetMeasuringStick(candles);
                        var lcrs = _utilityService.GetBodyLength(candles.Last()) / stick;

                        var dBB = bb[candles.FindIndex(c => c.Time == point_D.Candle.Time)];
                        var bbp = (point_D.Candle.Mid.H - dBB.UpperBolingerBand) / stick;

                        #region Write Sell pattern to file

                        File.AppendAllText(@"AlanLog\Harmonic.txt", "SELL " + instrument.Name + "[" + lcrs + "]" + DateTime.Now +
                            DisplayPattern(point_X, point_A, point_B, point_C, point_D) + "BBP:" + bbp + "\n" + Environment.NewLine);

                        if (bbp > 0)
                        {
                            File.AppendAllText(@"AlanLog\Harmonic.txt", "Checking if already in file: " + instrument.Name
                             + "||" + DateTime.Now + Environment.NewLine);
                            if (_boxService.GetHarmonicPatternDPoint(instrument.Name).Name == null)
                            {
                                File.AppendAllText(@"AlanLog\Harmonic.txt", "Not Prsent, new entry: " + instrument.Name
                            + "||" + DateTime.Now + Environment.NewLine
                                    + "\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n");
                                _boxService.AddHarmonicPatternDPoint(
                                    new HarmonicPatternDPoint
                                    {
                                        dTime = point_D.Candle.Time,
                                        Look4 = "sell",
                                        Name = instrument.Name
                                    });
                            }
                        }

                        #endregion
                    }
                }
                #endregion

                #region File look up

                var hpdp = _boxService.GetHarmonicPatternDPoint(instrument.Name);
                if (hpdp.Name != null)
                {
                    var dCandleIndex = candles.FindIndex(c => c.Time == hpdp.dTime);
                    File.AppendAllText(@"AlanLog\Harmonic_D_Index.txt", hpdp.Name + " DCandle Index--> " + dCandleIndex +
                        " Total Candles --> " + candles.Count() +
                        DateTime.Now + Environment.NewLine);

                    if (dCandleIndex < candles.Count() && dCandleIndex > 0)
                    {
                        var dCandle = candles[dCandleIndex];
                        var lastCandle = candles.Last();
                        File.AppendAllText(@"AlanLog\HarmonicExecution.txt", hpdp.Name + " DCandle--> " + DateTime.Parse(dCandle.Time) +
                            " Last Candle --> " + DateTime.Parse(candles.Last().Time) +
                            DateTime.Now + Environment.NewLine);

                        //NOTE: candles D is included, but not last candle,  in the below list
                        var candlesAfterD = candles.Skip(dCandleIndex).Take(candles.Count() - 1 - dCandleIndex).ToList();
                        File.AppendAllText(@"AlanLog\HarmonicExecution.txt", candlesAfterD.Count() +
                                 Environment.NewLine);
                        bool stillValid = true;
                        for (int i = 0; i < candlesAfterD.Count(); i++)
                        {
                            var index = candles.FindIndex(c => c.Time == candlesAfterD[i].Time);
                            //File.AppendAllText(@"AlanLog\HarmonicExecution.txt", DateTime.Parse(candlesAfterD[i].Time) +
                            //"BB SMA--> " + bb[index].SimpleMovingAverage + " |IsCUT-->" + IsCut(candlesAfterD[i], bb[index].SimpleMovingAverage)+
                            //Environment.NewLine);
                            if (_utilityService.IsCut(candlesAfterD[i], bb[index].SimpleMovingAverage))
                            {
                                stillValid = false;
                                _boxService.DeleteHarmonicPatternDPoint(hpdp.Name);
                            }
                        }
                        if (stillValid)
                        {
                            var ema_50 = _emaService.CalculateEma(closeList, 50).Last();
                            var ema_100 = _emaService.CalculateEma(closeList, 100).Last();
                            var ema_150 = _emaService.CalculateEma(closeList, 150).Last();

                            var threeEmaConfirmation = Ema50_100_150_Confirmation(ema_50, ema_100, ema_150);

                            var emaConfirmation = "";
                            if (threeEmaConfirmation == 1) emaConfirmation = " BUY "; else emaConfirmation = " SELL ";

                            File.AppendAllText(@"AlanLog\HarmonicExecution.txt", hpdp.Name + " STILL VALID FOR : " +
                                hpdp.Look4 + " 3 EMA Confirmation says : " + emaConfirmation +
                             DateTime.Now + Environment.NewLine);

                            if (hpdp.Look4 == "buy" && _utilityService.IsBullish(candles.Last()) && _utilityService.IsCut(candles.Last(), bb.Last().SimpleMovingAverage))
                            {
                                File.AppendAllText(@"AlanLog\HarmonicExecution.txt", hpdp.Name + "Buy Trade Placed");
                                _tradingService.PlaceTradeWithRisk3Values(instrument, candles.Last(), candles.Last().Mid.C,
                                    point_C.Candle.Mid.H, point_D.Candle.Mid.L, true, 2, 5, -1);
                            }

                            if (hpdp.Look4 == "sell" && _utilityService.IsBearish(candles.Last()) && _utilityService.IsCut(candles.Last(), bb.Last().SimpleMovingAverage))
                            {
                                File.AppendAllText(@"AlanLog\HarmonicExecution.txt", hpdp.Name + "Sell Trade Placed");
                                _tradingService.PlaceTradeWithRisk3Values(instrument, candles.Last(), candles.Last().Mid.C,
                                    point_C.Candle.Mid.L, point_D.Candle.Mid.H, false, 2, 5, -1);
                            }
                        }
                        File.AppendAllText(@"AlanLog\HarmonicExecution.txt", "\n++++++++++++++++++++++ Harmonic Exe Ends +++++++++++++++++++++\n");

                    }
                    else
                    {
                        _boxService.DeleteHarmonicPatternDPoint(hpdp.Name);
                    }





                }


                #endregion


            }
            catch (Exception ex)
            {
                File.AppendAllText(@"AlanLog\Exceptions.txt", ex.Message + "\n" + ex.StackTrace + Environment.NewLine);

            }
        }

        public void CheckForExit(Instrument instrument, int timeframe, List<Candle> candles)
        {
            File.AppendAllText("AlanLog/MonitorPlacedTrade.txt", "Inside CheckForExit for : " + instrument.Name + "\n");

            var placedTrade = _boxService.GetPlacedTrade(instrument.Name, timeframe);
            if (placedTrade.Count() > 0)
            {
                File.AppendAllText("AlanLog/MonitorPlacedTrade.txt", DateTime.Now + " | " + placedTrade.Count() +
                    " active trades found for " +
                    instrument.Name +
                    " at timeframe " + timeframe + "\n");

                CheckForClosure(placedTrade, instrument, candles);


            }
            else
            {
                File.AppendAllText("AlanLog/MonitorPlacedTrade.txt", "No Trades placed for for : " + instrument.Name + "\n");
            }
        }

        void CheckForClosure(List<PlacedTrade> placedTrades, Instrument instrument, List<Candle> candles)
        {
            foreach (var pt in placedTrades)
            {
                if (pt.StrategyNo == 2)
                {
                    CheckForClosure_Strategy_2(pt, instrument, candles);
                }
            }
        }

        void CheckForClosure_Strategy_2(PlacedTrade placedTrade, Instrument instrument, List<Candle> candles)
        {


            var closeList = candles.Select(c => c.Mid.C).ToList();

            var bb = _bollingerBandService.GenerateBolingerData_Raw(candles.Select(c => c.Mid.C).ToList(), 20, 2);
            var last3bb = bb.Skip(Math.Max(0, bb.Count() - 3)).ToList();

            var tradeCandleTime = DateTime.Parse(placedTrade.TradeCandleTime);
            var lastCandleTime = DateTime.Parse(candles.Last().Time);



            if (lastCandleTime > tradeCandleTime)
            {
                var index_lastCandle = candles.FindIndex(c => c.Time == candles.Last().Time);
                var index_tradeCandle = candles.FindIndex(c => c.Time == placedTrade.TradeCandleTime);
                var candlesAfterTrade = candles.Skip(index_tradeCandle).Take(index_lastCandle - index_tradeCandle);

                var message1 = "";

                message1 += "Candles completed after trade--> " + candlesAfterTrade.Count() + "\n" +
                                "Index(Last Candle)-->" + index_lastCandle + "\n" +
                                "Index(Trade Candle)-->" + index_tradeCandle + "\n";

                if (placedTrade.Action == "BUY")
                {
                    message1 += " Checking to close BUY trade \n" +
                        "Close(Last Candle) --> " + candles.Last().Mid.C + "\n" +
                        "BBSMA 20 --> " + last3bb[2].SimpleMovingAverage + "\n";
                    if (candles.Last().Mid.C < last3bb[2].SimpleMovingAverage)
                    {
                        message1 += " Latest candle has closed below BBSMA 20 closing the BUY trade \n";
                        _tradingService.CloseTrade(placedTrade.TradeId);
                        _boxService.DeletePlacedTrade(placedTrade);

                    }


                }
                else
                {
                    message1 += " Checking to close SELL trade \n" +
                        "Close(Last Candle) --> " + candles.Last().Mid.C + "\n" +
                        "BBSMA 20 --> " + last3bb[2].SimpleMovingAverage + "\n";

                    if (candles.Last().Mid.C > last3bb[2].SimpleMovingAverage)
                    {
                        message1 += " Latest candle has closed above 13 EMA closing the SELL trade \n";
                        _tradingService.CloseTrade(placedTrade.TradeId);
                        _boxService.DeletePlacedTrade(placedTrade);

                    }
                }

                File.AppendAllText("AlanLog/CheckForClosure_Strategy_2.txt", DateTime.Now + " | " + "Checking for : " + instrument.Name + " \n" +
                "Trade Candle Time: " + tradeCandleTime + "\n" +
                "Last Candle Time: " + lastCandleTime + "\n" +
                "Lastest BBSMA 20 : " + last3bb[2].SimpleMovingAverage + "\n" +
                message1 +
                "======================================================\n");


            }
            else
            {
                File.AppendAllText("AlanLog/CheckForClosure_Strategy_2.txt", DateTime.Now + " | " + "Checking for : " + instrument.Name + " \n" +
                "Trade Candle Time: " + tradeCandleTime + "\n" +
                "Last Candle Time: " + lastCandleTime + "\n" +
                placedTrade.Action + " trade just got placed \n" +
                "======================================================\n");
            }



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

    }
}
