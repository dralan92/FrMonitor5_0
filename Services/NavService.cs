using FrMonitor4_0.Models;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace FrMonitor4_0.Services
{
    public class NavService : INavService
    {
        MetaConfig _metaConfig;
        IMetaDataService _metaDataService;
        public NavService(IMetaDataService metaDataService)
        {
            _metaDataService = metaDataService;
            _metaConfig = _metaDataService.GetMetaConfig();
        }
        public double GetCurrentNav()
        {
            var uri = _metaConfig.BaseUrl + "/v3/accounts/" + _metaConfig.AccountNumber + "/summary";
            var restClient = new RestClient(uri);
            var restRequest = new RestRequest();
            restRequest.Method = RestSharp.Method.GET;

            var authHeader = _metaConfig.AuthHeader;
            restRequest.AddHeader("Authorization", authHeader);
            var response = restClient.Execute(restRequest);
            var nav = GetNav(response);
            return nav;
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
