using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
    [TestClass]
    public class InMemoryQueueWithPrefixTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureAriane(register =>
				{
                    register.AddQueue(new QueueSetting()
                    {
                        AutoStartReading = false,
                        Name = "sendreceivetestnoautostart",
                        TypeMedium = typeof(Ariane.InMemoryMedium),
                        TypeReader = typeof(PersonMessageReader)
                    });

					register.AddQueue(new QueueSetting()
					{
						Name = "sendreceivetest",
						TypeMedium = typeof(Ariane.InMemoryMedium),
						TypeReader = typeof(PersonMessageReader)
					});

					register.AddQueue(new QueueSetting()
					{
						AutoStartReading = false,
						Name = "sendreceivetest2",
						TypeMedium = typeof(Ariane.InMemoryMedium),
						TypeReader = typeof(BusDependencyPersonMessageReader)
					});


					register.AddMemoryWriter("same");
                    register.AddMemoryWriter("same");
				}, settings =>
				{
					settings.UniquePrefixName = "MyPrefix";
				});
			});
		}

        private static IServiceProvider ServiceProvider { get; set; }

		[TestMethod]
		public async Task Start_Stop_Start_Reader()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();
			await bus.StopReadingAsync("sendreceivetest");

			var personList = new List<Person>();

			for (int i = 0; i < 50; i++)
			{
				var person = new Person();
				person.FirstName = Guid.NewGuid().ToString();
				person.LastName = Guid.NewGuid().ToString();
				bus.Send("sendreceivetest", person);
				personList.Add(person);
			}

			var processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(0, processedCount);

			await bus.StartReadingAsync("sendreceivetest");

			await Task.Delay(10 * 1000);

			await bus.StopReadingAsync("sendreceivetest");

			processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(50, processedCount);

			foreach (var person in personList)
			{
				person.IsProcessed = false;
				bus.Send("sendreceivetest", person);
			}

			processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(0, processedCount);

			await bus.StartReadingAsync("sendreceivetest");

			await Task.Delay(10 * 1000);

			await bus.StopReadingAsync("sendreceivetest");

			processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(50, processedCount);
		}

		[TestMethod]
		public async Task Send_Then_Receive_Person_With_Memory_Medium()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			await bus.StartReadingAsync(); // Test if reader not autostarted

			for (int i = 0; i < 50; i++)
			{
				var person = new Person();
				person.FirstName = Guid.NewGuid().ToString();
				person.LastName = Guid.NewGuid().ToString();
				bus.Send("sendreceivetestnoautostart", person);
			}

			var personList = await bus.ReceiveAsync<Person>("sendreceivetestnoautostart", 10, 5 * 1000);

			Check.That(personList.Count()).Equals(10);

			personList = await bus.ReceiveAsync<Person>("sendreceivetestnoautostart", 50, 5 * 1000);

			Check.That(personList.Count()).Equals(40);
		}

		[TestMethod]
		public async Task Send_Then_Receive_Person_With_Dependency_Memory_Medium()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			await bus.StartReadingAsync(); // Test if reader not autostarted

			for (int i = 0; i < 50; i++)
			{
				var person = new Person();
				person.FirstName = Guid.NewGuid().ToString();
				person.LastName = Guid.NewGuid().ToString();
				bus.Send("sendreceivetest2", person);
			}

			var personList = await bus.ReceiveAsync<Person>("sendreceivetest2", 10, 10 * 1000);

			Check.That(personList.Count()).Equals(10);
		}
	}
}
