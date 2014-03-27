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
		private FluentRegister m_Register;

		public BusManager()
		{
			m_Register = new FluentRegister();
			ActionQueue = new Lazy<IActionQueue>(InitializeActionQueue, true);
		}

		#region IServiceBus Members

		public IFluentRegister Register
		{
			get
			{
				return m_Register;
			}
		}

		internal Lazy<IActionQueue> ActionQueue { get; set; }

		public void Send<T>(string queueName, T body, string label = null)
		{
			ActionQueue.Value.Add(() =>
			{
				SendInternal(queueName, body, label);
			});
		}

		private void SendInternal<T>(string queueName, T body, string label = null)
		{
			var registration = m_Register.List.SingleOrDefault(i => i.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase));
			if (registration == null)
			{
				return;
			}
			var mq = registration.Queue;
			var m = new Message<T>();
			m.Label = label ?? Guid.NewGuid().ToString();
			m.Body = body;
			mq.Send(m);
		}

		public void StartReading()
		{
			foreach (var item in m_Register.List)
			{
				var queue = item.Queue;
				if (item.AutoStartReading 
					&& item.Reader != null)
				{
					item.Reader.Start(queue);
				}
			}
		}

		public void StartReading(string queueName)
		{
			var q = m_Register.List.SingleOrDefault(i => i.QueueName.Equals(queueName, StringComparison.CurrentCultureIgnoreCase));
			if (q == null)
			{
				return;
			}

			q.Reader.Start(q.Queue);
		}

		public IEnumerable<T> Receive<T>(string queueName, int count, int timeout)
		{
			var registration = m_Register.List.SingleOrDefault(i => i.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase));
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
					continue;
				}
				var handles = new WaitHandle[] { item.AsyncWaitHandle };
				var index = WaitHandle.WaitAny(handles, timeout);
				if (index == 258) // Timeout
				{
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

				if (message == null)
				{
					continue;
				}

				result.Add(message);
				if (result.Count == count)
				{
					break;
				}
			}
			return result;
		}

		public void StopReading()
		{
			if (this.ActionQueue.IsValueCreated)
			{
				this.ActionQueue.Value.Stop();
			}
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
				item.Reader.Stop();
			}
		}

		public void StopReading(string queueName)
		{
			var q = m_Register.List.SingleOrDefault(i => i.QueueName.Equals(queueName, StringComparison.CurrentCultureIgnoreCase));
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
		public void SyncProcess<T>(string queueName, T body, string label = null)
		{
			var registration = m_Register.List.SingleOrDefault(i => i.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase));
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
		}

		public dynamic CreateMessage(string messageName)
		{
			dynamic result = new System.Dynamic.ExpandoObject();
			result.MessageName = messageName;
			return result;
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			if (this.ActionQueue.IsValueCreated)
			{
				this.ActionQueue.Value.Dispose();
			}
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

		#endregion

		private IActionQueue InitializeActionQueue()
		{
			var result = new ActionQueue();
			return result;
		}
	}
}
