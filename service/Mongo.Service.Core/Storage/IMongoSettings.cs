using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public interface IMongoSettings
    {
        MongoServerAddress[] GetMongoServers();
        string GetMongoDataBaseName();
        string GetMongoReplicaSetName();
        string GetMongoUserName();
        string GetMongoPassword();
    }
}