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
		private string m_QueueName;
		private QueueClient m_Queue;
		private ManualResetEvent m_Event;

		public AzureMessageQueue(QueueClient queueClient, string queueName)
		{
			m_QueueName = queueName;
			m_Queue = queueClient;
			m_Event = new ManualResetEvent(false);
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

		public string QueueName
		{
			get { return m_QueueName; }
		}

		public IAsyncResult BeginReceive()
		{
			return new AzureAsyncResult(m_Event, m_Queue);
		}

		public T EndReceive<T>(IAsyncResult result)
		{
			var brokeredMessage = result.AsyncState as BrokeredMessage;
			if (brokeredMessage == null)
			{
				return default(T);
			}
			var body = brokeredMessage.GetBody<T>();
			return body;
		}

		public T Receive<T>()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			m_Event.Reset();
		}

		public void Send<T>(Message<T> message)
		{
			var brokeredMessage = new BrokeredMessage(message.Body);
			brokeredMessage.Label = message.Label;
			// brokeredMessage.TimeToLive = new TimeSpan(24, 0, 0);
			m_Queue.Send(brokeredMessage);
		}

		#endregion

		public virtual void Dispose()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
		}
	}
}
