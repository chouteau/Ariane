using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	public class AzureMessageTopic : IMessageQueue, IDisposable
	{
		private ManualResetEvent m_Event;
		private AzureTopicAsyncResult m_AzureAsyncResult;
		private SubscriptionClient m_SubscriptionClient;
		private TopicClient m_Topic;

		public AzureMessageTopic(TopicClient topicClient, SubscriptionClient subscriptionClient)
		{
			QueueName = topicClient.Path;
			TopicName = subscriptionClient?.Name;
			m_Event = new ManualResetEvent(false);
			m_Topic = topicClient;
			m_SubscriptionClient = subscriptionClient;
			if (subscriptionClient != null)
			{
				m_AzureAsyncResult = new AzureTopicAsyncResult(m_Event, subscriptionClient);
			}
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
			return m_AzureAsyncResult;
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
			var message = m_SubscriptionClient.Receive();
			var result = message.GetBody<T>();
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
			m_Topic.Send(brokeredMessage);
		}

		#endregion

		public virtual void Dispose()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
			if (m_AzureAsyncResult != null)
			{
				m_AzureAsyncResult.Dispose();
			}
		}
	}
}
