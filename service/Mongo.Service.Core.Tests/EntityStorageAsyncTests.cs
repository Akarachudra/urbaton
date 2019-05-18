using System;
using System.Linq;
using System.Threading.Tasks;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storage;
using NUnit.Framework;

namespace Mongo.Service.Core.Tests
{
    [TestFixture]
    public class EntityStorageAsyncTests : TestsBase
    {
        private IEntityStorage<SampleEntity> storage;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            MongoStorage.ClearCollection<SampleEntity>();

            storage = new EntityStorage<SampleEntity>(MongoStorage, new Indexes<SampleEntity>());
        }

        [Test]
        public async Task CanWriteAndReadEntityByIdAsync()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await storage.WriteAsync(entity);

            var readedEntity = await storage.ReadAsync(entity.Id);

            Assert.AreEqual(entity.Id, readedEntity.Id);
            Assert.AreEqual(entity.SomeData, readedEntity.SomeData);
        }

        [Test]
        public async Task CanAutoFillLastModifiedDateTimeAsync()
        {
            var dateTimeBefore = DateTime.UtcNow.AddSeconds(-1);
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await storage.WriteAsync(entity);

            var dateTimeAfter = DateTime.UtcNow.AddSeconds(1);

            var readedEntity = await storage.ReadAsync(entity.Id);

            Assert.IsTrue(dateTimeBefore <= readedEntity.LastModified && readedEntity.LastModified <= dateTimeAfter);
        }

        [Test]
        public async Task CanWriteArrayAndReadWithFilterAsync()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "testData2"
                }
            };

            await storage.WriteAsync(entities);

            var readedEntities = await storage.ReadAsync(x => x.IsDeleted == false);
            Assert.AreEqual(2, readedEntities.Length);

            readedEntities = await storage.ReadAsync(x => x.SomeData == "testData1");
            Assert.AreEqual(1, readedEntities.Length);
            Assert.AreEqual("testData1", readedEntities[0].SomeData);
        }

        [Test]
        public async Task CanAutoFillIdIfItsEmptyAsync()
        {
            var entity = new SampleEntity
            {
                SomeData = "testData"
            };

            await storage.WriteAsync(entity);

            var readedEntities = await storage.ReadAsync(x => x.SomeData == "testData");
            Assert.IsTrue(readedEntities[0].Id != default(Guid));
        }

        [Test]
        public async Task TryReadIsCorrectAsync()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await storage.WriteAsync(entity);

            var readResult = await storage.TryReadAsync(entity.Id);
            var resultEntity = readResult.Item2;
            Assert.IsTrue(readResult.Item1);
            Assert.AreEqual("testData", resultEntity.SomeData);

            readResult = await storage.TryReadAsync(Guid.NewGuid());
            resultEntity = readResult.Item2;
            Assert.IsFalse(readResult.Item1);
            Assert.AreEqual(default(SampleEntity), resultEntity);
        }

        [Test]
        public async Task CanReadWithSkipAndLimitAsync()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "2"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                }
            };
            await storage.WriteAsync(entities);

            var anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData }).Take(2);
            var readedEntities = await storage.ReadAsync(0, 2);
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Where(x => x.SomeData == "3")
                                              .Select(x => new { x.Id, x.SomeData })
                                              .Skip(1)
                                              .Take(1);
            readedEntities = await storage.ReadAsync(x => x.SomeData == "3", 1, 1);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData })
                                              .Skip(1)
                                              .Take(2);
            readedEntities = await storage.ReadAsync(1, 2);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Reverse()
                                              .Select(x => new { x.Id, x.SomeData })
                                              .Skip(1)
                                              .Take(2);
            readedEntities = await storage.ReadAsync(x => !string.IsNullOrEmpty(x.SomeData), 1, 2, x => x.SomeData, true);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEqual(anonymousEntitiesBefore, anonymousEntitiesAfter);

            anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData })
                                              .Skip(1)
                                              .Take(2);
            readedEntities = await storage.ReadAsync(x => !string.IsNullOrEmpty(x.SomeData), 1, 2, x => x.SomeData);
            anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEqual(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public async Task CanRemoveEntitiesAsync()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await storage.WriteAsync(entity1);
            await storage.RemoveAsync(entity1);

            var readedEntity = await storage.ReadAsync(entity1.Id);
            Assert.IsTrue(readedEntity.IsDeleted);

            var entity3 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await storage.WriteAsync(new[] { entity2, entity3 });
            await storage.RemoveAsync(new[] { entity2, entity3 });
            readedEntity = await storage.ReadAsync(entity2.Id);
            Assert.IsTrue(readedEntity.IsDeleted);

            readedEntity = await storage.ReadAsync(entity3.Id);
            Assert.IsTrue(readedEntity.IsDeleted);
        }

        [Test]
        public async Task CheckWriteIsNotRestoreDeletedEntityAsync()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "testData"
            };

            await storage.WriteAsync(entity);
            await storage.RemoveAsync(entity);

            await storage.WriteAsync(entity);
            var readedEntity = await storage.ReadAsync(entity.Id);
            Assert.IsTrue(readedEntity.IsDeleted);
        }

        [Test]
        public async Task ExistsIsCorrectAsync()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid()
            };

            await storage.WriteAsync(entity);
            Assert.IsTrue(await storage.ExistsAsync(entity.Id));
            Assert.IsFalse(await storage.ExistsAsync(Guid.NewGuid()));
        }

        [Test]
        public async Task CanReadAllAsync()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "2"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "3"
                }
            };

            await storage.WriteAsync(entities);
            var anonymousEntitiesBefore = entities.Select(x => new { x.Id, x.SomeData });
            var readedEntities = await storage.ReadAllAsync();
            var anonymousEntitiesAfter = readedEntities.Select(x => new { x.Id, x.SomeData });
            CollectionAssert.AreEquivalent(anonymousEntitiesBefore, anonymousEntitiesAfter);
        }

        [Test]
        public async Task CanReadIdsOnlyAsync()
        {
            var entities = new[]
            {
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                },
                new SampleEntity
                {
                    Id = Guid.NewGuid(),
                    SomeData = "1"
                }
            };

            await storage.WriteAsync(entities);
            var idsBefore = entities.Select(x => x.Id);
            var idsAfer = await storage.ReadIdsAsync(x => x.SomeData == "1");
            CollectionAssert.AreEquivalent(idsBefore, idsAfer);
        }

        [Test]
        public async Task CountIsCorrectAsync()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };

            await storage.WriteAsync(entity1);
            Assert.AreEqual(1, storage.Count());

            await storage.WriteAsync(entity2);
            Assert.AreEqual(2, await storage.CountAsync());

            Assert.AreEqual(1, await storage.CountAsync(x => x.SomeData == "2"));
        }

        [Test]
        public async Task CanAutoincrementLastTickAsync()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };

            Assert.AreEqual(0, await storage.GetLastTickAsync());

            await storage.WriteAsync(entity1);
            var readedEntity1 = await storage.ReadAsync(entity1.Id);
            Assert.AreEqual(1, await storage.GetLastTickAsync());
            Assert.AreEqual(1, readedEntity1.Ticks);

            await storage.WriteAsync(entity2);
            var readedEntity2 = await storage.ReadAsync(entity2.Id);
            Assert.AreEqual(2, await storage.GetLastTickAsync());
            Assert.AreEqual(2, readedEntity2.Ticks);
        }

        [Test]
        public async Task CanReadSyncedDataAsync()
        {
            var syncResult = await storage.ReadSyncedDataAsync(0);

            Assert.AreEqual(0, syncResult.LastSync);

            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid()
            };
            await storage.WriteAsync(entity1);

            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid()
            };
            await storage.WriteAsync(entity2);

            syncResult = await storage.ReadSyncedDataAsync(syncResult.LastSync);
            var entities = syncResult.NewData;

            Assert.AreEqual(2, entities.Length);
            Assert.AreEqual(2, syncResult.LastSync);

            var previousSync = syncResult.LastSync;
            syncResult = await storage.ReadSyncedDataAsync(syncResult.LastSync);

            Assert.AreEqual(previousSync, syncResult.LastSync);

            await storage.RemoveAsync(entity2);
            syncResult = await storage.ReadSyncedDataAsync(syncResult.LastSync);
            var deletedEntities = syncResult.DeletedData;
            Assert.AreEqual(1, deletedEntities.Length);
            Assert.AreEqual(3, syncResult.LastSync);
        }

        [Test]
        public async Task CanReadSyncedDataWithFilterAsync()
        {
            var entity1 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "1"
            };
            await storage.WriteAsync(entity1);

            var entity2 = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "2"
            };
            await storage.WriteAsync(entity2);

            var syncResult = await storage.ReadSyncedDataAsync(0, x => x.SomeData == "2");
            var entities = syncResult.NewData;

            Assert.AreEqual(1, entities.Length);
            Assert.AreEqual(2, syncResult.LastSync);
        }

        [Test]
        public async Task CanUpdateTicksOnlyAsync()
        {
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid()
            };
            await storage.WriteAsync(entity);
            var readedEntity = await storage.ReadAsync(entity.Id);
            var ticksBefore = readedEntity.Ticks;
            await storage.UpdateTicksAsync(entity.Id);
            readedEntity = await storage.ReadAsync(entity.Id);
            Assert.AreEqual(ticksBefore + 1, readedEntity.Ticks);
        }

        [Test]
        public async Task CanUpdateEntityFieldsWithoutTicksAsync()
        {
            const string dataAfter = "data after";
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "data before"
            };
            await storage.WriteAsync(entity);
            var readedEntity = await storage.ReadAsync(entity.Id);
            var ticksBefore = readedEntity.Ticks;
            var updater = storage.Updater;
            var updateDefinition = updater.Set(x => x.SomeData, dataAfter);
            await storage.UpdateAsync(x => x.Id == entity.Id, updateDefinition);
            readedEntity = await storage.ReadAsync(entity.Id);
            Assert.AreEqual(ticksBefore, readedEntity.Ticks);
            Assert.AreEqual(dataAfter, readedEntity.SomeData);
        }

        [Test]
        public async Task CanUpdateEntityFieldsWitTicksAsync()
        {
            const string dataAfter = "data after";
            var entity = new SampleEntity
            {
                Id = Guid.NewGuid(),
                SomeData = "data before"
            };
            await storage.WriteAsync(entity);
            var readedEntity = await storage.ReadAsync(entity.Id);
            var ticksBefore = readedEntity.Ticks;
            var updater = storage.Updater;
            var updateDefinition = updater.Set(x => x.SomeData, dataAfter);
            await storage.UpdateWithTicksAsync(x => x.Id == entity.Id, updateDefinition);
            readedEntity = await storage.ReadAsync(entity.Id);
            Assert.AreEqual(ticksBefore + 1, readedEntity.Ticks);
            Assert.AreEqual(dataAfter, readedEntity.SomeData);
        }
    }
}