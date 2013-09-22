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
			return new QueueProviders.InMemoryMessageQueue(queueName);
		}
	}
}
