using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ariane.QueueProviders
{
	public class AsyncResult : IAsyncResult
	{
		private ManualResetEvent m_Event;

		public AsyncResult(ManualResetEvent @event)
		{
			m_Event = @event;
		}

		#region IAsyncResult Members

		public object AsyncState
		{
			get { return m_Event; }
		}

		public System.Threading.WaitHandle AsyncWaitHandle
		{
			get { return m_Event; }
		}

		public bool CompletedSynchronously
		{
			get { return true; }
		}

		public bool IsCompleted
		{
			get { return true; }
		}

		#endregion
	}
}
