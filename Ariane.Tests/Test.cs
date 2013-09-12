using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	[NUnit.Framework.TestFixture]
	public class Test
	{
		[NUnit.Framework.Test]
		public void Send_And_Process_Person()
		{
			var bus = new BusManager();
			bus.Register.AddMemoryReader("test.memory3", typeof(PersonMessageReader));

			var person = new Person();
			person.FirsName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.SyncProcess("test.memory3", person);

			NUnit.Framework.Assert.IsTrue(person.IsProcessed);
		}
	}
}
