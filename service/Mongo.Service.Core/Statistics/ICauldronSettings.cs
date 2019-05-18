namespace Mongo.Service.Core.Statistics
{
    public interface ICauldronSettings
    {
        bool IsEnabled();
        string GetApplicationName();
    }
}