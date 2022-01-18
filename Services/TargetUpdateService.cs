using FrMonitor4_0.Models;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FrMonitor4_0.Services
{
    public class TargetUpdateService : ITargetUpdateService
    {
        IMetaDataService _metaDataService;
        MetaConfig _metaConfig;
        INavService _navService;
        string _metaFile;
        public TargetUpdateService(IMetaDataService metaDataService, INavService navService)
        {
            _navService = navService;
            _metaDataService = metaDataService;
            _metaConfig = _metaDataService.GetMetaConfig();
            _metaFile = "metaConfig.json";
        }

        public void UpdateTarget()
        {
            var currentNav = _navService.GetCurrentNav();
            if (currentNav != null)
            {
                var otGain = _metaConfig.OneTradeGain;
                var newTarget = currentNav + otGain;
                _metaConfig.Target = newTarget;
                _metaConfig.TargetReached = false;
                UpdateMetaData(_metaConfig);

            }

        }

        void UpdateMetaData(MetaConfig newMetaConfig)
        {
            var newMetaConfigObj = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(newMetaConfig));
            string newResut = Newtonsoft.Json.JsonConvert.SerializeObject(newMetaConfigObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(_metaFile, newResut);
        }
    }
}
