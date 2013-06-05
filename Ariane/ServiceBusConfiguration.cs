using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class ServiceBusConfiguration
	{
		public ServiceBusConfiguration()
		{
			ReadingEnable = true;
		}
		public IDependencyResolver DependencyResolver { get; set; }
		public ILogger Logger { get; set; }
		public bool ReadingEnable { get; set; }
	}
}
