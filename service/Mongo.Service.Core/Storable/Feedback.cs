using Mongo.Service.Core.Storable.Base;
using Mongo.Service.Core.Storage;

namespace Mongo.Service.Core.Storable
{
    [CollectionName("Feedbacks")]
    public class Feedback : BaseEntity
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public int CameraNumber { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}