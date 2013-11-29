using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		}

		public string QueueName { get ; private set; }

		public IAsyncResult BeginReceive()
		{
			return m_Queue.BeginReceive();
		}

		public T EndReceive<T>(IAsyncResult result)
		{
			var message = m_Queue.EndReceive(result);
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
				throw mqex;
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
				throw ex;
			}
		}

		#endregion

		public override string ToString()
		{
			var result = base.ToString();
			result += string.Format("CanRead:{0}{1}", m_Queue.CanRead, System.Environment.NewLine);
			result += string.Format("CanWrite:{0}{1}", m_Queue.CanWrite, System.Environment.NewLine);
			result += string.Format("FormatName:{0}{1}", m_Queue.FormatName, System.Environment.NewLine);
			result += string.Format("Formatter:{0}{1}", m_Queue.Formatter, System.Environment.NewLine);
			result += string.Format("Label:{0}{1}", m_Queue.Label, System.Environment.NewLine);
			result += string.Format("MachineName:{0}{1}", m_Queue.MachineName, System.Environment.NewLine);
			result += string.Format("QueueName:{0}{1}", m_Queue.QueueName, System.Environment.NewLine);
			return result;
		}
	}
}
