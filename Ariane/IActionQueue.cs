using System;
namespace Ariane
{
	public interface IActionQueue
	{
		void Add(Action action);
		void Dispose();
		void Start();
	}
}
