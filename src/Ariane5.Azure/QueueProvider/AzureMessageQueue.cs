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

		public AzureMessageQueue(ServiceBusClient serviceBusClient, string queueName, ILogger logger)
		{
			this.Logger = logger;
			m_ServiceBusClient = serviceBusClient;
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
				var processor = m_ServiceBusClient.CreateProcessor(Name, new ServiceBusProcessorOptions()
				{
					AutoCompleteMessages = true,
					ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
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
			var receiver = m_ServiceBusClient.CreateReceiver(Name);
			var receivedMessage = await receiver.ReceiveMessageAsync();
			if (receivedMessage != null
				&& receivedMessage.Body != null)
            {
				var receiveMessage = JsonConvert.DeserializeObject<ReceivedMessage>(m_BinaryMessage.ToString());
				result = receiveMessage.Body.ToObject<T>();
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

		public void SendBatch<T>(IList<Message<T>> messages)
		{
			var client = new ServiceBusClient(ConnectionString);
			var sender = client.CreateSender(Name);
			var batch = sender.CreateMessageBatchAsync(new CancellationToken()).Result;
            foreach (var message in messages)
            {
				var data = JsonConvert.SerializeObject(messages);
				var busMessage = new ServiceBusMessage(System.Text.Encoding.UTF8.GetBytes(data));
				batch.TryAddMessage(busMessage);
			}
			sender.SendMessagesAsync(batch).Wait();
		}

		#endregion

		public virtual void Dispose()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
			if (m_ServiceBusClient != null)
            {
				m_ServiceBusClient.DisposeAsync();
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
