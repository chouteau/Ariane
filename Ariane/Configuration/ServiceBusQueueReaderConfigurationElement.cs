using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Ariane.Configuration
{
	public class ServiceBusQueueReaderConfigurationElement : ConfigurationElement
	{
		public ServiceBusQueueReaderConfigurationElement()
		{
			this.Enabled = true;
		}

		[ConfigurationProperty("queueName", IsRequired = true)]
		public string QueueName
		{
			get
			{
				return (string)this["queueName"];
			}
			set
			{
				this["queueName"] = value;
			}
		}

		[ConfigurationProperty("typeReader", IsRequired = true)]
		public string TypeReader
		{
			get
			{
				return (string)this["typeReader"];
			}
			set
			{
				this["typeReader"] = value;
			}
		}

		[ConfigurationProperty("typeMedium", IsRequired = false)]
		public string TypeMedium
		{
			get
			{
				return (string)this["typeMedium"];
			}
			set
			{
				this["typeMedium"] = value;
			}
		}

		[ConfigurationProperty("enabled", IsRequired = false)]
		public bool Enabled
		{
			get
			{
				return (bool)this["enabled"];
			}
			set
			{
				this["enabled"] = value;
			}
		}

	}
}
