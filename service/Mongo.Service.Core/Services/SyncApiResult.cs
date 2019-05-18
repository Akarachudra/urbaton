using System;
using Mongo.Service.Core.Types.Base;

namespace Mongo.Service.Core.Services
{
    public class SyncApiResult<TApi> where TApi : IApiBase
    {
        private TApi[] newData;
        private Guid[] deletedIds;
        public long LastSync { get; set; }

        public TApi[] NewData
        {
            get { return newData ?? (newData = new TApi[0]); }
            set { newData = value; }
        }

        public Guid[] DeletedIds
        {
            get { return deletedIds ?? (deletedIds = new Guid[0]); }
            set { deletedIds = value; }
        }
    }
}