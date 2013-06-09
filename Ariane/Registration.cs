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
			Reader = new Lazy<IMessageReader>(InitializeReader, true);
			Medium = new Lazy<IMedium>(InitializeMedium, true);
			Queue = new Lazy<IMessageQueue>(InitializeMessageQueue, true);
		}

		public string QueueName { get; set; }
		public Type TypeReader { get; set; }
		public Lazy<IMessageReader> Reader { get; set; }
		public Type TypeMedium { get; set; }
		public Lazy<IMedium> Medium { get; set; }
		public Lazy<IMessageQueue> Queue { get; set; }

		private IMessageReader InitializeReader()
		{
			if (TypeReader != null)
			{
				var result = GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeReader);
				return (IMessageReader)result;
			}
			return null;
		}

		private IMedium InitializeMedium()
		{
			return (IMedium)GlobalConfiguration.Configuration.DependencyResolver.GetService(TypeMedium);
		}

		private IMessageQueue InitializeMessageQueue()
		{
			return Medium.Value.CreateMessageQueue(QueueName);
		}
	}
}
