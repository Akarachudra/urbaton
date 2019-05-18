using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Microsoft.Owin;
using Owin;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using Kontur.Logging;
using Kontur.ThreadManagment;
using Kontur.Utilities;
using Mongo.Service.Core.Statistics;
using Mongo.Service.Core.Storable;
using Mongo.Service.Core.Storage;
using ZooKeeper.Recipes;

[assembly: OwinStartup(typeof(Mongo.Service.Core.WebApp.Startup))]

namespace Mongo.Service.Core.WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var container = new Container();
            container.ConfigureContainer();
            container.Verify();
            var cameraRepository = container.GetInstance<IEntityStorage<Camera>>();
            var cameras = cameraRepository.ReadAll();
            if (cameras.Length == 0)
            {
                cameraRepository.WriteAsync(
                                    new Camera
                                    {
                                        Description = "Парковка Питер",
                                        Number = 1,
                                        Url = "http://94.72.19.56/jpg/image.jpg?size=3"
                                    })
                                .Wait();
            }

            cameras = cameraRepository.ReadAll();
            Cache.Cameras = cameras.ToDictionary(x => x.Number, y => y);

            var infoUpdater = container.GetInstance<IInfoUpdater>();
            infoUpdater.Start();

            appBuilder.RecordRequest(container.GetInstance<IStatisticsRecorder>());
            appBuilder.UseRequestCounters(container.GetInstance<IRequestCounters>());

            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Services.Add(typeof(IExceptionLogger), container.GetInstance<ExceptionLogger>());
            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            config.Filters.Add(new CaptureActionForOwinFilter());

            appBuilder.UseWebApi(config);

            SetUpServices(appBuilder, container);
        }

        private void SetUpServices(IAppBuilder appBuilder, Container container)
        {
            var log = container.GetInstance<ILog>();
            var serviceSettings = container.GetInstance<IServiceSettings>();

            ProcessPriorityHelper.SetMemoryPriority(ProcessMemoryPriority.Normal, log);
            ProcessPriorityHelper.SetProcessPriorityClass(ProcessPriorityClass.Normal, log);
            ThreadPoolUtility.SetUp(log, serviceSettings.GetThreadMultiplier());

            var serviceBeacon = new ServiceBeacon(log, serviceSettings.GetPort());
            serviceBeacon.Start();

            var metricsWorker = container.GetInstance<MetricsWorker>();
            metricsWorker.Start();

            appBuilder.OnDisposing(
                () =>
                {
                    serviceBeacon.Stop();
                    metricsWorker.Stop();
                });
        }
    }
}