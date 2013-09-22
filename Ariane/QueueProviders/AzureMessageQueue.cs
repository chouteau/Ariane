using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	internal class AzureMessageQueue : IMessageQueue
	{
		private string m_QueueName;
		private QueueClient m_Queue;
		private ManualResetEvent m_Event;
		private DateTime m_LastReadTime;

		public AzureMessageQueue(QueueClient queueClient, string queueName)
		{
			m_QueueName = queueName;
			m_Queue = queueClient;
			m_Event = new ManualResetEvent(false);
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
			m_LastReadTime = DateTime.Now;
			var brokeredMessage = result.AsyncState as BrokeredMessage;
			if (brokeredMessage == null)
			{
				return default(T);
			}
			var body = brokeredMessage.GetBody<T>();
			return body;
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
	}
}
