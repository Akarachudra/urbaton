using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Extensions;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public class EntityStorage<TEntity> : IEntityStorage<TEntity> where TEntity : IBaseEntity
    {
        private const int TicksWriteTries = 500;

        public EntityStorage(IMongoStorage mongoStorage, IIndexes<TEntity> indexes)
        {
            Collection = mongoStorage.GetCollection<TEntity>();
            indexes.CreateIndexes(Collection);
        }

        public IMongoCollection<TEntity> Collection { get; }

        public UpdateDefinitionBuilder<TEntity> Updater => Builders<TEntity>.Update;

        public TEntity Read(Guid id)
        {
            var entity = Collection.FindSync(x => x.Id == id).FirstOrDefault();
            if (entity == null)
            {
                throw new Exception($"{typeof(TEntity).Name} with id {id} not found");
            }
            return entity;
        }

        public TEntity[] Read(Expression<Func<TEntity, bool>> filter)
        {
            var entities = Collection.FindSync(filter).ToList();
            return entities.ToArray();
        }

        public TEntity[] Read(int skip, int limit)
        {
            var entities = Collection.Aggregate().Skip(skip).Limit(limit).ToList();
            return entities.ToArray();
        }

        public TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = Collection.Aggregate().Match(filter).Skip(skip).Limit(limit).ToList();
            return entities.ToArray();
        }

        public TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField, bool desc = false)
        {
            var sorter = Builders<TEntity>.Sort;
            var sortDefinition = desc ? sorter.Descending(orderField) : sorter.Ascending(orderField);
            var entities = Collection.Aggregate().Match(filter).Sort(sortDefinition).Skip(skip).Limit(limit).ToList();
            return entities.ToArray();
        }

        public bool TryRead(Guid id, out TEntity outEntity)
        {
            var entity = Collection.FindSync(x => x.Id == id).FirstOrDefault();
            if (entity == null)
            {
                outEntity = default(TEntity);
                return false;
            }
            outEntity = entity;
            return true;
        }

        public TEntity[] ReadAll()
        {
            var entities = Collection.FindSync(FilterDefinition<TEntity>.Empty).ToList();
            return entities.ToArray();
        }

        public Guid[] ReadIds(Expression<Func<TEntity, bool>> filter)
        {
            var ids = Collection.FindSync(filter).ToList().Select(x => x.Id).ToArray();
            return ids;
        }

        public long ReadSyncedData(long lastSync, out TEntity[] newData, out TEntity[] deletedData,
                                   Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var newLastSync = GetLastTick();

            Expression<Func<TEntity, bool>> newFilter = x => !x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;
            Expression<Func<TEntity, bool>> deletedFilter = x => x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;

            if (additionalFilter != null)
            {
                newFilter = newFilter.And(additionalFilter);
                deletedFilter = deletedFilter.And(additionalFilter);
            }

            newData = Read(newFilter);
            deletedData = Read(deletedFilter);

            return newLastSync;
        }

        public bool Exists(Guid id)
        {
            return Collection.FindSync(x => x.Id == id).FirstOrDefault() != null;
        }

        public void Write(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            else
            {
                TEntity currentEntity;
                var exists = TryRead(entity.Id, out currentEntity);
                if (exists && currentEntity.IsDeleted)
                {
                    entity.IsDeleted = true;
                }
            }
            entity.LastModified = DateTime.UtcNow;

            TryWriteEntity(entity);
        }

        public void Write(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                Write(entity);
            }
        }

        public void Remove(Guid id)
        {
            var entity = Read(id);
            entity.IsDeleted = true;

            Write(entity);
        }

        public void Remove(Guid[] ids)
        {
            foreach (var id in ids)
            {
                Remove(id);
            }
        }

        public void Remove(TEntity entity)
        {
            Remove(entity.Id);
        }

        public void Remove(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity.Id);
            }
        }

        public long Count()
        {
            return Collection.Count(FilterDefinition<TEntity>.Empty);
        }

        public long Count(Expression<Func<TEntity, bool>> filter)
        {
            return Collection.Count(filter);
        }

        public long GetLastTick()
        {
            var sort = Builders<TEntity>.Sort.Descending(x => x.Ticks);
            var result = Collection.Find(FilterDefinition<TEntity>.Empty).Sort(sort).Limit(1).ToList();
            return result.Count == 0 ? 0 : result[0].Ticks;
        }

        public void UpdateTicks(Guid id)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = GetLastTick() + 1;
                    var updateTicks = Builders<TEntity>.Update.Set(x => x.Ticks, lastTicks);
                    Collection.UpdateOne(x => x.Id == id, updateTicks);
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Update ticks tries of {nameof(TEntity)} limit exceeded");
        }

        public void Update(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false)
        {
            Collection.UpdateOne(filter, updateDefinition, new UpdateOptions { IsUpsert = isUpsert });
        }

        public void UpdateWithTicks(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = GetLastTick() + 1;
                    var updateWithTicks = updateDefinition.Set(x => x.Ticks, lastTicks);
                    Collection.UpdateOne(filter, updateWithTicks, new UpdateOptions { IsUpsert = isUpsert });
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Update with ticks tries of {nameof(TEntity)} limit exceeded");
        }

        public async Task<TEntity> ReadAsync(Guid id)
        {
            var result = await Collection.FindAsync(x => x.Id == id);
            var entity = result.FirstOrDefault();
            if (entity == null)
            {
                throw new Exception($"{typeof(TEntity).Name} with id {id} not found");
            }
            return entity;
        }

        public async Task<TEntity[]> ReadAsync(Expression<Func<TEntity, bool>> filter)
        {
            var result = await Collection.FindAsync(filter);
            var entities = await result.ToListAsync();
            return entities.ToArray();
        }

        public async Task<TEntity[]> ReadAsync(int skip, int limit)
        {
            var entities = await Collection.Aggregate().Skip(skip).Limit(limit).ToListAsync();
            return entities.ToArray();
        }

        public async Task<TEntity[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = await Collection.Aggregate().Match(filter).Skip(skip).Limit(limit).ToListAsync();
            return entities.ToArray();
        }

        public async Task<TEntity[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField,
                                               bool desc = false)
        {
            var sorter = Builders<TEntity>.Sort;
            var sortDefinition = desc ? sorter.Descending(orderField) : sorter.Ascending(orderField);
            var entities = await Collection.Aggregate().Match(filter).Sort(sortDefinition).Skip(skip).Limit(limit).ToListAsync();
            return entities.ToArray();
        }

        public async Task<Tuple<bool, TEntity>> TryReadAsync(Guid id)
        {
            var result = await Collection.FindAsync(x => x.Id == id);
            var entity = result.FirstOrDefault();
            if (entity == null)
            {
                return Tuple.Create(false, default(TEntity));
            }
            return Tuple.Create(true, entity);
        }

        public async Task<TEntity[]> ReadAllAsync()
        {
            var result = await Collection.FindAsync(FilterDefinition<TEntity>.Empty);
            var entities = await result.ToListAsync();
            return entities.ToArray();
        }

        public async Task<Guid[]> ReadIdsAsync(Expression<Func<TEntity, bool>> filter)
        {
            var result = await Collection.FindAsync(filter);
            var entities = await result.ToListAsync();
            var ids = entities.Select(x => x.Id).ToArray();
            return ids;
        }

        public async Task<SyncEntityResult<TEntity>> ReadSyncedDataAsync(long lastSync, Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var newLastSync = await GetLastTickAsync();

            Expression<Func<TEntity, bool>> newFilter = x => !x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;
            Expression<Func<TEntity, bool>> deletedFilter = x => x.IsDeleted && x.Ticks > lastSync && x.Ticks <= newLastSync;

            if (additionalFilter != null)
            {
                newFilter = newFilter.And(additionalFilter);
                deletedFilter = deletedFilter.And(additionalFilter);
            }

            var syncResult = new SyncEntityResult<TEntity>
            {
                LastSync = newLastSync,
                NewData = await ReadAsync(newFilter),
                DeletedData = await ReadAsync(deletedFilter)
            };

            return syncResult;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            var result = await Collection.FindAsync(x => x.Id == id);
            var entities = await result.ToListAsync();
            return entities.FirstOrDefault() != null;
        }

        public async Task WriteAsync(TEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }
            else
            {
                var tryWriteResult = await TryReadAsync(entity.Id);
                var exists = tryWriteResult.Item1;
                var currentEntity = tryWriteResult.Item2;
                if (exists && currentEntity.IsDeleted)
                {
                    entity.IsDeleted = true;
                }
            }
            entity.LastModified = DateTime.UtcNow;

            await TryWriteEntityAsync(entity);
        }

        public async Task WriteAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                await WriteAsync(entity);
            }
        }

        public async Task RemoveAsync(Guid id)
        {
            var entity = await ReadAsync(id);
            entity.IsDeleted = true;

            await WriteAsync(entity);
        }

        public async Task RemoveAsync(Guid[] ids)
        {
            foreach (var id in ids)
            {
                await RemoveAsync(id);
            }
        }

        public async Task RemoveAsync(TEntity entity)
        {
            await RemoveAsync(entity.Id);
        }

        public async Task RemoveAsync(TEntity[] entities)
        {
            foreach (var entity in entities)
            {
                await RemoveAsync(entity.Id);
            }
        }

        public async Task<long> CountAsync()
        {
            return await Collection.CountAsync(FilterDefinition<TEntity>.Empty);
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await Collection.CountAsync(filter);
        }

        public async Task<long> GetLastTickAsync()
        {
            var sort = Builders<TEntity>.Sort.Descending(x => x.Ticks);
            var result = await Collection.Find(FilterDefinition<TEntity>.Empty).Sort(sort).Limit(1).ToListAsync();
            return result.Count == 0 ? 0 : result[0].Ticks;
        }

        public async Task UpdateTicksAsync(Guid id)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = await GetLastTickAsync() + 1;
                    var updateTicks = Builders<TEntity>.Update.Set(x => x.Ticks, lastTicks);
                    await Collection.UpdateOneAsync(x => x.Id == id, updateTicks);
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Update ticks tries of {nameof(TEntity)} limit exceeded");
        }

        public async Task UpdateAsync(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false)
        {
            await Collection.UpdateOneAsync(filter, updateDefinition, new UpdateOptions { IsUpsert = isUpsert });
        }

        public async Task UpdateWithTicksAsync(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false)
        {
            for (var i = 0; i < TicksWriteTries; i++)
            {
                try
                {
                    var lastTicks = await GetLastTickAsync() + 1;
                    var updateWithTicks = updateDefinition.Set(x => x.Ticks, lastTicks);
                    await Collection.UpdateOneAsync(filter, updateWithTicks, new UpdateOptions { IsUpsert = isUpsert });
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception($"Update with ticks tries of {nameof(TEntity)} limit exceeded");
        }

        private void TryWriteEntity(TEntity entity)
        {
            for (var i = 0; i < 500; i++)
            {
                entity.Ticks = GetLastTick() + 1;

                try
                {
                    Collection.ReplaceOne(x => x.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception("Write tries limit exceeded.");
        }

        private async Task TryWriteEntityAsync(TEntity entity)
        {
            for (var i = 0; i < 500; i++)
            {
                entity.Ticks = await GetLastTickAsync() + 1;

                try
                {
                    await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity, new UpdateOptions { IsUpsert = true });
                    return;
                }
                catch (MongoWriteException exception)
                {
                    if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            throw new Exception("Write tries limit exceeded.");
        }
    }
}