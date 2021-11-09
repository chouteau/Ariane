using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ariane.QueueProviders
{
	public class FileMessageQueue : IMessageQueue, IDisposable
	{
		private string m_QueueName;
		private string m_Path;
		private FileSystemWatcher m_FileWatcher;
		private ManualResetEvent m_Event;
		private System.Collections.Generic.Queue<string> m_Queue;

		public FileMessageQueue(string queueName, string path, ILogger logger)
		{
			m_QueueName = queueName;
			m_Path = path;
			m_Event = new ManualResetEvent(false);
			m_Queue = new Queue<string>(1000000);

			if (path.StartsWith("."))
			{
				var directory = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
				path = System.IO.Path.Combine(directory.TrimEnd('\\'), path.TrimStart('.').TrimStart('\\'));
				if (!System.IO.Directory.Exists(path))
				{
					System.IO.Directory.CreateDirectory(path);
				}
			}

			m_FileWatcher = new FileSystemWatcher();
			m_FileWatcher.Path = path;
			m_FileWatcher.NotifyFilter = NotifyFilters.CreationTime;
			m_FileWatcher.Created += (s, arg) =>
			{
				m_Queue.Enqueue(arg.FullPath);
				m_Event.Set();
			};
			m_FileWatcher.Filter = Filter;
			m_FileWatcher.EnableRaisingEvents = true;
		}

		protected ILogger Logger { get; }

		public virtual string Filter
		{
			get
			{
				return string.Format("{0}.*", m_QueueName);
			}
		}

		public int? Timeout
		{
			get
			{
				return 10 * 1000; // 10 seconds
			}
		}

		public void SetTimeout()
		{
			try
			{
				ScanExistingFiles();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, ex.Message);
			}
		}

		#region IMessageQueue Members

		public string Name
		{
			get { return m_QueueName; }
		}
		public string SubscriptionName { get; private set; }

		public async Task<T> ReceiveAsync<T>()
		{
			var fileName = m_Queue.Dequeue();
			T m = default(T);
			var retryCount = 0;
			while (true)
			{
				try
				{
					System.Threading.Thread.Sleep(100);
					var content = System.IO.File.ReadAllText(fileName);
					m = System.Text.Json.JsonSerializer.Deserialize<T>(content);
					System.IO.File.Delete(fileName);
					break;
				}
				catch (System.IO.FileNotFoundException)
				{
					break;
				}
				catch (System.IO.IOException)
				{
					if (retryCount > 4)
					{
						break;
					}
					retryCount++;
					System.Threading.Thread.Sleep(500);
				}
			}
			return await Task.FromResult(m);
		}

		public IAsyncResult BeginReceive()
		{
			if (m_Queue.Count > 0)
			{
				m_Event.Set();
			}

			return new AsyncResult(m_Event);
		}

		public T EndReceive<T>(IAsyncResult result)
		{
			return ReceiveAsync<T>().Result;
		}

		public void Reset()
		{
			m_Event.Reset();
		}

		public void Send<T>(Message<T> message)
		{
			var json = System.Text.Json.JsonSerializer.Serialize(message.Body);
			string fileName = System.IO.Path.Combine(m_Path, string.Format("{0}.{1}", m_QueueName, Guid.NewGuid().ToString()));
			System.IO.File.WriteAllText(fileName, json, Encoding.UTF8);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (m_FileWatcher != null)
			{
				m_FileWatcher.EnableRaisingEvents = false;
				m_FileWatcher.Dispose();
			}
			if (m_Event != null)
			{
				m_Event.Dispose();
			}
			if (m_Queue != null)
			{
				m_Queue.Clear();
				m_Queue = null;
			}
		}

		#endregion

		private void ScanExistingFiles()
		{
			var list = from file in System.IO.Directory.GetFiles(m_Path, Filter, SearchOption.TopDirectoryOnly)
					   let fileInfo = new System.IO.FileInfo(file)
					   orderby fileInfo.CreationTime
					   select file;

			foreach (var file in list)
			{
				m_Queue.Enqueue(file);
			}

			if (m_Queue.Count > 0)
			{
				m_Event.Set();
			}
		}

	}
}
