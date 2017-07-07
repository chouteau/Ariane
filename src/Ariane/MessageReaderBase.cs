using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Generic reader base
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class MessageReaderBase<T>
	{
		/// <summary>
		/// Name of queue 
		/// </summary>
		public virtual string FromQueueName { get; set; }
		/// <summary>
		/// Topic name for multiple subscription
		/// </summary>
		public virtual string FromTopicName { get; set; }
		/// <summary>
		/// Starts processing the associated message
		/// </summary>
		/// <param name="message"></param>
		public abstract void ProcessMessage(T message);
		/// <summary>
		/// Raised when timeout
		/// </summary>
		public virtual void Elapsed()
		{
			// Do nothing
		}
	}
}
