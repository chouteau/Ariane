using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Azure
{
	public class GlobalConfiguration
	{
		private static Lazy<GlobalConfiguration> m_LazyConfiguration = new Lazy<GlobalConfiguration>(() =>
		{
			var result = new GlobalConfiguration();
			return result;
		}, true);

		private GlobalConfiguration()
		{

		}

		public static GlobalConfiguration Current
		{
			get
			{
				return m_LazyConfiguration.Value;
			}
		}

		public string DefaultAzureConnectionString { get; set; }
	}
}
