using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Represents a generic message
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Message<T>
	{
		/// <summary>
		/// Name of queue
		/// </summary>
		public string QueueName { get; set; }
		/// <summary>
		/// Label
		/// </summary>
		public string Label { get; set; }
		/// <summary>
		/// Body
		/// </summary>
		public T Body { get; set; }
		/// <summary>
		/// Indicates whether the message should persist in the queue for a subsequent recovery
		/// </summary>
		public bool Recoverable { get; set; }
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
		}
	}
}
