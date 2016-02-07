using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Service Bus
	/// </summary>
	public interface IServiceBus : IDisposable
	{
		/// <summary>
		/// Register queues
		/// </summary>
		IRegister Register { get; }
		/// <summary>
		/// Start reading for item in queues and process
		/// </summary>
		void StartReading();
		/// <summary>
		/// Start reading specific queue 
		/// </summary>
		/// <param name="queueName"></param>
		void StartReading(string queueName);
		/// <summary>
		/// Receive list of item from queue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="count"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		IEnumerable<T> Receive<T>(string queueName, int count, int timeout);
		/// <summary>
		/// Stop reading queues
		/// </summary>
		void StopReading();
		/// <summary>
		/// Stop reading specific queue
		/// </summary>
		/// <param name="queueName"></param>
		void StopReading(string queueName);
		/// <summary>
		/// Send typed object in queue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		/// <param name="label"></param>
		/// <param name="priority"></param>
		void Send<T>(string queueName, T body, string label = null, int priority = 0);
		/// <summary>
		/// Process message directly synchronously for unit tests 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		/// <param name="label"></param>
		/// <param name="priority"></param>
		void SyncProcess<T>(string queueName, T body, string label = null, int priority = 0);
		/// <summary>
		/// Create dynamic message with name
		/// </summary>
		/// <param name="messageName"></param>
		/// <returns></returns>
		dynamic CreateMessage(string messageName);

		/// <summary>
		/// Replace existing action queue service by an other instance
		/// used by unit test
		/// </summary>
		/// <param name="service"></param>
		void ReplaceActionQueue(IActionQueue service);

		/// <summary>
		/// Dispose service bus
		/// </summary>
		void Dispose();
	}
}
