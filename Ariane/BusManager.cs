using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

namespace Ariane
{
	public class BusManager : IServiceBus, IDisposable
	{
		private class Registration
		{
			public Registration()
			{
				Reader = new Lazy<IMessageReader>(() =>
					{
						var result = GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeReader);
						return (IMessageReader)result;
					}, true);

				Medium = new Lazy<IMedium>(() =>
					{
						return (IMedium)GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeMedium);
					}, true);

				Queue = new Lazy<IMessageQueue>(() =>
					{
						return Medium.Value.CreateMessageQueue(QueueName);
					}, true);
			}
			public string QueueName { get; set; }
			public Type TypeReader { get; set; }
			public Lazy<IMessageReader> Reader { get; set; }
			public Type TypeMedium { get; set; }
			public Lazy<IMedium> Medium { get; set; }
			public Lazy<IMessageQueue> Queue { get; set; }
		}

		private SynchronizedCollection<Registration> m_RegistrationList;

		private Queue<Action> m_Queue;
		private ManualResetEvent m_NewMessage = new ManualResetEvent(false);
		private ManualResetEvent m_Terminate = new ManualResetEvent(false);
		private bool m_Terminated = false;
		private Thread m_SendThread;

		public BusManager()
		{
			m_RegistrationList = new SynchronizedCollection<Registration>();
		}

		#region IServiceBus Members

		public void Send(string queueName, object body, string label = null)
		{
			if (m_Queue == null)
			{
				InitializeQueueSenderThread();
			}
			lock(m_Queue)
			{
				m_Queue.Enqueue(() =>
					{
						SendInternal(queueName, body, label);
					});
				m_NewMessage.Set();
			}
		}

		private void SendInternal(string queueName, object body, string label = null)
		{
			var registration = m_RegistrationList.SingleOrDefault(i => i.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase));
			if (registration == null)
			{
				return;
			}
			var mq = registration.Queue.Value;
			var m = registration.Medium.Value.CreateMessage();
			m.Label = label ?? Guid.NewGuid().ToString();
			m.Body = body;
			mq.Send(m);
		}

		public void RegisterReadersFromConfig(string configFileName = null)
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
				var reader = Type.GetType(item.TypeReader);
				if (reader == null)
				{
					GlobalConfiguration.Configuration.Logger.Warn("Type {0} reader for servicebus does not exists", item.TypeReader);
					continue;
				}
				var medium = Type.GetType(item.TypeMedium);
				if (medium == null)
				{
					if (!string.IsNullOrEmpty(item.TypeMedium))
					{
						GlobalConfiguration.Configuration.Logger.Warn("Type {0} medium for servicebus does not exists", item.TypeMedium);
						continue;
					}
					medium = typeof(InMemoryMedium);
				}
				RegisterReader(item.QueueName, reader, medium);
			}
		}

		public void RegisterReader(string queueName, Type reader, Type medium = null)
		{
			if (m_RegistrationList.Any(i => i.QueueName.Equals(queueName, StringComparison.InvariantCultureIgnoreCase)))
			{
				return;
			}
			lock (m_RegistrationList.SyncRoot)
			{
				var registration = new Registration()
				{
					QueueName = queueName,
					TypeReader = reader,
					TypeMedium = medium ?? typeof(InMemoryMedium),
				};

				m_RegistrationList.Add(registration);
			}
		}

		public void StartReading()
		{
			foreach (var item in m_RegistrationList)
			{
				if (item.Reader.IsValueCreated)
				{
					item.Reader.Value.Stop();
				}
			}

			foreach (var item in m_RegistrationList)
			{
				var medium = item.Medium.Value;
				var queue = item.Queue.Value;
				item.Reader.Value.Start(queue);
			}
		}

		public void StopReading()
		{
			foreach (var item in m_RegistrationList)
			{
				item.Reader.Value.Stop();
				item.Reader.Value.Dispose();
			}
		}

		public void PauseReading()
		{
			foreach (var item in m_RegistrationList)
			{
				item.Reader.Value.Pause();
			}
		}

		#endregion

		void SendInQueue()
		{
			while (!m_Terminated)
			{
				var waitHandles = new WaitHandle[] { m_Terminate, m_NewMessage };
				int result = ManualResetEvent.WaitAny(waitHandles, 60 * 1000, true);
				if (result == 0)
				{
					m_Terminated = true;
					break;
				}
				m_NewMessage.Reset();

				if (m_Queue.Count == 0)
				{
					continue;
				}
				// Enqueue
				Queue<Action> queueCopy;
				lock (m_Queue)
				{
					queueCopy = new Queue<Action>(m_Queue);
					m_Queue.Clear();
				}

				foreach (var send in queueCopy)
				{
					try
					{
						send();
					}
					catch(Exception ex)
					{
						GlobalConfiguration.Configuration.Logger.Error(ex);
					}
				}
			}
		}

		private void InitializeQueueSenderThread()
		{
			m_Queue = new Queue<Action>();
			m_SendThread = new Thread(new ThreadStart(SendInQueue));
			m_SendThread.IsBackground = true;
			m_SendThread.Start();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (m_Queue != null)
			{
				m_Queue.Clear();
			}
			m_Terminated = true;
			if (m_Terminate != null)
			{
				m_Terminate.Set();
			}
			if (m_SendThread != null)
			{
				if (m_SendThread != null 
					&& !m_SendThread.Join(TimeSpan.FromSeconds(5)))
				{
					m_SendThread.Abort();
				}
			}
		}

		#endregion
	}
}
