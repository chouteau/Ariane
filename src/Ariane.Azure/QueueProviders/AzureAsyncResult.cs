using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	internal class AzureAsyncResult : IAsyncResult
	{
		private ManualResetEvent m_Event;
		private QueueClient m_Queue;
		private BrokeredMessage m_BrokeredMessage;

		public AzureAsyncResult(ManualResetEvent @event, QueueClient queueClient)
		{
			m_Event = @event;
			m_Queue = queueClient;
			IsCompleted = false;
			CompletedSynchronously = false;
			m_Queue.BeginReceive(ProcessEndReceive, m_Queue);
		}


		void ProcessEndReceive(IAsyncResult result)
		{
			var qc = result.AsyncState as QueueClient;
			var m = qc.EndReceive(result);
			if (m != null)
			{
				m.BeginComplete(ProcessEndComplete, m);
			}
		}

		void ProcessEndComplete(IAsyncResult result)
		{
			m_BrokeredMessage = result.AsyncState as BrokeredMessage;
			m_BrokeredMessage.EndComplete(result);
			IsCompleted = true;
			CompletedSynchronously = true;
			m_Event.Set();
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

	}
}
