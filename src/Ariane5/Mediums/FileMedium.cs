using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        public FileMedium(IConfiguration configuration,
			ILogger<FileMedium> logger)
        {
			this.Configuration = configuration;
			this.Logger = logger;
        }
		public bool TopicCompliant => true;

		protected IConfiguration Configuration { get; }
		protected ILogger Logger { get; }

		#region IMedium Members

		/// <summary>
		/// Create message queue compatible with file system
		/// </summary>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public IMessageQueue CreateMessageQueue(QueueSetting queueSetting)
		{
			string path = null;
			path = Configuration.GetConnectionString(queueSetting.ConnectionStringName ?? queueSetting.Name);
			if (path == null)
            {
				throw new Exception(string.Format("Queue {0} not declared in configuration file", queueSetting.Name));
			}
			if (path.StartsWith(".\\"))
			{
				path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
			}
			if (!System.IO.Directory.Exists(path))
			{
				Logger.LogInformation($"try to create folder {path}");
				System.IO.Directory.CreateDirectory(path);
			}
			return new QueueProviders.FileMessageQueue(queueSetting.Name, path, Logger);
		}

		#endregion
	}
}
