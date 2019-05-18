using Kontur.Configuration;

namespace Mongo.Service.Core.WebApp
{
    [Configuration("kontur.mobile/mongo.service.core/settings", false, ConfigureFrom.ClusterConfig)]
    public class ServiceSettings : IServiceSettings
    {
        public int Port;

        [Comment("Множитель для установки тредпула")]
        public int ThreadMultiplier;

        public ServiceSettings()
        {
            Port = 12512;
            ThreadMultiplier = 20;
        }

        public int GetPort()
        {
            return Port;
        }

        public int GetThreadMultiplier()
        {
            return ThreadMultiplier;
        }
    }
}