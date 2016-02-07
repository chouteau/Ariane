using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Messaging;

namespace Ariane.QueueProviders
{
	internal class MSMQJSonMessageFormatter : System.Messaging.IMessageFormatter
	{
		#region IMessageFormatter Members

		public bool CanRead(System.Messaging.Message message)
		{
			if (message == null)
			{
				throw new ArgumentNullException();
			}

			if (message.BodyStream == null)
			{
				return false;
			}

			if (message.BodyStream.CanRead
				&& message.BodyStream.Length > 0)
			{
				return true;
			}

			return false;
		}

		public object Read(System.Messaging.Message message)
		{
			if (!CanRead(message))
			{
				return null;
			}

			object result = null;
			using (var r = new System.IO.StreamReader(message.BodyStream, Encoding.UTF8))
			{
				result = r.ReadToEnd();
			}
			return result;
		}

		public void Write(System.Messaging.Message message, object obj)
		{
			if (message == null
				|| obj == null)
			{
				throw new ArgumentNullException();
			}

			object body = obj.GetType().GetProperty("Body").GetValue(obj, null);
			var priority = (int)obj.GetType().GetProperty("Priority").GetValue(obj, null);
			var label = (string)obj.GetType().GetProperty("Label").GetValue(obj, null);
			var recoverable = (bool)obj.GetType().GetProperty("Recoverable").GetValue(obj, null);

			var json = JsonConvert.SerializeObject(body, Formatting.None);
			var buffer = System.Text.Encoding.UTF8.GetBytes(json);
			message.BodyStream = new System.IO.MemoryStream(buffer);
			message.BodyType = 0;
			switch (priority)
			{
				case 0:
					message.Priority = MessagePriority.Normal;
					break;
				case 1:
					message.Priority = MessagePriority.High;
					break;
				case 2:
					message.Priority = MessagePriority.VeryHigh;
					break;
				case 3:
					message.Priority = MessagePriority.Highest;
					break;
			}
			message.Label = label;
			message.Recoverable = recoverable;
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			return new MSMQJSonMessageFormatter();
		}

		#endregion
	}
}
