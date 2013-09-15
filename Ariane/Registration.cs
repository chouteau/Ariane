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
			Reader = new Lazy<IMessageReader>(InitializeMessageReader, true);
			MessageSubscriberList = new List<Type>();
		}

		public string QueueName { get; set; }
		public Lazy<IMessageReader> Reader { get; set; }
		public Type TypeMedium { get; set; }
		public Lazy<IMedium> Medium { get; set; }
		public Lazy<IMessageQueue> Queue { get; set; }
		public IList<Type> MessageSubscriberList { get; set; }

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

		private IMessageReader InitializeMessageReader()
		{
			var messageSubscriber = MessageSubscriberList.FirstOrDefault();
			if (messageSubscriber == null)
			{
				return null;
			}
			Type messageType = messageSubscriber.BaseType;
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
			var result = (IMessageReader)Activator.CreateInstance(typeReader);
			result.AddMessageSubscribers(MessageSubscriberList);
			return result;
		}

		public void AddSubscriber(Type messageSubscriber)
		{
			if (messageSubscriber == null)
			{
				return;
			}
			if (MessageSubscriberList.Contains(messageSubscriber))
			{
				return;
			}
			MessageSubscriberList.Add(messageSubscriber);
		}
	}
}
