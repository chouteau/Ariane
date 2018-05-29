using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;

namespace ArianeAzureTopicReaderConsole
{
	public class PersonMessageReader : MessageReaderBase<ArianeAzureTopicSenderConsole.Person>
	{
		private int m_Count;

		public override void ProcessMessage(ArianeAzureTopicSenderConsole.Person message)
		{
			m_Count++;
			Console.WriteLine($"{message} / {m_Count}");
		}
	}
}
