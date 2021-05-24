using System;
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
	public class AzureMessageTopic : IMessageQueue
	{
		private ManualResetEvent m_Event;
		private readonly ServiceBusClient m_ServiceBusClient;
		private BinaryData m_BinaryMessage;
		private readonly ServiceBusSender m_ServiceBusSender;
		private readonly ServiceBusReceiver m_ServiceBusReceiver;
		private readonly ServiceBusProcessor m_ServiceBusProcessor;

		public AzureMessageTopic(ServiceBusClient serviceBusClient, string topicName, string subscriptionName, ILogger logger)
		{
			this.Logger = logger;
			Name = topicName;
			SubscriptionName = subscriptionName;
			m_ServiceBusClient = serviceBusClient;
			m_ServiceBusSender = m_ServiceBusClient.CreateSender(topicName);
			m_ServiceBusReceiver = m_ServiceBusClient.CreateReceiver(topicName, subscriptionName, new ServiceBusReceiverOptions()
			{
				PrefetchCount = 10,
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
			});
			m_ServiceBusProcessor = m_ServiceBusClient.CreateProcessor(topicName, SubscriptionName, new ServiceBusProcessorOptions()
			{
				AutoCompleteMessages = true,
				ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete,
				PrefetchCount = 10,
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
					await m_ServiceBusProcessor.StartProcessingAsync();
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
				result = receiveMessage; //.Body.ToObject<T>();
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
			Task.Run(async () =>
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

		public virtual void Dispose()
		{
			if (m_ServiceBusClient != null)
            {
				m_ServiceBusClient.DisposeAsync();
			}
			if (m_Event != null)
            {
				m_Event.Dispose();
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
	}
}
