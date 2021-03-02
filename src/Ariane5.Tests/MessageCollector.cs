using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Tests
{
	public class MessageCollector
	{
		private List<Person> m_PersonList;

		public MessageCollector()
		{
			m_PersonList = new List<Person>();
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

		public IEnumerable<Person> GetList()
        {
			return m_PersonList;
        }

		public void AddPerson(Person person)
		{
			m_PersonList.Add(person);
		}
	}
}
