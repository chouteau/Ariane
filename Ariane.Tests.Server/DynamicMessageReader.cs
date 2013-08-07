using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests.Server
{
	public class DynamicMessageReader : MessageReaderBase<dynamic>
	{
		public override void ProcessMessage(dynamic message)
		{
			Console.WriteLine("Id : {0}, Test : {1}", message.id, message.test);
		}
	}
}
