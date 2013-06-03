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
			m_Bus.RegisterQueuesFromConfig();
			m_Bus.RegisterQueue(new QueueSetting() 
			{
				Name = "test.memory2", 
				TypeReader = typeof(PersonMessageReader)
			});

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

			Console.Read();
		}
	}
}
