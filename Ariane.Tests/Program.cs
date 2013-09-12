using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public class Program 
	{
		public static void Main()
		{
			var bus = new BusManager();
			var configFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "ariane.config");

			bus.Register
				.AddFromConfig(configFileName)
				.AddQueue(new QueueSetting()
				{
					Name = "test.memory2",
					TypeReader = typeof(PersonMessageReader)
				})
				.AddMemoryReader("test.memory3", typeof(PersonMessageReader))
				.AddMemoryReader("dynamic.memory", typeof(DynamicMemoryMessageReader));
				

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
				bus.Send<dynamic>("dynamic.msmq", new { id = i, test = Guid.NewGuid().ToString() });
				bus.Send<dynamic>("dynamic.memory", new { id = i, test = Guid.NewGuid().ToString() });
				dynamic message = new System.Dynamic.ExpandoObject();
				message.id = i;
				message.text = Guid.NewGuid().ToString();
				bus.Send<System.Dynamic.ExpandoObject>("expando.msmq", message);
			}

			Console.Read();

			bus.StopReading();
			bus.Dispose();
		}
	}
}
