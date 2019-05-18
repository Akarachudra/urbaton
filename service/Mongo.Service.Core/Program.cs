using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Mongo.Service.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                       .UseKestrel()
                       .UseContentRoot(Directory.GetCurrentDirectory())
                       .UseIISIntegration()
                       .UseStartup<Startup>()
                       .UseUrls("http://localhost:5001/")
                       .Build();

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>();
    }
}