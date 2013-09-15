using System;
using System.Collections.Generic;
namespace Ariane
{
	public interface IMessageReader : IDisposable
	{
		void AddMessageSubscribers(IList<Type> messageSubscriber);
		void Pause();
		void Start(IMessageQueue queue);
		void Stop();
		void Terminate();
		void Dispose();
	}
}
