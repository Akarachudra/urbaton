using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Storable.Base;
using MongoDB.Driver;

namespace Mongo.Service.Core.Storage
{
    public interface IEntityStorage<TEntity> where TEntity : IBaseEntity
    {
        IMongoCollection<TEntity> Collection { get; }
        UpdateDefinitionBuilder<TEntity> Updater { get; }
        TEntity Read(Guid id);
        TEntity[] Read(Expression<Func<TEntity, bool>> filter);
        TEntity[] Read(int skip, int limit);
        TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit);
        TEntity[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField, bool desc = false);
        bool TryRead(Guid id, out TEntity outEntity);
        TEntity[] ReadAll();
        Guid[] ReadIds(Expression<Func<TEntity, bool>> filter);

        long ReadSyncedData(long lastSync, out TEntity[] newData, out TEntity[] deletedData,
                            Expression<Func<TEntity, bool>> additionalFilter = null);

        bool Exists(Guid id);
        void Write(TEntity entity);
        void Write(TEntity[] entities);
        void Remove(Guid id);
        void Remove(Guid[] ids);
        void Remove(TEntity entity);
        void Remove(TEntity[] entities);
        long Count();
        long Count(Expression<Func<TEntity, bool>> filter);
        long GetLastTick();
        void UpdateTicks(Guid id);
        void Update(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false);
        void UpdateWithTicks(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false);

        Task<TEntity> ReadAsync(Guid id);
        Task<TEntity[]> ReadAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity[]> ReadAsync(int skip, int limit);
        Task<TEntity[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit);

        Task<TEntity[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField,
                                  bool desc = false);

        Task<Tuple<bool, TEntity>> TryReadAsync(Guid id);
        Task<TEntity[]> ReadAllAsync();
        Task<Guid[]> ReadIdsAsync(Expression<Func<TEntity, bool>> filter);
        Task<SyncEntityResult<TEntity>> ReadSyncedDataAsync(long lastSync, Expression<Func<TEntity, bool>> additionalFilter = null);
        Task<bool> ExistsAsync(Guid id);
        Task WriteAsync(TEntity entity);
        Task WriteAsync(TEntity[] entities);
        Task RemoveAsync(Guid id);
        Task RemoveAsync(Guid[] ids);
        Task RemoveAsync(TEntity entity);
        Task RemoveAsync(TEntity[] entities);
        Task<long> CountAsync();
        Task<long> CountAsync(Expression<Func<TEntity, bool>> filter);
        Task<long> GetLastTickAsync();
        Task UpdateTicksAsync(Guid id);
        Task UpdateAsync(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false);
        Task UpdateWithTicksAsync(Expression<Func<TEntity, bool>> filter, UpdateDefinition<TEntity> updateDefinition, bool isUpsert = false);
    }
}