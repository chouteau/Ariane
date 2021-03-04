using Ariane;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Ariane5.ClientReaders
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging(config =>
                {
                    config.AddConsole();
                    config.AddDebug();
                });

            var configuration = new ConfigurationBuilder()
                                        .SetBasePath(System.Environment.CurrentDirectory)
                                        .AddJsonFile("appsettings.json",
                                            optional: false,
                                             reloadOnChange: false)
                                        .Build();

            serviceCollection.AddSingleton<IConfiguration>(configuration);

            serviceCollection.ConfigureAriane(register =>
            {
                register.AddMSMQReader<R1>("q1");
                register.AddMSMQReader<R2>("q2");
                register.AddMSMQReader<R3>("q3");
                register.AddMSMQReader<R4>("q4");
                register.AddMSMQReader<R5>("q5");
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sb = serviceProvider.GetRequiredService<IServiceBus>();
            await sb.StartReadingAsync();

            Console.Read();
        }
    }
}
