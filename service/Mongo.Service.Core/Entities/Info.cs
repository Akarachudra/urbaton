using System;
using Mongo.Service.Core.Entities.Base;
using Mongo.Service.Core.Repository.Attributes;

namespace Mongo.Service.Core.Entities
{
    [CollectionName("Infos")]
    public class Info : BaseEntity
    {
        public Guid CameraId { get; set; }

        public int TotalPlaces { get; set; }

        public int FreePlaces { get; set; }

        public int OccupiedPlaces { get; set; }
    }
}