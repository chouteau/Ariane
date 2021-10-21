using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Ariane
{
	/// <summary>
	/// Implementation of Service Bus
	/// </summary>
	internal class BusManager : IServiceBus
	{
		private readonly ActionQueue m_ActionQueue;

		public BusManager(/* IRegister register, */
			ActionQueue actionQueue,
			ILogger<BusManager> logger,
			IEnumerable<IMessageQueue> messageQueues,
			IServiceProvider serviceProvider,
			ArianeSettings arianeSettings)
		{
			m_ActionQueue = actionQueue;
			this.Logger = logger;
			this.MessageQueueList = messageQueues;
			this.ServiceProvider = serviceProvider;
			this.ArianeSettings = arianeSettings;
		}

        protected ILogger<BusManager> Logger { get; }
		protected IEnumerable<IMessageQueue> MessageQueueList { get; }
		protected IServiceProvider ServiceProvider { get; }
		protected ArianeSettings ArianeSettings { get; }


		private IList<IMessageDispatcher> _messageDispatchers;
		protected IList<IMessageDispatcher> MessageDispatcherList 
		{ 
			get
			{
				if (_messageDispatchers == null)
				{
					var list = ServiceProvider.GetServices<IMessageDispatcher>().ToList();
					var register = ServiceProvider.GetRequiredService<IRegister>();
					if (register.ConfigurationException != null)
					{
						throw register.ConfigurationException;
					}
					_messageDispatchers = list;
				}
				return _messageDispatchers;
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

		public virtual void Send<T>(string queueName, T body, MessageOptions options)
		{
			m_ActionQueue.Add(() =>
			{
				SendInternal(queueName, body, options);
			});
		}

		protected virtual void SendInternal<T>(string queueName, T body, MessageOptions options)
		{
			queueName = $"{ArianeSettings.UniquePrefixName}{queueName}";
			options = options ?? new MessageOptions();
			var mq = MessageQueueList.FirstOrDefault(i => i.Name == queueName);
			if (mq == null)
			{
				Logger.LogError($"queue not defined for {queueName} {mq}");
				return;
			}
			var m = new Message<T>();
			m.Label = options.Label ?? Guid.NewGuid().ToString();
			m.Body = body;
			m.Priority = Math.Max(0, options.Priority);
			m.TimeToLive = options.TimeToLive;
			m.ScheduledEnqueueTimeUtc = options.ScheduledEnqueueTimeUtc;
			Logger.LogTrace($"Try to send {m.ToJsonStringTraceLog()} in queue {queueName}");
			mq.Send(m);
		}

		public virtual async Task StartReadingAsync()
		{
			foreach (var dispatcher in MessageDispatcherList)
			{
				await dispatcher.StartAsync();
			}
		}

		public virtual Task StartReadingAsync(string queueName)
		{
			queueName = $"{ArianeSettings.UniquePrefixName}{queueName}";
            foreach (var dispatcher in MessageDispatcherList)
            {
				if (!dispatcher.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase))
                {
					continue;
                }
				dispatcher.StartAsync();
            }
			return Task.CompletedTask;
		}

		public virtual Task StopReadingAsync()
		{
			foreach (var dispatcher in MessageDispatcherList)
			{
				dispatcher.StopAsync(new CancellationToken());
			}
			return Task.CompletedTask;
		}

		public virtual Task StopReadingAsync(string queueName)
		{
			queueName = $"{ArianeSettings.UniquePrefixName}{queueName}";
			foreach (var dispatcher in MessageDispatcherList)
			{
				if (!dispatcher.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase))
				{
					continue;
				}
				dispatcher.StopAsync(new CancellationToken());
			}
			return Task.CompletedTask;
		}

		public virtual async Task<IEnumerable<T>> ReceiveAsync<T>(string queueName, int count)
		{
			queueName = $"{ArianeSettings.UniquePrefixName}{queueName}";
			var mq = MessageQueueList.SingleOrDefault(i => i.Name == queueName);
			var result = new List<T>();
            for (int i = 0; i < count; i++)
            {
				var message = await mq.ReceiveAsync<T>();
				if (message != null)
                {
					result.Add(message);
                }
            }
			return result;
		}

		public virtual async Task<IEnumerable<T>> ReceiveAsync<T>(string queueName, int count, int timeoutInMillisecond)
		{
			timeoutInMillisecond = Math.Min(60 * 1000, timeoutInMillisecond);
			return await ReceiveInternalAsync<T>(queueName, count, timeoutInMillisecond);
		}

		internal virtual async Task<IEnumerable<T>> ReceiveInternalAsync<T>(string queueName, int count, int timeoutInMillisecond)
		{
			queueName = $"{ArianeSettings.UniquePrefixName}{queueName}";
			var mq = MessageQueueList.SingleOrDefault(i => i.Name == queueName);
			if (mq == null)
            {
				return null;
            }
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
					Logger.LogError(ex, ex.Message);
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
					Logger.LogError(ex, ex.Message);
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
			return await Task.FromResult(result);
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

		}

		public virtual dynamic CreateMessage(string messageName)
		{
			dynamic result = new System.Dynamic.ExpandoObject();
			result.MessageName = messageName;
			return result;
		}

		public virtual IEnumerable<string> GetRegisteredQueueList()
        {
			return MessageQueueList.Select(i => i.Name);
        }
	}
}
