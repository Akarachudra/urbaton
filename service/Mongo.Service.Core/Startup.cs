using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mongo.Service.Core.Entities;
using Mongo.Service.Core.Repository;
using Mongo.Service.Core.Repository.Indexes;
using Mongo.Service.Core.Services;
using Mongo.Service.Core.Services.Mapping;
using Mongo.Service.Core.Types;

namespace Mongo.Service.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IMongoStorage, MongoStorage>();
            services.AddSingleton<IMongoSettings, MongoSettings>();
            services.AddSingleton<IMongoRepository<SampleEntity>, MongoRepository<SampleEntity>>();
            services.AddSingleton<IMongoRepository<Camera>, MongoRepository<Camera>>();
            services.AddSingleton<IIndexes<SampleEntity>, Indexes<SampleEntity>>();
            services.AddSingleton<IIndexes<Camera>, Indexes<Camera>>();
            services.AddSingleton<IEntityService<ApiSample, SampleEntity>, EntityService<ApiSample, SampleEntity>>();
            services.AddSingleton<IMapper<ApiSample, SampleEntity>, Mapper<ApiSample, SampleEntity>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            var serviceProvider = app.ApplicationServices;
            var cameraRepository = serviceProvider.GetService<IMongoRepository<Camera>>();
            var cameras = cameraRepository.ReadAllAsync().Result;
            if (cameras.Count == 0)
            {
                cameraRepository.WriteAsync(
                    new Camera
                    {
                        Description = "Парковка Питер",
                        Number = 1,
                        Url = "http://94.72.19.56/jpg/image.jpg?size=3"
                    });
            }

            cameras = cameraRepository.ReadAllAsync().Result;
            Cache.Cameras = cameras.ToDictionary(x => x.Number, y => y);
        }
    }
}