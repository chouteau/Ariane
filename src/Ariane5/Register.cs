﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	internal class Register : IRegister
	{
		private bool m_IsInitialized = false;

		public Register(ArianeSettings arianeSettings,
			IServiceCollection serviceServiceCollection
			)
		{
			this.Settings = arianeSettings;
			this.ServiceCollection = serviceServiceCollection;
			this.QueueList = new List<QueueSetting>();
			this.SubscriberByQueueList = new Dictionary<string, List<Type>>();
		}

		protected IServiceCollection ServiceCollection { get; }
		protected ArianeSettings Settings { get; }
		protected IList<QueueSetting> QueueList { get; }
		protected IDictionary<string, List<Type>> SubscriberByQueueList { get; }
		public Exception ConfigurationException { get; set; }

		public IRegister AddQueue(QueueSetting queueSetting)
		{
			if (m_IsInitialized)
			{
				throw new Exception("Ariane is already initialized");
			}

			if (queueSetting == null)
			{
				throw new ArgumentNullException("The queue settings is null");
			}

            if (string.IsNullOrWhiteSpace(queueSetting.Name))
            {
                throw new ArgumentException("The name of queue is null or empty");
            }

            if (Settings.UniqueTopicNameForTest != null)
			{
				queueSetting.SubscriptionName = Settings.UniqueTopicNameForTest;
			}

			if (Settings.WorkSynchronized)
            {
				queueSetting.AutoStartReading = false;
            }

			queueSetting.Name = $"{Settings.UniquePrefixName}{queueSetting.Name}";
			var queueName = queueSetting.Name;
			var existing = QueueList.SingleOrDefault(i => queueSetting.Name.Equals(i.Name, StringComparison.InvariantCultureIgnoreCase)
													&& (i.SubscriptionName??string.Empty).Equals(queueSetting.SubscriptionName??string.Empty, StringComparison.InvariantCultureIgnoreCase));

			if (existing == null)
			{
				QueueList.Add(queueSetting);
				ServiceCollection.AddSingleton(sp =>
				{
					var medium = (IMedium)sp.GetService(queueSetting.TypeMedium);
					if (medium == null)
					{
						var message = $"Ariane medium {queueSetting.TypeMedium} not registed";
						throw new KeyNotFoundException(message);
					}
					var messageQueue = medium.CreateMessageQueue(queueSetting);
					return messageQueue;
				});
			}

			if (queueSetting.TypeReader != null)
			{
				if (!SubscriberByQueueList.ContainsKey(queueName))
                {
					SubscriberByQueueList.Add(queueName, new List<Type>());
				}
				SubscriberByQueueList[queueName].Add(queueSetting.TypeReader);
			}

			return this;
		}

		public void Initialize()
		{
			if (m_IsInitialized)
            {
				return;
            }

			foreach (var item in SubscriberByQueueList)
			{
				var dispatcherTypeList = item.Value;
				var queueSettingsList = QueueList.Where(i => i.Name == item.Key).ToList();
                foreach (var queueSettings in queueSettingsList)
                {
					ServiceCollection.AddSingleton(sp =>
					{
						IMessageDispatcher md = null;
						try
						{
							md = CreateMessageDispatcher(sp, dispatcherTypeList, queueSettings);
						}
						catch(Exception ex)
						{
							ConfigurationException = ex;
						}
						return md;
					});
					if (ConfigurationException != null)
					{
						break;
					}
				}
			}

			m_IsInitialized = true;
		}

		public IEnumerable<string> GetRegisteredQueues()
		{
			return QueueList.Select(i => i.Name);
		}

		private IMessageDispatcher CreateMessageDispatcher(IServiceProvider sp, List<Type> messageTypeList, QueueSetting queueSettings)
		{
			var baseType = typeof(MessageDispatcher<>);
			Type messageType = null;
			var singleTypeList = new List<Type>();
			foreach (var item in messageTypeList)
			{
				var mt = item;
				while (true)
				{
					var arguments = mt.GetGenericArguments();
					if (arguments.Count() == 1
						|| mt == baseType)
					{
						messageType = arguments[0];
						break;
					}
					mt = mt.BaseType;
				}
				if (singleTypeList.Any()
					&& !singleTypeList.Contains(messageType))
				{
					throw new Exception($"Reader for queue {queueSettings.Name} must contain single messageType {messageType}");
				}
				singleTypeList.Add(messageType);
			}
			var typeDispatcher = baseType.MakeGenericType(messageType);

			var messageDispatcher = (IMessageDispatcher)ActivatorUtilities.CreateInstance(sp, typeDispatcher);
			messageDispatcher.AutoStart = queueSettings.AutoStartReading;
			messageDispatcher.InitializeMedium(sp, queueSettings);
			foreach (var item in SubscriberByQueueList)
			{
				if (item.Key != queueSettings.Name)
				{
					continue;
				}

				foreach (var subscriberType in item.Value)
				{
					messageDispatcher.AddMessageSubscriberType(subscriberType);
				}
			}
			messageDispatcher.InitializeSubscribers(sp);
			return messageDispatcher;
		}
	}
}
