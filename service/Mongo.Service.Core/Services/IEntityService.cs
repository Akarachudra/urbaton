using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services
{
    public interface IEntityService<TApi, TEntity> where TApi : IApiBase where TEntity : IBaseEntity
    {
        IEntityStorage<TEntity> Storage { get; }
        TApi Read(Guid id);
        bool TryRead(Guid id, out TApi apiEntity);
        TApi[] Read(int skip, int limit);
        TApi[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit);
        TApi[] Read(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField, bool desc = false);
        TApi[] Read(Expression<Func<TEntity, bool>> filter);
        TApi[] ReadAll();
        Guid[] ReadIds(Expression<Func<TEntity, bool>> filter);
        long ReadSyncedData(long lastSync, out TApi[] newData, out Guid[] deletedData,
                            Expression<Func<TEntity, bool>> additionalFilter = null);
        bool Exists(Guid id);
        void Write(TApi apiEntity);
        void Write(TApi[] apiEntities);
        void Remove(Guid id);
        void Remove(Guid[] ids);
        void Remove(TApi apiEntity);
        void Remove(TApi[] apiEntities);

        Task<TApi> ReadAsync(Guid id);
        Task<Tuple<bool, TApi>> TryReadAsync(Guid id);
        Task<TApi[]> ReadAsync(int skip, int limit);
        Task<TApi[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit);
        Task<TApi[]> ReadAsync(Expression<Func<TEntity, bool>> filter, int skip, int limit, Expression<Func<TEntity, object>> orderField, bool desc = false);
        Task<TApi[]> ReadAsync(Expression<Func<TEntity, bool>> filter);
        Task<TApi[]> ReadAllAsync();
        Task<Guid[]> ReadIdsAsync(Expression<Func<TEntity, bool>> filter);
        Task<SyncApiResult<TApi>> ReadSyncedDataAsync(long lastSync, Expression<Func<TEntity, bool>> additionalFilter = null);
        Task<bool> ExistsAsync(Guid id);
        Task WriteAsync(TApi apiEntity);
        Task WriteAsync(TApi[] apiEntities);
        Task RemoveAsync(Guid id);
        Task RemoveAsync(Guid[] ids);
        Task RemoveAsync(TApi apiEntity);
        Task RemoveAsync(TApi[] apiEntities);
    }
}