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
		private IActionQueue m_ActionQueue;
		private FluentRegister m_Register;

		public BusManager()
		{
			m_Register = new FluentRegister();
		}

		#region IServiceBus Members

		public IFluentRegister Register
		{
			get
			{
				return m_Register;
			}
		}

		public void Send<T>(string queueName, T body, string label = null)
		{
			if (m_ActionQueue == null)
			{
				// TODO : Use DI Registration
				m_ActionQueue = new ActionQueue();
				m_ActionQueue.Start();
			}
			m_ActionQueue.Add(() =>
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
			var m = registration.Medium.Value.CreateMessage<T>();
			m.Label = label ?? Guid.NewGuid().ToString();
			m.Body = body;
			mq.Send(m);
		}

		public void StartReading()
		{
			foreach (var item in m_Register.List)
			{
				if (item.Reader.IsValueCreated)
				{
					if (item.Reader.Value != null)
					{
						item.Reader.Value.Stop();
					}
				}
			}

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
			var reader = (MessageReaderBase<T>)registration.Reader.Value;
			reader.ProcessMessage(body);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (m_ActionQueue != null)
			{
				m_ActionQueue.Dispose();
			}
		}

		#endregion
	}
}
