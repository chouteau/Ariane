using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

namespace Ariane
{
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

		protected Lazy<IActionQueue> ActionQueue { get; set; }

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
			var mq = registration.Queue.Value;
			var m = new Message<T>();
			m.Label = label ?? Guid.NewGuid().ToString();
			m.Body = body;
			mq.Send(m);
		}

		public void StartReading()
		{
			foreach (var item in m_Register.List)
			{
				var queue = item.Queue.Value;
				if (item.Reader.Value != null)
				{
					item.Reader.Value.Start(queue);
				}
			}
		}

		public void StopReading()
		{
			foreach (var item in m_Register.List)
			{
				if (item.Reader.Value == null)
				{
					continue;
				}
				item.Reader.Value.Stop();
				item.Reader.Value.Dispose();
			}
		}

		public void PauseReading()
		{
			foreach (var item in m_Register.List)
			{
				if (item.Reader.Value == null)
				{
					continue;
				}
				item.Reader.Value.Pause();
			}
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
			var mq = registration.Queue.Value;
			var reader = (MessageDispatcher<T>)registration.Reader.Value;
			reader.ProcessMessage(body);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (ActionQueue != null)
			{
				ActionQueue.Value.Dispose();
			}
		}

		#endregion

		private IActionQueue InitializeActionQueue()
		{
			var result = new ActionQueue();
			result.Start();
			return result;
		}
	}
}
