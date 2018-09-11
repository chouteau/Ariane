using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ariane
{
	public class ActionQueue
	{
		private readonly System.Collections.Concurrent.ConcurrentQueue<Action> m_Queue 
			= new System.Collections.Concurrent.ConcurrentQueue<Action>();
		private readonly SemaphoreSlim semaphore 
			= new SemaphoreSlim(1, 1);

		private bool IsSending
		{
			get
			{
				return semaphore.CurrentCount < 1;
			}
		}

		public void Add(Action action)
		{
			m_Queue.Enqueue(action);
			if (m_Queue.Count > 15)
			{
				GlobalConfiguration.Configuration.Logger.Warn("Ariane action queue is greater than 15");
			}
			if (IsSending)
			{
				return;
			}
			InvokeAction();
		}

		private void InvokeAction()
		{
			Action action;
			while(true)
			{
				bool result = m_Queue.TryDequeue(out action);
				if (result)
				{
					InternalInvokeAction(action);
					continue;
				}
				break;
			}
		}

		private void InternalInvokeAction(Action action)
		{
			semaphore.Wait();
			try
			{
				action();
			}
			finally
			{
				semaphore.Release();
			}
			InvokeAction();
		}
	}
}
