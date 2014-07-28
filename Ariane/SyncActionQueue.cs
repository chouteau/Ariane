using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class SyncActionQueue : IActionQueue
	{
		public void Add(Action action)
		{
			action();
		}

		public void Stop()
		{
			
		}

		public void Dispose()
		{
			
		}
	}
}
