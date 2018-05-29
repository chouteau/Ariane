// #define Watch

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using Ariane.Azure;

namespace Ariane.QueueProviders
{
	public class AzureMessageQueue : IMessageQueue, IDisposable
	{
		private QueueClient m_Queue;
		private ManualResetEvent m_Event;
		private System.Collections.Concurrent.ConcurrentQueue<BrokeredMessage> m_BrokeredMessageQueue;

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
			if (m_Event == null)
			{
				m_Event = new ManualResetEvent(false);
				m_BrokeredMessageQueue = new System.Collections.Concurrent.ConcurrentQueue<BrokeredMessage>();
				var options = new OnMessageOptions();
				options.AutoComplete = false;
				options.ExceptionReceived += (s, ex) =>
				{
					GlobalConfiguration.Configuration.Logger.Error(ex.Exception);
				};
				m_Queue.OnMessage(message =>
				{
					var bm = message.Clone();
					m_BrokeredMessageQueue.Enqueue(bm);
					m_Event.Set();
				}, options);
			}
			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult asyncResult)
		{
			BrokeredMessage brokeredMessage = null;
			bool result = m_BrokeredMessageQueue.TryDequeue(out brokeredMessage);
			if (result)
			{
				var body = default(T);
				if (brokeredMessage == null)
				{
					return body;
				}
				body = brokeredMessage.GetAndDeserializeBody<T>();
				brokeredMessage.Dispose();
				return body;
			}
			return default(T);
		}

		public T Receive<T>()
		{
			T result = default(T);
			var mre = new ManualResetEvent(false);
			m_Queue.OnMessage(message =>
			{
				result = message.GetAndDeserializeBody<T>();
				mre.Set();
			});
			mre.WaitOne(10 * 1000);
			return result;
		}

		public void Reset()
		{
			if (m_Event != null
				&& m_BrokeredMessageQueue.Count == 0)
			{
				m_Event.Reset();
			}
		}

		public void Send<T>(Message<T> message)
		{
#if Watch
			var watch = System.Diagnostics.Stopwatch.StartNew();
#endif
			var brokeredMessage = message.Body.CreateSerializedBrokeredMessage();
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
		}

	}
}
