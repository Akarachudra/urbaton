using System.Collections.Generic;
using Mongo.Service.Core.Entities.Base;
using Mongo.Service.Core.Repository.Attributes;

namespace Mongo.Service.Core.Entities
{
    [CollectionName("Cameras")]
    public class Camera : BaseEntity
    {
        private IList<Place> places;

        public string Url { get; set; }

        public int Number { get; set; }

        public string Description { get; set; }

        public IList<Place> Places
        {
            get => this.places ?? (this.places = new List<Place>());
            set => this.places = value;
        }
    }
}