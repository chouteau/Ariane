using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

namespace ArianeAzureTopicReaderConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			var bus = new BusManager();
			var cs = System.Configuration.ConfigurationManager.ConnectionStrings["AzureQueue"].ConnectionString;
			Ariane.Azure.GlobalConfiguration.Current.DefaultAzureConnectionString = cs;
			bus.Register.AddAzureTopicReader("stress.person.topic", System.Environment.MachineName, typeof(PersonMessageReader));

			bus.StartReading();

			Console.Read();

			bus.Dispose();

		}
	}
}
