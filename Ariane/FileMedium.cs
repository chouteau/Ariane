using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class FileMedium : IMedium
	{

		#region IMedium Members

		public IMessageQueue CreateMessageQueue(string queueName)
		{
			string path = System.Configuration.ConfigurationManager.ConnectionStrings[queueName].ConnectionString;
			return new QueueProviders.FileMessageQueue(queueName, path);
		}

		#endregion
	}
}
