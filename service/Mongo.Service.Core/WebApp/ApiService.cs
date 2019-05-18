using System;

namespace Mongo.Service.Core.WebApp
{
    public class ApiService : IApiService
    {
        private readonly IServiceSettings settings;
        private IDisposable app;

        public ApiService(IServiceSettings settings)
        {
            this.settings = settings;
        }
        
        public void Start()
        {
            app = Microsoft.Owin.Hosting.WebApp.Start<Startup>($"http://+:{settings.GetPort()}/");
        }

        public void Stop()
        {
            app?.Dispose();
            app = null;
        }
    }
}