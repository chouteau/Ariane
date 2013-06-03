using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class QueueSetting
	{
		public QueueSetting()
		{
			TypeMedium = typeof(InMemoryMedium);
		}
		public string Name { get; set; }
		public Type TypeReader { get; set; }
		public Type TypeMedium { get; set; }
	}
}
