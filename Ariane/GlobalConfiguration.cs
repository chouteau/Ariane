using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public static class GlobalConfiguration
	{
		private static object m_Lock = new object();
		private static ServiceBusConfiguration m_Configuration;

		public static ServiceBusConfiguration Configuration 
		{
			get
			{
				if (m_Configuration != null)
				{
					return m_Configuration;
				}
				lock (m_Lock)
				{
					if (m_Configuration == null)
					{
						m_Configuration = new ServiceBusConfiguration();
						m_Configuration.DependencyResolver = new DefaultDependencyResolver();
						m_Configuration.Logger = new DiagnosticsLogger();
					}
				}
				return m_Configuration;
			}
		}
	}
}
