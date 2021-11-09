using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

using Ariane;

namespace Ariane5.ClientWriters
{
    class Program
    {
        static void Main(string[] args)
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
                register.AddMSMQWriter("q1");
                register.AddMSMQWriter("q2");
                register.AddMSMQWriter("q3");
                register.AddMSMQWriter("q4");
                register.AddMSMQWriter("q5");
            });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var sb = serviceProvider.GetRequiredService<IServiceBus>();

            var sw = new System.Diagnostics.Stopwatch();
            int count = 0;

            sb.Send("q1", new User());

            while (true)
            {
                sw.Start();
                Parallel.For(1, 6, (i) =>
                {
                    Parallel.For(0, 10000, (x) =>
                    {
                        sb.Send($"q{i}", new User());
                        if (count % 500 == 0)
                        {
                            Console.WriteLine($"{count} messages sent");
                        }
                        count++;
                    });
                });
                sw.Stop();
                Console.WriteLine($"5 * 10000 user queued in {sw.ElapsedMilliseconds} ms");
            }


        }
    }
}
