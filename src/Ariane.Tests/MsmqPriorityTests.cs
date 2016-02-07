using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Ariane;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class MsmqPriorityTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			GlobalConfiguration.Configuration.Logger 
				= new Ariane.DiagnosticsLogger();
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
		public void Send_And_Read_MSMQ_Normal_Priority()
		{
			var bus = new BusManager();
			bus.Register.AddMSMQWriter("test.msmq");

			// Empty queue
			bus.Receive<Person>("test.msmq", 100, 2 * 1000);

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

			bus.Send("test.msmq", personNormal, priority : 0);
			bus.Send("test.msmq", personHigh, priority: 1);
			bus.Send("test.msmq", personVeryHigh, priority: 2);
			bus.Send("test.msmq", personHighest, priority: 3);

			IEnumerable<Person> list = null;
			while (true)
			{
				System.Threading.Thread.Sleep(4 * 1000);
				list = bus.Receive<Person>("test.msmq", 100, 4 * 1000);
				if (list != null && list.Count() != 0)
				{
					break;
				}
			}

			Check.That(list.ElementAt(0).FirstName).Equals("highest");
			Check.That(list.ElementAt(1).FirstName).Equals("veryhigh");
			Check.That(list.ElementAt(2).FirstName).Equals("high");
			Check.That(list.ElementAt(3).FirstName).Equals("normal");

			bus.Dispose();
		}

	}
}
