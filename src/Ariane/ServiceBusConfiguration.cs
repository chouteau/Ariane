using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Configuration of Service Bus
	/// </summary>
	public class ServiceBusConfiguration
	{
		public ServiceBusConfiguration()
		{
		}
		/// <summary>
		/// Resolver 
		/// </summary>
		public IDependencyResolver DependencyResolver { get; set; }
		/// <summary>
		/// Logger
		/// </summary>
		public ILogger Logger { get; set; }
		/// <summary>
		/// Unique name of topics for test
		/// </summary>
		public string UniqueTopicNameForTest { get; set; }
	}
}
