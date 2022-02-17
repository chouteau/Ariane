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
    public class DynamicInMemoryQueueTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureAriane(register =>
				{
					register.AddMemoryReader<MyDynamicMessageReader>("dynamic");
				});
			});
		}

        private static IServiceProvider ServiceProvider { get; set; }


		[TestMethod]
		public async Task Send_Then_Receive_Person_With_Dynamic_Reader()
        {
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();
			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Reset();

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();
			var d = bus.CreateMessage("test");
			d.Person = person;
			await bus.SendAsync("dynamic", d);

			await bus.StartReadingAsync();

			await messageCollector.WaitForReceiveMessage(5 * 1000);

			Check.That(messageCollector.Count).IsEqualTo(1);

			await bus.StopReadingAsync();
		}

	}
}
