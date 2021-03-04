using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Ariane.QueueProviders
{
	public class AzureMessageQueue : IMessageQueue, IDisposable
	{
		private ManualResetEvent m_Event;
		private ServiceBusClient m_ServiceBusClient;
		private BinaryData m_BinaryMessage;
		private ServiceBusSender m_ServiceBusSender;
		private ServiceBusReceiver m_ServiceBusReceiver;
		private ServiceBusProcessor m_ServiceBusProcessor;

		public AzureMessageQueue(ServiceBusClient serviceBusClient, string queueName, ILogger logger)
		{
			this.Logger = logger;
			m_ServiceBusClient = serviceBusClient;
			m_ServiceBusSender = m_ServiceBusClient.CreateSender(queueName);
			m_ServiceBusReceiver = m_ServiceBusClient.CreateReceiver(queueName, new ServiceBusReceiverOptions()
			{
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
				PrefetchCount = 10
			});
			m_ServiceBusProcessor = m_ServiceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions()
			{
				AutoCompleteMessages = true,
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
				PrefetchCount = 10
			});
			m_ServiceBusProcessor.ProcessMessageAsync += MessageHandler;
			m_ServiceBusProcessor.ProcessErrorAsync += ProcessError;

			Name = queueName;
		}

		protected ILogger Logger { get; }

		public int? Timeout
		{
			get
			{
				return 30 * 1000; // Toutes les 30 secondes
			}
		}

		public void SetTimeout()
		{
			
		}

		#region IMessageQueue Members

		public string Name { get; internal set; }
		public string SubscriptionName { get; internal set; }
		public string ConnectionString { get; internal set; }

        public IAsyncResult BeginReceive()
		{
			if (m_Event == null)
			{
				Task.Run(async () =>
				{
					await m_ServiceBusProcessor.StartProcessingAsync(new CancellationToken());
				});
				m_Event = new ManualResetEvent(false);
			}
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult asyncResult)
		{
			if (m_BinaryMessage != null)
            {
				var receiveMessage = JsonConvert.DeserializeObject<T>(m_BinaryMessage.ToString());
				return receiveMessage;
			}
			return default(T);
		}

		public async Task<T> ReceiveAsync<T>()
		{
			T result = default(T);
			var receivedMessage = await m_ServiceBusReceiver.ReceiveMessageAsync();
			if (receivedMessage != null
				&& receivedMessage.Body != null)
            {
				var receiveMessage = JsonConvert.DeserializeObject<T>(receivedMessage.Body.ToString());
				result = receiveMessage; // .Body.ToObject<T>();
			}
			return result;
		}

		public void Reset()
		{
			if (m_Event != null)
			{
				m_Event.Reset();
			}
		}

		public void Send<T>(Message<T> message)
		{
			Task.Run(async ()  => 
			{
				// var client = new ServiceBusClient(ConnectionString);
				var data = JsonConvert.SerializeObject(message.Body);
				var busMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(data));
				busMessage.Subject = message.Label;
				if (message.ScheduledEnqueueTimeUtc.HasValue)
				{
					busMessage.ScheduledEnqueueTime = message.ScheduledEnqueueTimeUtc.Value;
				}
				if (message.TimeToLive.HasValue)
				{
					busMessage.TimeToLive = message.TimeToLive.Value;
				}
				try
				{
					await m_ServiceBusSender.SendMessageAsync(busMessage);
				}
				catch(Exception ex)
                {
					ex.Data.Add("QueueName", Name);
					ex.Data.Add("Message", data);
					Logger.LogError(ex, ex.Message);
					await Task.Delay(200);
                }
			});
		}

		public void SendBatch<T>(IList<Message<T>> messages)
		{
			Task.Run(async () =>
			{
				var batch = await m_ServiceBusSender.CreateMessageBatchAsync(new CancellationToken());
				foreach (var message in messages)
				{
					var data = JsonConvert.SerializeObject(message.Body);
					var busMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(data));
					busMessage.Subject = message.Label;
					if (message.ScheduledEnqueueTimeUtc.HasValue)
					{
						busMessage.ScheduledEnqueueTime = message.ScheduledEnqueueTimeUtc.Value;
					}
					if (message.TimeToLive.HasValue)
					{
						busMessage.TimeToLive = message.TimeToLive.Value;
					}
					batch.TryAddMessage(busMessage);
				}
				await m_ServiceBusSender.SendMessagesAsync(batch);
			});
		}

		#endregion

		public virtual void Dispose()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
			var disposeList = new List<Task>();
			if (m_ServiceBusSender != null)
            {
				disposeList.Add(m_ServiceBusSender.DisposeAsync().AsTask());
            }
			if (m_ServiceBusReceiver != null)
            {
				disposeList.Add(m_ServiceBusReceiver.DisposeAsync().AsTask());
            }
			if (m_ServiceBusClient != null)
            {
				disposeList.Add(m_ServiceBusClient.DisposeAsync().AsTask());
            }
			Task.WhenAll(disposeList).GetAwaiter().GetResult();
		}

		private Task MessageHandler(ProcessMessageEventArgs args)
        {
			m_BinaryMessage = args.Message.Body;
			m_Event.Set();
			return Task.CompletedTask;
        }

		private Task ProcessError(ProcessErrorEventArgs args)
		{
			Logger.LogError(args.Exception, args.Exception.Message);
			return Task.CompletedTask;
		}


	}
}
