using FrMonitor4_0.Models;
using Newtonsoft.Json.Linq;
using System.IO;

namespace FrMonitor4_0.Services
{
    public class MetaDataService : IMetaDataService
    {
        string _metaFile = "metaConfig.json";
        public MetaConfig GetMetaConfig()
        {
            var nicheJson = _metaFile;
            var json = File.ReadAllText(nicheJson);
            var jsonObj = JObject.Parse(json);
            return jsonObj.ToObject<MetaConfig>();
        }

        public void SetTargetReached()
        {
            var mcJson = _metaFile;
            var json = File.ReadAllText(mcJson);
            var jsonObj = JObject.Parse(json);
            var mc = jsonObj.ToObject<MetaConfig>();

            mc.TargetReached = true;

            var newMc = JObject.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(mc));
            jsonObj = newMc;
            string newResult = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(mcJson, newResult);
        }

        public double GetTarget()
        {
            var json = ReadMetaFile();
            var jsonObj = JObject.Parse(json);
            var metaConfig = jsonObj.ToObject<MetaConfig>();
            return metaConfig.Target;
        }

        public bool IsTargetReached()
        {
            var json = ReadMetaFile();
            var jsonObj = JObject.Parse(json);
            var metaConfig = jsonObj.ToObject<MetaConfig>();
            return metaConfig.TargetReached;
        }

        string ReadMetaFile()
        {
            string result = "";
            using (StreamReader sr = File.OpenText(_metaFile))
            {
                if((result = sr.ReadToEnd()) != null)
                {
                    return result;
                }
            }
            return result;
        }
    }
}
