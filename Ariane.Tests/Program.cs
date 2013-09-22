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
			// AzureClient();
			// DynamicMessage();
			DefaultClient();
		}

		public static void DynamicMessage()
		{
			var bus = new BusManager();
			bus.Register.AddQueue(new QueueSetting()
			{
				Name = "inherited.dynamic"
				, TypeMedium = typeof(Ariane.InMemoryMedium)
				, TypeReader = typeof(DynamicReader)
			});

			dynamic message = new System.Dynamic.ExpandoObject();
			message.Id = Guid.NewGuid().ToString();
			bus.Send("inherited.dynamic", message);

			bus.StartReading();

			System.Threading.Thread.Sleep(5 * 1000);

			bus.StopReading();
		}

		public static void AzureClient()
		{
			var azureTest = new AzureTests();
			azureTest.Send_Person();
			// azureTest.Receive_Person();
			Console.WriteLine("messageSent");
		}

		public static void DefaultClient()
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
				.AddMemoryReader("dynamic.memory", typeof(DynamicMemoryMessageReader))
				.AddFileWriter("test.file");
				

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
				bus.Send("expando.msmq", message);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				bus.Send("test.file", person);
			}

			Console.Read();

			bus.StopReading();
			bus.Dispose();
		}
	}
}
