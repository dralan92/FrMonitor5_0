using FrMonitor4_0.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class EmaCrossService : IEmaCrossService
    {
        IEmaService _emaService;
        ITradingService _tradingService;
        IUtilityService _utilityService;
        IBoxService _boxService;
        public EmaCrossService(IEmaService emaService, ITradingService tradingService, IUtilityService utilityService, IBoxService boxService)
        {
            _emaService = emaService;
            _tradingService = tradingService;
            _utilityService = utilityService;
            _boxService = boxService;
        }
        public void CheckForEntry(Instrument instrument, int timeframe, List<Candle> candles)
        {
            var closeList = candles.Select(c => c.Mid.C).ToList();

            var ema_3 = _emaService.CalculateEma(closeList, 3);
            var last3ema_3 = ema_3.Skip(Math.Max(0, ema_3.Count() - 3)).ToList();

            var ema_9 = _emaService.CalculateEma(closeList, 9);
            var last3ema_9 = ema_9.Skip(Math.Max(0, ema_9.Count() - 3)).ToList();


            if (last3ema_3[2] > last3ema_9[2])
            {
                if (last3ema_3[0] < last3ema_9[0])
                {
                    var stopLoss = candles.Last().Mid.L;
                    var stick = _utilityService.GetMeasuringStick(candles);
                    _tradingService.PlaceTradeWithRisk4(instrument, candles.Last(), candles.Last().Mid.C, stopLoss, true, 3, timeframe, stick);

                }
            }

            if (last3ema_3[2] < last3ema_9[2])
            {
                if (last3ema_3[0] > last3ema_9[0])
                {
                    var stopLoss = candles.Last().Mid.H;
                    var stick = _utilityService.GetMeasuringStick(candles);
                    _tradingService.PlaceTradeWithRisk4(instrument, candles.Last(), candles.Last().Mid.C, stopLoss, false, 3, timeframe, stick);
                }
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
                if (pt.StrategyNo == 3)
                {
                    CheckForClosure_Strategy_3(pt, instrument, candles);
                }
            }
        }

        void CheckForClosure_Strategy_3(PlacedTrade placedTrade, Instrument instrument, List<Candle> candles)
        {


            var closeList = candles.Select(c => c.Mid.C).ToList();

            var ema_3 = _emaService.CalculateEma(closeList, 3);
            var last3ema_3 = ema_3.Skip(Math.Max(0, ema_3.Count() - 3)).ToList();

            var ema_9 = _emaService.CalculateEma(closeList, 9);
            var last3ema_9 = ema_9.Skip(Math.Max(0, ema_9.Count() - 3)).ToList();

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
                        "EMA 3 --> " + last3ema_3[2] + "\n" +
                        "EMA 9 --> " + last3ema_9[2] + "\n";

                    if (last3ema_3[2] < last3ema_9[2])
                    {
                        message1 += " EMA 3 crossed below EMA 9 >> closing the BUY trade !!! \n";

                        _tradingService.CloseTrade(placedTrade.TradeId);
                        _boxService.DeletePlacedTrade(placedTrade);

                    }


                }
                else
                {
                    message1 += " Checking to close BUY trade \n" +
                       "EMA 3 --> " + last3ema_3[2] + "\n" +
                       "EMA 9 --> " + last3ema_9[2] + "\n";

                    if (last3ema_3[2] > last3ema_9[2])
                    {
                        message1 += " EMA 3 crossed above EMA 9 >> closing the SELL trade !!! \n";

                        _tradingService.CloseTrade(placedTrade.TradeId);
                        _boxService.DeletePlacedTrade(placedTrade);

                    }
                }

                File.AppendAllText("AlanLog/CheckForClosure_Strategy_1.txt", DateTime.Now + " | " + "Checking for : " + instrument.Name + " \n" +
                "Trade Candle Time: " + tradeCandleTime + "\n" +
                "Last Candle Time: " + lastCandleTime + "\n" +
                "EMA 3 --> " + last3ema_3[2] + "\n" +
                "EMA 9 --> " + last3ema_9[2] + "\n" +
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
