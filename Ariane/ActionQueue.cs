using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ariane
{
	internal class ActionQueue : IDisposable
	{
		private Queue<Action> m_Queue;
		private ManualResetEvent m_NewMessage = new ManualResetEvent(false);
		private ManualResetEvent m_Terminate = new ManualResetEvent(false);
		private bool m_Terminated = false;
		private Thread m_SendThread;

		public void Start()
		{
			m_Queue = new Queue<Action>();
			m_SendThread = new Thread(new ThreadStart(SendInQueue));
			m_SendThread.IsBackground = true;
			m_SendThread.Start();
		}

		public void Add(Action action)
		{
			lock (this.m_Queue)
			{
				this.m_Queue.Enqueue(action);
			}
			this.m_NewMessage.Set();
		}

		private void SendInQueue()
		{
			while (!m_Terminated)
			{
				var waitHandles = new WaitHandle[] { m_Terminate, m_NewMessage };
				int result = ManualResetEvent.WaitAny(waitHandles, 60 * 1000, true);
				if (result == 0)
				{
					m_Terminated = true;
					break;
				}
				m_NewMessage.Reset();

				if (m_Queue.Count == 0)
				{
					continue;
				}
				// Enqueue
				Queue<Action> queueCopy;
				lock (m_Queue)
				{
					queueCopy = new Queue<Action>(m_Queue);
					m_Queue.Clear();
				}

				foreach (var send in queueCopy)
				{
					try
					{
						send();
					}
					catch (Exception ex)
					{
						GlobalConfiguration.Configuration.Logger.Error(ex);
					}
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (m_Queue != null)
			{
				m_Queue.Clear();
			}
			m_Terminated = true;
			if (m_Terminate != null)
			{
				m_Terminate.Set();
			}
			if (m_SendThread != null)
			{
				if (m_SendThread != null
					&& !m_SendThread.Join(TimeSpan.FromSeconds(5)))
				{
					m_SendThread.Abort();
				}
			}
		}

		#endregion
	}
}
