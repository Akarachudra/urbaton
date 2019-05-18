namespace Mongo.Service.Core.WebApp
{
    public interface IServiceSettings
    {
        int GetPort();
        int GetThreadMultiplier();
    }
}