﻿using Mongo.Service.Core.Entities.Base;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Service.Core.Repository.Indexes
{
    public class Indexes<TEntity> : IIndexes<TEntity>
        where TEntity : IBaseEntity
    {
        public void CreateIndexes(IMongoCollection<TEntity> collection)
        {
            collection.Indexes.CreateOne(new CreateIndexModel<TEntity>(
                new BsonDocumentIndexKeysDefinition<TEntity>(new BsonDocument()).Descending(x => x.Ticks),
                new CreateIndexOptions { Background = true, Unique = true }));
            this.CreateCustomIndexes(collection);
        }

        protected virtual void CreateCustomIndexes(IMongoCollection<TEntity> collection)
        {
        }
    }
}