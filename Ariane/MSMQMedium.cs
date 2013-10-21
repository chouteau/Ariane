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
			var pathConfig = System.Configuration.ConfigurationManager.ConnectionStrings[queueName];
			if (pathConfig == null)
			{
				throw new ArgumentException("queueName {0} does not exists", queueName);
			}

			var path = pathConfig.ConnectionString;

			if (!path.StartsWith("formatname:direct")
				&& !System.Messaging.MessageQueue.Exists(path))
			{
				string everyone = new System.Security.Principal.SecurityIdentifier(
					"S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();
				var queue = System.Messaging.MessageQueue.Create(path);
				queue.SetPermissions(everyone, MessageQueueAccessRights.FullControl);
			}

			var  result = new System.Messaging.MessageQueue(path, System.Messaging.QueueAccessMode.SendAndReceive);
			return new QueueProviders.MSMQMessageQueue(result, queueName);
		}
	}
}
