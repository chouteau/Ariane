﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

namespace Ariane.QueueProviders
{
	public class AzureMessageQueue : IMessageQueue, IAsyncDisposable, IDisposable
	{
		private readonly bool m_FlushReceivedMessageToDiskBeforeProcess;
		private ManualResetEvent m_Event;
		private ManualResetEvent m_MessageProcessed;

		private BinaryData m_BinaryMessage;
		private readonly ServiceBusClient m_ServiceBusClient;
		private readonly ServiceBusSender m_ServiceBusSender;
		private readonly ServiceBusProcessor m_ServiceBusProcessor;

		public AzureMessageQueue(ServiceBusClient serviceBusClient, AzureBusSettings settings, string queueName, ILogger logger, bool flushReceivedMessageToDiskBeforeProcess)
		{
			this.m_FlushReceivedMessageToDiskBeforeProcess = flushReceivedMessageToDiskBeforeProcess;
			this.Logger = logger;
			m_ServiceBusClient = serviceBusClient;
			m_ServiceBusSender = m_ServiceBusClient.CreateSender(queueName);
			m_ServiceBusProcessor = m_ServiceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions()
			{
				AutoCompleteMessages = true,
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
				PrefetchCount = settings.ProcessorPrefetchCount,
				MaxConcurrentCalls = 1
			});
			m_ServiceBusProcessor.ProcessMessageAsync += MessageHandler;
			m_ServiceBusProcessor.ProcessErrorAsync += ProcessError;

			Name = queueName;

			m_MessageProcessed = new ManualResetEvent(false);
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
				m_Event = new ManualResetEvent(false);
				Task.Run(async () =>
				{
					await m_ServiceBusProcessor.StartProcessingAsync(new CancellationToken());
				});
			}
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult asyncResult)
		{
			var result = default(T);
			if (m_BinaryMessage != null)
            {
				var body = m_BinaryMessage.ToString();
				if (m_FlushReceivedMessageToDiskBeforeProcess)
                {
					DiagnosticHelper.FlushToDisk(body);
				}
				result = System.Text.Json.JsonSerializer.Deserialize<T>(body);
			}
			m_MessageProcessed.Set();
			return result;
		}

		public async Task<T> ReceiveAsync<T>()
		{
			await using var serviceBusReceiver = m_ServiceBusClient.CreateReceiver(this.Name, new ServiceBusReceiverOptions()
			{
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
				PrefetchCount = 0
			});

			T result = default(T);
			var receivedMessage = await serviceBusReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
			if (receivedMessage != null
				&& receivedMessage.Body != null)
            {
				var body = receivedMessage.Body.ToString();
				var receiveMessage = System.Text.Json.JsonSerializer.Deserialize<T>(body);
				result = receiveMessage; 
			}
			return result;
		}

		public void Reset()
		{
			m_MessageProcessed.Reset();
			if (m_Event != null)
			{
				m_Event.Reset();
			}
		}

		public async Task SendAsync<T>(Message<T> message)
		{
			if (message == null
				|| message.Body == null)
			{
				return;
			}

			ServiceBusMessage busMessage = null;
			string data = null;
			try
			{
				data = System.Text.Json.JsonSerializer.Serialize(message.Body);
				busMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(data));
				busMessage.Subject = message.Label;
				if (message.ScheduledEnqueueTimeUtc.HasValue)
				{
					busMessage.ScheduledEnqueueTime = message.ScheduledEnqueueTimeUtc.Value;
				}
				if (message.TimeToLive.HasValue)
				{
					busMessage.TimeToLive = message.TimeToLive.Value;
				}
			}
			catch(Exception ex)
			{
				ex.Data.Add("QueueName", Name);
				ex.Data.Add("Message", data);
				Logger.LogError(ex, ex.Message);
				return;
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
		}

		public void SendBatch<T>(IList<Message<T>> messages)
		{
			Task.Run(async () =>
			{
				var batch = await m_ServiceBusSender.CreateMessageBatchAsync(new CancellationToken());
				foreach (var message in messages)
				{
					var data = System.Text.Json.JsonSerializer.Serialize(message.Body);
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

		private Task MessageHandler(ProcessMessageEventArgs args)
        {
			m_BinaryMessage = args.Message.Body;
			m_Event.Set();
			m_MessageProcessed.WaitOne();
			return Task.CompletedTask;
        }

		private Task ProcessError(ProcessErrorEventArgs args)
		{
			Logger.LogError(args.Exception, args.Exception.Message);
			return Task.CompletedTask;
		}

		public async ValueTask DisposeAsync()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
			var disposeList = new List<Task>();
			if (m_ServiceBusProcessor != null)
			{
				await m_ServiceBusProcessor.StopProcessingAsync();
				disposeList.Add(m_ServiceBusProcessor.DisposeAsync().AsTask());
			}
			if (m_ServiceBusSender != null)
			{
				disposeList.Add(m_ServiceBusSender.DisposeAsync().AsTask());
			}
			if (m_ServiceBusClient != null)
			{
				disposeList.Add(m_ServiceBusClient.DisposeAsync().AsTask());
			}
			await Task.WhenAll(disposeList);
		}

		public void Dispose()
		{
			DisposeAsync().ConfigureAwait(false);
		}
	}
}
