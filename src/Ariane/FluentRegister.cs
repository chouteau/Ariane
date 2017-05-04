using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Ariane
{
	internal class FluentRegister : IRegister
	{
		public FluentRegister()
		{
			List = new SynchronizedCollection<Registration>();
		}

		public SynchronizedCollection<Registration> List { get; private set; }

		public IRegister AddFromConfig(string configFileName = null)
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

		public IRegister AddQueue(QueueSetting queueSetting)
		{
			if (queueSetting == null)
			{
				throw new ArgumentNullException();
			}

			lock (List.SyncRoot)
			{
				var queueName = queueSetting.Name;
				var registration = List.SingleOrDefault(i => queueSetting.Name.Equals(i.QueueName, StringComparison.InvariantCultureIgnoreCase)
														&& i.TopicName == queueSetting.TopicName);
				if (registration == null)
				{
					registration = new Registration()
					{
						QueueName = queueSetting.Name,
						TypeMedium = queueSetting.TypeMedium,
						AutoStartReading = queueSetting.AutoStartReading,
						TopicName = queueSetting.TopicName
					};
					List.Add(registration);
					GlobalConfiguration.Configuration.Logger.Info($"Add queue {List.Count} {registration.QueueName} {registration.TypeMedium} {registration.TopicName} {queueSetting.TypeReader} {registration.AutoStartReading}");
					registration.AddSubscriberType(queueSetting.TypeReader);
				}
			}
			return this;
		}

		public IRegister AddQueue<T>(QueueSetting queueSetting, Action<T> predicate)
		{
			if (queueSetting == null
				|| predicate == null)
			{
				throw new ArgumentNullException();
			}

			lock (List.SyncRoot)
			{
				var registration = List.SingleOrDefault(i => i.QueueName.Equals(queueSetting.Name, StringComparison.InvariantCultureIgnoreCase));
				if (registration == null)
				{
					registration = new Registration()
					{
						QueueName = queueSetting.Name,
						TypeMedium = queueSetting.TypeMedium,
						AutoStartReading = queueSetting.AutoStartReading
					};
					List.Add(registration);
				}
				var subscriber = new AnonymousMessageSubscriber<T>(predicate);
				registration.AddSubscriber(subscriber);
			}
			return this;
		}

		public void Clear()
		{
			lock (List.SyncRoot)
			{
				foreach (var registration in List)
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
				List.Clear();
			}
		}

		public IEnumerable<string> GetRegisteredQueues()
		{
			lock (List.SyncRoot)
			{
				return List.Select(i => i.QueueName);
			}
		}
	}
}
