using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Ariane5.Tests")]

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

            if (s.WorkSynchronized)
            {
                services.AddSingleton<IServiceBus>(sp =>
                {
                    var bus = (IServiceBus)ActivatorUtilities.CreateInstance(sp, typeof(BusManager));
                    return new SyncBusManager(sp, bus, s);
                });
            }
			else
			{
                services.AddSingleton<IServiceBus, BusManager>();
                if (s.AutoStart)
				{
                    services.AddHostedService<AutoStartBackgroundService>();
				}
            }

            register.Initialize();
            return services;
        }
    }
}
