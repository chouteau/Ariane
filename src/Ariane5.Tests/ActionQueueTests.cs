using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ariane.Tests
{
	[TestClass]
	public class ActionQueueTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureAriane();
			});
		}

		public static IServiceProvider ServiceProvider { get; set; }


		[TestMethod]
		public async Task Add_Action_From_MultiThreads()
		{
			var actionQueue = ServiceProvider.GetRequiredService<ActionQueue>();

			int endProcess = 0;
			Action write = () => 
			{ 
			 	System.Diagnostics.Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId}|{endProcess}|{DateTime.Now}");
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
				await Task.Delay(1500);
				threadPool.Add(t);
				t.Start(actionQueue);
			}

			while(endProcess != 5 * 1000)
			{
				await Task.Delay(100);
			}
		}

	}
}
