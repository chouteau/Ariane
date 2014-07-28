using System;
namespace Ariane
{
	public interface IActionQueue : IDisposable
	{
		void Add(Action action);
		void Stop();
	}
}
