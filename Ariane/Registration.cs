using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	internal class Registration
	{
		public Registration()
		{
			Medium = new Lazy<IMedium>(InitializeMedium, true);
			Queue = new Lazy<IMessageQueue>(InitializeMessageQueue, true);
			Reader = new Lazy<IMessageDispatcher>(InitializeMessageReader, true);
			MessageSubscriberTypeList = new List<Type>();
			AnonymousMessageSubscriberList = new List<object>();
		}

		public string QueueName { get; set; }
		public Lazy<IMessageDispatcher> Reader { get; set; }
		public Type TypeMedium { get; set; }
		public Lazy<IMedium> Medium { get; set; }
		public Lazy<IMessageQueue> Queue { get; set; }
		public IList<Type> MessageSubscriberTypeList { get; set; }
		public IList<object> AnonymousMessageSubscriberList { get; set; }

		private IMedium InitializeMedium()
		{
			var result = (IMedium)GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeMedium);
			return result;
		}

		private IMessageQueue InitializeMessageQueue()
		{
			var result = Medium.Value.CreateMessageQueue(QueueName);
			return result;
		}

		private IMessageDispatcher InitializeMessageReader()
		{
			var messageSubscriber = MessageSubscriberTypeList.FirstOrDefault();
			if (messageSubscriber == null)
			{
				return null;
			}
			var dispatcher = CreateMessageDispatcher(messageSubscriber);
			dispatcher.AddMessageSubscriberTypeList(MessageSubscriberTypeList);
			dispatcher.AddMessageSubscriberList(AnonymousMessageSubscriberList);
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

		private IMessageDispatcher CreateMessageDispatcher(Type messageSubscriber)
		{
			var messageType = messageSubscriber.BaseType;
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
