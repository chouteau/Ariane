using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

namespace Ariane
{
	/// <summary>
	/// Implementation of Service Bus
	/// </summary>
	public class BusManager : IServiceBus, IDisposable
	{
		private static object m_Lock = new object();
		private IActionQueue m_ActionQueue;
		private FluentRegister m_Register;

		public BusManager()
		{
			m_Register = new FluentRegister();
			m_ActionQueue = new ActionQueue();
		}

		#region IServiceBus Members

		public IRegister Register 
		{ 
			get 
			{ 
				return m_Register; 
			} 
		}

		public virtual void Send<T>(string queueName, T body)
		{
			var options = new MessageOptions()
			{
				Label = null,
				Priority = 0
			};
			Send(queueName, body, options);
		}

		[Obsolete("Use MessageOptions instead", false)]
		public virtual void Send<T>(string queueName, T body, string label = null, int priority = 0)
		{
			m_ActionQueue.Add(() =>
			{
				var options = new MessageOptions()
				{
					Label = label,
					Priority = priority
				};
				SendInternal(queueName, body, options);
			});
		}

		public virtual void Send<T>(string queueName, T body, MessageOptions options)
		{
			m_ActionQueue.Add(() =>
			{
				SendInternal(queueName, body, options);
			});
		}

		protected virtual void SendInternal<T>(string queueName, T body, MessageOptions options)
		{
			var registration = GetRegistrationByQueueName(queueName);
			if (registration == null)
			{
				return;
			}
			var mq = registration.Queue;
			if (mq == null)
			{
				GlobalConfiguration.Configuration.Logger.Fatal($"queue not defined for {queueName} {registration.TypeMedium}");
				return;
			}
			var m = new Message<T>();
			m.Label = options.Label ?? Guid.NewGuid().ToString();
			m.Body = body;
			m.Priority = Math.Max(0, options.Priority);
			m.TimeToLive = options.TimeToLive;
			mq.Send(m);
		}

		public virtual void StartReading()
		{
			lock(m_Register.List.SyncRoot)
			{
				GlobalConfiguration.Configuration.Logger.Info("Start reading all {0} configured queues", m_Register.List.Count);
				foreach (var item in m_Register.List)
				{
					GlobalConfiguration.Configuration.Logger.Info("Try to start queue {0}", item.QueueName);
					var queue = item.Queue;
					if (item.Reader != null)
					{
						if (item.AutoStartReading
							&& queue != null)
						{
							item.Reader.Start(queue);
						}
						else
						{
							GlobalConfiguration.Configuration.Logger.Info("queue {0} is not autostarted", item.QueueName);
						}
					}
					else
					{
						GlobalConfiguration.Configuration.Logger.Info("queue {0} is already started", item.QueueName);
					}
				}
			}
		}

		public virtual void StartReading(string queueName)
		{
			var q = GetRegistrationByQueueName(queueName); 
			if (q == null)
			{
				return;
			}

			q.Reader.Start(q.Queue);
		}

		public virtual IEnumerable<T> Receive<T>(string queueName, int count, int timeoutInMillisecond)
		{
			timeoutInMillisecond = Math.Min(60 * 1000, timeoutInMillisecond);
			lock(m_Lock)
			{
				return ReceiveInternal<T>(queueName, count, timeoutInMillisecond);
			}
		}

		internal virtual IEnumerable<T> ReceiveInternal<T>(string queueName, int count, int timeoutInMillisecond)
		{
			var registration = GetRegistrationByQueueName(queueName);
			if (registration == null)
			{
				return null;
			}
			var mq = registration.Queue;
			var result = new List<T>();
			while (true)
			{
				IAsyncResult item = null;
				mq.Reset();
				mq.SetTimeout();
				try
				{
					item = mq.BeginReceive();
				}
				catch (Exception ex)
				{
					GlobalConfiguration.Configuration.Logger.Error(ex);
				}
				var handles = new WaitHandle[] { item.AsyncWaitHandle };
				var index = WaitHandle.WaitAny(handles, timeoutInMillisecond);
				if (index == 258) // Timeout
				{
					mq.SetTimeout();
					break;
				}

				T message = default(T);
				try
				{
					message = mq.EndReceive<T>(item);
				}
				catch (Exception ex)
				{
					GlobalConfiguration.Configuration.Logger.Error(ex);
				}
				finally
				{
					mq.Reset();
				}

				if (message != null)
				{
					result.Add(message);
				}

				if (result.Count == count)
				{
					break;
				}
			}
			return result;
		}

		public virtual void StopReading()
		{
			if (m_ActionQueue != null)
			{
				this.m_ActionQueue.Stop();
			}
			lock(m_Register.List.SyncRoot)
			{
				foreach (var item in m_Register.List)
				{
					if (!item.AutoStartReading)
					{
						continue;
					}
					if (!item.IsReaderCreated)
					{
						continue;
					}
					if (item.Reader != null)
					{
						item.Reader.Stop();
					}
				}
			}
		}

		public virtual void StopReading(string queueName)
		{
			var q = GetRegistrationByQueueName(queueName); 
			if (q == null)
			{
				return;
			}

			q.Reader.Stop();
		}

		/// <summary>
		/// Used by Unit Test
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		/// <param name="label"></param>
		/// <param name="priority"></param>
		[Obsolete("Use MessageOptions instead", false)]
		public virtual void SyncProcess<T>(string queueName, T body, string label = null, int priority = 0)
		{
			var options = new MessageOptions()
			{
				Label = label,
				Priority = priority
			};
			SyncProcess(queueName, body, options);
		}

		/// <summary>
		/// Used by Unit Test
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		/// <param name="label"></param>
		/// <param name="priority"></param>
		public virtual void SyncProcess<T>(string queueName, T body, MessageOptions options)
		{
			var registration = GetRegistrationByQueueName(queueName); 
			if (registration == null)
			{
				return;
			}
			var mq = registration.Queue;
			var reader = (MessageDispatcher<T>)registration.Reader;
			if (reader != null)
			{
				reader.ProcessMessage(body);
			}
			else
			{
				SendInternal(queueName, body, options);
			}
		}

		public virtual dynamic CreateMessage(string messageName)
		{
			dynamic result = new System.Dynamic.ExpandoObject();
			result.MessageName = messageName;
			return result;
		}

		public void ReplaceActionQueue(IActionQueue actionQueue)
		{
			if (actionQueue == null)
			{
				return;
			}
			m_ActionQueue = actionQueue;
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			if (this.m_ActionQueue != null)
			{
				this.m_ActionQueue.Dispose();
			}
			lock(m_Register.List.SyncRoot)
			{
				foreach (var item in m_Register.List)
				{
					if (!item.AutoStartReading)
					{
						continue;
					}
					if (item.Reader == null)
					{
						continue;
					}
					item.Reader.Dispose();
				}
			}
		}

		#endregion

		private Registration GetRegistrationByQueueName(string queueName)
		{
			Registration result = null;
			lock(m_Register.List.SyncRoot)
			{
				result = m_Register.List.FirstOrDefault(i =>
									i.QueueName.Equals(queueName, StringComparison.CurrentCultureIgnoreCase)
						);
			}
			return result;
		}

	}
}
