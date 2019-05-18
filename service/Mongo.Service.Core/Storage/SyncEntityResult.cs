using Mongo.Service.Core.Storable.Base;

namespace Mongo.Service.Core.Storage
{
    public class SyncEntityResult<TEntity> where TEntity : IBaseEntity
    {
        private TEntity[] newData;
        private TEntity[] deletedData;
        public long LastSync { get; set; }

        public TEntity[] NewData
        {
            get { return newData ?? (newData = new TEntity[0]); }
            set { newData = value; }
        }

        public TEntity[] DeletedData
        {
            get { return deletedData ?? (deletedData = new TEntity[0]); }
            set { deletedData = value; }
        }
    }
}