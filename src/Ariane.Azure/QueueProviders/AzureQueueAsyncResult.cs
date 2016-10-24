using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	internal class AzureQueueAsyncResult : IAsyncResult, IDisposable
	{
		private ManualResetEvent m_Event;
		private QueueClient m_Queue;
		private BrokeredMessage m_BrokeredMessage;

		public AzureQueueAsyncResult(ManualResetEvent @event, QueueClient queueClient)
		{
			m_Event = @event;
			m_Queue = queueClient;
			IsCompleted = false;
			CompletedSynchronously = false;
			m_Queue.OnMessage(message =>
			{
				m_BrokeredMessage = message.Clone();
				CompletedSynchronously = true;
				m_Event.Set();
			});
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
