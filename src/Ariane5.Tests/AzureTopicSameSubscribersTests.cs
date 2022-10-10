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
	public class AzureTopicSameSubscribersTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureArianeAzure(config =>
                {
                });
				services.ConfigureAriane(register =>
				{
					register.AddAzureTopicWriter("MyTopic");
					register.AddAzureTopicReader<PersonMessageReader>("MyTopic", "subsame");
					register.AddAzureTopicReader<PersonMessageReader>("MyTopic", "subsame");
				});
			});
		}

		private static IServiceProvider ServiceProvider { get; set; }

		[TestMethod]
		public async Task Send_And_Receive_Person_Topic()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Reset(3);

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			await bus.SendAsync("MyTopic", person);

			await bus.StartReadingAsync();

			await messageCollector.WaitForReceiveMessage(15 * 1000);

			Check.That(messageCollector.Count).IsEqualTo(1);

			await bus.StopReadingAsync();
		}
	}
}
