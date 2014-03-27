using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public class PersonMessageReader : MessageReaderBase<Person>
	{
		public override void ProcessMessage(Person message)
		{
			message.IsProcessed = true;
			StaticContainer.Model = message;
			GlobalConfiguration.Configuration.Logger.Info("{0}:{1}:{2}", FromQueueName, System.Threading.Thread.CurrentThread.Name, message.FirstName);
		}
	}
}
