namespace Mongo.Service.Core.Statistics
{
    public interface ISystemMetricsProvider
    {
        SystemMetrics GetSystemMetrics();
    }
}