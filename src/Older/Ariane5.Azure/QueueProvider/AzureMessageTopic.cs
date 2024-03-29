﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Azure.Messaging.ServiceBus;

using Microsoft.Extensions.Logging;

namespace Ariane.QueueProviders
{
	public class AzureMessageTopic : IMessageQueue, IAsyncDisposable, IDisposable
	{
		private readonly bool m_FlushReceivedMessageToDiskBeforeProcess;
		private ManualResetEvent m_Event;
		private BinaryData m_BinaryMessage;
		private readonly ServiceBusClient m_ServiceBusClient;
		private readonly ServiceBusSender m_ServiceBusSender;
		private readonly ServiceBusProcessor m_ServiceBusProcessor;

		public AzureMessageTopic(ServiceBusClient serviceBusClient, AzureBusSettings settings, string topicName, string subscriptionName, ILogger logger, bool flushReceivedMessageToDiskBeforeProcess)
		{
			this.m_FlushReceivedMessageToDiskBeforeProcess = flushReceivedMessageToDiskBeforeProcess;
			this.Logger = logger;
			Name = topicName;
			SubscriptionName = subscriptionName;
			m_ServiceBusClient = serviceBusClient;
			m_ServiceBusSender = m_ServiceBusClient.CreateSender(topicName);
			m_ServiceBusProcessor = m_ServiceBusClient.CreateProcessor(topicName, SubscriptionName, new ServiceBusProcessorOptions()
			{
				AutoCompleteMessages = true,
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
				PrefetchCount = settings.ProcessorPrefetchCount,
				MaxConcurrentCalls = 1
			});
			m_ServiceBusProcessor.ProcessMessageAsync += MessageHandler;
			m_ServiceBusProcessor.ProcessErrorAsync += ProcessError;
		}

		protected ILogger Logger { get; }

		public int? Timeout
		{
			get
			{
				return 30 * 1000;
			}
		}

		public void SetTimeout()
		{

		}

		public string Name { get; private set; }
		public string SubscriptionName { get; private set; }
        public string ConnectionString { get; internal set; }

        public IAsyncResult BeginReceive()
		{
			if (m_Event == null)
			{
				Task.Run(async ()  =>
				{
					try
					{
						await m_ServiceBusProcessor.StartProcessingAsync();
					}
					catch(Exception ex)
					{
						Logger.LogError(ex, ex.Message);
					}
				});
				m_Event = new ManualResetEvent(false);
			}
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult asyncResult)
		{
			if (m_BinaryMessage != null)
			{
				var body = m_BinaryMessage.ToString();
				if (m_FlushReceivedMessageToDiskBeforeProcess)
				{
					DiagnosticHelper.FlushToDisk(body);
				}
				var receiveMessage = System.Text.Json.JsonSerializer.Deserialize<T>(body);
				return receiveMessage;
			}
			return default(T);
		}

		public async Task<T> ReceiveAsync<T>()
		{
			await using var serviceBusReceiver = m_ServiceBusClient.CreateReceiver(Name, SubscriptionName, new ServiceBusReceiverOptions()
			{
				PrefetchCount = 0,
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
			});

			T result = default(T);
			var receivedMessage = await serviceBusReceiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
			if (receivedMessage != null
				&& receivedMessage.Body != null)
			{
				var receiveMessage = System.Text.Json.JsonSerializer.Deserialize<T>(receivedMessage.Body.ToString());
				result = receiveMessage;
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
			catch (Exception ex)
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
