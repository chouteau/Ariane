using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	internal class AzureTopicAsyncResult : IAsyncResult, IDisposable
	{
		private ManualResetEvent m_Event;
		private SubscriptionClient m_SubscriptionClient;
		private BrokeredMessage m_BrokeredMessage;

		public AzureTopicAsyncResult(ManualResetEvent @event, SubscriptionClient subscriptionClient)
		{
			m_Event = @event;
			m_SubscriptionClient = subscriptionClient;
			IsCompleted = false;
			CompletedSynchronously = false;
			var options = new OnMessageOptions();
			options.AutoComplete = false;
			m_SubscriptionClient.OnMessage(message =>
			{
				m_BrokeredMessage = message.Clone();
				CompletedSynchronously = true;
				message.Complete();
				m_Event.Set();
			}, options);
		}

		#region IAsyncResult Members

		public object AsyncState
		{
			get { return m_BrokeredMessage; }
		}

		public System.Threading.WaitHandle AsyncWaitHandle
		{
			get { return m_Event; }
		}

		public bool CompletedSynchronously { get ; private set; }
		public bool IsCompleted { get; private set; }

		#endregion

		public void Dispose()
		{
			if (m_Event != null)
			{
				m_Event.Dispose();
			}
			if (m_BrokeredMessage != null)
			{
				m_BrokeredMessage.Dispose();
			}
		}

	}
}
