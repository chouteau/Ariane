using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection ConfigureAriane(this IServiceCollection services, Action<IRegister> registerExpression = null, Action<ArianeSettings> settings = null)
        {
            var s = new ArianeSettings();
            if (settings != null)
            {
                settings.Invoke(s);
            }
            services.AddSingleton(s);

            services.AddTransient<ActionQueue>();
            services.AddLogging();

            var register = new Register(s,services);
            if (registerExpression != null)
            {
                registerExpression.Invoke(register);
            }

            services.AddSingleton<IRegister>(register);

            services.AddSingleton<FileMedium>();
            services.AddSingleton<InMemoryMedium>();
            services.AddSingleton<MSMQMedium>();
            
            if (s.WorkSynchronized)
            {
                services.AddSingleton<IServiceBus, SyncBusManager>();
            }
            else
            {
                services.AddSingleton<IServiceBus>(sp =>
                {
                    var instance = (BusManager)ActivatorUtilities.CreateInstance(sp, typeof(BusManager));
                    var mdList = ((Register)instance.Register).CreateMessageDispatcherList(sp);
					foreach (var md in mdList)
					{
                        instance.MessageDispatcherList.Add(md);
					}
                    return instance;
                });
            }

            return services;
        }
    }
}
