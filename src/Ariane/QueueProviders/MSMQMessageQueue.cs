using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.QueueProviders
{
	public class MSMQMessageQueue : IMessageQueue
	{
		private System.Messaging.MessageQueue m_Queue;

		public MSMQMessageQueue(System.Messaging.MessageQueue queue, string queueName)
		{
			QueueName = queueName;
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

		public string QueueName { get ; private set; }
		public string TopicName { get; private set; }

		public T Receive<T>()
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

			}
			return body;
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

		private T GetBody<T>(System.Messaging.Message message)
		{
			if (message == null)
			{
				return default(T);
			}
			var content = message.Body as string;
			T body = default(T);
			try
			{
				body = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
			}
			catch (Exception ex)
			{
				ex.Data.Add("jsoncontent", content);
				GlobalConfiguration.Configuration.Logger.Error(ex);
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
			catch (System.Messaging.MessageQueueException mqex)
			{
				try
				{
					mqex.Data.Add("QueueName", QueueName);
					mqex.Data.Add("MsmqErrorCode", mqex.MessageQueueErrorCode.ToString());
					mqex.Data.Add("MessageType", message.GetType().FullName);
					var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
					mqex.Data.Add("Message", json);
				}
				catch { /* Dead for science */ }
			}
			catch(Exception ex)
			{
				try
				{
					ex.Data.Add("QueueName", QueueName);
					ex.Data.Add("MessageType", message.GetType().FullName);
					var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
					ex.Data.Add("Message", json);
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
