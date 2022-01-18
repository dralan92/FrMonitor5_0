using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class DeloreanService : IDeloreanService
    {
        sbyte _strategyNumber;
        IEmaService _emaService;
        IUtilityService _utilityService;
        ITradingService _tradingService;
        IBoxService _boxService;
        IMetaDataService _metaDataService;
        MetaConfig _metaConfig;
        public DeloreanService(IEmaService emaService, IUtilityService utilityService, ITradingService tradingService, IBoxService boxService,
            IMetaDataService metaDataService)
        {
            _strategyNumber = 1;
            _emaService = emaService;
            _utilityService = utilityService;
            _tradingService = tradingService;
            _boxService = boxService;
            _metaDataService = metaDataService;
            _metaConfig = _metaDataService.GetMetaConfig();
        }

        double GetEmaDistance(Candle candle, double ema)
        {
            if (_utilityService.IsCut(candle, ema))
            {
                return 0;
            }
            if (_utilityService.IsBullish(candle))
            {
                return candle.Mid.O - ema;
            }
            else
            {
                return ema - candle.Mid.O;
            }
        }

        void LogLastCandle(string instName, Candle lastCandle, double ema13)
        {
            var status = lastCandle.Complete ? "COMPLETED" : "INCOMPLETE";
            var cut = _utilityService.IsCut(lastCandle, ema13) ? "RED CUT" : "NO CUT";
            var line1 = instName + " last candle is " + status + "\n" +
                "H:" + lastCandle.Mid.H + "L:" + lastCandle.Mid.L + "O:" + lastCandle.Mid.O + "C:" + lastCandle.Mid.C + "\n";
            var ctime = DateTime.Parse(lastCandle.Time);
            var line2 = " Last candle creation time : " + ctime + "\n"; ;
            var line3 = " Current time : " + DateTime.Now + "\n"; ;
            var line4 = "+++++++++++++++++++" + cut + "++++++++++++++++++++++++" + "\n";
            File.AppendAllText(@"AlanLog\LastCandles.txt", line1 + line2 + line3 + line4 + Environment.NewLine);
        }

        string CandleSizeCheck(List<Candle> candles, double stick)
        {
            var fiveCandles = "\n{" + _utilityService.GetBodyLength(candles[0]) / stick +
                "|" + _utilityService.GetBodyLength(candles[1]) / stick + "|" + _utilityService.GetBodyLength(candles[2]) / stick + "|++" +
                _utilityService.GetBodyLength(candles[3]) / stick + "|" + _utilityService.GetBodyLength(candles[4]) / stick + "++}\n";

            return fiveCandles;
        }
        int GetDecimalPrecision(double input)
        {
            var parts = input.ToString().Split('.');
            int numberOfDecimalPlaces = 0;
            if (parts.Count() > 1)
            {
                numberOfDecimalPlaces = parts[1].Count();
            }

            return numberOfDecimalPlaces;
        }

        double GetStopLoss(Candle candle, double ema50, bool isLong, Instrument instrument)
        {
            var dp = GetDecimalPrecision(candle.Mid.C);
            dp = dp > 5 ? 5 : dp;
            if (isLong)
            {
                if (_utilityService.IsCut(candle, ema50))
                {
                    return candle.Mid.L;
                }
                else
                {
                    return Math.Round(ema50, dp);
                }
            }
            else
            {
                if (_utilityService.IsCut(candle, ema50))
                {
                    return candle.Mid.H;
                }
                else
                {
                    return Math.Round(ema50, dp);
                }
            }
        }
        bool TimeRight()
        {
            TimeSpan start = new TimeSpan(2, 0, 0); //2 o'clock
            TimeSpan end = new TimeSpan(10, 0, 0); //10 o'clock
            TimeSpan now = DateTime.Now.TimeOfDay;

            if ((now > start) && (now < end))
            {
                return true;
            }
            return false;
        }

        public void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles)
        {
            try
            {
                if (TimeRight())
                {
                    var closeList = candles.Select(c => c.Mid.C).ToList();

                    var last5Candles = candles.Skip(Math.Max(0, candles.Count() - 5)).ToList();

                    var ema_5 = _emaService.CalculateEma(closeList, 5);
                    var last3ema_5 = ema_5.Skip(Math.Max(0, ema_5.Count() - 3)).ToList();

                    var ema_13 = _emaService.CalculateEma(closeList, 13);
                    var last3ema_13 = ema_13.Skip(Math.Max(0, ema_13.Count() - 3)).ToList();

                    var ema_50 = _emaService.CalculateEma(closeList, 50);
                    var last3ema_50 = ema_50.Skip(Math.Max(0, ema_50.Count() - 3)).ToList();

                    var ema_200 = _emaService.CalculateEma(closeList, 200);
                    var last3ema_200 = ema_200.Skip(Math.Max(0, ema_200.Count() - 3)).ToList();

                    var ema_800 = _emaService.CalculateEma(closeList, 800);
                    var last3ema_800 = ema_800.Skip(Math.Max(0, ema_800.Count() - 3)).ToList();

                    var stick = _utilityService.GetMeasuringStick(candles);

                    var lastCandleRelativeSize = Math.Round(_utilityService.GetBodyLength(last5Candles[4]) / stick, 2);

                    var dp = _utilityService.GetDecimalPrecision2(last5Candles[4].Mid.C);
                    dp = dp > 5 ? 5 : dp;

                    #region Delorean
                    //if (CandleSizeCheckConfirmation(last5Candles, stick))
                    {
                        if (_utilityService.IsCut(last5Candles[4], last3ema_13[2]) && _utilityService.IsCut(last5Candles[3], last3ema_13[1]))
                        {
                            if (_utilityService.IsBullish(last5Candles[4]) && _utilityService.IsBearish(last5Candles[3]) && _utilityService.IsBearish(last5Candles[2]))//BUY
                            {
                                var br = _utilityService.GetBodyRatio(last5Candles[3], last5Candles[4]);
                                if (br > 1)
                                {
                                    var ema50Dist = Math.Round(GetEmaDistance(last5Candles[4], last3ema_50[2]) / stick, 2);
                                    var ema200Dist = Math.Round(GetEmaDistance(last5Candles[4], last3ema_200[2]) / stick, 2);
                                    var ema800Dist = Math.Round(GetEmaDistance(last5Candles[4], last3ema_800[2]) / stick, 2);
                                    var emaGap = Math.Abs(ema800Dist - ema200Dist);

                                    if (ema50Dist < 2)
                                    {
                                        #region CASE1--> BTS1: target: Blue, sl: aqua + grey

                                        if (ema50Dist > 0 && ema200Dist >= 0 && ema800Dist < 0)
                                        {
                                            LogLastCandle(instrument.Name, last5Candles[4], last3ema_13[2]);

                                            var message = "----------CASE1: BTS--------------\n";
                                            message += "BUY " + instrument.Name + "|BR:" + br + "|RLC:" + lastCandleRelativeSize + "|50EMA:" + ema50Dist + "|200EMA:" + ema200Dist + "|800EMA:" + ema800Dist + "|" + DateTime.Now +
                                                CandleSizeCheck(last5Candles, stick);
                                            File.AppendAllText(@"AlanLog\DEL_" + timeframe + "_" + instrument.Name + ".txt", message + Environment.NewLine);

                                            var tp = Math.Round(last3ema_800[2], dp);
                                            var sl = Math.Round(last3ema_50[2], dp);
                                            _tradingService.PlaceTradeWithRisk3Values(instrument, last5Candles[4], last5Candles[4].Mid.C, tp, sl, true, 1, timeframe, stick);

                                        }
                                        #endregion

                                        #region CASE2 --> trend is friend
                                        if (ema50Dist > 0 && ema200Dist > 0 && ema800Dist > 0)
                                        {
                                            LogLastCandle(instrument.Name, last5Candles[4], last3ema_13[2]);

                                            var aqGrGap = Math.Abs(ema50Dist - ema200Dist);
                                            var GrBlGap = Math.Abs(ema800Dist - ema200Dist);
                                            var message = "----------CASE2: Trend is Friend--------------\n";
                                            message += "BUY " + instrument.Name + "|BR:" + br + "|RLC:" + lastCandleRelativeSize + "|50EMA:" + ema50Dist + "|200EMA:" + ema200Dist + "|800EMA:" + ema800Dist + "|" +
                                               "|Aqua Grey Gap:" + aqGrGap + "|Grey Blue Gap:" + GrBlGap + "|" + DateTime.Now +
                                               CandleSizeCheck(last5Candles, stick);
                                            File.AppendAllText(@"AlanLog\DEL_" + timeframe + "_" + instrument.Name + ".txt", message + Environment.NewLine);
                                            var stopLoss = GetStopLoss(last5Candles[4], last3ema_50[2], true, instrument);

                                            _tradingService.PlaceTradeWithRisk4(instrument, last5Candles[4], last5Candles[4].Mid.C, stopLoss, true, 1, timeframe, stick);
                                        }

                                        #endregion

                                        #region CASE3--> BTS2: target: Blue + Grey, sl: aqua 

                                        if (ema50Dist > 0 && ema200Dist < 0 && ema800Dist < 0 && Math.Abs(ema200Dist) < Math.Abs(ema800Dist))
                                        {
                                            LogLastCandle(instrument.Name, last5Candles[4], last3ema_13[2]);

                                            var message = "----------CASE2: BTS--------------\n";
                                            message += "BUY " + instrument.Name + "|BR:" + br + "|RLC:" + lastCandleRelativeSize + "|50EMA:" + ema50Dist + "|200EMA:" + ema200Dist + "|800EMA:" + ema800Dist + "|" + DateTime.Now +
                                                CandleSizeCheck(last5Candles, stick);
                                            File.AppendAllText(@"AlanLog\DEL_" + timeframe + "_" + instrument.Name + ".txt", message + Environment.NewLine);

                                            var tp = Math.Round(last3ema_200[2], dp);
                                            var sl = Math.Round(last3ema_50[2], dp);

                                            _tradingService.PlaceTradeWithRisk3Values(instrument, last5Candles[4], last5Candles[4].Mid.C, tp, sl, true, 1, timeframe, stick);

                                        }
                                        #endregion
                                    }


                                }


                            }
                            if (_utilityService.IsBearish(last5Candles[4]) && _utilityService.IsBullish(last5Candles[3]) && _utilityService.IsBullish(last5Candles[2]))//SELL
                            {
                                var br = _utilityService.GetBodyRatio(last5Candles[3], last5Candles[4]);
                                if (br > 1)
                                {
                                    var ema50Dist = Math.Round(GetEmaDistance(last5Candles[4], last3ema_50[2]) / stick, 2);
                                    var ema200Dist = Math.Round(GetEmaDistance(last5Candles[4], last3ema_200[2]) / stick, 2);
                                    var ema800Dist = Math.Round(GetEmaDistance(last5Candles[4], last3ema_800[2]) / stick, 2);
                                    var emaGap = Math.Abs(ema800Dist - ema200Dist);

                                    if (ema50Dist < 2)
                                    {
                                        #region CASE1-->BTS1: target: Blue, sl: aqua + grey
                                        if (ema50Dist > 0 && ema200Dist >= 0 && ema800Dist < 0)
                                        {
                                            LogLastCandle(instrument.Name, last5Candles[4], last3ema_13[2]);

                                            var message = "----------CASE1: BTS--------------\n";
                                            message += "SELL " + instrument.Name + "|BR:" + br + "|RLC:" + lastCandleRelativeSize + "|50EMA:" + ema50Dist + "|200EMA:" + ema200Dist + "|800EMA:" + ema800Dist + "|" + DateTime.Now +
                                                CandleSizeCheck(last5Candles, stick);
                                            File.AppendAllText(@"AlanLog\DEL_" + timeframe + "_" + instrument.Name + ".txt", message + Environment.NewLine);

                                            var tp = Math.Round(last3ema_800[2], dp);
                                            var sl = Math.Round(last3ema_50[2], dp);

                                            _tradingService.PlaceTradeWithRisk3Values(instrument, last5Candles[4], last5Candles[4].Mid.C, tp, sl, false, 1, timeframe, stick);
                                        }
                                        #endregion

                                        #region CASE2--> Trend is friend
                                        if (ema50Dist > 0 && ema200Dist > 0 && ema800Dist > 0)
                                        {
                                            LogLastCandle(instrument.Name, last5Candles[4], last3ema_13[2]);

                                            var aqGrGap = Math.Abs(ema50Dist - ema200Dist);
                                            var GrBlGap = Math.Abs(ema800Dist - ema200Dist);
                                            var message = "----------CASE2: Trend is Friend--------------\n";
                                            message += "SELL " + instrument.Name + "|BR:" + br + "|RLC:" + lastCandleRelativeSize + "|50EMA:" + ema50Dist + "|200EMA:" + ema200Dist + "|800EMA:" + ema800Dist + "|" +
                                               "|Aqua Grey Gap:" + aqGrGap + "|Grey Blue Gap:" + GrBlGap + "|" + DateTime.Now +
                                               CandleSizeCheck(last5Candles, stick);
                                            File.AppendAllText(@"AlanLog\DEL_" + timeframe + "_" + instrument.Name + ".txt", message + Environment.NewLine);
                                            var stopLoss = GetStopLoss(last5Candles[4], last3ema_50[2], true, instrument);

                                            _tradingService.PlaceTradeWithRisk4(instrument, last5Candles[4], last5Candles[4].Mid.C, stopLoss, false, 1, timeframe, stick);


                                        }
                                        #endregion

                                        #region CASE3-->BTS2: target: Blue + Grey, sl: aqua 
                                        if (ema50Dist > 0 && ema200Dist < 0 && ema800Dist < 0 && Math.Abs(ema200Dist) < Math.Abs(ema800Dist))
                                        {
                                            LogLastCandle(instrument.Name, last5Candles[4], last3ema_13[2]);

                                            var message = "----------CASE2: BTS--------------\n";
                                            message += "SELL " + instrument.Name + "|BR:" + br + "|RLC:" + lastCandleRelativeSize + "|50EMA:" + ema50Dist + "|200EMA:" + ema200Dist + "|800EMA:" + ema800Dist + "|" + DateTime.Now +
                                                CandleSizeCheck(last5Candles, stick);
                                            File.AppendAllText(@"AlanLog\DEL_" + timeframe + "_" + instrument.Name + ".txt", message + Environment.NewLine);

                                            var tp = Math.Round(last3ema_200[2], dp);
                                            var sl = Math.Round(last3ema_50[2], dp);

                                            _tradingService.PlaceTradeWithRisk3Values(instrument, last5Candles[4], last5Candles[4].Mid.C, tp, sl, false, 1, timeframe, stick);
                                        }
                                        #endregion
                                    }



                                }


                            }
                        }
                    }
                    #endregion
                }
            }
            catch(Exception ex)
            {
                File.AppendAllText(@"AlanLog\Exceptions.txt", ex.Message + "\n" + ex.StackTrace + Environment.NewLine);
            }
        }

        public void CheckForExit(Instrument instrument, int timeframe, List<Candle> candles)
        {
            var placedTrade = _boxService.GetPlacedTrade(instrument.Name, timeframe);
            if (placedTrade.Count() > 0)
            {
                File.AppendAllText("AlanLog/MonitorPlacedTrade.txt", DateTime.Now + " | " + placedTrade.Count() +
                    " active trades found for " +
                    instrument.Name +
                    " at timeframe " + timeframe + "\n");

                CheckForClosure(placedTrade, instrument, candles);


            }
        }

        void CheckForClosure(List<PlacedTrade> placedTrades, Instrument instrument, List<Candle> candles)
        {
            foreach (var pt in placedTrades)
            {
                if (pt.StrategyNo == 1)
                {
                    CheckForClosure_Strategy_1(pt, instrument, candles);
                }
            }
        }

        void CheckForClosure_Strategy_1(PlacedTrade placedTrade, Instrument instrument, List<Candle> candles)
        {
           

            var closeList = candles.Select(c => c.Mid.C).ToList();

            var ema_13 = _emaService.CalculateEma(closeList, 13);
            var last3ema_13 = ema_13.Skip(Math.Max(0, ema_13.Count() - 3)).ToList();

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
                        "EMA 13 --> " + last3ema_13[2] + "\n";
                    if (candles.Last().Mid.C < last3ema_13[2])
                    {
                        message1 += " Latest candle has closed below 13 EMA closing the BUY trade \n";
                        _tradingService.CloseTrade(placedTrade.TradeId);
                        _boxService.DeletePlacedTrade(placedTrade);

                    }


                }
                else
                {
                    message1 += " Checking to close SELL trade \n" +
                        "Close(Last Candle) --> " + candles.Last().Mid.C + "\n" +
                        "EMA 13 --> " + last3ema_13[2] + "\n";

                    if (candles.Last().Mid.C > last3ema_13[2])
                    {
                        message1 += " Latest candle has closed above 13 EMA closing the SELL trade \n";
                        _tradingService.CloseTrade(placedTrade.TradeId);
                        _boxService.DeletePlacedTrade(placedTrade);

                    }
                }

                File.AppendAllText("AlanLog/CheckForClosure_Strategy_1.txt", DateTime.Now + " | " + "Checking for : " + instrument.Name + " \n" +
                "Trade Candle Time: " + tradeCandleTime + "\n" +
                "Last Candle Time: " + lastCandleTime + "\n" +
                "Lastest 13 EMA : " + last3ema_13[2] + "\n" +
                message1 +
                "======================================================\n");


            }
            else
            {
                File.AppendAllText("AlanLog/CheckForClosure_Strategy_1.txt", DateTime.Now + " | " + "Checking for : " + instrument.Name + " \n" +
                "Trade Candle Time: " + tradeCandleTime + "\n" +
                "Last Candle Time: " + lastCandleTime + "\n" +
                placedTrade.Action + " trade just got placed \n" +
                "======================================================\n");
            }



        }
    }
}
