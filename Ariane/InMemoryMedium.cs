using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class InMemoryMedium : IMedium
	{
		public InMemoryMedium()
		{
		}

		public virtual IMessageQueue CreateMessageQueue(string queueName)
		{
			return new InMemoryMessageQueueWrapper(queueName);
		}

		public virtual IMessage<T> CreateMessage<T>()
		{
			return new InMemoryMessage<T>();
		}

	}
}
