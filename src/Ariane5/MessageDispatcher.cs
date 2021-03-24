using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ariane
{
	internal class MessageDispatcher<T> : IMessageDispatcher, IDisposable
	{
		private IList<Type> m_MessageSubscriberTypeList;
		private Task m_runningTask;
		private CancellationTokenSource m_StoppingTask = new CancellationTokenSource();

		public MessageDispatcher(ILogger<MessageDispatcher<T>> logger)
		{
			this.Logger = logger;
			m_MessageSubscriberTypeList = new List<Type>();
			AutoStart = true;
		}

		protected ILogger Logger { get; set; }
		protected IList<MessageReaderBase<T>> MessageSubscriberList { get; private set; }
		protected IMessageQueue MessageQueue { get; private set; }
		public string QueueName { get; private set; }
        public bool AutoStart { get; set; }

        public void AddMessageSubscriberType(Type subscriberType)
		{
			if (subscriberType == null)
            {
				throw new ArgumentNullException();
            }
			if (!m_MessageSubscriberTypeList.Contains(subscriberType))
            {
				m_MessageSubscriberTypeList.Add(subscriberType);
			}
		}

		public void InitializeMedium(IServiceProvider serviceProvider, QueueSetting settings)
        {
			var messageQueues = serviceProvider.GetServices<IMessageQueue>();
			var messageQueueList = messageQueues.Where(i => i.Name == settings.Name).ToList();
			if (messageQueueList.Any())
            {
				if (messageQueueList.Count == 1)
				{
					MessageQueue = messageQueueList.Single();
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(settings.SubscriptionName))
					{
						MessageQueue = messageQueueList.Single(i => i.SubscriptionName == settings.SubscriptionName);
					}
				}
			}
			this.QueueName = settings.Name;
		}

		public void InitializeSubscribers(IServiceProvider serviceProvider)
        {
			var result = new List<MessageReaderBase<T>>();
			foreach (var subscriberType in m_MessageSubscriberTypeList)
			{
				var subscriber = ActivatorUtilities.CreateInstance(serviceProvider, subscriberType) as MessageReaderBase<T>;
				if (MessageQueue != null
					&& subscriber != null)
				{
					subscriber.FromQueueName = MessageQueue.Name;
					subscriber.FromSubscriptionName = MessageQueue.SubscriptionName;
					result.Add(subscriber);
				}
			}
			MessageSubscriberList = result;
		}

        public Task StartAsync()
        {
			if (!AutoStart)
            {
				return Task.CompletedTask;
            }
			if (m_runningTask != null)
			{
				return Task.CompletedTask;
			}
			m_runningTask = Task.Run(() => ExecuteAsync(m_StoppingTask.Token));
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
        {
			if (m_runningTask == null)
            {
				return;
            }
            try
            {
				m_StoppingTask.Cancel();
            }
			finally
            {
				await Task.WhenAny(m_runningTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
			m_StoppingTask = new CancellationTokenSource();
		}

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
			if (MessageQueue == null)
            {
				return;
            }

			Logger.LogInformation("Reading for queue {0} was started", MessageQueue.Name);

			if (MessageQueue.Timeout.HasValue)
			{
				MessageQueue.SetTimeout();
			}

			long tooManyErrorCount = 0;
			var lastSentError = DateTime.MinValue;
			bool fatalSent = false;
			while (!stoppingToken.IsCancellationRequested)
			{
				IAsyncResult result = null;
				try
				{
					result = MessageQueue.BeginReceive();
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, ex.Message);
					await Task.Delay(200);
					continue;
				}
				if (result == null)
				{
					continue;
				}
				var waitHandles = new WaitHandle[] { stoppingToken.WaitHandle, result.AsyncWaitHandle };
				int index = 0;
				if (MessageQueue.Timeout.HasValue)
				{
					index = WaitHandle.WaitAny(waitHandles, MessageQueue.Timeout.Value);
				}
				else
				{
					index = WaitHandle.WaitAny(waitHandles);
				}
				if (index == 0)
				{
					break;
				}
				else if (index == 258) // Timeout
				{
					MessageQueue.SetTimeout();
					Elapsed();
					continue;
				}

				T message = default(T);
				try
				{
					message = MessageQueue.EndReceive<T>(result);
				}
				catch (Exception ex)
				{
					if ((DateTime.Now - lastSentError).TotalMilliseconds > 1000)
					{
						Logger.LogError(ex, ex.Message);
						lastSentError = DateTime.Now;
					}
					else
					{
						tooManyErrorCount++;
					}

					if (!fatalSent
						&& tooManyErrorCount > 100)
					{
						Logger.LogCritical("Too many error {0}", ex.Message);
						tooManyErrorCount = 0;
						fatalSent = true;
					}
				}
				finally
				{
					MessageQueue.Reset();
				}

				if (message == null)
				{
					continue;
				}

				await ProcessMessageAsync(message);
			}
			m_runningTask = null;
		}

		public async Task ProcessMessageAsync(T message)
		{
			foreach (var subscriber in MessageSubscriberList)
			{
				try
				{
					await subscriber.ProcessMessageAsync(message);
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, ex.Message);
				}
			}
		}

		public virtual void Elapsed()
		{
			foreach (var subscriber in MessageSubscriberList)
			{
				try
				{
					subscriber.Elapsed();
				}
				catch (Exception ex)
				{
					Logger.LogError(ex, ex.Message);
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			m_MessageSubscriberTypeList.Clear();
			m_StoppingTask.Cancel();
		}

		#endregion

	}
}
