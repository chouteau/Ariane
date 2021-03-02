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
	public class AzureTopicTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureArianeAzure();
				services.ConfigureAriane(register =>
				{
					register.AddAzureTopicWriter("MyTopic");
					register.AddAzureTopicReader<PersonMessageReader>("MyTopic", "sub1");
					register.AddAzureTopicReader<PersonMessageReader>("MyTopic", "sub2");
					register.AddAzureTopicReader<PersonMessageReader>("MyTopic", "sub3");

					register.AddQueue(new QueueSetting()
					{
						AutoStartReading = false,
						Name = "MyTopic2",
						SubscriptionName = "Sub1",
						TypeMedium = typeof(AzureTopicMedium),
						TypeReader = typeof(PersonMessageReader)
					});

				});
			});
		}

		private static IServiceProvider ServiceProvider { get; set; }


		[TestMethod]
		public async Task Send_And_Receive_Person_Topic()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Clear();

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("MyTopic", person);

			await bus.StartReadingAsync();

			await Task.Delay(5 * 1000);

			Check.That(messageCollector.Count).IsEqualTo(3);
		}

		[TestMethod]
		public async Task Write_In_Azure_Queue_And_Receive()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = Person.CreateTestPerson();
			bus.Send("MyTopic2", person);

			await Task.Delay(3 * 1000);

			var list = await bus.ReceiveAsync<Person>("MyTopic2", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsStrictlyGreaterThan(0);
		}

	}
}
