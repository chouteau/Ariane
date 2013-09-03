using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IMessage<T> : IDisposable
	{
		string QueueName { get; set; }
		string Label { get; set; }
		T Body { get; set; }
		bool Recoverable { get; set; }
	}
}
