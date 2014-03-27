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
		/// Starts processing the associated message
		/// </summary>
		/// <param name="message"></param>
		public abstract void ProcessMessage(T message);
	}
}
