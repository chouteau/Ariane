using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

using Ariane;

namespace Ariane5.AzureTopicClientReaders
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
                register.AddAzureTopicReader<T1>("t1","s1");
                register.AddAzureTopicReader<T2>("t2", "s2");
                register.AddAzureTopicReader<T3>("t3", "s3");
                register.AddAzureTopicReader<T4>("t4", "s4");
                register.AddQueue(new QueueSetting()
                {
                    AutoStartReading = false,
                    Name = "t5",
                    SubscriptionName = "s5",
                    TypeMedium = typeof(Ariane.AzureTopicMedium),
                    TypeReader = typeof(T5)
                });
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sb = serviceProvider.GetRequiredService<IServiceBus>();
            await sb.StartReadingAsync();

            var list = await sb.ReceiveAsync<User>("t5", 1000);
            Console.WriteLine($"receive {list.Count()} from t5");

            Console.Read();

        }
    }
}
