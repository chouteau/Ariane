using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

namespace ArianeAzureQueueSenderConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			var bus = new BusManager();

			var cs = System.Configuration.ConfigurationManager.ConnectionStrings["AzureQueue"].ConnectionString;
			Ariane.Azure.GlobalConfiguration.Current.DefaultAzureConnectionString = cs;

			bus.Register.AddAzureQueueWriter("stress.person2");

			var loop = new int[1000];
			var count = 0;
			Parallel.ForEach(loop, (i) =>
			{
				var person = new Person();
				person.Id = count;
				bus.Send("stress.person2", person);
				count++;
				Console.WriteLine($"{count} items sent");
			});

			bus.Dispose();
		}
	}
}
