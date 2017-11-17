using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	public class AzureMessageQueue : IMessageQueue, IDisposable
	{
		private AzureQueueAsyncResult m_LazyAsyncResult = null;
		private QueueClient m_Queue;
		private ManualResetEvent m_Event;

		public AzureMessageQueue(QueueClient queueClient)
		{
			QueueName = queueClient.Path;
			m_Queue = queueClient;
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
			var message = result.AsyncState as BrokeredMessage;
			if (message == null)
			{
				return default(T);
			}
			var body = message.GetBody<T>();
			message.Dispose();
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
			m_Event.Reset();
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
