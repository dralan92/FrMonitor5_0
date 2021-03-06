using FrMonitor4_0.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FrMonitor4_0.Services
{
    public class MonitorService : IMonitorService
    {
        IMetaDataService _metaDataService;
        IInstrumentService _instrumentService;
        ICandleService _candleService;
        List<string> _itemsToMonitor;
        IDeloreanService _deloreanService;
        MetaConfig _metaConfig;
        IBoxService _boxService;
        ITradingService _tradingService;
        IHarmonicService _harmonicService;
        IEmaCrossService _emaCrossService;
        IRiskCalculationService _riskCalculationService;
        IBollingerRisHfxService _bollingerRisHfxService;
        ITargetUpdateService _targetUpdateService;
        IRsiDivergenceService _rsiDivergenceService;
        bool _canTrade;

        public MonitorService(IMetaDataService metaDataService, IInstrumentService instrumentService,
            ICandleService candleService, IDeloreanService deloreanService, IBoxService boxService, ITradingService tradingService,
            IHarmonicService harmonicService, IEmaCrossService emaCrossService, IRiskCalculationService riskCalculationService,
            IBollingerRisHfxService bollingerRisHfxService, ITargetUpdateService targetUpdateService, IRsiDivergenceService rsiDivergenceService)
        {
            _metaDataService = metaDataService;
            _candleService = candleService;
            _instrumentService = instrumentService;
            _deloreanService = deloreanService;
            _metaConfig = _metaDataService.GetMetaConfig();
            _itemsToMonitor = LoadMonitorList(_metaConfig.InstrumentsToMonitor);
            _boxService = boxService;
            _tradingService = tradingService;
            _harmonicService = harmonicService;
            _emaCrossService = emaCrossService;
            _riskCalculationService = riskCalculationService;
            _bollingerRisHfxService = bollingerRisHfxService;
            _targetUpdateService = targetUpdateService;
            _rsiDivergenceService = rsiDivergenceService;
            _canTrade = true;

        }
        public void Monitor(int timeframe)
        {
            try
            {
                File.AppendAllText("AlanLog/RunCheck_"+ timeframe + ".txt", timeframe + " min check :" + DateTime.Now);
                Thread.Sleep(500);
                object instrumentListObj = null;
                instrumentListObj = _instrumentService.GetInstrumentList();

                var instrumentList = (InstrumentList)instrumentListObj;

                if (_metaConfig.RiskCalculationMode)
                {

                    Console.WriteLine("Instrument Name: \n");
                    var iname = Console.ReadLine();
                    foreach (var instrument in instrumentList.Instruments)
                    {
                        if (instrument.Name == iname)
                        {
                            var units = _riskCalculationService.CalculateUnits(instrument);
                            var lots = units / 100000;
                            Console.WriteLine("UNITS-->" + units + "\n" + "LOTS-->" + lots + "\n");
                        }
                    }
                }
                else
                {
                   
                    var targetReached = _metaDataService.IsTargetReached();
                    if ((!targetReached))
                    {
                        foreach (var instrument in instrumentList.Instruments)
                        {
                            CanTrade(instrument.Name);

                            if (_itemsToMonitor.Contains(instrument.Name) && _canTrade)
                            {
                                var candleList = _candleService.GetCandles(instrument.Name, timeframe, 2001);

                                if (_metaConfig.StrategyNumber == 1)
                                {
                                    _deloreanService.CheckForEntry(instrument, timeframe, candleList);
                                    _deloreanService.CheckForExit(instrument, timeframe, candleList);
                                }

                                if (_metaConfig.StrategyNumber == 2)
                                {
                                    _harmonicService.CheckForEntry(instrument, timeframe, candleList);
                                    _harmonicService.CheckForExit(instrument, timeframe, candleList);

                                }

                                if (_metaConfig.StrategyNumber == 3)
                                {
                                    _emaCrossService.CheckForEntry(instrument, timeframe, candleList);
                                    _emaCrossService.CheckForExit(instrument, timeframe, candleList);
                                }

                                if (_metaConfig.StrategyNumber == 4)
                                {
                                    _bollingerRisHfxService.CheckForEntry(instrument, timeframe, candleList);
                                }

                                if(_metaConfig.StrategyNumber == 5)
                                {
                                    _rsiDivergenceService.CheckForEntry(instrument, timeframe, candleList);
                                }

                            }

                        }


                    }
                }
            }
            catch(Exception ex)
            {
                File.AppendAllText("AlanLog/Exception.txt",DateTime.Now + "exception happened \n");
            }
            
            
        }

        void CanTrade(string instrumentName)
        {
           
            if (_metaConfig.OneTrade)
            {
                File.AppendAllText("AlanLog/OneTrade.txt", instrumentName + " OneTrade is ON!!!" + DateTime.Now + "\n");
                _canTrade = false;
                var alltrades = _boxService.GetAllPlacedTrade();
                if (alltrades.Count == 0)
                {
                    _canTrade = true;
                    File.AppendAllText("AlanLog/OneTrade.txt", instrumentName + " READY TO TRADE!!! No Placed Trade For Now in db " + DateTime.Now + "\n");

                }
                else
                {
                    var openPosition = _tradingService.GetOpenPositions();
                    if (openPosition.Positions.Count == 0)
                    {
                        File.AppendAllText("AlanLog/OneTrade.txt", instrumentName +  " Remnant trade found in alanDb!!!! Removing it...." + DateTime.Now + "\n");
                        var remnanatTrades = _boxService.GetAllPlacedTrade();
                        foreach (var rt in remnanatTrades)
                        {
                            _boxService.DeletePlacedTrade(rt);
                        }
                        _canTrade = true;
                    }
                    else if (openPosition.Positions.Count != alltrades.Count)
                    {
                        File.AppendAllText("AlanLog/OneTrade.txt", instrumentName + " Critical Error, number of trades in db does not match open position count!!! Close all trades and clear db " + DateTime.Now + "\n");
                        File.AppendAllText("AlanLog/OneTrade.txt", instrumentName + " CANNOT TRADE!!!! You have OneTrade turned ON and have placed trades present in alanDb " + DateTime.Now + "\n");

                    }
                }
            }
            else
            {
                File.AppendAllText("AlanLog/OneTrade.txt", instrumentName + " OneTrade is OFF can trade" + DateTime.Now + "\n");

            }
            File.AppendAllText("AlanLog/OneTrade.txt","=======================================================================\n");

        }

        List<string> LoadMonitorList(string rawString)
        {
            var items = rawString.Split("|");
            return items.ToList();
        }

        public void TargetCheck()
        {
            var target = _metaDataService.GetTarget();
            var uri = _metaConfig.BaseUrl + "/v3/accounts/"+ _metaConfig.AccountNumber  + "/summary";
            var restClient = new RestClient(uri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.GET;

            var authHeader = _metaConfig.AuthHeader;
            restRequest.AddHeader("Authorization", authHeader);
            var response = restClient.Execute(restRequest);
            var nav = GetNav(response);
            var message = "NAV-->" + nav + " |  " + "TARGET -->" + target + "\n";
            if (nav >= target)
            {
                message += "Target Met!!! Stopping Business...\n";
                _metaDataService.SetTargetReached();
                CloseAll();
                if (_metaConfig.OneTrade)
                {
                    UpdateTarget();
                }
            }
            message += "=========================================\n";
            File.AppendAllText(@"AlanLog\TargetCheck.txt", DateTime.Now + " | " + message + Environment.NewLine);
        }
        void CloseAll()
        {
            var allTrades = _boxService.GetAllPlacedTrade();
            foreach (var trade in allTrades)
            {
                _tradingService.CloseTrade(trade.TradeId);
                _boxService.DeletePlacedTrade(trade);
            }

        }

        void UpdateTarget()
        {
            _targetUpdateService.UpdateTarget();
            File.AppendAllText(@"AlanLog\TargetCheck.txt", DateTime.Now + " | " +"Target updated, new target is :"+_metaDataService.GetTarget() + Environment.NewLine);

        }

        double GetNav(IRestResponse restResponse)
        {
            double nav;
            var resJobj = JObject.Parse(restResponse.Content);
            var account = resJobj.GetValue("account").ToString();
            var accountJobj = JObject.Parse(account);
            var navParsed = Double.TryParse(accountJobj.GetValue("NAV").ToString(), out nav);
            return nav;
        }
    }
}
