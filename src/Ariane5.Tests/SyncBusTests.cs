using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class SyncBusTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureAriane(
					reg =>
					{
						reg.AddQueue(new QueueSetting()
						{
							AutoStartReading = true,
							Name = "sendsync",
							TypeMedium = typeof(Ariane.InMemoryMedium),
							TypeReader = typeof(PersonMessageReader)
						});
					},
					s =>
					{
						s.WorkSynchronized = true;
					});
			});
		}

		public static IServiceProvider ServiceProvider { get; set; }

		[TestMethod]
		public async Task Send_Synchronized_Message()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();
			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Reset();

			Check.That(bus.GetType()).IsEqualTo(typeof(Ariane.SyncBusManager));

			await bus.StartReadingAsync();

			var person = new Person();
			var firstName = person.FirstName = $"{Guid.NewGuid()}";

			bus.Send("sendsync", person);

			var syncReceivePeson = messageCollector.GetList().Single();
			
			Check.That(syncReceivePeson.FirstName).IsEqualTo(firstName);


		}
	}
}
