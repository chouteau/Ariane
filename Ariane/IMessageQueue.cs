using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ariane
{
	public interface IMessageQueue
	{
		string QueueName { get; }
		IAsyncResult BeginReceive();
		T EndReceive<T>(IAsyncResult result);
		void Reset();
		void Send(IMessage message);
	}
}
