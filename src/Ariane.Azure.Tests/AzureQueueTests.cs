using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Azure.Tests
{
	[TestClass]
	public class AzureQueueTests
	{
		[TestMethod]
		public void Send_Person()
		{
			var bus = new BusManager(); 
			bus.Register.AddAzureQueueWriter("test.azure");

			var person = new Person();
			person.FirsName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("test.azure", person);

			System.Threading.Thread.Sleep(5 * 1000);

			bus.Dispose();
		}

		[TestMethod]
		public void Receive_Person()
		{
			var bus = new BusManager();
			bus.Register.AddAzureQueueReader("test.azure", typeof(PersonMessageReader));

			bus.StartReading();

			System.Threading.Thread.Sleep(5 * 1000);

			bus.Dispose();
		}
	}
}
