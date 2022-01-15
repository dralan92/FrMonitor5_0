using FrMonitor4_0.Models;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;

namespace FrMonitor4_0.Services
{
    public class CandleService : ICandleService
    {
        IMetaDataService _metaDataService;
        MetaConfig _metaConfig;
        public CandleService(IMetaDataService metaDataService)
        {
            _metaDataService = metaDataService;
            _metaConfig = _metaDataService.GetMetaConfig();
        }
        public List<Candle> GetCandles(string instrumentName, int timeSlice, int count)
        {
            var granurarity = "M" + timeSlice;
            if (timeSlice == 60)
            {
                granurarity = "H1";

            }
            else if (timeSlice == 240)
            {
                granurarity = "H4";

            }
            else if (timeSlice == 1440)
            {
                granurarity = "D";
            }
            else if (timeSlice == 43800)
            {
                granurarity = "M";
            }

            var uri = _metaConfig.BaseUrl + "/v3/instruments/" + instrumentName + "/candles?count=" + count + "&granularity=" + granurarity;
            var restClient = new RestClient(uri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.GET;
            restRequest.AddHeader("Authorization", _metaConfig.AuthHeader);
            var response = restClient.Execute(restRequest);
            return CorrectCandleWindow(JsonConvert.DeserializeObject<CandleWindow>(response.Content)).Candles;
        }

        CandleWindow CorrectCandleWindow(CandleWindow candleWindow)
        {
            var candles = candleWindow.Candles.ToList();
            candles.RemoveAt(candles.Count - 1);
            return new CandleWindow()
            {
                Candles = candles
            };

        }
    }
}
