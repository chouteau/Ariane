using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Ariane
{
	internal class FluentRegister : IFluentRegister
	{
		private SynchronizedCollection<Registration> m_RegistrationList;

		public FluentRegister()
		{
			m_RegistrationList = new SynchronizedCollection<Registration>();
		}

		public SynchronizedCollection<Registration> List
		{
			get
			{
				return m_RegistrationList;
			}
		}

		public IFluentRegister AddFromConfig(string configFileName = null)
		{
			string sectionName = "ariane/serviceBus";
			Configuration.ServiceBusConfigurationSection section = null;
			if (configFileName == null) // Default file configuration
			{
				section = System.Configuration.ConfigurationManager.GetSection(sectionName) as Configuration.ServiceBusConfigurationSection;
			}
			else
			{
				var map = new ExeConfigurationFileMap();
				map.ExeConfigFilename = configFileName;
				var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
				section = config.GetSection(sectionName) as Configuration.ServiceBusConfigurationSection;
			}

			foreach (Configuration.ServiceBusQueueReaderConfigurationElement item in section.QueueReaders)
			{
				if (!item.Enabled)
				{
					continue;
				}
				Type reader = null;
				if (!string.IsNullOrWhiteSpace(item.TypeReader))
				{
					reader = Type.GetType(item.TypeReader);
					if (reader == null)
					{
						GlobalConfiguration.Configuration.Logger.Warn("Type {0} reader for servicebus does not exists", item.TypeReader);
						continue;
					}
				}
				var medium = Type.GetType(item.TypeMedium);
				if (medium == null)
				{
					if (!string.IsNullOrWhiteSpace(item.TypeMedium))
					{
						GlobalConfiguration.Configuration.Logger.Warn("Type {0} medium for servicebus does not exists", item.TypeMedium);
						continue;
					}
					medium = typeof(InMemoryMedium);
				}
				var qs = new QueueSetting() { Name = item.QueueName, TypeReader = reader, TypeMedium = medium };
				AddQueue(qs);
			}
			return this;
		}

		public IFluentRegister AddQueue(QueueSetting queueSetting)
		{
			if (m_RegistrationList.Any(i => i.QueueName.Equals(queueSetting.Name, StringComparison.InvariantCultureIgnoreCase)))
			{
				return this;
			}
			lock (m_RegistrationList.SyncRoot)
			{
				var registration = new Registration()
				{
					QueueName = queueSetting.Name,
					TypeMedium = queueSetting.TypeMedium,
				};
				registration.AddSubscriber(queueSetting.TypeReader);
				if (!m_RegistrationList.Any(i => i.QueueName == queueSetting.Name))
				{
					m_RegistrationList.Add(registration);
				}
			}
			return this;
		}

		public IFluentRegister AddMemoryReader(string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(InMemoryMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddMemoryWriter(string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(InMemoryMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddMSMQReader(string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(MSMQMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddMSMQWriter(string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(MSMQMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddAzureReader(string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(AzureMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddAzureWriter(string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(AzureMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddFileReader(string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(FileMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

		public IFluentRegister AddFileWriter(string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(FileMedium),
			};
			AddQueue(queueSetting);
			return this;
		}

	}
}
