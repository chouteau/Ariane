using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Settings of queue
	/// </summary>
	public class QueueSetting
	{
		public QueueSetting()
		{
			TypeMedium = typeof(InMemoryMedium);
			AutoStartReading = true;
		}
		/// <summary>
		/// Name
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Type of reader
		/// </summary>
		public Type TypeReader { get; set; }
		/// <summary>
		/// Type of medium
		/// </summary>
		public Type TypeMedium { get; set; }
		/// <summary>
		/// Whether reading the messages in the queue should start automatically
		/// </summary>
		public bool AutoStartReading { get; set; }
		/// <summary>
		/// Name of connection string
		/// </summary>
		[Obsolete("planified in next version", true)]
		public string ConnectionStringName { get; set; }
	}
}
