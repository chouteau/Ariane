using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Queue in memory
	/// </summary>
	public class InMemoryMedium : IMedium
	{
		/// <summary>
		/// Create queue
		/// </summary>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public virtual IMessageQueue CreateMessageQueue(string queueName)
		{
			return new QueueProviders.InMemoryMessageQueue(queueName);
		}
	}
}
