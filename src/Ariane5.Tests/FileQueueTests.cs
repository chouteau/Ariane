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
    public class FileQueueTests
    {
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureAriane(register =>
				{
					register.AddFileReader("test.file", typeof(PersonMessageReader));
				});
			});
		}

		private static IServiceProvider ServiceProvider { get; set; }

		[TestMethod]
		public async Task Send_And_Read_In_File()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Reset();

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("test.file", person);
			await bus.StartReadingAsync();

			await messageCollector.WaitForReceiveMessage(2 * 1000);

			Check.That(messageCollector.Count).IsStrictlyGreaterThan(0);

			await bus.StopReadingAsync();
		}


	}
}
