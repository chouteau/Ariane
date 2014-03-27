using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Represents a queue MSMQ Type
	/// </summary>
	public class MSMQMedium : IMedium
	{
		/// <summary>
		/// Create message compatible with MSMQ
		/// </summary>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public virtual IMessageQueue CreateMessageQueue(string queueName)
		{
			var pathConfig = System.Configuration.ConfigurationManager.ConnectionStrings[queueName];
			if (pathConfig == null)
			{
				throw new ArgumentException(string.Format("queueName {0} does not exists in connectionStrings section configuration", queueName));
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
