using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;
using System.Threading;

namespace Ariane.Tests
{
	[TestClass]
	public class MediumTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			GlobalConfiguration.Configuration.Logger = new Ariane.DiagnosticsLogger();
		}

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
		public void Send_And_Read_In_Memory()
		{
			var bus = new BusManager();
			bus.Register.AddMemoryReader("test.memory3", typeof(PersonMessageReader));

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.SyncProcess("test.memory3", person);

			Assert.IsTrue(person.IsProcessed);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_And_Read_With_Anonymous_In_Memory()
		{
			var mre = new ManualResetEvent(false);
			var bus = new Ariane.BusManager();

			bus.Register.AddMemoryReader<Person>("person", (p) =>
			{
				p.IsProcessed = true;
				mre.Set();
			});

			var person = Person.CreateTestPerson();

			bus.Send("person", person);
			bus.StartReading("person");

			mre.WaitOne(2 * 1000);

			Check.That(person.IsProcessed).IsTrue();

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_Dynamic_Message_And_Read_With_Anonymous_In_Memory()
		{
			var bus = new Ariane.BusManager();

			var person = bus.CreateMessage("person");
			person.IsProcessed = false;

			var mre = new ManualResetEvent(false);
			bus.Register.AddMemoryReader<System.Dynamic.ExpandoObject>("dynamicperson", (o) =>
			{
				var p = o as dynamic;
				p.IsProcessed = true;
				mre.Set();
			});

			bus.Send("dynamicperson", person);
			bus.StartReading("dynamicperson");

			mre.WaitOne(10 * 1000);

			Assert.AreEqual(true, person.IsProcessed);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Write_In_Memory_And_Receive()
		{
			var bus = new BusManager();
			bus.Register.AddMemoryWriter("test.memory4");

			var person = Person.CreateTestPerson();
			bus.Send("test.memory4", person);

			System.Threading.Thread.Sleep(3 * 1000);

			var list = bus.Receive<Person>("test.memory4", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsGreaterThan(0);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_And_Read_In_File()
		{
			var bus = new BusManager();
			bus.Register.AddFileReader("test.file", typeof(PersonMessageReader));

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("test.file", person);
			bus.StartReading();

			System.Threading.Thread.Sleep(1000);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_And_Read_With_Anonymous_In_File()
		{
			var person = Person.CreateTestPerson();

			var bus = new BusManager();
			var mre = new ManualResetEvent(false);
			bus.Register.AddFileReader<Person>("test.file", (p) =>
				{
					person = p;					
					p.IsProcessed = true;
					mre.Set();
				});

			var firstName = person.FirstName;
			bus.StartReading("test.file");
			bus.Send("test.file", person);

			mre.WaitOne();

			Check.That(person.IsProcessed).IsTrue();
			Check.That(person.FirstName).Equals(firstName);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_Dynamic_And_Read_With_Anonymous_In_File()
		{
			var bus = new BusManager();
			bus.Register.AddFileReader("test.file", typeof(MyDynamicMessageReader));

			var d = bus.CreateMessage("test");
			var firstName = d.FirstName = Guid.NewGuid().ToString();
			var lastName = d.LastName = Guid.NewGuid().ToString();

			bus.StartReading();
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

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Write_In_File_And_Receive()
		{
			var bus = new BusManager();
			bus.Register.AddFileWriter("test.file");

			var person = Person.CreateTestPerson();
			bus.Send("test.file", person);

			System.Threading.Thread.Sleep(3 * 1000);

			var list = bus.Receive<Person>("test.file", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsGreaterThan(0);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Use_Msmq_With_Missing_ConnectionString()
		{
			var bus = new BusManager();
			bus.Register.AddMSMQReader("emptycs", typeof(PersonMessageReader));
			try
			{
				bus.StartReading();
			}
			catch(Exception ex)
			{
				Check.That(ex.Message).Equals("queueName emptycs does not exists in connectionStrings section configuration");
			}

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_And_Read_In_Msmq()
		{
			var person = Person.CreateTestPerson();

			var bus = new BusManager();
			bus.Register.AddMSMQReader("test3.msmq", typeof(PersonMessageReader));
			bus.StartReading();

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

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_And_Read_With_Anonymous_In_Msmq()
		{
			var person = Person.CreateTestPerson();
			var firstName = person.FirstName;

			var bus = new BusManager();
			var mre = new ManualResetEvent(false);
			bus.Register.AddMSMQReader<Person>("test5.msmq", (p) =>
				{
					person = p;
					p.IsProcessed = true;
					mre.Set();
				});

			bus.Send("test5.msmq", person);
			bus.StartReading();

			mre.WaitOne(10 * 1000);

			Check.That(person.IsProcessed).IsTrue();
			Check.That(person.FirstName).Equals(firstName);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Send_Dynamic_And_Read_With_Anonymous_In_Msmq()
		{
			var bus = new BusManager();

			dynamic person = bus.CreateMessage("dynamicperson");
			var firstName = person.FirstName = Guid.NewGuid().ToString();
			person.IsProcessed = false;

			var mre = new ManualResetEvent(false);
			bus.Register.AddMSMQReader<System.Dynamic.ExpandoObject>("test4.msmq", (p) =>
			{
				person = p as dynamic;
				person.IsProcessed = true;
				mre.Set();
			});

			bus.Send("test4.msmq", person);
			bus.StartReading();

			mre.WaitOne(10 * 1000);

			Assert.IsTrue(person.IsProcessed);
			Assert.AreEqual(firstName,person.FirstName);

			bus.StopReading();
			bus.Dispose();
		}

		[TestMethod]
		public void Write_In_MSMQ_And_Receive()
		{
			var bus = new BusManager();
			bus.Register.AddMSMQWriter("test6.msmq");

			var person = Person.CreateTestPerson();
			bus.Send("test6.msmq", person);

			System.Threading.Thread.Sleep(3 * 1000);

			var list = bus.Receive<Person>("test6.msmq", 10, 1 * 1000);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsGreaterThan(0);

			bus.StopReading();
			bus.Dispose();
		}
	}
}
