using System;
namespace Ariane
{
	public interface IMessageReader : IDisposable
	{
		void Pause();
		void Start(IMessageQueue queue);
		void Stop();
		void Terminate();
		void Dispose();
	}
}
