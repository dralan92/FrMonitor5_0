using FrMonitor4_0.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;

namespace FrMonitor4_0.Services
{
    public class InstrumentService : IInstrumentService
    {
        IMetaDataService _metaDataservice;
        MetaConfig _metaconfig;
        public InstrumentService(IMetaDataService metaDataService)
        {
            _metaDataservice = metaDataService;
            _metaconfig = _metaDataservice.GetMetaConfig();


        }
        public InstrumentList GetInstrumentList()
        {
            try
            {
                var uri = _metaconfig.BaseUrl + "/v3/accounts/" + _metaconfig.AccountNumber + "/instruments";
                var restClient = new RestClient(uri);
                var restRequest = new RestRequest();
                restRequest.Method = RestSharp.Method.GET;
                restRequest.AddHeader("Authorization", _metaconfig.AuthHeader);
                var response = restClient.Execute(restRequest);
                return JsonConvert.DeserializeObject<InstrumentList>(response.Content);
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"AlanLog\Exceptions.txt", ex.Message + "\n" + ex.StackTrace + Environment.NewLine);
            }
            return new InstrumentList();

        }
    }
}
