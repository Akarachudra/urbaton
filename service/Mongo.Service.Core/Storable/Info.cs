using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;

namespace Mongo.Service.Core.Storable
{
    [CollectionName("Infos")]
    public class Info : BaseEntity
    {
        public int CameraNumber { get; set; }

        public int TotalPlaces { get; set; }

        public int FreePlaces { get; set; }

        public int OccupiedPlaces { get; set; }
    }
}