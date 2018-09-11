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
			string cs = null;
			if (!string.IsNullOrWhiteSpace(Azure.GlobalConfiguration.Current.DefaultAzureConnectionString))
			{
				cs = Azure.GlobalConfiguration.Current.DefaultAzureConnectionString;
			}
			else
			{
				cs = System.Configuration.ConfigurationManager.ConnectionStrings[queueName].ConnectionString;
			}

			if (cs == "off")
			{
				return null;
			}

			var nsManager = NamespaceManager.CreateFromConnectionString(cs);
			if (!nsManager.QueueExists(queueName))
			{
				var qd = new QueueDescription(queueName);
				qd.DefaultMessageTimeToLive = TimeSpan.FromHours(24);
				qd.MaxDeliveryCount = 1;
				nsManager.CreateQueue(qd);
			}

			nsManager.Settings.RetryPolicy = new RetryExponential(
				minBackoff: TimeSpan.FromSeconds(0),
				maxBackoff: TimeSpan.FromSeconds(30),
				maxRetryCount: 3);

			var queueClient = QueueClient.CreateFromConnectionString(cs, queueName);
			var mq = new QueueProviders.AzureMessageQueue(queueClient);
			return mq;
		}

		#endregion
	}
}
