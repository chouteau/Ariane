using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ariane
{
	internal class MessageDispatcher<T> : IDisposable, IMessageDispatcher
	{
		private ManualResetEvent m_EventStop;
		private bool m_Terminated = false;
		private Thread m_Thread;
		private IMessageQueue m_Queue;
		private IList<Type> m_MessageSubscriberTypeList;

		public MessageDispatcher()
		{
			Logger = GlobalConfiguration.Configuration.Logger;
			m_MessageSubscriberTypeList = new List<Type>();
			MessageSubscriberList = new Lazy<IList<MessageReaderBase<T>>>(InitilizeSubscriberList, true);
		}

		protected ILogger Logger { get; set; }
		protected Lazy<IList<MessageReaderBase<T>>> MessageSubscriberList;

		public void AddMessageSubscriberTypeList(IList<Type> list)
		{
			m_MessageSubscriberTypeList = list;
		}

		public void AddMessageSubscriberList(IList<object> list)
		{
			foreach (var item in list)
			{
				MessageSubscriberList.Value.Add(item as MessageReaderBase<T>);
			}
		}

		public virtual void Start(IMessageQueue queue)
		{
			if (m_Thread != null
				&& m_Thread.ThreadState != ThreadState.Stopped)
			{
				return;
			}
			m_Terminated = false;
			m_EventStop = new ManualResetEvent(false);
			m_Queue = queue;
			m_Thread = new Thread(new ThreadStart(Run));
			m_Thread.Name = string.Format("Ariane.MessageReaderBase:{0}", queue.QueueName);
			m_Thread.Start();

			foreach (var item in MessageSubscriberList.Value)
			{
				item.FromQueueName = m_Queue.QueueName;
			}
		}

		public virtual void Stop()
		{
			m_Terminated = true;
			if (m_EventStop != null)
			{
				if (!m_EventStop.SafeWaitHandle.IsClosed)
				{
					m_EventStop.Set();
				}
				m_EventStop.Dispose();
			}
			if (m_Thread != null 
				&& !m_Thread.Join(TimeSpan.FromSeconds(5)))
			{
				m_Thread.Abort();
			}
		}

		private void Run()
		{
			Logger.Info("Reading for queue {0} was started", this.m_Queue.QueueName);

			if (m_Queue.Timeout.HasValue)
			{
				m_Queue.SetTimeout();
			}

			long tooManyErrorCount = 0;
			var lastSentError = DateTime.MinValue;
			bool fatalSent = false;
			while (!m_Terminated 
				&& m_Queue != null)
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
				int index = 0;
				if (m_Queue.Timeout.HasValue)
				{
					index = WaitHandle.WaitAny(waitHandles, m_Queue.Timeout.Value);
				}
				else
				{	
					index = WaitHandle.WaitAny(waitHandles);
				}
				if (index == 0)
				{
					m_Terminated = true;
					break;
				}
				else if (index == 258)
				{
					m_Queue.SetTimeout();
					continue;
				}

				T message = default(T);
				try
				{
					message = m_Queue.EndReceive<T>(result);
				}
				catch (Exception ex)
				{
					if ((DateTime.Now - lastSentError).TotalMilliseconds > 1000)
					{
						Logger.Error(ex);
						lastSentError = DateTime.Now;
					}
					else
					{
						tooManyErrorCount++;
					}

					if (!fatalSent 
						&& tooManyErrorCount > 100)
					{
						Logger.Fatal("Too many error {0}", ex.Message);
						tooManyErrorCount = 0;
						fatalSent = true;
					}
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
			Stop();
			m_MessageSubscriberTypeList.Clear();
		}

		#endregion

		private IList<MessageReaderBase<T>> InitilizeSubscriberList()
		{
			var result = new List<MessageReaderBase<T>>();
			foreach (var subscriberType in m_MessageSubscriberTypeList)
			{
				var subscriber = (MessageReaderBase<T>) GlobalConfiguration.Configuration.DependencyResolver.GetService(subscriberType);
				result.Add(subscriber);
			}
			return result;
		}
	}
}
