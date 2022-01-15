using FrMonitor4_0.Models;
using Newtonsoft.Json;
using RestSharp;

namespace FrMonitor4_0.Services
{
    public class PriceService : IPriceService
    {
        public PriceList GetPriceList(Instrument instrument, AccountDetail accountDetail)
        {
            var uri = accountDetail.InstrumentPriceUri + instrument.Name;
            var restClient = new RestClient(uri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.GET;

            restRequest.AddHeader("Authorization", accountDetail.AuthHeader);
            var response = restClient.Execute(restRequest);
            return JsonConvert.DeserializeObject<PriceList>(response.Content);
        }

        public PriceList GetPriceListRaw(string instrumentName, AccountDetail accountDetail)
        {
            var uri = accountDetail.InstrumentPriceUri + instrumentName;

            var restClient = new RestClient(uri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.GET;

            restRequest.AddHeader("Authorization", accountDetail.AuthHeader);

            var response = restClient.Execute(restRequest);
            return JsonConvert.DeserializeObject<PriceList>(response.Content);


        }
    }
}
