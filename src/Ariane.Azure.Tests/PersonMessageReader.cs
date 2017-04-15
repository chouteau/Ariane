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
			message.TopicName = FromTopicName;
			MessageCollector.Current.AddPerson(message);
		}
	}
}
