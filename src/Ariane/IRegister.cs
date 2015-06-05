using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Fluent registration for queues
	/// </summary>
	public interface IRegister
	{
		/// <summary>
		/// Add from file
		/// </summary>
		/// <param name="configFileName"></param>
		/// <returns></returns>
		IRegister AddFromConfig(string configFileName = null);
		/// <summary>
		/// Add with Settings
		/// </summary>
		/// <param name="queueSetting"></param>
		/// <returns></returns>
		IRegister AddQueue(QueueSetting queueSetting);
		/// <summary>
		/// Add with settings for anonymous reader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="settings"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		IRegister AddQueue<T>(QueueSetting settings, Action<T> predicate);
		/// <summary>
		/// Clear all registered queues
		/// </summary>
		void Clear();
		/// <summary>
		/// Return the list of name for registered queues
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> GetRegisteredQueues();
	}
}
