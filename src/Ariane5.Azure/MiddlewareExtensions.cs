using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection ConfigureArianeAzure(this IServiceCollection services, Action<AzureBusSettings> settings = null)
        {
            var s = new AzureBusSettings();
            if (settings != null)
            {
                settings.Invoke(s);
            }
            services.AddSingleton(s);

            services.AddSingleton<AzureQueueMedium>();
            services.AddSingleton<AzureTopicMedium>();
            return services;
        }
    }
}
