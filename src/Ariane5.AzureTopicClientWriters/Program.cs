using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using Ariane;


namespace Ariane5.AzureTopicClientWriters
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
                register.AddAzureTopicWriter("t1");
                //register.AddAzureTopicWriter("t2");
                //register.AddAzureTopicWriter("t3");
                //register.AddAzureTopicWriter("t4");
                //register.AddAzureTopicWriter("t5");
                //register.AddAzureQueueWriter("BankTest");
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            await SendMessages(serviceProvider);
        }

        private static async Task SendBigMessage(IServiceProvider serviceProvider)
        {
            var sb = serviceProvider.GetRequiredService<IServiceBus>();

            var content = System.IO.File.ReadAllText("localconfig\\message.txt");
            var message = System.Text.Json.JsonSerializer.Deserialize<ValidationPaymentRequest>(content);

            for (int i = 0; i < 100; i++)
            {
                await sb.SendAsync($"BankTest", message);
            }

            Console.WriteLine($"message sent");
            Console.Read();
        }

        private static async Task SendMessages(IServiceProvider serviceProvider)
        {
            var sb = serviceProvider.GetRequiredService<IServiceBus>();

            var sw = new System.Diagnostics.Stopwatch();
            int count = 0;

            // await sb.SendAsync("t1", new User());

            sw.Start();

            for (int m = 0; m < 20; m++)
            {
                await sb.SendAsync($"t1", new User()
                {
                    Name = $"name{m}"
                });
                Console.WriteLine($"{count} messages queued");
                count++;
            }

            /*
            Parallel.For(1, 6, (i) =>
            {
                for (int m = 0; m < 10000; m++)
                {
                    sb.Send($"t{i}", new User());
                    if (count % 500 == 0)
                    {
                        Console.WriteLine($"{count} messages queued");
                    }
                    count++;
                }
            }); */
            sw.Stop();

            Console.WriteLine($"{count} messages");
            Console.Read();

        }

        
    }
}
