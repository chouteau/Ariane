using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Tests
{
    public class RootTests
    {
        public static IServiceProvider Initialize(Action<IServiceCollection> services)
        {
            var serviceCollection = new ServiceCollection()
                                        .AddLogging(config =>
                                        {
                                            config.AddConsole();
                                            config.AddDebug();
                                        });

            var configuration = new ConfigurationBuilder()
                                        .SetBasePath(System.Environment.CurrentDirectory)
                                        .AddJsonFile("localconfig\\appsettings.json",
                                            optional: false,
                                             reloadOnChange: false)
                                        .Build();

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton<MessageCollector>();

            services.Invoke(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
