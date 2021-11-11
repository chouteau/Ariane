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
			FlushReceivedMessageToDiskBeforeProcess = false;
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
		public string ConnectionStringName { get; set; }
		/// <summary>
		/// Topic name for multiple subscriptions
		/// </summary>
		public string SubscriptionName { get; set; }
		/// <summary>
		/// Flush message to disk before process (warning is slow, for debug purpose)
		/// </summary>
        public bool FlushReceivedMessageToDiskBeforeProcess { get; set; }

        public override string ToString()
        {
			var sb = new StringBuilder();
			sb.AppendLine($"Name : {Name}");
			sb.AppendLine($"TypeReader : {TypeReader}");
			sb.AppendLine($"TypeMedium : {TypeMedium}");
			sb.AppendLine($"SubscriptionName : {SubscriptionName}");
			return base.ToString();
        }

    }
}
