using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Ariane
{
	public class AzureTopicMedium : IMedium
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

			var nsManager = NamespaceManager.CreateFromConnectionString(cs);
			if (!nsManager.TopicExists(queueName))
			{
				var qd = new TopicDescription(queueName);
				nsManager.CreateTopic(qd);
			}

			var topicClient = TopicClient.CreateFromConnectionString(cs, queueName);

			SubscriptionClient receiver = null;
			if (topicName != null)
			{
				receiver = SubscriptionClient.CreateFromConnectionString(cs, queueName, topicName);
				if (topicName != null
					&& !nsManager.SubscriptionExists(queueName, topicName))
				{
					var sd = new SubscriptionDescription(queueName, topicName);
					nsManager.CreateSubscription(sd);
				}
			}

			return new QueueProviders.AzureMessageTopic(topicClient, receiver);
		}

		#endregion
	}
}
