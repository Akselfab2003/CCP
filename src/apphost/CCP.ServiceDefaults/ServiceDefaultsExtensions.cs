using CCP.ServiceDefaults.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace CCP.ServiceDefaults
{
    public static class ServiceDefaultsExtensions
    {

        public static void AddServiceDefaults(this IServiceCollection collection, string ServiceName)
        {
            collection.ConfigureDefaultOpenTelemetry(ServiceName);
        }
    }
}
