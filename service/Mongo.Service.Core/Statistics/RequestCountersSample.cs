namespace Mongo.Service.Core.Statistics
{
    public struct RequestCountersSample
    {
        public int WorkingRequests { get; set; }
        public float AverageRps { get; set; }
    }
}