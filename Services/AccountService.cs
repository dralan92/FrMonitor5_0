using FrMonitor4_0.Models;

namespace FrMonitor4_0.Services
{
    public class AccountService : IAccountService
    {
        IMetaDataService _metaDataService;
        MetaConfig _metaConfig;
        public AccountService(IMetaDataService metaDataService)
        {
            _metaDataService = metaDataService;
            _metaConfig = metaDataService.GetMetaConfig();
        }
        public AccountDetail GetAccountDetail()
        {
            var instrumentPriceUri = _metaConfig.BaseUrl + "/v3/accounts/" + _metaConfig.AccountNumber + "/pricing?instruments=";
            var orderUri = _metaConfig.BaseUrl + "/v3/accounts/" + _metaConfig.AccountNumber + "/orders";

            return new AccountDetail
            {
                AccountId = _metaConfig.AccountNumber,
                AuthHeader = _metaConfig.AuthHeader,
                Balance = -1.0,
                InstrumentPriceUri = instrumentPriceUri,
                OpenPositionCount = -1,
                OpenTradeCount = -1,
                OrderUri = orderUri

            };
        }
    }
}
