using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	/// <summary>
	/// Service Bus
	/// </summary>
	public interface IServiceBus 
	{
		/// <summary>
		/// Start reading for item in queues and process
		/// </summary>
		Task StartReadingAsync();

		/// <summary>
		/// Start reading specific queue 
		/// </summary>
		/// <param name="queueName"></param>
		Task StartReadingAsync(string queueName);

		Task<IEnumerable<T>> ReceiveAsync<T>(string queueName, int count);
		/// <summary>
		/// Receive list of item from queue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="count"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		Task<IEnumerable<T>> ReceiveAsync<T>(string queueName, int count, int timeoutInMillisecond);

		/// <summary>
		/// Stop reading queues
		/// </summary>
		Task StopReadingAsync();

		/// <summary>
		/// Stop reading specific queue
		/// </summary>
		/// <param name="queueName"></param>
		Task StopReadingAsync(string queueName);

		/// <summary>
		/// Send typed object in queue with options
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		/// <param name="options"></param>
		Task SendAsync<T>(string queueName, T body, MessageOptions options = null);

		/// <summary>
		/// Create dynamic message with name
		/// </summary>
		/// <param name="messageName"></param>
		/// <returns></returns>
		dynamic CreateMessage(string messageName);

		IEnumerable<string> GetRegisteredQueueList();
    }
}
