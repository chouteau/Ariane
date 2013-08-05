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
			var m_Bus = new BusManager();
			var configFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Tests).Assembly.Location), "ariane.config");

			m_Bus.Register
				.AddFromConfig(configFileName)
				.AddQueue(new QueueSetting() 
				{
					Name = "test.memory2", 
					TypeReader = typeof(PersonMessageReader)
				})
				.AddMemoryReader("test.memory3", typeof(PersonMessageReader));

			m_Bus.StartReading();

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				m_Bus.Send("test.memory", person);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				m_Bus.Send("test.msmq", person);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				m_Bus.Send("test.memory2", person);
			}

			for (int i = 0; i < 100; i++)
			{
				var person = new Person();
				person.FirsName = i.ToString();
				person.LastName = Guid.NewGuid().ToString();

				m_Bus.Send("test.memory3", person);
			}

			Console.Read();

			m_Bus.StopReading();
			m_Bus.Dispose();
		}
	}
}
