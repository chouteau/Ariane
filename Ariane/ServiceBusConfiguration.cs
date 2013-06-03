using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class ServiceBusConfiguration
	{
		public IDependencyResolver DependencyResolver { get; set; }
		public ILogger Logger { get; set; }
	}
}
