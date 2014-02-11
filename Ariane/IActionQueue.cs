using System;
namespace Ariane
{
	internal interface IActionQueue : IDisposable
	{
		void Add(Action action);
		void Stop();
	}
}
