using System;
using System.Linq;
using System.Threading.Tasks;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Tests.Helpers;
using Mongo.Service.Core.Types;
using NUnit.Framework;

namespace Mongo.Service.Core.Tests
{
    [TestFixture]
    public class EntityServiceAsyncTests : TestsBase
    {
        private readonly IEntityService<ApiSample, SampleEntity> service;

        public EntityServiceAsyncTests()
        {
            var storage = new EntityStorage<SampleEntity>(MongoStorage, new Indexes<SampleEntity>());
            var mapper = new Mapper<ApiSample, SampleEntity>();
            service = new EntityService<ApiSample, SampleEntity>(storage, mapper);
        }

        [SetUp]
        public void RunBeforeAnyTest()
        {
            MongoStorage.ClearCollection<SampleEntity>();
        }

        [Test]
        public async Task CanWriteAndReadAsync()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            await service.WriteAsync(apiEntity);
            var readedApiEntity = await service.ReadAsync(apiEntity.Id);

            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, readedApiEntity));
        }

        [Test]
        public async Task TryReadIsCorrectAsync()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            await service.WriteAsync(apiEntity);
            var readResult = await service.TryReadAsync(apiEntity.Id);
            var resultApiEntity = readResult.Item2;
            Assert.IsTrue(readResult.Item1);
            Assert.IsTrue(ObjectsComparer.AreEqual(apiEntity, resultApiEntity));

            readResult = await service.TryReadAsync(Guid.NewGuid());
            resultApiEntity = readResult.Item2;
            Assert.IsFalse(readResult.Item1);
            Assert.AreEqual(default(ApiSample), resultApiEntity);
        }

        [Test]
        public async Task CanWriteArrayAndReadWithFilterAsync()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData1"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData2"
                }
            };

            await service.WriteAsync(apiEntities);

            var readedApiEntities = await service.ReadAsync(x => x.SomeData == "testData1");
            Assert.AreEqual(1, readedApiEntities.Length);
            Assert.AreEqual("testData1", readedApiEntities[0].SomeData);
        }

        [Test]
        public async Task ExistsIsCorrectAsync()
        {
            var apiEntity = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "test"
            };

            await service.WriteAsync(apiEntity);
            Assert.IsTrue(await service.ExistsAsync(apiEntity.Id));
            Assert.IsFalse(await service.ExistsAsync(Guid.NewGuid()));
        }

        [Test]
        public async Task CanReadAllAsync()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData1"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData2"
                }
            };

            var anonymousBefore = apiEntities.Select(x => new { x.Id, x.SomeData });
            await service.WriteAsync(apiEntities);
            var readedAllApiEntities = await service.ReadAllAsync();
            var anonymousAfter = readedAllApiEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousBefore, anonymousAfter);
        }

        [Test]
        public async Task CanReadWithSkipAndLimitAsync()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "2"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                },
                new ApiSample
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                }
            };
            await service.WriteAsync(apiEntities);

            var anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData }).Take(2);
            var readedEntities = await service.ReadAsync(0, 2);
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Where(x => x.SomeData == "3")
                                                 .Select(x => new { x.Id, x.SomeData })
                                                 .Skip(1)
                                                 .Take(1);
            readedEntities = await service.ReadAsync(x => x.SomeData == "3", 1, 1);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData })
                                                 .Skip(1)
                                                 .Take(2);
            readedEntities = await service.ReadAsync(1, 2);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Reverse()
                                                 .Select(x => new { x.Id, x.SomeData })
                                                 .Skip(1)
                                                 .Take(2);
            readedEntities = await service.ReadAsync(x => !string.IsNullOrEmpty(x.SomeData), 1, 2, x => x.SomeData, true);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEqual(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = apiEntities.Select(x => new { x.Id, x.SomeData })
                                                 .Skip(1)
                                                 .Take(2);
            readedEntities = await service.ReadAsync(x => !string.IsNullOrEmpty(x.SomeData), 1, 2, x => x.SomeData);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEqual(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public async Task CanRemoveEntitiesAsync()
        {
            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            var apiEntity3 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await service.WriteAsync(apiEntity1);
            await service.RemoveAsync(apiEntity1);
            var readedEntities = await service.ReadAsync(x => !x.IsDeleted);
            Assert.AreEqual(0, readedEntities.Length);

            await service.WriteAsync(new[] { apiEntity2, apiEntity3 });
            await service.RemoveAsync(new[] { apiEntity2, apiEntity3 });
            readedEntities = await service.ReadAsync(x => !x.IsDeleted);
            Assert.AreEqual(0, readedEntities.Length);
        }

        [Test]
        public async Task CanReadIdsOnlyAsync()
        {
            var apiEntities = new[]
            {
                new ApiSample
                {
                    Id = Guid.NewGuid()
                },
                new ApiSample
                {
                    Id = Guid.NewGuid()
                },
                new ApiSample
                {
                    Id = Guid.NewGuid()
                },
                new ApiSample
                {
                    Id = Guid.NewGuid()
                }
            };

            await service.WriteAsync(apiEntities);
            var idsBefore = apiEntities.Select(x => x.Id);
            var idsAfter = await service.ReadIdsAsync(x => !x.IsDeleted);
            CollectionAssert.AreEquivalent(idsBefore, idsAfter);
        }

        [Test]
        public async Task CanReadSyncedDataAsync()
        {
            var syncResult = await service.ReadSyncedDataAsync(0);

            Assert.AreEqual(0, syncResult.LastSync);

            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            await service.WriteAsync(apiEntity1);

            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid()
            };
            await service.WriteAsync(apiEntity2);

            syncResult = await service.ReadSyncedDataAsync(syncResult.LastSync);
            var apiEntities = syncResult.NewData;

            Assert.AreEqual(2, apiEntities.Length);
            Assert.AreEqual(2, syncResult.LastSync);

            var previousSync = syncResult.LastSync;
            syncResult = await service.ReadSyncedDataAsync(syncResult.LastSync);

            Assert.AreEqual(previousSync, syncResult.LastSync);

            await service.RemoveAsync(apiEntity2);
            syncResult = await service.ReadSyncedDataAsync(syncResult.LastSync);
            var deletedIds = syncResult.DeletedIds;
            Assert.AreEqual(1, deletedIds.Length);
            Assert.AreEqual(3, syncResult.LastSync);
        }

        [Test]
        public async Task CanReadSyncedDataWithFilterAsync()
        {
            var apiEntity1 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            await service.WriteAsync(apiEntity1);

            var apiEntity2 = new ApiSample
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };
            await service.WriteAsync(apiEntity2);

            var syncResult = await service.ReadSyncedDataAsync(0, x => x.SomeData == "2");

            Assert.AreEqual(1, syncResult.NewData.Length);
            Assert.AreEqual(2, syncResult.LastSync);
        }
    }
}