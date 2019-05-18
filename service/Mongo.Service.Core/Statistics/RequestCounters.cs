using System.Diagnostics;
using System.Threading;

namespace Mongo.Service.Core.Statistics
{
    public class RequestCounters : IRequestCounters
    {
        private readonly Stopwatch stopwatch;
        private int workingRequests;
        private long sampleRequests;

        public RequestCounters()
        {
            stopwatch = Stopwatch.StartNew();
        }

        public void OnBeginRequest()
        {
            Interlocked.Increment(ref sampleRequests);
            Interlocked.Increment(ref workingRequests);
        }

        public void OnEndRequest()
        {
            Interlocked.Decrement(ref workingRequests);
        }

        public RequestCountersSample NextSample()
        {
            var elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
            var sample = new RequestCountersSample
            {
                WorkingRequests = workingRequests,
                AverageRps = elapsedSeconds > 0 ? sampleRequests / elapsedSeconds : 0
            };

            if (elapsedSeconds >= 3)
            {
                stopwatch.Restart();
                sampleRequests = 0;
            }

            return sample;
        }
    }
}