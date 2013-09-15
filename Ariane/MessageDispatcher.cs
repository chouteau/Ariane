using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ariane
{
	internal class MessageDispatcher<T> : IDisposable, IMessageReader
	{
		private ManualResetEvent m_EventStop;
		private bool m_Terminated = false;
		private Thread m_Thread;
		private IMessageQueue m_Queue;
		private IList<Type> m_MessageSubscriberList;

		public MessageDispatcher()
		{
			Logger = GlobalConfiguration.Configuration.Logger;
			m_MessageSubscriberList = new List<Type>();
			MessageSubscriberList = new Lazy<IList<MessageReaderBase<T>>>(InitilizeSubscriberList, true);
		}

		protected ILogger Logger { get; set; }
		protected Lazy<IList<MessageReaderBase<T>>> MessageSubscriberList;

		public void AddMessageSubscribers(IList<Type> messageSubscriber)
		{
			m_MessageSubscriberList = messageSubscriber;
		}

		public virtual void Start(IMessageQueue queue)
		{
			m_EventStop = new ManualResetEvent(false);
			m_Queue = queue;
			m_Thread = new Thread(new ThreadStart(Run));
			m_Thread.Name = string.Format("MessageReaderBase:{0}", queue.QueueName);
			m_Thread.Start();
		}

		public virtual void Pause()
		{
			m_Thread.Interrupt();
		}

		public void Terminate()
		{
			m_Terminated = true;
			if (m_EventStop != null)
			{
				m_EventStop.Set();
			}
			m_MessageSubscriberList.Clear();
		}

		public virtual void Stop()
		{
			Terminate();
			// Attendre 5 secondes avant de tuer le process
			if (m_Thread != null && !m_Thread.Join(TimeSpan.FromSeconds(5)))
			{
				m_Thread.Abort();
			}
		}

		// public abstract void ProcessMessage(T message);

		private void Run()
		{
			while (!m_Terminated && m_Queue != null)
			{
				IAsyncResult result = null;
				try
				{
					result = m_Queue.BeginReceive();
				}
				catch(Exception ex)
				{
					Logger.Error(ex);
					System.Threading.Thread.Sleep(500);
					continue;
				}
				var waitHandles = new WaitHandle[] { m_EventStop, result.AsyncWaitHandle };
				int index = WaitHandle.WaitAny(waitHandles);
				if (index == 0)
				{
					m_Terminated = true;
					break;
				}

				T message = default(T);
				try
				{
					message = m_Queue.EndReceive<T>(result);
				}
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
				finally
				{
					m_Queue.Reset();
				}

				if (message == null)
				{
					continue;
				}

				ProcessMessage(message);
			}
		}

		public void ProcessMessage(T message)
		{
			foreach (var subscriber in MessageSubscriberList.Value)
			{
				try
				{
					subscriber.ProcessMessage(message);
				}
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
			}
		}

		#region IDisposable Members

		public virtual void Dispose()
		{
			Terminate();
		}

		#endregion

		private IList<MessageReaderBase<T>> InitilizeSubscriberList()
		{
			var result = new List<MessageReaderBase<T>>();
			foreach (var subscriberType in m_MessageSubscriberList)
			{
				var subscriber = GlobalConfiguration.Configuration.DependencyResolver.GetService(subscriberType);
				result.Add(subscriber as MessageReaderBase<T>);
			}
			return result;
		}
	}
}
