using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane
{
	public class AzureQueueMedium : IMedium
	{
		#region IMedium Members

		public IMessageQueue CreateMessageQueue(string queueName, string topicName = null)
		{
			var cs = System.Configuration.ConfigurationManager.ConnectionStrings[queueName].ConnectionString;

			var nsManager = NamespaceManager.CreateFromConnectionString(cs);
			if (!nsManager.QueueExists(queueName))
			{
				var qd = new QueueDescription(queueName);
				nsManager.CreateQueue(qd);
			}

			var queueClient = QueueClient.CreateFromConnectionString(cs, queueName);
			return new QueueProviders.AzureMessageQueue(queueClient);
		}

		#endregion
	}
}
