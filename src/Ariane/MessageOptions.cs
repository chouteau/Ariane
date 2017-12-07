using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class MessageOptions
	{
		public MessageOptions()
		{
			Priority = 0;
		}
		public string Label { get; set; }
		public int Priority { get; set; }
		public TimeSpan? TimeToLive { get; set; }
		public DateTime? ScheduledEnqueueTimeUtc { get; set; }
	}
}
