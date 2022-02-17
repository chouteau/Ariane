using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using Ariane;

namespace Ariane5.AzureQueueClientWriters
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
                register.AddAzureQueueWriter("q1");
                register.AddAzureQueueWriter("q2");
                register.AddAzureQueueWriter("q3");
                register.AddAzureQueueWriter("q4");
                register.AddAzureQueueWriter("q5");
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sb = serviceProvider.GetRequiredService<IServiceBus>();

            var sw = new System.Diagnostics.Stopwatch();
            int count = 0;

            sw.Start();

			for (int m = 0; m < 100; m++)
			{
                var user = new User();
                user.MessageId = m;
                await sb.SendAsync($"q1", user);

				if (count % 500 == 0)
				{
					Console.WriteLine($"{count} messages sent");
				}
				count++;
			}

			//Parallel.For(1, 6, (i) =>
			//{
			//    for (int m = 0; m < 10000; m++)
			//    {
			//        sb.Send($"q{i}", new User() { MessageId = messa});
			//        if (count % 500 == 0)
			//        {
			//            Console.WriteLine($"{count} messages sent");
			//        }
			//        count++;
			//    }
			//});
			sw.Stop();

            Console.WriteLine($"5 * 10000 user queued in {sw.ElapsedMilliseconds} ms");
            Console.Read();

        }
    }
}
