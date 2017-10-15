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
			if (IsSending)
			{
				return;
			}
			InvokeAction();
		}

		private void InvokeAction()
		{
			Action action;
			while(m_Queue.TryDequeue(out action))
			{
				InternalInvokeAction(action);
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
