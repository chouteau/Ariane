using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests.Server
{
	public class ExpandoMessageReader : MessageReaderBase<System.Dynamic.ExpandoObject>
	{
		public override void ProcessMessage(System.Dynamic.ExpandoObject message)
		{
			dynamic result = message;
			Console.WriteLine("Id : {0}, Test : {1}", result.id, result.text);
		}
	}
}
