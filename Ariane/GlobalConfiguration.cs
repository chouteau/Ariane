using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public static class GlobalConfiguration
	{
		private static object m_Lock = new object();
		private static Lazy<ServiceBusConfiguration> m_Configuration
			= new Lazy<ServiceBusConfiguration>(() =>
				{
					var config = new ServiceBusConfiguration();
					config.DependencyResolver = new DefaultDependencyResolver();
					config.Logger = new DiagnosticsLogger();
					return config;
				}, true);

		public static ServiceBusConfiguration Configuration 
		{
			get
			{
				return m_Configuration.Value;
			}
		}
	}
}
