using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public class MyDynamicMessageReader : Ariane.DynamicMessageReaderBase
	{
		protected override void Process(string messageName, dynamic message)
		{
			if (messageName != "test")
			{
				return;
			}

			Console.WriteLine(message);
			StaticContainer.Model = message;
		}
	}
}
