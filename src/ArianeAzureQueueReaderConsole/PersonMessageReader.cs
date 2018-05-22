using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;

namespace ArianeAzureQueueReaderConsole
{
	public class PersonMessageReader : MessageReaderBase<ArianeAzureQueueSenderConsole.Person>
	{
		public override void ProcessMessage(ArianeAzureQueueSenderConsole.Person message)
		{
			Console.WriteLine($"{message}");
		}
	}
}
