// #define Watch

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane.QueueProviders
{
	public class AzureMessageQueue : IMessageQueue, IDisposable
	{
		private AzureQueueAsyncResult m_LazyAsyncResult = null;
		private QueueClient m_Queue;
		private ManualResetEvent m_Event;

		public AzureMessageQueue(QueueClient queueClient)
		{
			QueueName = queueClient.Path;
			m_Queue = queueClient;
		}

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

		public string QueueName { get; private set; }
		public string TopicName { get; private set; }

		public IAsyncResult BeginReceive()
		{
			if (m_LazyAsyncResult == null)
			{
				m_LazyAsyncResult = InitializeAsyncResult();
			}
			return m_LazyAsyncResult;
		}

		public T EndReceive<T>(IAsyncResult result)
		{
			var brokeredMessage = result.AsyncState as BrokeredMessage;
			var body = default(T);
			if (brokeredMessage == null)
			{
				return body;
			}
			body = GetAndDeserializeBody<T>(brokeredMessage);
			return body;
		}

		public T Receive<T>()
		{
			T result = default(T);
			var mre = new ManualResetEvent(false);
			m_Queue.OnMessage(message =>
			{
				result = GetAndDeserializeBody<T>(message);
				mre.Set();
			});
			mre.WaitOne(10 * 1000);
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
#if Watch
			var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
			var brokeredMessage = CreateSerializedBrokeredMessage(message.Body);
			brokeredMessage.Label = message.Label;
			if (message.ScheduledEnqueueTimeUtc.HasValue)
			{
				brokeredMessage.ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc.Value;
			}
			if (message.TimeToLive.HasValue)
			{
				brokeredMessage.TimeToLive = message.TimeToLive.Value;
			}
			m_Queue.Send(brokeredMessage);
#if Watch
			watch.Stop();
			Console.WriteLine($"message sent = {watch.ElapsedMilliseconds}ms");
#endif
		}

		#endregion

		public virtual void Dispose()
		{
			if (this.m_Event != null)
			{
				this.m_Event.Dispose();
			}
			if (m_LazyAsyncResult != null)
			{
				m_LazyAsyncResult.Dispose();
			}
		}

		private AzureQueueAsyncResult InitializeAsyncResult()
		{
			m_Event = new ManualResetEvent(false);
			var result = new AzureQueueAsyncResult(m_Event, m_Queue);
			return result;
		}

		private BrokeredMessage CreateSerializedBrokeredMessage(object body)
		{
			var content = Newtonsoft.Json.JsonConvert.SerializeObject(body);
			var bytes = Encoding.UTF8.GetBytes(content);
			var stream = new System.IO.MemoryStream(bytes, false);
			var brokeredMessage = new BrokeredMessage(stream);
			brokeredMessage.ContentType = "application/json";
			return brokeredMessage;
		}

		private T GetAndDeserializeBody<T>(BrokeredMessage brokeredMessage)
		{
			var body = default(T);
			var stream = brokeredMessage.GetBody<System.IO.Stream>();
			using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
			{
				var content = reader.ReadToEnd();
				try
				{
					body = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
				}
				catch(Exception ex)
				{
					ex.Data.Add("content", content);
					throw ex;
				}
			}
			return body;
		}
	}
}
