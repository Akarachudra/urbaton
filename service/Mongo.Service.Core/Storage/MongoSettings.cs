using System.Collections.Generic;
using Kontur.Configuration;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    [Configuration("kontur.mobile/mongo.service.core/mongoSettings", false, ConfigureFrom.ClusterConfig)]
    public class MongoSettings : IMongoSettings
    {
        public string MongoServers;
        public string MongoDatabaseName;
        public string MongoUserName;
        public string MongoPassword;
        public string MongoReplicaSetName;

        public MongoSettings()
        {
            MongoDatabaseName = "MongoServiceCore";
            MongoServers = "localhost:27017";
        }

        public MongoServerAddress[] GetMongoServers()
        {
            var servers = new List<MongoServerAddress>();
            var mongoServerArray = MongoServers.Split(',');
            foreach (var mongoServerValue in mongoServerArray)
            {
                var splitted = mongoServerValue.Split(':');
                var mongoServer = splitted[0];
                var mongoPort = int.Parse(splitted[1]);
                servers.Add(new MongoServerAddress(mongoServer, mongoPort));
            }
            return servers.ToArray();
        }

        public string GetMongoDataBaseName()
        {
            return MongoDatabaseName;
        }

        public string GetMongoReplicaSetName()
        {
            return MongoReplicaSetName;
        }

        public string GetMongoUserName()
        {
            return MongoUserName;
        }

        public string GetMongoPassword()
        {
            return MongoPassword;
        }
    }
}