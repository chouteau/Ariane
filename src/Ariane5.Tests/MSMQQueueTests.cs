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
    public class MSMQQueueTests
    {
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureArianeMsmq();
				services.ConfigureAriane(register =>
				{
					register.AddMSMQWriter("test6.msmq");
					register.AddMSMQWriter("test.msmq");
					register.AddQueue(new QueueSetting()
					{
						AutoStartReading = false,
						Name = "receive.msmq",
						TypeMedium = typeof(Ariane.MSMQMedium)
					});

					register.AddMSMQReader("test3.msmq", typeof(PersonMessageReader));
				});
			});
		}

		private static IServiceProvider ServiceProvider { get; set; }

		[TestMethod]
		public async Task Send_And_Read_In_Msmq()
		{
			var person = Person.CreateTestPerson();

			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Reset();

			await bus.StartReadingAsync();

			var firstName = person.FirstName;
			await bus.SendAsync("test3.msmq", person);

			await messageCollector.WaitForReceiveMessage(10 * 1000);

			person = messageCollector.GetList().First();

			Check.That(person).IsNotNull();
			Check.That(person.IsProcessed).IsTrue();
			Check.That(person.FirstName).Equals(firstName);

			await bus.StopReadingAsync();
		}

		[TestMethod]
		public async Task Send_And_Read_MSMQ_Normal_Priority()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			// Empty queue
			await bus.ReceiveAsync<Person>("test.msmq", 100, 2 * 1000);

			var personNormal = new Person();
			personNormal.FirstName = "normal";
			personNormal.LastName = Guid.NewGuid().ToString();

			var personHigh = new Person();
			personHigh.FirstName = "high";
			personHigh.LastName = Guid.NewGuid().ToString();

			var personVeryHigh = new Person();
			personVeryHigh.FirstName = "veryhigh";
			personVeryHigh.LastName = Guid.NewGuid().ToString();

			var personHighest = new Person();
			personHighest.FirstName = "highest";
			personHighest.LastName = Guid.NewGuid().ToString();

			await bus.SendAsync("test.msmq", personNormal, new MessageOptions() { Priority = 0 });
			await bus.SendAsync("test.msmq", personHigh, new MessageOptions() { Priority = 1 });
			await bus.SendAsync("test.msmq", personVeryHigh, new MessageOptions() { Priority = 2 });
			await bus.SendAsync("test.msmq", personHighest, new MessageOptions() { Priority = 3 });

			IEnumerable<Person> list = null;
			while (true)
			{
				await Task.Delay(4 * 1000);
				list = await bus.ReceiveAsync<Person>("test.msmq", 100, 4 * 1000);
				if (list != null && list.Count() != 0)
				{
					break;
				}
			}

			Check.That(list.ElementAt(0).FirstName).Equals("highest");
			Check.That(list.ElementAt(1).FirstName).Equals("veryhigh");
			Check.That(list.ElementAt(2).FirstName).Equals("high");
			Check.That(list.ElementAt(3).FirstName).Equals("normal");
		}

		[TestMethod]
		public async Task Receive_Empty_Queue_With_MSMQ_Medium()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var personList = await bus.ReceiveAsync<Person>("receive.msmq", 10, 5 * 1000);

			Check.That(personList.Count()).IsEqualTo(0);
		}

		[TestMethod]
		public async Task Write_In_MSMQ_And_Receive()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = Person.CreateTestPerson();
			await bus.SendAsync("test6.msmq", person);

			await Task.Delay(3 * 1000);

			var list = await bus.ReceiveAsync<Person>("test6.msmq", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsStrictlyGreaterThan(0);

			await bus.StopReadingAsync();
		}

	}
}
