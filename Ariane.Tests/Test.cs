using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;

namespace Ariane.Tests
{
	[NUnit.Framework.TestFixture]
	public class Test
	{
		[NUnit.Framework.Test]
		public void Send_And_Process_Person_In_Memory()
		{
			var bus = new BusManager();
			bus.Register.AddMemoryReader("test.memory3", typeof(PersonMessageReader));

			var person = new Person();
			person.FirsName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.SyncProcess("test.memory3", person);

			NUnit.Framework.Assert.IsTrue(person.IsProcessed);

			bus.StopReading();
		}

		[NUnit.Framework.Test]
		public void Send_And_Process_Person_In_File()
		{
			var bus = new BusManager();
			bus.Register.AddFileReader("test.file", typeof(PersonMessageReader));

			var person = new Person();
			person.FirsName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("test.file", person);
			bus.StartReading();

			System.Threading.Thread.Sleep(1000);

			bus.StopReading();
		}

		[NUnit.Framework.Test]
		public void Send_And_Process_Dynamic_Message()
		{
			var bus = new BusManager();
			bus.Register.AddFileReader("test.file", typeof(MyDynamicMessageReader));

			var d = bus.CreateMessage("test");
			var firstName = d.FirstName = Guid.NewGuid().ToString();
			var lastName = d.LastName = Guid.NewGuid().ToString();

			bus.Send("test.file", d);

			bus.StartReading();

			System.Threading.Thread.Sleep(15000);

			var result = StaticContainer.Model as dynamic;

			NUnit.Framework.Assert.AreEqual(result.FirstName, firstName);
			NUnit.Framework.Assert.AreEqual(result.LastName, lastName);

			bus.StopReading();
		}
	}
}
