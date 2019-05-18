﻿using System;
using Mongo.Service.Core.Entities.Base;
using Mongo.Service.Core.Repository.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Service.Core.Repository
{
    public class MongoStorage : IMongoStorage
    {
        private readonly IMongoDatabase database;

        public MongoStorage(IMongoSettings settings)
        {
            var mongoDataBaseName = settings.MongoDatabaseName;

            var mongoClientSettings = new MongoClientSettings
            {
                Servers = settings.MongoServers,
                WriteConcern = WriteConcern.W1,
                ReadPreference = ReadPreference.Primary,
                GuidRepresentation = GuidRepresentation.Standard
            };

            var mongoReplicaSetName = settings.MongoReplicaSetName;
            if (!string.IsNullOrEmpty(mongoReplicaSetName))
            {
                mongoClientSettings.ReplicaSetName = mongoReplicaSetName;
            }

            var mongoUserName = settings.MongoUserName;
            if (!string.IsNullOrEmpty(mongoUserName))
            {
                mongoClientSettings.Credential =
                    MongoCredential.CreateCredential(
                        mongoDataBaseName,
                        mongoUserName,
                        settings.MongoPassword);
            }

            var client = new MongoClient(mongoClientSettings);

            this.database = client.GetDatabase(mongoDataBaseName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>()
            where TEntity : IBaseEntity
        {
            var collectionName = GetCollectionName(typeof(TEntity));
            return this.database.GetCollection<TEntity>(collectionName);
        }

        public void DropCollection<TEntity>()
            where TEntity : IBaseEntity
        {
            this.database.DropCollection(GetCollectionName(typeof(TEntity)));
        }

        public void ClearCollection<TEntity>()
            where TEntity : IBaseEntity
        {
            this.GetCollection<TEntity>().DeleteMany(FilterDefinition<TEntity>.Empty);
        }

        private static string GetCollectionName(Type type)
        {
            foreach (var attr in type.GetCustomAttributes(false))
            {
                var attribute = attr as CollectionNameAttribute;
                if (attribute != null)
                {
                    var collectionName = attribute.Name;
                    if (string.IsNullOrEmpty(collectionName))
                    {
                        throw new ArgumentException(
                            $"There is empty collection name at {typeof(CollectionNameAttribute).Name} in {type.Name}");
                    }

                    return collectionName;
                }
            }

            throw new ArgumentException(
                $"There is no {typeof(CollectionNameAttribute).Name} attribute at {type.Name}");
        }
    }
}