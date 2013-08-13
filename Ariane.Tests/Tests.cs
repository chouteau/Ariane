using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public class Tests 
	{
		public static void Main()
		{
			var bus = new BusManager();
			var configFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Tests).Assembly.Location), "ariane.config");

			bus.Register
				.AddFromConfig(configFileName)
				.AddQueue(new QueueSetting()
				{
					Name = "test.memory2",
					TypeReader = typeof(PersonMessageReader)
				})
				.AddMemoryReader("test.memory3", typeof(PersonMessageReader));
				

			bus.StartReading();

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				bus.Send("test.memory", person);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				bus.Send("test.msmq", person);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				bus.Send("test.memory2", person);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				bus.Send("test.memory3", person);
			} 

			for (int i = 0; i < 100; i++)
			{
				bus.Send("dynamic.msmq", new { id = i, test = Guid.NewGuid().ToString() });
			}

			Console.Read();

			bus.StopReading();
			bus.Dispose();
		}
	}
}
