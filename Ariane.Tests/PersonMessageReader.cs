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
			Console.WriteLine("{0}:{1}", System.Threading.Thread.CurrentThread.Name, message.FirsName);
			message.IsProcessed = true;
		}
	}
}
