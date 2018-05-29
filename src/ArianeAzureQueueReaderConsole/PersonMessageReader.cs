using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;

namespace ArianeAzureQueueReaderConsole
{
	public class PersonMessageReader : MessageReaderBase<ArianeAzureQueueSenderConsole.Person>
	{
		private int m_Count;

		public override void ProcessMessage(ArianeAzureQueueSenderConsole.Person message)
		{
			m_Count++;
			Console.WriteLine($"{message} / {m_Count}");
		}
	}
}
