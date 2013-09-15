using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Ariane
{
	public class InMemoryMessageQueueWrapper : IMessageQueue
	{
		private System.Collections.Queue m_Queue;
		private ManualResetEvent m_Event;

		public InMemoryMessageQueueWrapper(string queueName)
		{
			m_Queue = new System.Collections.Queue();
			m_Event = new ManualResetEvent(false);
			QueueName = queueName;
		}

		#region IMessageQueue Members

		public string QueueName { get; private set; }

		public IAsyncResult BeginReceive()
		{
			return new MessageEnqueuedAsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult r)
		{
			var message = m_Queue.Dequeue();
			var wrapped = message as Message<T>;
			return wrapped.Body;
		}

		public void Reset()
		{
			if (m_Queue.Count == 0)
			{
				m_Event.Reset();
			}
		}

		public void Send<T>(Message<T> message)
		{
			lock (m_Queue)
			{
				m_Queue.Enqueue(message);
			}
			m_Event.Set();
		}

		#endregion

	}
}
