﻿using System;
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
				var item = System.Configuration.ConfigurationManager.ConnectionStrings[queueName];
				if (item != null)
				{
					cs = item.ConnectionString;
				}
				else
				{
					GlobalConfiguration.Configuration.Logger.Error($"queue {queueName} does not exists");
					cs = "off";
				}
			}

			if (cs == "off")
			{
				return null;
			}

			var nsManager = NamespaceManager.CreateFromConnectionString(cs);
			if (!nsManager.TopicExists(queueName))
			{
				var qd = new TopicDescription(queueName);
				qd.DefaultMessageTimeToLive = TimeSpan.FromHours(24);
				nsManager.CreateTopic(qd);
			}

			nsManager.Settings.RetryPolicy = new RetryExponential(
				minBackoff: TimeSpan.FromSeconds(0),
				maxBackoff: TimeSpan.FromSeconds(30),
				maxRetryCount: 3);

			var topicClient = TopicClient.CreateFromConnectionString(cs, queueName);

			SubscriptionClient receiver = null;
			if (topicName != null)
			{
				receiver = SubscriptionClient.CreateFromConnectionString(cs, queueName, topicName);
				if (topicName != null
					&& !nsManager.SubscriptionExists(queueName, topicName))
				{
					var sd = new SubscriptionDescription(queueName, topicName);
					sd.DefaultMessageTimeToLive = TimeSpan.FromHours(24);
					sd.MaxDeliveryCount = 1;
					nsManager.CreateSubscription(sd);
				}
			}

			return new QueueProviders.AzureMessageTopic(topicClient, receiver);
		}

		#endregion
	}
}
