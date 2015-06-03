using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class ReceiveTests
	{
		[TestMethod]
		public void Send_Then_Receive_Person_With_Memory_Medium()
		{
			var bus = new BusManager();
			bus.Register.AddQueue(new QueueSetting()
			{
				AutoStartReading = false,
				Name = "sendreceivetest",
				TypeMedium = typeof(Ariane.InMemoryMedium)
			});

			bus.StartReading(); // Test if reader not autostarted

			for (int i = 0; i < 50; i++)
			{
				var person = new Person();
				person.FirstName = Guid.NewGuid().ToString();
				person.LastName = Guid.NewGuid().ToString();
				bus.Send("sendreceivetest", person);
			}

			var personList = bus.Receive<Person>("sendreceivetest", 10, 5 * 1000);

			Check.That(personList.Count()).Equals(10);

			personList = bus.Receive<Person>("sendreceivetest", 50, 2 * 1000);

			Check.That(personList.Count()).Equals(40);

			bus.Dispose();
		}

		[TestMethod]
		public void Receive_With_Unkwnow_Queue()
		{
			var bus = new Ariane.BusManager();
			var result = bus.Receive<Person>("unknown", 10, 10);

			Check.That(result).IsNull();
		}

	}
}
