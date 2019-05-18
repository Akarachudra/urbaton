using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storage;

namespace Mongo.Service.Core
{
    public class InfoUpdater : IInfoUpdater
    {
        private readonly IEntityStorage<Info> infoRepository;

        public InfoUpdater(IEntityStorage<Info> infoRepository)
        {
            this.infoRepository = infoRepository;
        }
    }
}