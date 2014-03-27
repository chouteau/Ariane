using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Represent the type of queue
	/// </summary>
	public interface IMedium
	{
		/// <summary>
		/// Create message queue
		/// </summary>
		/// <param name="queueName"></param>
		/// <returns></returns>
		IMessageQueue CreateMessageQueue(string queueName);
	}
}
