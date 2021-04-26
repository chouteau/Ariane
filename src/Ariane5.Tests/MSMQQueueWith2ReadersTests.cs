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
    public class MSMQQueueWith2ReadersTests
    {
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureAriane(register =>
				{
					register.AddMSMQReader<PersonMessageReader>("test7.msmq");
					register.AddMSMQReader<PersonMessageReader2>("test7.msmq");
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
			messageCollector.Clear();

			await bus.StartReadingAsync();

			var firstName = person.FirstName;
			bus.Send("test7.msmq", person);

			await Task.Delay(10 * 1000);

			var personList = messageCollector.GetList();
			person = personList.First();

			Check.That(person).IsNotNull();
			Check.That(person.IsProcessed).IsTrue();
			Check.That(person.FirstName).Equals(firstName);

			Check.That(personList.Count()).IsEqualTo(2);

			await bus.StopReadingAsync();
		}

	}
}
