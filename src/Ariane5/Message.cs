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
		public Message()
		{
			Recoverable = true;
		}
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
		/// Priority of message, 0 = normal, 1 = high, 2 = hightest, 3 = veryhigh
		/// </summary>
		public int Priority { get; set; }
		/// <summary>
		/// Message expire at UTC Date
		/// </summary>
		public TimeSpan? TimeToLive { get; set; }
		/// <summary>
		/// Gets or sets the date and time in UTC at which the message will be enqueued.
		/// This property returns the time in UTC; when setting the property, the supplied
		/// DateTime value must also be in UTC.
		/// </summary>
		public DateTime? ScheduledEnqueueTimeUtc { get; set; }

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
		}
	}
}
