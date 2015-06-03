using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Represents the queue of type File System
	/// </summary>
	public class FileMedium : IMedium
	{
		#region IMedium Members

		/// <summary>
		/// Create message queue compatible with file system
		/// </summary>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public IMessageQueue CreateMessageQueue(string queueName)
		{
			string path = null;
			try
			{
				path = System.Configuration.ConfigurationManager.ConnectionStrings[queueName].ConnectionString;
			}
			catch
			{
				throw new Exception(string.Format("Queue {0} not declared in configuration file", queueName));
			}
			return new QueueProviders.FileMessageQueue(queueName, path);
		}

		#endregion
	}
}
