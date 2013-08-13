using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests.Server
{
	class Program 
	{
		static void Main(string[] args)
		{
			var bus = new BusManager();
			var configFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "ariane.config");

			bus.Register.AddFromConfig(configFileName);

			bus.StartReading();

			Console.ReadKey();

			bus.StopReading();

		}
	}
}
