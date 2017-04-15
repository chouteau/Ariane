using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Azure.Tests
{
	public class MessageCollector
	{
		private static Lazy<MessageCollector> m_LazyInstance = new Lazy<MessageCollector>(() =>
		{
			return new MessageCollector();
		}, true);
		private List<Person> m_PersonList;

		public MessageCollector()
		{
			m_PersonList = new List<Person>();
		}

		public static MessageCollector Current
		{
			get
			{
				return m_LazyInstance.Value;
			}
		}

		public int Count
		{
			get
			{
				return m_PersonList.Count;
			}
		}

		public void Clear()
		{
			m_PersonList.Clear();
		}

		public void AddPerson(Person person)
		{
			m_PersonList.Add(person);
		}

	}
}
