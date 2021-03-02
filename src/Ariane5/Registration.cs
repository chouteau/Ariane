using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	internal class Registration 
	{
		private Lazy<IMessageDispatcher> m_LazyReader { get; set; }
		private Lazy<IMedium> m_LazyMedium { get; set; }
		private Lazy<IMessageQueue> m_LazyQueue { get; set; }
		private IServiceProvider m_ServiceProvider;

		public Registration()
		{
			m_LazyMedium = new Lazy<IMedium>(InitializeMedium, true);
			m_LazyQueue = new Lazy<IMessageQueue>(InitializeMessageQueue, true);
			m_LazyReader = new Lazy<IMessageDispatcher>(InitializeMessageReader, true);
			MessageSubscriberTypeList = new List<Type>();
			AnonymousMessageSubscriberList = new List<object>();
			AutoStartReading = true;
		}

		protected ILogger Logger { get; }

		public string QueueName { get; set; }
		public string TopicName { get; set; }

		public bool IsReaderCreated
		{

			get
			{
				return m_LazyReader.IsValueCreated;
			}
		}

		public IMessageDispatcher Reader 
		{
			get
			{
				return m_LazyReader.Value;
			}
		}
		public IMedium Medium 
		{
			get
			{
				return m_LazyMedium.Value;
			}
		}
		public IMessageQueue Queue 
		{
			get
			{
				return m_LazyQueue.Value;
			}
		}
		public Type TypeMedium { get; set; }
		public IList<Type> MessageSubscriberTypeList { get; set; }
		public IList<object> AnonymousMessageSubscriberList { get; set; }
		public bool AutoStartReading { get; set; }

		private IMedium InitializeMedium()
		{
			using var scope = m_ServiceProvider.CreateScope();
			var instance = (IMedium)ActivatorUtilities.CreateInstance(scope.ServiceProvider, TypeMedium);
			return instance;
		}

		private IMessageQueue InitializeMessageQueue()
		{
			IMessageQueue result = null;
			try
			{
				result = Medium.CreateMessageQueue(QueueName, TopicName);
			}
			catch(Exception ex)
			{
				ex.Data.Add("QueueName", QueueName);
				Logger.LogError(ex, ex.Message);
			}
			return result;
		}

		private IMessageDispatcher InitializeMessageReader()
		{
			var dispatcher = CreateMessageDispatcher();
			if (dispatcher != null)
			{
				dispatcher.AddMessageSubscriberTypeList(MessageSubscriberTypeList);
				dispatcher.AddMessageSubscriberList(AnonymousMessageSubscriberList);
			}
			return dispatcher;
		}

		public void AddSubscriberType(Type messageSubscriber)
		{
			if (messageSubscriber == null)
			{
				return;
			}
			if (MessageSubscriberTypeList.Contains(messageSubscriber))
			{
				return;
			}
			MessageSubscriberTypeList.Add(messageSubscriber);
		}

		public void AddSubscriber(object subscriber)
		{
			AnonymousMessageSubscriberList.Add(subscriber);
		}

		private IMessageDispatcher CreateMessageDispatcher()
		{
			Type messageType = null;
			var messageSubscriber = MessageSubscriberTypeList.FirstOrDefault();
			if (messageSubscriber == null)
			{
				if (AnonymousMessageSubscriberList.Count > 0)
				{
					var f = AnonymousMessageSubscriberList.First();
					messageType = f.GetType();
				}
			}
			else
			{
				messageType = messageSubscriber.BaseType;
			}

			if (messageType == null)
			{
				return null;
			}

			var baseType = typeof(MessageDispatcher<>);
			while (true)
			{
				var arguments = messageType.GetGenericArguments();
				if (arguments.Count() == 1
					|| messageType == baseType)
				{
					messageType = arguments[0];
					break;
				}
				messageType = messageType.BaseType;
			}
			var typeReader = baseType.MakeGenericType(messageType);
			using var scope = m_ServiceProvider.CreateScope();
			var result = (IMessageDispatcher)ActivatorUtilities.CreateInstance(scope.ServiceProvider, typeReader);
			return result;
		}
	}
}
