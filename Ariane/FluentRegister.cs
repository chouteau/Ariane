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
				if (section == null)
				{
					// Check if ariane.config is present with location of current assembly
					var path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
					configFileName = System.IO.Path.Combine(path.TrimEnd('\\'), "ariane.config");
					if (!System.IO.File.Exists(configFileName))
					{
						configFileName = null;
					}
				}
			}

			if (section == null
				&& configFileName != null)
			{
				var map = new ExeConfigurationFileMap();
				map.ExeConfigFilename = configFileName;
				var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
				section = config.GetSection(sectionName) as Configuration.ServiceBusConfigurationSection;
			}
			else
			{
				throw new System.Configuration.ConfigurationErrorsException(string.Format("section {0} does not exists", sectionName));
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
				var qs = new QueueSetting() 
				{ 
					Name = item.QueueName, 
					TypeReader = reader, 
					TypeMedium = medium,
					AutoStartReading = item.AutoStartReading
				};
				AddQueue(qs);
			}
			return this;
		}

		public IFluentRegister AddQueue(QueueSetting queueSetting)
		{
			if (queueSetting == null)
			{
				throw new ArgumentNullException();
			}

			lock (m_RegistrationList.SyncRoot)
			{
				var registration = m_RegistrationList.SingleOrDefault(i => i.QueueName.Equals(queueSetting.Name, StringComparison.InvariantCultureIgnoreCase));
				if (registration == null)
				{
					registration = new Registration()
					{
						QueueName = queueSetting.Name,
						TypeMedium = queueSetting.TypeMedium,
						AutoStartReading = queueSetting.AutoStartReading
					};
					m_RegistrationList.Add(registration);
				}
				registration.AddSubscriberType(queueSetting.TypeReader);
			}
			return this;
		}

		public IFluentRegister AddQueue<T>(QueueSetting queueSetting, Action<T> predicate)
		{
			if (queueSetting == null
				|| predicate == null)
			{
				throw new ArgumentNullException();
			}

			lock (m_RegistrationList.SyncRoot)
			{
				var registration = m_RegistrationList.SingleOrDefault(i => i.QueueName.Equals(queueSetting.Name, StringComparison.InvariantCultureIgnoreCase));
				if (registration == null)
				{
					registration = new Registration()
					{
						QueueName = queueSetting.Name,
						TypeMedium = queueSetting.TypeMedium,
						AutoStartReading = queueSetting.AutoStartReading
					};
					m_RegistrationList.Add(registration);
				}
				var subscriber = new AnonymousMessageSubscriber<T>(predicate);
				registration.AddSubscriber(subscriber);
			}
			return this;
		}

		public void Clear()
		{
			lock (m_RegistrationList.SyncRoot)
			{
				foreach (var registration in m_RegistrationList)
				{
					if (!registration.AutoStartReading)
					{
						continue;
					}
					if (registration.Reader == null)
					{
						continue;
					}
					registration.Reader.Dispose();
				}
				m_RegistrationList.Clear();
			}
		}

		public IEnumerable<string> GetRegisteredQueues()
		{
			lock (m_RegistrationList.SyncRoot)
			{
				return m_RegistrationList.Select(i => i.QueueName);
			}
		}
	}
}
