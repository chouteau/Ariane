using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ariane
{
	/// <summary>
	/// Represent a generic queue
	/// </summary>
	public interface IMessageQueue
	{
		/// <summary>
		/// The name of queue
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Name of topic for multiple subscribers
		/// </summary>
		string SubscriptionName { get; }
		/// <summary>
		/// Receive syncrhonized message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		Task<T> ReceiveAsync<T>();
		/// <summary>
		/// Start begin receive async message
		/// </summary>
		/// <returns></returns>
		IAsyncResult BeginReceive();
		/// <summary>
		/// End receive async message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <returns></returns>
		T EndReceive<T>(IAsyncResult result);
		/// <summary>
		/// Reset receive
		/// </summary>
		void Reset();
		/// <summary>
		/// Send message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message"></param>
		void Send<T>(Message<T> message);
		/// <summary>
		/// Timeout between read messages
		/// </summary>
		int? Timeout { get; }
		/// <summary>
		/// Called when timeout raised
		/// </summary>
		void SetTimeout();
	}
}
