using Experimental.System.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Represents a queue MSMQ Type
	/// </summary>
	public class MSMQMedium : IMedium
	{
        public MSMQMedium(IConfiguration configuration,
			ILogger<MSMQMedium> logger)
        {
			this.Configuration = configuration;
			this.Logger = logger;
        }

		public bool TopicCompliant => true;
		protected IConfiguration Configuration { get; }
		protected ILogger<MSMQMedium> Logger { get; }

		/// <summary>
		/// Create message compatible with MSMQ
		/// </summary>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public virtual IMessageQueue CreateMessageQueue(QueueSetting queueSetting)
		{
			var path = Configuration.GetConnectionString(queueSetting.ConnectionStringName ?? queueSetting.Name);
			if (path == null)
			{
				throw new ArgumentException(string.Format("queueName {0} does not exists in connectionStrings section configuration", queueSetting.Name));
			}

			if (!path.StartsWith("formatname:direct")
				&& !Experimental.System.Messaging.MessageQueue.Exists(path))
			{
				try
				{
					string everyone = new System.Security.Principal.SecurityIdentifier(
						"S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();
					var queue = Experimental.System.Messaging.MessageQueue.Create(path);
					// queue.SetPermissions(everyone, MessageQueueAccessRights.FullControl);
				}
				catch(Exception ex)
				{
					ex.Data.Add("queueName", queueSetting.Name);
					ex.Data.Add("queuePath", path);
					Logger.LogError(ex, ex.Message);
				}
			}

			var  result = new Experimental.System.Messaging.MessageQueue(path, Experimental.System.Messaging.QueueAccessMode.SendAndReceive);
			return new QueueProviders.MSMQMessageQueue(result, queueSetting.Name);
		}
	}
}
