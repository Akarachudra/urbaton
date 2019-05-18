using Mongo.Service.Core.Storage;
using NUnit.Framework;

namespace Mongo.Service.Core.Tests
{
    [TestFixture]
    public class TestsBase
    {
        protected readonly IMongoStorage MongoStorage;

        public TestsBase()
        {
            MongoStorage = new MongoStorage(new MongoSettings());
        }
    }
}