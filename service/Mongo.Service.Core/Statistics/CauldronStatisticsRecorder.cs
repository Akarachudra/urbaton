using System;
using Kontur.Cauldron;
using Kontur.Logging;

namespace Mongo.Service.Core.Statistics
{
    public class CauldronStatisticsRecorder : IStatisticsRecorder
    {
        private readonly string applicationName;
        private readonly string machineName;
        private readonly ILog log;

        public CauldronStatisticsRecorder(ICauldronSettings settings, ILog log)
        {
            this.log = log;
            machineName = Environment.MachineName;
            applicationName = settings.GetApplicationName();
            Enabled = settings.IsEnabled();

            if (Enabled && !Cauldron.IsInitialized(applicationName))
            {
                Cauldron.Init(applicationName, log);
            }
        }

        public bool Enabled { get; }

        public void RecordSystemMetrics(SystemMetrics metrics)
        {
            if (!Enabled) return;

            try
            {
                Cauldron.CreateRecord(applicationName, "systemMetrics")
                    .AddField("host", machineName)
                    .AddField("thrd", metrics.UsedThreads)
                    .AddField("iocp", metrics.UsedIocpThreads)
                    .AddField("wr", metrics.WorkingRequests)
                    .AddField("rps", (int)metrics.RequestsPerSecond)
                    .Commit();
            }
            catch (Exception exc)
            {
                log.Warn(exc);
            }
        }

        public void RecordAction(string action, string method, int statusCode, TimeSpan time, string route)
        {
            if (!Enabled) return;

            try
            {
                Cauldron.CreateRecord(applicationName, action)
                    .AddField("met", method, true)
                    .AddField("code", statusCode)
                    .AddField("time", time.Ticks)
                    .AddField("host", machineName)
                    .AddField("rt", route, true)
                    .Commit();
            }
            catch (Exception exc)
            {
                log.Warn(exc);
            }
        }
    }
}