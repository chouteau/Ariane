using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;

namespace Ariane
{
	public class AzureMedium : IMedium
	{
		#region IMedium Members

		public IMessageQueue CreateMessageQueue(string queueName)
		{
			var cs = System.Configuration.ConfigurationManager.ConnectionStrings[queueName].ConnectionString;

			var nsManager = NamespaceManager.CreateFromConnectionString(cs);
			if (!nsManager.QueueExists(queueName))
			{
				var qd = new QueueDescription(queueName);
				nsManager.CreateQueue(qd);
			}

			var queueClient = QueueClient.CreateFromConnectionString(cs, queueName);
			return new QueueProviders.AzureMessageQueue(queueClient, queueName);
		}

		#endregion
	}
}
