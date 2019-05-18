using System.Collections.Generic;
using Mongo.Service.Core.Storable;

namespace Mongo.Service.Core
{
    public static class Cache
    {
        public static Dictionary<int, Camera> Cameras { get; set; }

        public static object ImageLocker = new object();
    }
}