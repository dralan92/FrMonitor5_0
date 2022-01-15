namespace FrMonitor4_0.Services
{
    public interface IMonitorService
    {
        void Monitor(int timeframe);

        void TargetCheck();
    }
}