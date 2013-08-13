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
			var result = new System.Collections.Queue();
			return new InMemoryMessageQueueWrapper(result, queueName);
		}

		public virtual IMessage CreateMessage()
		{
			return new InMemoryMessage();
		}

	}
}
