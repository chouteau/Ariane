using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	public class AzureDefaultMessageQueue : IMessageQueue, IDisposable
	{
		private AzureQueueAsyncResult m_LazyAsyncResult = null;
		private QueueClient m_Queue;
		private ManualResetEvent m_Event;

		public AzureDefaultMessageQueue(QueueClient queueClient)
		{
			QueueName = queueClient.Path;
			m_Queue = queueClient;
		}

		public int? Timeout
		{
			get
			{
				return 30 * 1000;
			}
		}

		public void SetTimeout()
		{

		}

		#region IMessageQueue Members

		public string QueueName { get; private set; }
		public string TopicName { get; private set; }

		public IAsyncResult BeginReceive()
		{
			if (m_LazyAsyncResult == null)
			{
				m_LazyAsyncResult = InitializeAsyncResult();
			}
			return m_LazyAsyncResult;
		}

		public T EndReceive<T>(IAsyncResult result)
		{
			var brokeredMessage = result.AsyncState as BrokeredMessage;
			var body = default(T);
			if (brokeredMessage == null)
			{
				return body;
			}
			body = brokeredMessage.GetBody<T>();
			return body;
		}

		public T Receive<T>()
		{
			T result = default(T);
			var mre = new ManualResetEvent(false);
			m_Queue.OnMessage(message =>
			{
				result = message.GetBody<T>();
				mre.Set();
			});
			mre.WaitOne(10 * 1000);
			return result;
		}

		public void Reset()
		{
			if (m_Event != null)
			{
				m_Event.Reset();
			}
		}

		public void Send<T>(Message<T> message)
		{
			var brokeredMessage = new BrokeredMessage(message.Body);
			brokeredMessage.Label = message.Label;
			if (message.TimeToLive.HasValue)
			{
				brokeredMessage.TimeToLive = message.TimeToLive.Value;
			}
			m_Queue.Send(brokeredMessage);
		}

		#endregion

		public virtual void Dispose()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
			if (m_LazyAsyncResult != null)
			{
				m_LazyAsyncResult.Dispose();
			}
		}

		private AzureQueueAsyncResult InitializeAsyncResult()
		{
			m_Event = new ManualResetEvent(false);
			var result = new AzureQueueAsyncResult(m_Event, m_Queue);
			return result;
		}
	}
}
