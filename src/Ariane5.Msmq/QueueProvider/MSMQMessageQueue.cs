using System;
using System.Collections.Generic;
using System.Linq;
using Experimental.System.Messaging;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ariane.QueueProviders
{
	public class MSMQMessageQueue : IMessageQueue
	{
		private Experimental.System.Messaging.MessageQueue m_Queue;

        public MSMQMessageQueue(ILogger<MSMQMessageQueue> logger)
        {
			this.Logger = logger;
        }

		protected ILogger<MSMQMessageQueue> Logger { get; }

		public MSMQMessageQueue(Experimental.System.Messaging.MessageQueue queue, string queueName)
		{
			Name = queueName;
			m_Queue = queue;
			m_Queue.Formatter = new MSMQJSonMessageFormatter();
		}

		#region IMessageQueue Members

		public int? Timeout
		{
			get
			{
				return null;
			}
		}

		public void SetTimeout()
		{
			m_Queue.Close();
			m_Queue.Refresh();
		}

		public string Name { get ; private set; }
		public string SubscriptionName { get; private set; }

		public async Task<T> ReceiveAsync<T>()
		{
			T body = default(T);
			try
			{
				var timeout = new TimeSpan(0, 0, 0, 0, 100);
				var message = m_Queue.Receive(timeout);
				body = GetBody<T>(message);
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, ex.Message);
			}
			return await Task.FromResult(body);
		}

		public IAsyncResult BeginReceive()
		{
			return m_Queue.BeginReceive();
		}

		public T EndReceive<T>(IAsyncResult result)
		{
			var message = m_Queue.EndReceive(result);
			var body = GetBody<T>(message);
			return body;
		}

		private T GetBody<T>(Experimental.System.Messaging.Message message)
		{
			if (message == null)
			{
				return default(T);
			}
			var content = message.Body as string;
			T body = default(T);
			try
			{
				body = System.Text.Json.JsonSerializer.Deserialize<T>(content);
			}
			catch (Exception ex)
			{
				ex.Data.Add("jsoncontent", content);
				Logger.LogError(ex, ex.Message);
			}
			return body;
		}

		public void Reset()
		{
			m_Queue.Refresh();
		}

		public void Send<T>(Message<T> message)
		{
			try
			{
				m_Queue.Send(message);
			}
			catch (Experimental.System.Messaging.MessageQueueException mqex)
			{
				try
				{
					mqex.Data.Add("QueueName", Name);
					mqex.Data.Add("MsmqErrorCode", mqex.MessageQueueErrorCode.ToString());
					mqex.Data.Add("MessageType", message.GetType().FullName);
					var json = System.Text.Json.JsonSerializer.Serialize(message);
					mqex.Data.Add("Message", json);
					Logger.LogError(mqex, mqex.Message);
				}
				catch { /* Dead for science */ }
			}
			catch(Exception ex)
			{
				try
				{
					ex.Data.Add("QueueName", Name);
					ex.Data.Add("MessageType", message.GetType().FullName);
					var json = System.Text.Json.JsonSerializer.Serialize(message);
					ex.Data.Add("Message", json);
					Logger.LogError(ex, ex.Message);
				}
				catch { /* Dead for science */ }
			}
		}

		#endregion

		public override string ToString()
		{
			var result = new StringBuilder();
			result.AppendLine(string.Format("CanRead:{0}", m_Queue.CanRead));
			result.AppendLine(string.Format("CanWrite:{0}", m_Queue.CanWrite));
			result.AppendLine(string.Format("FormatName:{0}", m_Queue.FormatName));
			result.AppendLine(string.Format("Formatter:{0}", m_Queue.Formatter));
			result.AppendLine(string.Format("Label:{0}", m_Queue.Label));
			result.AppendLine(string.Format("MachineName:{0}", m_Queue.MachineName));
			result.AppendLine(string.Format("QueueName:{0}", m_Queue.QueueName));
			return result.ToString();
		}

		private long MessageCount()
		{
			long count = 0;
			var cursor = m_Queue.GetMessageEnumerator2();
			while(cursor.MoveNext())
			{
				count++;
			}
			return count;
		}
	}
}
