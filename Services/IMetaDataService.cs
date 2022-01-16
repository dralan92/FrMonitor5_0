using FrMonitor4_0.Models;

namespace FrMonitor4_0.Services
{
    public interface IMetaDataService
    {
        MetaConfig GetMetaConfig();

        void SetTargetReached();
        double GetTarget();
        bool IsTargetReached();
    }
}