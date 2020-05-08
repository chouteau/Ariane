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
	public class AzureTopicTests
	{
		[TestMethod]
		public void Send_And_Receive_Person_Topic()
		{
			var bus = new BusManager(); 
			bus.Register.AddAzureTopicWriter("MyTopic");

			var person = new Person();
			person.FirsName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			bus.Send("MyTopic", person);

			bus.Register.AddAzureTopicWriter("MyTopic");
			bus.Register.AddAzureTopicReader("MyTopic", "sub1", typeof(PersonMessageReader));
			bus.Register.AddAzureTopicReader("MyTopic", "sub2", typeof(PersonMessageReader));
			bus.Register.AddAzureTopicReader("MyTopic", "sub3", typeof(PersonMessageReader));

			MessageCollector.Current.Clear();

			bus.StartReading();

			System.Threading.Thread.Sleep(10 * 1000);

			Check.That(MessageCollector.Current.Count).IsEqualTo(3);

			bus.Dispose();
		}
	}
}
