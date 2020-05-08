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
		public const string QUEUE_NAME = "test.azure2";

		[TestMethod]
		public void Send_And_Receive_Person_Queue()
		{
			var bus = new BusManager(); 
			bus.Register.AddAzureQueueWriter(QUEUE_NAME);

			var person = new Person();
			person.FirsName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send(QUEUE_NAME, person);

			bus.Register.AddAzureQueueReader(QUEUE_NAME, typeof(PersonMessageReader));

			bus.StartReading();

			System.Threading.Thread.Sleep(5 * 1000);
			Check.That(MessageCollector.Current.Count).IsGreaterThan(0);

			bus.Dispose();
		}
	}
}
