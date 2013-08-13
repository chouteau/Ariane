using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;

namespace Ariane
{
	public class MSMQMedium : IMedium
	{
		public MSMQMedium()
		{
		}

		public virtual IMessageQueue CreateMessageQueue(string queueName)
		{
			string path = System.Configuration.ConfigurationManager.ConnectionStrings[queueName].ConnectionString;

			var  result = new System.Messaging.MessageQueue(path, System.Messaging.QueueAccessMode.SendAndReceive);
			return new MSMQMessageQueueWrapper(result, queueName);
		}

		public virtual IMessage CreateMessage()
		{
			var result = new System.Messaging.Message();
			result.Priority = System.Messaging.MessagePriority.Normal;
			result.Recoverable = true;
			return new MSMQMessageWrapper(result);
		}
	}
}
