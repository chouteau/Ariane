using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;
using Ariane.Azure;

namespace ArianeAzureQueueReaderConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			var bus = new BusManager();
			bus.Register.AddAzureQueueReader("stress.person", typeof(PersonMessageReader));

			bus.StartReading();

			Console.Read();

			bus.Dispose();
		}
	}
}
