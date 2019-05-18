using System;

namespace Mongo.Service.Core.Statistics
{
    public interface IStatisticsRecorder
    {
        bool Enabled { get; }
        void RecordSystemMetrics(SystemMetrics metrics);
        void RecordAction(string action, string method, int statusCode, TimeSpan time, string route = null);
    }
}
