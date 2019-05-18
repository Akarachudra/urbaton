using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services
{
    public class EntityService<TApi, TEntity> : IEntityService<TApi, TEntity> where TApi : IApiBase where TEntity : IBaseEntity
    {
        private readonly IMapper<TApi, TEntity> mapper;

        public EntityService(IEntityStorage<TEntity> storage, IMapper<TApi, TEntity> mapper)
        {
            Storage = storage;
            this.mapper = mapper;
        }

        public IEntityStorage<TEntity> Storage { get; }

        public virtual TApi Read(Guid id)
        {
            var entity = Storage.Read(id);
            return mapper.GetApiFromEntity(entity);
        }

        public virtual bool TryRead(Guid id, out TApi apiEntity)
        {
            TEntity entity;
            var result = Storage.TryRead(id, out entity);
            if (result)
            {
                apiEntity = mapper.GetApiFromEntity(entity);
                return true;
            }
            apiEntity = default(TApi);
            return false;
        }

        public virtual TApi[] Read(int skip, int limit)
        {
            var entities = Storage.Read(skip, limit);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = Storage.Read(filter, skip, limit);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField,
                                   bool desc = false)
        {
            var entities = Storage.Read(filter, skip, limit, orderField, desc);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] Read(Expression<Func<TEntity, bool>> filter)
        {
            var entities = Storage.Read(filter);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual TApi[] ReadAll()
        {
            var entities = Storage.ReadAll();
            return mapper.GetApiFromEntity(entities);
        }

        public virtual Guid[] ReadIds(Expression<Func<TEntity, bool>> filter)
        {
            return Storage.ReadIds(filter);
        }

        public virtual long ReadSyncedData(long lastSync, out TApi[] newData, out Guid[] deletedData,
                                           Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            TEntity[] newEntities;
            TEntity[] deletedEntities;

            var newSync = Storage.ReadSyncedData(lastSync, out newEntities, out deletedEntities, additionalFilter);

            newData = mapper.GetApiFromEntity(newEntities);
            deletedData = deletedEntities.Select(x => x.Id).ToArray();

            return newSync;
        }

        public virtual bool Exists(Guid id)
        {
            return Storage.Exists(id);
        }

        public virtual void Write(TApi apiEntity)
        {
            var entity = mapper.GetEntityFromApi(apiEntity);
            Storage.Write(entity);
        }

        public virtual void Write(TApi[] apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                Write(apiEntity);
            }
        }

        public virtual void Remove(Guid id)
        {
            Storage.Remove(id);
        }

        public virtual void Remove(Guid[] ids)
        {
            Storage.Remove(ids);
        }

        public virtual void Remove(TApi apiEntity)
        {
            Storage.Remove(apiEntity.Id);
        }

        public virtual void Remove(TApi[] apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                Storage.Remove(apiEntity.Id);
            }
        }

        public virtual async Task<TApi> ReadAsync(Guid id)
        {
            var entity = await Storage.ReadAsync(id);
            return mapper.GetApiFromEntity(entity);
        }

        public virtual async Task<Tuple<bool, TApi>> TryReadAsync(Guid id)
        {
            var result = await Storage.TryReadAsync(id);
            var exists = result.Item1;
            if (exists)
            {
                var entity = result.Item2;
                var apiEntity = mapper.GetApiFromEntity(entity);
                return Tuple.Create(true, apiEntity);
            }
            return Tuple.Create(false, default(TApi));
        }

        public virtual async Task<TApi[]> ReadAsync(int skip, int limit)
        {
            var entities = await Storage.ReadAsync(skip, limit);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<TApi[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit)
        {
            var entities = await Storage.ReadAsync(filter, skip, limit);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<TApi[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit,
                                                    Expression<Func<TEntity, object>> orderField, bool desc = false)
        {
            var entities = await Storage.ReadAsync(filter, skip, limit, orderField, desc);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<TApi[]> ReadAsync(Expression<Func<TEntity, bool>> filter)
        {
            var entities = await Storage.ReadAsync(filter);
            return mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<TApi[]> ReadAllAsync()
        {
            var entities = await Storage.ReadAllAsync();
            return mapper.GetApiFromEntity(entities);
        }

        public virtual async Task<Guid[]> ReadIdsAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await Storage.ReadIdsAsync(filter);
        }

        public virtual async Task<SyncApiResult<TApi>> ReadSyncedDataAsync(long lastSync, Expression<Func<TEntity, bool>> additionalFilter = null)
        {
            var syncEntityResult = await Storage.ReadSyncedDataAsync(lastSync, additionalFilter);

            var syncApiResult = new SyncApiResult<TApi>
            {
                LastSync = syncEntityResult.LastSync,
                NewData = mapper.GetApiFromEntity(syncEntityResult.NewData),
                DeletedIds = syncEntityResult.DeletedData.Select(x => x.Id).ToArray()
            };

            return syncApiResult;
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            return await Storage.ExistsAsync(id);
        }

        public virtual async Task WriteAsync(TApi apiEntity)
        {
            var entity = mapper.GetEntityFromApi(apiEntity);
            await Storage.WriteAsync(entity);
        }

        public virtual async Task WriteAsync(TApi[] apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                await WriteAsync(apiEntity);
            }
        }

        public virtual async Task RemoveAsync(Guid id)
        {
            await Storage.RemoveAsync(id);
        }

        public virtual async Task RemoveAsync(Guid[] ids)
        {
            await Storage.RemoveAsync(ids);
        }

        public virtual async Task RemoveAsync(TApi apiEntity)
        {
            await Storage.RemoveAsync(apiEntity.Id);
        }

        public virtual async Task RemoveAsync(TApi[] apiEntities)
        {
            foreach (var apiEntity in apiEntities)
            {
                await Storage.RemoveAsync(apiEntity.Id);
            }
        }
    }
}