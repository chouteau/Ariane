using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	/// <summary>
	/// Decorator for synchronized bus for tests
	/// </summary>
	internal class SyncBusManager : Ariane.IServiceBus
	{
		private Ariane.IServiceBus m_Decorated;

		public SyncBusManager(
			IRegister register,
			ActionQueue actionQueue,
			ILogger<BusManager> logger,
			IEnumerable<IMessageQueue> messageQueues,
			IEnumerable<IMessageDispatcher> messageDispatchers)
			: this(new BusManager(register, actionQueue, logger, messageQueues, messageDispatchers))
		{
		}

		public SyncBusManager(Ariane.IServiceBus decorated)
		{
			m_Decorated = decorated;
		}

		public void Send<T>(string queueName, T body, string label = null, int priority = 0)
		{
			m_Decorated.SyncProcess(queueName, body, new MessageOptions()
			{
				Label = label,
				Priority = priority
			});
		}

		public void Send<T>(string queueName, T body, MessageOptions options)
		{
			m_Decorated.SyncProcess(queueName, body, options);
		}

		public async Task StartReadingAsync()
		{
			await m_Decorated.StartReadingAsync();
		}

		public async Task StartReadingAsync(string queueName)
		{
			await m_Decorated.StartReadingAsync(queueName);
		}

		public async Task<IEnumerable<T>> ReceiveAsync<T>(string queueName, int count)
		{
			return await m_Decorated.ReceiveAsync<T>(queueName, count);
		}

		public async Task<IEnumerable<T>> ReceiveAsync<T>(string queueName, int count, int timeout)
		{
			return await m_Decorated.ReceiveAsync<T>(queueName, count, timeout);
		}

		public async Task StopReadingAsync()
		{
			await m_Decorated.StopReadingAsync();
		}

		public async Task StopReadingAsync(string queueName)
		{
			await m_Decorated.StopReadingAsync(queueName);
		}

		public void SyncProcess<T>(string queueName, T body, string label = null, int priority = 0)
		{
			m_Decorated.SyncProcess(queueName, body, new MessageOptions()
			{
				Label = label,
				Priority = priority
			});
		}

		public void SyncProcess<T>(string queueName, T body, MessageOptions options)
		{
			m_Decorated.SyncProcess(queueName, body, options);
		}

		public dynamic CreateMessage(string name)
		{
			return m_Decorated.CreateMessage(name);
		}

		public IEnumerable<string> GetRegisteredQueueList()
        {
			return m_Decorated.GetRegisteredQueueList();
        }
	}
}
