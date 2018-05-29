using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

namespace ArianeAzureTopicSenderConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			var bus = new BusManager();

			var cs = System.Configuration.ConfigurationManager.ConnectionStrings["AzureQueue"].ConnectionString;
			Ariane.Azure.GlobalConfiguration.Current.DefaultAzureConnectionString = cs;

			bus.Register.AddAzureTopicWriter("stress.person.topic");

			int count = 0;
			while (count < 1000)
			{
				var person = new Person();
				person.Id = count;
				bus.Send("stress.person.topic", person);
				count++;
				Console.WriteLine($"{count} items sent");
			}

			bus.Dispose();
		}
	}
}
