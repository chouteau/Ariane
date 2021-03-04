using Microsoft.Extensions.Logging;
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

		private readonly int m_WarningCount = 50;
		private int m_ProcessedQueue = 0;

		public ActionQueue(ILogger<ActionQueue> logger)
        {
			this.Logger = logger;
        }

		protected ILogger<ActionQueue> Logger { get; }

		private bool IsProcessing
		{
			get
			{
				return semaphore.CurrentCount < 1;
			}
		}

		public void Add(Action action)
		{
			m_Queue.Enqueue(action);
			if (m_Queue.Count > m_WarningCount)
			{
				Logger.LogWarning($"Ariane action in queue is high {m_Queue.Count}, processed {m_ProcessedQueue}");
			}
			if (IsProcessing)
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
				m_ProcessedQueue++;
			}
			finally
			{
				semaphore.Release();
			}
			InvokeAction();
		}
	}
}
