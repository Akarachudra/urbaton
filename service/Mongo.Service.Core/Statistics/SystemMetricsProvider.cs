using Kontur.ThreadManagment;

namespace Mongo.Service.Core.Statistics
{
    public class SystemMetricsProvider : ISystemMetricsProvider
    {
        private readonly IRequestCounters requestCounters;

        public SystemMetricsProvider(IRequestCounters requestCounters)
        {
            this.requestCounters = requestCounters;
        }

        public SystemMetrics GetSystemMetrics()
        {
            var threadPoolState = ThreadPoolUtility.GetPoolState();
            var sample = requestCounters.NextSample();

            return new SystemMetrics
            {
                UsedThreads = threadPoolState.UsedThreads,
                UsedIocpThreads = threadPoolState.UsedIocpThreads,
                WorkingRequests = sample.WorkingRequests,
                RequestsPerSecond = sample.AverageRps
            };
        }
    }
}