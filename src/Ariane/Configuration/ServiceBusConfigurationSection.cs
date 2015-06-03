using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Ariane.Configuration
{
	public class ServiceBusConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("queueReaders", IsRequired = true)]
		public QueueReaderConfigurationElementCollection QueueReaders
		{
			get
			{
				return (QueueReaderConfigurationElementCollection)this["queueReaders"];
			}
			set
			{
				this["queueReaders"] = value;
			}
		}

	}
}
