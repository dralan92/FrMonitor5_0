using FrMonitor4_0.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class TradingService : ITradingService
    {
        IAccountService _accountService;
        IPriceService _priceService;
        IUnitService _unitService;
        IMetaDataService _metaDataService;
        MetaConfig _metaConfig;
        IBoxService _boxService;
        INavService _navService;
        public TradingService(IAccountService accountService, IPriceService priceService, IUnitService unitService,
            IMetaDataService metaDataService, IBoxService boxService, INavService navService)
        {
            _accountService = accountService;
            _priceService = priceService;
            _unitService = unitService;
            _metaDataService = metaDataService;
            _metaConfig = _metaDataService.GetMetaConfig();
            _boxService = boxService;
            _navService = navService;
        }
        string GetStrategyName(sbyte sNo)
        {
            if (sNo == 1)
                return "DEL";
            if (sNo == 2)
                return "BB_B_E";
            if (sNo == 3)
                return "EMA_Cross";
            if (sNo == 30)
                return "US30_Strategy";
            return "";
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

        double GetRiskAmount()
        {
            if (_metaConfig.OneTrade)
            {
                var nav = _navService.GetCurrentNav();
                var target = _metaDataService.GetTarget();
                if(nav < target)
                {
                    var amountToMakeUp = target - nav;
                    var riskAmount = amountToMakeUp * (1 / _metaConfig.RequiredRr);
                    File.AppendAllText("AlanLog/OneTrade_RiskUpdates.txt", "Current Risk -->"+ riskAmount  + "\n");

                    return riskAmount;
                }
                else
                {
                    File.AppendAllText("AlanLog/OneTrade.txt", "Risk amount 0 as target already met \n");
                    return 0;
                }
            }
            else
            {
                return _metaConfig.RiskAmount;

            }
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

        double CalculateTpRiskAccurate(bool isLong, Instrument instrument, double lastClose, double risk)
        {
            var pipValue = GetPipValue(instrument);
            var tpLength = pipValue * risk * 0.5;

            if (isLong)
            {
                return lastClose + tpLength;
            }
            else
            {
                return lastClose - tpLength;
            }

        }

        void PlaceRealOrder2_New(string instrument, double tp, double sl, double units, Candle lastCandle, AccountDetail accountDetail, int timeframe, sbyte sno)
        {
            var jsonBody = "{\"order\": {\r\n    \"units\": \"" +
                units +
                "\",\r\n    \"instrument\": \"" +
               instrument +
                "\",\r\n    \"timeInForce\": \"FOK\",\r\n    \"type\": \"MARKET\",\r\n    \"takeProfitOnFill\":{\"price\":\"" +
                tp +
                "\"},\r\n    \"stopLossOnFill\" :{\"price\":\"" +
                sl +
                "\"},\r\n    \"positionFill\": \"DEFAULT\"\r\n  }\r\n}";



            var action = units < 0 ? "SELL" : "BUY";
            var restClient = new RestClient(accountDetail.OrderUri);

            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.POST;

            restRequest.AddHeader("Authorization", accountDetail.AuthHeader);

            restRequest.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            var response = restClient.Execute(restRequest);
            WriteTradeToFile(response, instrument, timeframe, sno, lastCandle, action);


            File.AppendAllText(@"AlanLog\Postman.txt", jsonBody + Environment.NewLine +
                accountDetail.OrderUri + Environment.NewLine +
                accountDetail.AuthHeader + Environment.NewLine +
                accountDetail.AccountId + Environment.NewLine);


        }

        void PlaceRealOrder_New(string instrument, double tp, double sl, int units, Candle lastCandle, AccountDetail accountDetail, int timeframe, sbyte sno)
        {
            var jsonBody = "{\"order\": {\r\n    \"units\": \"" +
                units +
                "\",\r\n    \"instrument\": \"" +
               instrument +
                "\",\r\n    \"timeInForce\": \"FOK\",\r\n    \"type\": \"MARKET\",\r\n    \"takeProfitOnFill\":{\"price\":\"" +
                tp +
                "\"},\r\n    \"stopLossOnFill\" :{\"price\":\"" +
                sl +
                "\"},\r\n    \"positionFill\": \"DEFAULT\"\r\n  }\r\n}";



            var action = units < 0 ? "SELL" : "BUY";
            var restClient = new RestClient(accountDetail.OrderUri);

            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.POST;

            restRequest.AddHeader("Authorization", accountDetail.AuthHeader);

            restRequest.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            var response = restClient.Execute(restRequest);//===========================================
            WriteTradeToFile(response, instrument, timeframe, sno, lastCandle, action);


            File.AppendAllText(@"AlanLog\Postman.txt", jsonBody + Environment.NewLine +
                accountDetail.OrderUri + Environment.NewLine +
                accountDetail.AuthHeader + Environment.NewLine +
                accountDetail.AccountId + Environment.NewLine);

        }

        void WriteTradeToFile(IRestResponse restResponse, string instrumentName, int timespan, sbyte strageyNo, Candle lastCandle, string action)
        {
            var resJobj = JObject.Parse(restResponse.Content);
            try
            {
                var orderFillTransaction = resJobj.GetValue("orderFillTransaction").ToString();
                //https://api-fxtrade.oanda.com/v3/accounts/001-002-4175284-008/trades/2522/close
                var oftJobj = JObject.Parse(orderFillTransaction);
                var tradeOpened = oftJobj.GetValue("tradeOpened").ToString();
                var toJobj = JObject.Parse(tradeOpened);
                var tradeID = toJobj.GetValue("tradeID").ToString();
                _boxService.AddPlacedTrade(new PlacedTrade
                {
                    InstrumentName = instrumentName,
                    StrategyNo = strageyNo,
                    Timeframe = timespan,
                    TradeCandleTime = lastCandle.Time,
                    TradeId = tradeID,
                    Action = action

                });
            }
            catch(Exception ex)
            {
                File.AppendAllText("AlanLog/TradeException.txt", restResponse.Content + 
                    Environment.NewLine +
                    "=================================================" +
                    Environment.NewLine);
            }


        }

        public OpenPositions GetOpenPositions()
        {
            var restUri = _metaConfig.BaseUrl + "/v3/accounts/" + _metaConfig.AccountNumber + "/openPositions";
            var restClient = new RestClient(restUri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.GET;
            restRequest.AddHeader("Authorization", _metaConfig.AuthHeader);
            var response = restClient.Execute(restRequest);
            return JsonConvert.DeserializeObject<OpenPositions>(response.Content);


        }

        public void CloseTrade(string tradeId)
        {
            var restUri = _metaConfig.BaseUrl + "/v3/accounts/"+ _metaConfig.AccountNumber + "/trades/" + tradeId + "/close";
            var restClient = new RestClient(restUri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.PUT;
            restRequest.AddHeader("Authorization", _metaConfig.AuthHeader);
            var response = restClient.Execute(restRequest);
        }

        public void PlaceTradeWithRisk3Values(Instrument instrument, Candle lastCandle, double entry, double tp, double sl, bool isLong, sbyte strategyNo, int timeFrame, double stick)
        {
            try
            {
                var strategy = GetStrategyName(strategyNo);
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
                        var risk = (Math.Abs(entry - sl)) / pv;


                        var fullSpreadRisk = (Math.Abs(entry - sl) + (spread)) / pv;
                        var reward = isLong ? (tp - entry) / pv : (entry - tp) / pv;
                        File.AppendAllText(@"AlanLog\TradeAttempt_" + instrument.Name + ".txt",
                            " FS Risk:" + fullSpreadRisk + " Reward:" + reward + Environment.NewLine);

                        var availableUnits = "unknown";//isLong ? price.UnitsAvailable.Default.Long : price.UnitsAvailable.Default.Short;

                        File.AppendAllText(@"AlanLog\TradeAttempt_" + instrument.Name + ".txt",
                            " AvailableUnits:" + availableUnits + Environment.NewLine);


                        var units = _unitService.CalculateUnitsUniversal(instrument, GetRiskAmount(), fullSpreadRisk, accountDetail);

                        File.AppendAllText(@"AlanLog\TradeAttempt_" + instrument.Name + ".txt",
                            " Units for the trade:" + units + Environment.NewLine);


                        var mts = instrument.MinimumTradeSize;
                        var riskToStickRatio = stick > 0 ? Math.Round((Math.Abs(entry - sl)) / stick, 2).ToString() : "Unknown";



                        var message = instrument.Name + "\n" +
                            " MTS:" + mts + "\n" +
                            " AvailableUnits:" + availableUnits + "\n" +
                            " Spread :" + spread + "\n" +
                            " Spread pips:" + sp + "\n" +
                            " FS Risk:" + fullSpreadRisk + "\n" +
                            " Reward:" + reward + "\n" +
                            " pipVal:" + pv + "\n" +
                            " Risk to Spread ratio:" + Math.Round((Math.Abs(entry - sl)) / spread, 2) + "\n" +
                            " Risk to Stick ratio:" + riskToStickRatio + "\n" +
                            " Units for the trade:" + units + "\n"
                            ;

                        var neededRrr = 0.4;
                        //if (spread * 2 < risk)
                        {
                            if (reward / fullSpreadRisk >= neededRrr || neededRrr == 0)
                            {
                                var dp = GetDecimalPrecision(ask);
                                dp = dp > 5 ? 5 : dp;
                                var takeProfit = Math.Round(CalculateTpRiskAccurate(isLong, instrument, entry, fullSpreadRisk), dp);
                                sl = Math.Round(sl, dp);
                                if (instrument.MinimumTradeSize == 0.1)
                                {
                                    var partialUnit = Math.Round(units, 1);
                                    if (partialUnit >= 0.1)
                                    {
                                        if (!isLong) { partialUnit *= -1; }
                                        var messageTrade = instrument.Name + "|TP:" + takeProfit + "|SL:" + sl + "|Pu:" + partialUnit + "|Entry:" + entry + "\n";
                                        var candleTime = "Last Candle Creation time : " + DateTime.Parse(lastCandle.Time) + "| Current Time : " + DateTime.Now + "\n";

                                        var tradeEnd = "***************TRADE PLACED***************";
                                        File.AppendAllText(@"AlanLog\" + strategy + "_" + timeFrame + "_" + instrument.Name + ".txt", message + messageTrade + candleTime + tradeEnd + Environment.NewLine);


                                        //PlaceRealOrder2(instrument.Name, takeProfit, sl, partialUnit, entry, accountDetail);
                                        PlaceRealOrder2_New(instrument.Name, takeProfit, sl, partialUnit, lastCandle, accountDetail, timeFrame, strategyNo);
                                        //PlaceRealOrder2(instrument.Name, takeProfit, sl, partialUnit, entry, accountDetail);
                                        //PlaceMockOrder(instrument.Name, takeProfit, sl, partialUnit, entry, accountDetail);
                                        //_fileService.WriteTradeDataToFileForUpload(instrument.Name, takeProfit, sl, entry, partialUnit, GetRiskAmount());

                                    }
                                }
                                else if (instrument.MinimumTradeSize == 1)
                                {
                                    var partialUnit = (int)Math.Round(units);
                                    if (partialUnit >= 1)
                                    {
                                        if (!isLong) { partialUnit *= -1; }
                                        var messageTrade = instrument.Name + "|TP:" + takeProfit + "|SL:" + sl + "|Pu:" + partialUnit + "|Entry:" + entry + "\n";
                                        var candleTime = "Last Candle Creation time : " + DateTime.Parse(lastCandle.Time) + "| Current Time : " + DateTime.Now + "\n";

                                        var tradeEnd = "***************TRADE PLACED***************";
                                        File.AppendAllText(@"AlanLog\" + strategy + "_" + timeFrame + "_" + instrument.Name + ".txt", message + messageTrade + candleTime + tradeEnd + Environment.NewLine);

                                        //PlaceRealOrder(instrument.Name, takeProfit, sl, partialUnit, entry, accountDetail);
                                        PlaceRealOrder_New(instrument.Name, takeProfit, sl, partialUnit, lastCandle, accountDetail, timeFrame, strategyNo);

                                        //PlaceRealOrder(instrument.Name, takeProfit, sl, partialUnit, entry, accountDetail);
                                        //PlaceMockOrder(instrument.Name, takeProfit, sl, partialUnit, entry, accountDetail);

                                        //_fileService.WriteTradeDataToFileForUpload(instrument.Name, takeProfit, sl, entry, partialUnit, GetRiskAmount());

                                    }

                                }
                            }
                        }




                    }
                    else
                    {

                    }

                }
                else
                {

                }


            }
            catch (Exception ex)
            {
                File.AppendAllText(@"AlanLog\Exceptions.txt", ex.Message + "\n" + ex.StackTrace + Environment.NewLine);

            }
        }

        public void PlaceTradeWithRisk4(Instrument instrument, Candle lastCandle, double entry, double stopLoss, bool isLong, sbyte strategyNo, int timeFrame, double stick)
        {
            try
            {

                var strategy = GetStrategyName(strategyNo);
                var accountDetail = _accountService.GetAccountDetail();
                if (accountDetail != null)
                {
                    var price = _priceService.GetPriceList(instrument, accountDetail).Prices[0];
                    if (price.Instrument != null)
                    {
                        var ask = price.CloseOutAsk;
                        var bid = price.CloseOutBid;
                        var spread = ask - bid;

                        var dp = GetDecimalPrecision(ask);
                        dp = dp > 5 ? 5 : dp;
                        var sp = GetAccurateSpreadPips(ask, bid, instrument);
                        var pv = GetPipValue(instrument);

                        //File.AppendAllText(@"AlanLog\US30_11.txt", "BUY SL:" + stopLoss + "|LC:" + entry + "|Spread:" + spread+"|SP:"+ sp + DateTime.Now + Environment.NewLine);

                        //var correctedSL = CorrectSLUs30(stopLoss, pv, isLong);
                        var risk = (Math.Abs(entry - stopLoss)) / pv;
                        var fullSpreadRisk = (Math.Abs(entry - stopLoss) + (spread)) / pv;
                        var availableUnits = "Unknown";//isLong ? price.UnitsAvailable.Default.Long : price.UnitsAvailable.Default.Short;
                        var units = _unitService.CalculateUnitsUniversal(instrument, GetRiskAmount(), fullSpreadRisk, accountDetail);
                        var mts = instrument.MinimumTradeSize;
                        var riskToStickRatio = stick > 0 ? Math.Round((Math.Abs(entry - stopLoss)) / stick, 2).ToString() : "Unknown";


                        var takeProfit = Math.Round(CalculateTpRiskAccurate(isLong, instrument, entry, fullSpreadRisk), dp);
                        var message = instrument.Name + "\n" +
                           " MTS:" + mts + "\n" +
                           " AvailableUnits:" + availableUnits + "\n" +
                           " Spread:" + spread + "\n" +
                           " Spread pips:" + sp + "\n" +
                           " FS Risk:" + fullSpreadRisk + "\n" +
                           " pipVal:" + pv + "\n" +
                           " TP:" + takeProfit + "\n" +
                           " E:" + entry + "\n" +
                           " SL:" + stopLoss + "\n" +
                           " Risk to Spread ratio:" + Math.Round((Math.Abs(entry - stopLoss)) / spread, 2) + "\n" +
                           " Risk to Stick ratio:" + riskToStickRatio + "\n" +
                           " Units for the trade:" + units + "\n"
                           ;

                        File.AppendAllText(@"AlanLog\PlaceTradeWithRisk4.txt", message + Environment.NewLine);

                        //if (spread * 2 < risk)
                        {
                            if (instrument.MinimumTradeSize == 0.1)
                            {
                                var partialUnit = Math.Round(units, 1);
                                if (partialUnit >= 0.1)
                                {
                                    if (!isLong) { partialUnit *= -1; }
                                    var messageTrade = instrument.Name + "|TP:" + takeProfit + "|SL:" + stopLoss + "|Pu:" + partialUnit + "|Entry:" + entry + "\n";
                                    var candleTime = "Last Candle Creation time : " + DateTime.Parse(lastCandle.Time) + "| Current Time : " + DateTime.Now + "\n";
                                    var tradeEnd = "***************TRADE PLACED***************";
                                    File.AppendAllText(@"AlanLog\" + strategy + "_" + timeFrame + "_" + instrument.Name + ".txt", message + messageTrade + candleTime + tradeEnd + Environment.NewLine);
                                    //PlaceRealOrder2(instrument.Name, takeProfit, stopLoss, partialUnit, entry, accountDetail);
                                    PlaceRealOrder2_New(instrument.Name, takeProfit, stopLoss, partialUnit, lastCandle, accountDetail, timeFrame, strategyNo);
                                    //PlaceRealOrder2(instrument.Name, takeProfit, stopLoss, partialUnit, entry, accountDetail);
                                    //PlaceRealOrder2(instrument.Name, takeProfit, stopLoss, partialUnit, entry, accountDetail);
                                    //_fileService.WriteTradeDataToFileForUpload(instrument.Name, takeProfit, stopLoss, entry, partialUnit, GetRiskAmount());


                                }
                            }
                            else if (instrument.MinimumTradeSize == 1)
                            {
                                var partialUnit = (int)Math.Round(units);
                                if (partialUnit >= 1)
                                {
                                    if (!isLong) { partialUnit *= -1; }
                                    var messageTrade = instrument.Name + "|TP:" + takeProfit + "|SL:" + stopLoss + "|Pu:" + partialUnit + "|Entry:" + entry + "\n";
                                    var candleTime = "Last Candle Creation time : " + DateTime.Parse(lastCandle.Time) + "| Current Time : " + DateTime.Now + "\n";
                                    var tradeEnd = "***************TRADE PLACED***************";
                                    File.AppendAllText(@"AlanLog\" + strategy + "_" + timeFrame + "_" + instrument.Name + ".txt", message + messageTrade + candleTime + tradeEnd + Environment.NewLine);
                                    //PlaceRealOrder(instrument.Name, takeProfit, stopLoss, partialUnit, entry, accountDetail);
                                    PlaceRealOrder_New(instrument.Name, takeProfit, stopLoss, partialUnit, lastCandle, accountDetail, timeFrame, strategyNo);
                                    //PlaceRealOrder(instrument.Name, takeProfit, stopLoss, partialUnit, entry, accountDetail);
                                    //PlaceRealOrder(instrument.Name, takeProfit, stopLoss, partialUnit, entry, accountDetail);
                                    //_fileService.WriteTradeDataToFileForUpload(instrument.Name, takeProfit, stopLoss, entry, partialUnit, GetRiskAmount(instrument.Name));

                                }

                            }
                        }




                    }
                    else
                    {

                    }

                }
                else
                {

                }


            }
            catch (Exception ex)
            {
            }
        }
    }
}
