using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class InMemoryMessage : IMessage
	{
		public string QueueName { get; set; }
		public string Label { get; set; }
		public object Body { get; set; }
		public bool Recoverable { get; set; }
		public void Dispose()
		{
		}
	}
}
