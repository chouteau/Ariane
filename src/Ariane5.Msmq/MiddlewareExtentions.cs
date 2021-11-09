using Ariane;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    public static class MiddlewareExtentions
    {
        public static IServiceCollection ConfigureArianeMsmq(this IServiceCollection services)
        {
            services.AddSingleton<MSMQMedium>();
            return services;
        }

    }
}
