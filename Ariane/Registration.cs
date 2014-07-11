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

		public Registration()
		{
			m_LazyMedium = new Lazy<IMedium>(InitializeMedium, true);
			m_LazyQueue = new Lazy<IMessageQueue>(InitializeMessageQueue, true);
			m_LazyReader = new Lazy<IMessageDispatcher>(InitializeMessageReader, true);
			MessageSubscriberTypeList = new List<Type>();
			AnonymousMessageSubscriberList = new List<object>();
			AutoStartReading = true;
		}

		public string QueueName { get; set; }

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
			var result = (IMedium)GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeMedium);
			return result;
		}

		private IMessageQueue InitializeMessageQueue()
		{
			var result = Medium.CreateMessageQueue(QueueName);
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
			var result = (IMessageDispatcher)Activator.CreateInstance(typeReader);
			return result;
		}
	}
}
