using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public class DynamicReader : DynamicReaderBase
	{
		public override void ProcessMessage(dynamic message)
		{
			Console.WriteLine(message.Id);
		}
	}
}
