using System;
namespace Ariane
{
	internal interface IActionQueue
	{
		void Add(Action action);
		void Dispose();
		void Start();
	}
}
