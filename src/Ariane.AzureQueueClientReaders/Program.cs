using Ariane;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Ariane5.AzureQueueClientReaders
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
                                        .AddJsonFile("localconfig\\appsettings.json",
                                            optional: false,
                                             reloadOnChange: false)
                                        .Build();

            serviceCollection.AddSingleton<IConfiguration>(configuration);

            serviceCollection.ConfigureArianeAzure();
            serviceCollection.ConfigureAriane(register =>
            {
                register.AddAzureQueueReader<R1>("q1");
                // register.AddAzureQueueReader<R2>("q2");
                // register.AddAzureQueueReader<R3>("q3");
                // register.AddAzureQueueReader<R4>("q4");
                //register.AddQueue(new QueueSetting()
                //{
                //    AutoStartReading = false,
                //    Name = "q5",
                //    TypeMedium = typeof(Ariane.AzureQueueMedium),
                //    TypeReader = typeof(R5)
                //});
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sb = serviceProvider.GetRequiredService<IServiceBus>();
            await sb.StartReadingAsync();

            //var list = await sb.ReceiveAsync<User>("q5", 1000);
            //Console.WriteLine($"receive {list.Count()} from q5");

            Console.Read();

            await sb.StopReadingAsync();
        }
    }
}
