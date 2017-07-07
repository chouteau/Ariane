using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ariane
{
	internal class ActionQueue : Ariane.IActionQueue
	{
		private ConcurrentQueue<Action> m_Queue;
		private ManualResetEvent m_NewMessage = new ManualResetEvent(false);
		private ManualResetEvent m_Terminate = new ManualResetEvent(false);
		private bool m_Terminated = false;
		private Thread m_SendThread;
		private static object m_Lock = new object();

		public ActionQueue()
		{
			m_Queue = new ConcurrentQueue<Action>();
			Timeout = 60;
		}

		public int Timeout { get; set; }

		public void Add(Action action)
		{
			if (m_SendThread == null)
			{
				Start();
			}

			this.m_Queue.Enqueue(action);
			this.m_NewMessage.Set();
		}

		public void Stop()
		{
			m_Terminated = true;
		}

		private void Start()
		{
			if (m_SendThread != null)
			{
				return;
			}
			m_SendThread = new Thread(new ThreadStart(SendInQueue));
			m_SendThread.IsBackground = true;
			m_SendThread.Start();
		}

		private void SendInQueue()
		{
			while (!m_Terminated)
			{
				var waitHandles = new WaitHandle[] { m_Terminate, m_NewMessage };
				int result = 0;
				try
				{
					result = ManualResetEvent.WaitAny(waitHandles, Timeout * 1000, false);
				}
				catch { }
				if (result == 0)
				{
					m_Terminated = true;
					break;
				}
				else if (result == 258
					&& m_Queue.Count == 0)
				{
					break;
				}
				m_NewMessage.Reset();

				if (m_Queue.Count == 0)
				{
					continue;
				}

				while(true)
				{
					Action a = null;
					var success = m_Queue.TryDequeue(out a);
					if (!success 
						|| a == null)
					{
						break;
					}
					try
					{
						a();
					}
					catch (Exception ex)
					{
						GlobalConfiguration.Configuration.Logger.Error(ex);
					}
				}
			}

			if (m_SendThread != null
				&& m_SendThread.Join(TimeSpan.FromSeconds(1)))
			{
				m_SendThread.Abort();
			}
			m_SendThread = null;
		}

		#region IDisposable Members

		public void Dispose()
		{
			m_Terminated = true;
			if (m_Terminate != null)
			{ 
				if (!m_Terminate.SafeWaitHandle.IsClosed)
				{
					m_Terminate.Set();
				}
				m_Terminate.Dispose();
			}
			if (m_NewMessage != null)
			{
				m_NewMessage.Dispose();
			}
			if (m_SendThread != null)
			{
				if (!m_SendThread.Join(TimeSpan.FromSeconds(5)))
				{
					m_SendThread.Abort();
				}
			}
		}

		#endregion
	}
}
