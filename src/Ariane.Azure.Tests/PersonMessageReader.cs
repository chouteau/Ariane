using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Azure.Tests
{
	public class PersonMessageReader : MessageReaderBase<Person>
	{
		public override void ProcessMessage(Person message)
		{
			Console.WriteLine("{0}:{1}:{2}", System.Threading.Thread.CurrentThread.Name, message.FirsName, FromTopicName);
			System.Diagnostics.Trace.WriteLine($"{System.Threading.Thread.CurrentThread.Name},{message.FirsName},{FromTopicName}");
			message.TopicName = FromTopicName;
			message.IsProcessed = true;
			MessageCollector.AddPerson(message);
		}
	}
}
