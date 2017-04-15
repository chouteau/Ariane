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
		private static object m_Lock = new object();

		private ManualResetEvent m_Event;
		private Ariane.QueueProviders.AsyncResult m_AzureAsyncResult;
		private SubscriptionClient m_SubscriptionClient;
		private TopicClient m_Topic;
		private OnMessageOptions m_MessageOptions;

		public AzureMessageTopic(TopicClient topicClient, SubscriptionClient subscriptionClient)
		{
			QueueName = topicClient.Path;
			TopicName = subscriptionClient?.Name;
			m_Event = new ManualResetEvent(false);
			m_Topic = topicClient;
			m_SubscriptionClient = subscriptionClient;
			if (m_SubscriptionClient != null)
			{
				m_AzureAsyncResult = new Ariane.QueueProviders.AsyncResult(m_Event);

				m_MessageOptions = new OnMessageOptions();
				m_MessageOptions.AutoComplete = true;
				m_MessageOptions.AutoRenewTimeout = TimeSpan.FromMinutes(1);
				m_SubscriptionClient.OnMessage(message =>
				{
					var bm = message.Clone();
					m_AzureAsyncResult.AsyncState = bm;
					m_Event.Set();
				}, m_MessageOptions);

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
			var bm = result.AsyncState as BrokeredMessage;
			var body = bm.GetBody<T>();
			bm.Dispose();
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
			//if (this.m_Event != null)
			//{
			//	this.m_Event.Dispose();
			//}
			//if (m_AzureAsyncResult != null)
			//{
			//	m_AzureAsyncResult.Dispose();
			//}
		}
	}
}
