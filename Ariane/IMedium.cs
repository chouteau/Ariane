using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IMedium
	{
		IMessageQueue CreateMessageQueue(string queueName);
		IMessage CreateMessage();
	}
}
