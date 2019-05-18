using System.Threading;
using Kontur.Logging;
using Kontur.ThreadManagment;

namespace Mongo.Service.Core.Statistics
{
    public class MetricsWorker
    {
        private readonly IStatisticsRecorder statisticsRecorder;
        private readonly ISystemMetricsProvider systemMetricsProvider;
        private readonly ILog log;

        public MetricsWorker(IStatisticsRecorder statisticsRecorder, ISystemMetricsProvider systemMetricsProvider, ILog log)
        {
            this.statisticsRecorder = statisticsRecorder;
            this.systemMetricsProvider = systemMetricsProvider;
            this.log = log;
        }

        public void Start()
        {
            if (!statisticsRecorder.Enabled || thread != null)
            {
                return;
            }

            thread = ThreadRunner.Run(ThreadRoutine, log);
        }

        public void Stop()
        {
            thread?.AbortAndWaitCompleted();
        }

        private void ThreadRoutine()
        {
            while (true)
            {
                var metrics = systemMetricsProvider.GetSystemMetrics();
                statisticsRecorder.RecordSystemMetrics(metrics);
                Thread.Sleep(5000);
            }
        }

        private Thread thread;
    }
}