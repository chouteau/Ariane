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
					var options = new OnMessageOptions();
					options.AutoComplete = true;
					options.MaxConcurrentCalls = 1;
					// m_MessageOptions.AutoRenewTimeout = TimeSpan.FromMinutes(1);
					options.ExceptionReceived += (s, ex) =>
					{
						var x = ex.Exception;
						x.Data.Add("QueueType", "Topic");
						x.Data.Add("Action", ex.Action);
						x.Data.Add("QueueName", this.QueueName);
						GlobalConfiguration.Configuration.Logger.Error(x);
					};
					m_SubscriptionClient.OnMessage(message =>
					{
						// GlobalConfiguration.Configuration.Logger.Debug($"receive topic message {message.MessageId} {this.QueueName} {this.TopicName}");
						var bm = message.Clone();
						m_BrokeredMessageQueue.Enqueue(bm);
						//if (!message.IsBodyConsumed || message.State == MessageState.Active)
						//{
						//	message.Complete();
						//}
						m_Event.Set();
					}, options);
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
				// GlobalConfiguration.Configuration.Logger.Debug($"dequeue {QueueName} topic {TopicName} with type {body.GetType().Name}");
				return body;
			}
			return default(T);
		}

		public T Receive<T>()
		{
			T result = default(T);
			var message = m_SubscriptionClient.Receive(TimeSpan.FromSeconds(10));
			if (message != null)
			{
				result = message.GetAndDeserializeBody<T>();
			}
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
