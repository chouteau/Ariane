using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Ariane;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Ariane.Tests
{
	[TestClass]
	public class MediumTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = (IServiceProvider)AppDomain.CurrentDomain.GetData("ServiceProvider");
		}

		public static IServiceProvider ServiceProvider { get; set; }

		[TestInitialize()]
		public void MyTestInitialize()
		{
			StaticContainer.Model = null;
		}

		[TestCleanup()]
		public void MyTestCleanup()
		{
			System.Threading.Thread.Sleep(3 * 1000);
		}

		[TestMethod]
		public async Task Send_And_Read_In_Memory()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.SyncProcess("test.memory3", person);

			Assert.IsTrue(person.IsProcessed);

			await bus.StopReadingAsync();
			bus.Dispose();
		}



		[TestMethod]
		public async Task Write_In_Memory_And_Receive()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = Person.CreateTestPerson();
			bus.Send("test.memory4", person);

			System.Threading.Thread.Sleep(3 * 1000);

			var list = bus.Receive<Person>("test.memory4", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsStrictlyGreaterThan(0);

			await bus.StopReadingAsync();
			bus.Dispose();
		}

		[TestMethod]
		public async Task Send_And_Read_In_File()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("test.file", person);
			await bus.StartReadingAsync();

			System.Threading.Thread.Sleep(1000);

			await bus.StopReadingAsync();
			bus.Dispose();
		}

		[TestMethod]
		public async Task Send_Dynamic_And_Read_With_Anonymous_In_File()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var d = bus.CreateMessage("test");
			var firstName = d.FirstName = Guid.NewGuid().ToString();
			var lastName = d.LastName = Guid.NewGuid().ToString();

			await bus.StartReadingAsync();
			bus.Send("test.file", d);

			dynamic result = null;
			int loopcount = 0;
			while (true)
			{
				result = StaticContainer.Model as dynamic;
				if (result != null
					|| loopcount++ > 10)
				{
					break;
				}
				System.Threading.Thread.Sleep(1 * 1000);
			}

			Assert.AreEqual(result.FirstName, firstName);
			Assert.AreEqual(result.LastName, lastName);

			await bus.StopReadingAsync();
			bus.Dispose();
		}

		[TestMethod]
		public async Task Write_In_File_And_Receive()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = Person.CreateTestPerson();
			bus.Send("test.file", person);

			System.Threading.Thread.Sleep(3 * 1000);

			var list = bus.Receive<Person>("test.file", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsStrictlyGreaterThan(0);

			await bus.StopReadingAsync();
			bus.Dispose();
		}

		[TestMethod]
		public async Task Send_And_Read_In_Msmq()
		{
			var person = Person.CreateTestPerson();

			var bus = ServiceProvider.GetRequiredService<IServiceBus>();
			await bus.StartReadingAsync();

			var firstName = person.FirstName;
			bus.Send("test3.msmq", person);

			int loopcount = 0;
			while (true)
			{
				person = StaticContainer.Model as Person;
				if (person != null
					|| loopcount++ > 10)
				{
					break;
				}
				System.Threading.Thread.Sleep(1 * 1000);
			}

			Check.That(person).IsNotNull();
			Check.That(person.IsProcessed).IsTrue();
			Check.That(person.FirstName).Equals(firstName);

			await bus.StopReadingAsync();
			bus.Dispose();
		}


	}
}
