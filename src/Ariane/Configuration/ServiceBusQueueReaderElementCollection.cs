using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Ariane.Configuration
{
	public class QueueReaderConfigurationElementCollection : ConfigurationElementCollection
	{
		public bool IsDirty
		{
			get
			{
				return base.IsModified();
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ServiceBusQueueReaderConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ServiceBusQueueReaderConfigurationElement)element).QueueName;
		}

		public void Add(ServiceBusQueueReaderConfigurationElement element)
		{
			base.BaseAdd(element);
		}

		public void Remove(ServiceBusQueueReaderConfigurationElement element)
		{
			base.BaseRemove(element);
		}

		public void Clear()
		{
			base.BaseClear();
		}

		public override bool IsReadOnly()
		{
			return false;
		}

		public bool ContainsKey(string name)
		{
			foreach (var item in base.BaseGetAllKeys())
			{
				if (item.ToString().Equals(name, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		public new ConfigurationElement this[string name]
		{
			get
			{
				return (ServiceBusQueueReaderConfigurationElement)base.BaseGet(name);
			}
		}

		protected override bool ThrowOnDuplicate
		{
			get
			{
				return true;
			}
		}

		public override System.Configuration.ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return System.Configuration.ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}
	}
}
