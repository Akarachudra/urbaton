namespace Mongo.Service.Core.Statistics
{
    public class SystemMetrics
    {
        public int UsedThreads { get; set; }
        public int UsedIocpThreads { get; set; }
        public int WorkingRequests { get; set; }
        public float RequestsPerSecond { get; set; }
    }
}