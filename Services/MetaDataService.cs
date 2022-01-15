using FrMonitor4_0.Models;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FrMonitor4_0.Services
{
    public class MetaDataService : IMetaDataService
    {
        public MetaConfig GetMetaConfig()
        {
            var nicheJson = "metaConfig.json";
            var json = File.ReadAllText(nicheJson);
            var jsonObj = JObject.Parse(json);
            return jsonObj.ToObject<MetaConfig>();
        }

        public void SetTargetReached()
        {
            var mcJson = "metaConfig.json";
            var json = File.ReadAllText(mcJson);
            var jsonObj = JObject.Parse(json);
            var mc = jsonObj.ToObject<MetaConfig>();

            mc.TargetReached = true;

            var newMc = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(mc));
            jsonObj = newMc;
            string newResult = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(mcJson, newResult);
        }
    }
}
