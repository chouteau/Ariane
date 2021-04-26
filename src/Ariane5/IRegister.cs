using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	/// <summary>
	/// Fluent registration for queues
	/// </summary>
	public interface IRegister
	{
		/// <summary>
		/// Add with Settings
		/// </summary>
		/// <param name="queueSetting"></param>
		/// <returns></returns>
		IRegister AddQueue(QueueSetting queueSetting);
		/// <summary>
		/// Return the list of name for registered queues
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> GetRegisteredQueues();
		/// <summary>
		/// 
		/// </summary>
		Exception ConfigurationException { get; }
	}
}
