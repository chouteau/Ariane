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
	public class AzureMessageTopic : IMessageQueue, IDisposable
	{
		private ManualResetEvent m_Event;
		private ServiceBusClient m_ServiceBusClient;
		private BinaryData m_BinaryMessage;

		public AzureMessageTopic(ServiceBusClient serviceBusClient, string topicName, string subscriptionName, ILogger logger)
		{
			this.Logger = logger;
			m_ServiceBusClient = serviceBusClient;
			Name = topicName;
			SubscriptionName = subscriptionName;
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
				var processor = m_ServiceBusClient.CreateProcessor(Name, SubscriptionName, new ServiceBusProcessorOptions()
				{
					AutoCompleteMessages = true,
				});
				processor.ProcessMessageAsync += MessageHandler;
				processor.ProcessErrorAsync += ProcessError;
				processor.StartProcessingAsync(new CancellationToken());
				m_Event = new ManualResetEvent(false);
			}
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult asyncResult)
		{
			if (m_BinaryMessage != null)
			{
				var receiveMessage = JsonConvert.DeserializeObject<ReceivedMessage>(m_BinaryMessage.ToString());
				return receiveMessage.Body.ToObject<T>();
			}
			return default(T);
		}

		public async Task<T> ReceiveAsync<T>()
		{
			T result = default(T);
			var receiver = m_ServiceBusClient.CreateReceiver(SubscriptionName, Name);
			var receivedMassage = await receiver.ReceiveMessageAsync();
			if (receivedMassage != null
				&& receivedMassage.Body != null)
			{
				var body = System.Text.Encoding.UTF8.GetString(receivedMassage.Body);
				result = JsonConvert.DeserializeObject<T>(body);
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
			var client = new ServiceBusClient(ConnectionString);
			var sender = client.CreateSender(Name);
			var data = JsonConvert.SerializeObject(message);
			var busMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(data));
			sender.SendMessageAsync(busMessage).Wait();
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
