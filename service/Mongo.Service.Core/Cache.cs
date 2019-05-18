using System.Collections.Generic;
using Mongo.Service.Core.Entities;

namespace Mongo.Service.Core
{
    public static class Cache
    {
        public static Dictionary<int, Camera> Cameras { get; set; }
    }
}