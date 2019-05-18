namespace Mongo.Service.Core.Statistics
{
    public interface IRequestCounters
    {
        void OnBeginRequest();
        void OnEndRequest();
        RequestCountersSample NextSample();
    }
}