using Kontur.Configuration;

namespace Mongo.Service.Core.Statistics
{
    [Configuration("kontur.mobile/mongo.service.core/cauldronSettings", false, ConfigureFrom.ClusterConfig)]
    public class CauldronSettings : ICauldronSettings
    {
        public bool Enabled = true;
        public string ApplicationName = "mongo-service-core";

        public bool IsEnabled()
        {
            return Enabled;
        }

        public string GetApplicationName()
        {
            return ApplicationName;
        }
    }
}