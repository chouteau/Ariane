using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace Ariane.Tests
{
	[NUnit.Framework.TestFixture]
	public class ActionQueueTests
	{
		[NUnit.Framework.Test]
		public void Add_Action_From_MultiThreads()
		{
			var actionQueue = new Ariane.ActionQueue();
			actionQueue.Timeout = 1;

			int endProcess = 0;
			Action write = () => 
			{ 
				Console.WriteLine("{0}|{1}|{2}", Thread.CurrentThread.ManagedThreadId, endProcess, DateTime.Now);
				Interlocked.Add(ref endProcess, 1);
			};

			var threadPool = new List<System.Threading.Thread>();
			for (int i = 0; i < 5; i++)
			{
				var t = new Thread((target) =>
				{
					for (int j = 0; j < 1000; j++)
					{
						actionQueue.Add(write);
					}
				});
				System.Threading.Thread.Sleep(1500);
				threadPool.Add(t);
				t.Start(actionQueue);
			}

			while(endProcess != 5 * 1000)
			{
				System.Threading.Thread.Sleep(100);
			}

			actionQueue.Dispose();
		}

	}
}
