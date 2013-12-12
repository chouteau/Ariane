using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IServiceBus
	{
		/// <summary>
		/// Register queues
		/// </summary>
		IFluentRegister Register { get; }
		/// <summary>
		/// Start reading for item in queues and process
		/// </summary>
		void StartReading();
		/// <summary>
		/// Stop reading queues
		/// </summary>
		void StopReading();
		/// <summary>
		/// Pause reading queues
		/// </summary>
		void PauseReading();
		/// <summary>
		/// Send typed object in queue
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		/// <param name="label"></param>
		void Send<T>(string queueName, T body, string label = null);
		/// <summary>
		/// Process message directly synchronously for unit tests 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queueName"></param>
		/// <param name="body"></param>
		void SyncProcess<T>(string queueName, T body, string label = null);
		/// <summary>
		/// Create dynamic message with name
		/// </summary>
		/// <param name="messageName"></param>
		/// <returns></returns>
		dynamic CreateMessage(string messageName);
	}
}
