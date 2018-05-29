using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using Ariane.Azure;

namespace Ariane.QueueProviders
{
	public class AzureMessageTopic : IMessageQueue, IDisposable
	{
		private ManualResetEvent m_Event;
		private System.Collections.Concurrent.ConcurrentQueue<BrokeredMessage> m_BrokeredMessageQueue;
		private SubscriptionClient m_SubscriptionClient;
		private TopicClient m_Topic;
		private OnMessageOptions m_MessageOptions;

		public AzureMessageTopic(TopicClient topicClient, SubscriptionClient subscriptionClient)
		{
			m_Topic = topicClient;
			m_SubscriptionClient = subscriptionClient;

			QueueName = topicClient.Path;
			TopicName = subscriptionClient?.Name;
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
			if (m_Event == null)
			{
				m_Event = new ManualResetEvent(false);
				m_BrokeredMessageQueue = new System.Collections.Concurrent.ConcurrentQueue<BrokeredMessage>();
				if (m_SubscriptionClient != null)
				{
					m_MessageOptions = new OnMessageOptions();
					m_MessageOptions.AutoComplete = true;
					m_MessageOptions.AutoRenewTimeout = TimeSpan.FromMinutes(1);
					m_SubscriptionClient.OnMessage(message =>
					{
						var bm = message.Clone();
						m_BrokeredMessageQueue.Enqueue(bm);
						m_Event.Set();
					}, m_MessageOptions);
				}
			}
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult asyncResult)
		{
			BrokeredMessage brokeredMessage = null;
			bool result = m_BrokeredMessageQueue.TryDequeue(out brokeredMessage);
			if (result)
			{
				var body = default(T);
				if (brokeredMessage == null)
				{
					return body;
				}
				body = brokeredMessage.GetAndDeserializeBody<T>();
				brokeredMessage.Dispose();
				return body;
			}
			return default(T);
		}

		public T Receive<T>()
		{
			var message = m_SubscriptionClient.Receive();
			var result = message.GetAndDeserializeBody<T>();
			return result;
		}

		public void Reset()
		{
			if (m_Event != null
				&& m_BrokeredMessageQueue.Count == 0)
			{
				m_Event.Reset();
			}
		}

		public void Send<T>(Message<T> message)
		{
			var brokeredMessage = message.Body.CreateSerializedBrokeredMessage();
			brokeredMessage.Label = message.Label;
			if (message.ScheduledEnqueueTimeUtc.HasValue)
			{
				brokeredMessage.ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc.Value;
			}
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
