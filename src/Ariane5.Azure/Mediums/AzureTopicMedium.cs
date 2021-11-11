using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Threading.Tasks;

namespace Ariane
{
	public class AzureTopicMedium : IMedium
	{
		public AzureTopicMedium(IConfiguration configuration,
					ArianeSettings settings,
					ILogger<AzureTopicMedium> logger,
					AzureBusSettings azureBusSettings)
		{
			this.Configuration = configuration;
			this.Settings = settings;
			this.Logger = logger;
			this.AzureBusSettings = azureBusSettings;
		}

		public bool TopicCompliant => false;
		protected IConfiguration Configuration { get; }
		protected ArianeSettings Settings { get; }
		protected ILogger Logger { get; }
		protected AzureBusSettings AzureBusSettings { get; }


		public IMessageQueue CreateMessageQueue(QueueSetting queueSetting)
		{
			string cs = null;
			if (!string.IsNullOrWhiteSpace(Settings.DefaultAzureConnectionString))
			{
				cs = Settings.DefaultAzureConnectionString;
			}
			else
			{
				var item = Configuration.GetConnectionString(queueSetting.Name);
				if (item != null)
				{
					cs = item;
				}
				else
				{
					Logger.LogError($"queue {queueSetting.Name} does not exists");
					cs = "off";
				}
			}

			if (cs == "off")
			{
				return null;
			}

			var managementClient = new ServiceBusAdministrationClient(cs);
			var topicExists = managementClient.TopicExistsAsync(queueSetting.Name).Result;
			if (!topicExists)
			{
				var topicOptions = new CreateTopicOptions(queueSetting.Name)
				{
					DefaultMessageTimeToLive = TimeSpan.FromDays(AzureBusSettings.DefaultMessageTimeToLiveInDays),
					AutoDeleteOnIdle = TimeSpan.FromDays(AzureBusSettings.AutoDeleteOnIdleInDays),
					EnableBatchedOperations = true,
				};
				topicOptions.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims"
					, new[] { AccessRights.Manage, AccessRights.Send, AccessRights.Listen }));

				managementClient.CreateTopicAsync(topicOptions).Wait();

				Logger.LogInformation($"Azure topic {queueSetting.Name} created");
			}

			if (!string.IsNullOrWhiteSpace(queueSetting.SubscriptionName))
            {
				var subscriptionExists = managementClient.SubscriptionExistsAsync(queueSetting.Name, queueSetting.SubscriptionName).Result;
				if (!subscriptionExists)
				{
					var subscriptionOptions = new CreateSubscriptionOptions(queueSetting.Name, queueSetting.SubscriptionName)
					{
						EnableBatchedOperations = true,
						DefaultMessageTimeToLive = TimeSpan.FromDays(AzureBusSettings.DefaultMessageTimeToLiveInDays),
						AutoDeleteOnIdle = TimeSpan.FromDays(AzureBusSettings.AutoDeleteOnIdleInDays)
					};

					managementClient.CreateSubscriptionAsync(subscriptionOptions).Wait();

					Logger.LogInformation($"Azure subscription {queueSetting.SubscriptionName} created for topic {queueSetting.Name}");
				}
			}

			var serviceBusClient = new ServiceBusClient(cs, new ServiceBusClientOptions()
			{
				TransportType = ServiceBusTransportType.AmqpTcp,
				RetryOptions = new ServiceBusRetryOptions()
				{
					Mode = ServiceBusRetryMode.Exponential,
					MaxRetries = 3,
					MaxDelay = TimeSpan.FromSeconds(10)
				}
			});
			var mq = new QueueProviders.AzureMessageTopic(serviceBusClient, AzureBusSettings, queueSetting.Name, queueSetting.SubscriptionName, Logger, , queueSetting.FlushReceivedMessageToDiskBeforeProcess);
			mq.ConnectionString = cs;
			return mq;
		}
	}
}
