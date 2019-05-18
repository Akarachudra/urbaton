using Kontur.Configuration;
using Kontur.Logging;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Statistics;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storable.Indexes;
using Mongo.Service.Core.Storage;
using Mongo.Service.Core.Types;
using SimpleInjector;

namespace Mongo.Service.Core.WebApp
{
    internal static class ContainerConfig
    {
        public static Container ConfigureContainer(this Container container)
        {
            container.RegisterSingleton<ILog>(() => new Log4netWrapper());
            container.RegisterSingleton<IServiceSettings>(() => Configuration<ServiceSettings>.Get());
            container.RegisterSingleton<IMongoSettings>(() => Configuration<MongoSettings>.Get());
            container.RegisterSingleton<ICauldronSettings>(() => Configuration<CauldronSettings>.Get());
            container.RegisterSingleton<IStatisticsRecorder, CauldronStatisticsRecorder>();
            container.RegisterSingleton<ISystemMetricsProvider, SystemMetricsProvider>();
            container.RegisterSingleton<IRequestCounters, RequestCounters>();

            container.RegisterSingleton<IMongoStorage, MongoStorage>();
            container.RegisterSingleton<IEntityStorage<SampleEntity>, EntityStorage<SampleEntity>>();
            container.RegisterSingleton<IEntityStorage<Camera>, EntityStorage<Camera>>();
            container.RegisterSingleton<IEntityStorage<Info>, EntityStorage<Info>>();
            container.RegisterSingleton<IEntityStorage<Feedback>, EntityStorage<Feedback>>();
            container.RegisterSingleton<IIndexes<SampleEntity>, Indexes<SampleEntity>>();
            container.RegisterSingleton<IIndexes<Camera>, Indexes<Camera>>();
            container.RegisterSingleton<IIndexes<Info>, Indexes<Info>>();
            container.RegisterSingleton<IIndexes<Feedback>, Indexes<Feedback>>();
            container.RegisterSingleton<IEntityService<ApiSample, SampleEntity>, EntityService<ApiSample, SampleEntity>>();
            container.RegisterSingleton<IMapper<ApiSample, SampleEntity>, Mapper<ApiSample, SampleEntity>>();
            container.RegisterSingleton<IInfoUpdater, InfoUpdater>();

            return container;
        }
    }
}