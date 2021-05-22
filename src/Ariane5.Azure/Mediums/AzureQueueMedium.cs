using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Ariane
{
	public class AzureQueueMedium : IMedium
	{
        public AzureQueueMedium(IConfiguration configuration,
			ArianeSettings settings,
			ILogger<AzureQueueMedium> logging
			)
        {
			this.Configuration = configuration;
			this.Settings = settings;
			this.Logger = logging;
        }

		public bool TopicCompliant => false;
		protected IConfiguration Configuration { get; }
		protected ArianeSettings Settings { get; }
		protected ILogger Logger { get; }

		public IMessageQueue CreateMessageQueue(QueueSetting queueSetting)
		{
			string cs = null;
			if (!string.IsNullOrWhiteSpace(Settings.DefaultAzureConnectionString))
			{
				cs = Settings.DefaultAzureConnectionString;
			}
			else
			{
				cs = Configuration.GetConnectionString(queueSetting.ConnectionStringName ?? queueSetting.Name);
			}

			if (cs == "off")
			{
				return null;
			}

			var managementClient = new ServiceBusAdministrationClient(cs);
			var queueExists = managementClient.QueueExistsAsync(queueSetting.Name).Result;
			if (!queueExists)
			{
				var options = new CreateQueueOptions(queueSetting.Name)
				{
					DefaultMessageTimeToLive = TimeSpan.FromDays(1),
					AutoDeleteOnIdle = TimeSpan.FromDays(7),
					EnableBatchedOperations = true,
					MaxDeliveryCount = 1,
				};
				options.AuthorizationRules.Add(new SharedAccessAuthorizationRule("allClaims"
					, new[] { AccessRights.Manage, AccessRights.Send, AccessRights.Listen }));

				managementClient.CreateQueueAsync(options).Wait();

				Logger.LogInformation($"Azure queue {queueSetting.Name} created");
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
			var mq = new QueueProviders.AzureMessageQueue(serviceBusClient, queueSetting.Name, Logger);
			mq.ConnectionString = cs;
			return mq;
		}
	}
}
