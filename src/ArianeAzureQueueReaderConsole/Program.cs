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
			var cs = System.Configuration.ConfigurationManager.ConnectionStrings["AzureQueue"].ConnectionString;
			Ariane.Azure.GlobalConfiguration.Current.DefaultAzureConnectionString = cs;
			bus.Register.AddAzureQueueReader("stress.person2", typeof(PersonMessageReader));

			bus.StartReading();

			Console.Read();

			bus.Dispose();
		}
	}
}
