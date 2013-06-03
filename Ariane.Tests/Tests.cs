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
			m_Bus.RegisterReadersFromConfig();
			m_Bus.RegisterReader("test.memory2", typeof(PersonMessageReader));

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
