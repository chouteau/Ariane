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
		private SynchronizedCollection<Person> m_PersonCollection;

		public MessageCollector()
		{
			m_PersonCollection = new SynchronizedCollection<Person>();
		}

		public static MessageCollector Current
		{
			get
			{
				return m_LazyInstance.Value;
			}
		}

		public static void AddPerson(Person person)
		{
			lock(Current.m_PersonCollection.SyncRoot)
			{
				Current.m_PersonCollection.Add(person);
			}
		}

		public static int PersonCount
		{
			get
			{
				var result = 0;
				lock(Current.m_PersonCollection.SyncRoot)
				{
					result = Current.m_PersonCollection.Count;
				}
				return result;
			}
		}
	}
}
