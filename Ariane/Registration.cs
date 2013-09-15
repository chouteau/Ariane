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
			return (IMedium)GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeMedium);
		}

		private IMessageQueue InitializeMessageQueue()
		{
			return Medium.Value.CreateMessageQueue(QueueName);
		}

		private IMessageReader InitializeMessageReader()
		{
			var messageSubscriber = MessageSubscriberList.First();
			var messageType = messageSubscriber.BaseType.GetGenericArguments()[0];
			var baseType = typeof(MessageDispatcher<>);
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
