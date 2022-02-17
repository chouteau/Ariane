using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Ariane.QueueProviders
{
	public class InMemoryMessageQueue : IMessageQueue, IDisposable
	{
		private System.Collections.Concurrent.ConcurrentQueue<object> m_Queue;
		private ManualResetEvent m_Event;

		public InMemoryMessageQueue(string queueName)
		{
			m_Queue = new System.Collections.Concurrent.ConcurrentQueue<object>();
			m_Event = new ManualResetEvent(false);
			Name = queueName;
		}

		public int? Timeout
		{
			get
			{
				return null;
			}
		}

		public void SetTimeout()
		{

		}

		#region IMessageQueue Members

		public string Name { get; private set; }
		public string SubscriptionName { get; private set; }

		public async Task<T> ReceiveAsync<T>()
		{
			object message = null;
			bool result = m_Queue.TryDequeue(out message);
			if (result)
			{
				var wrapped = message as Message<T>;
				if (wrapped != null)
				{
					return await Task.FromResult(wrapped.Body);
				}
			}
			return await Task.FromResult(default(T));
		}

		public IAsyncResult BeginReceive()
		{
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult r)
		{
			return ReceiveAsync<T>().Result;
		}

		public void Reset()
		{
			if (m_Queue.Count == 0)
			{
				m_Event.Reset();
			}
		}

		public Task SendAsync<T>(Message<T> message)
		{
			m_Queue.Enqueue(message);
			m_Event.Set();
			return Task.CompletedTask;
		}

		#endregion


		public void Dispose()
		{
			if (m_Event != null)
			{
				m_Event.Dispose();
			}
		}
	}
}
