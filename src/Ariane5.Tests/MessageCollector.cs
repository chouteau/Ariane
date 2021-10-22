using Microsoft.Extensions.Logging;
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
		private System.Threading.ManualResetEvent m_ManualResetEvent;
		private ILogger m_Logger;

		public MessageCollector(ILogger<MessageCollector> logger)
		{
			m_PersonList = new List<Person>();
			m_ManualResetEvent = new System.Threading.ManualResetEvent(false);
			m_Logger = logger;
		}

		public int Count
		{
			get
			{
				return m_PersonList.Count;
			}
		}

		public void Reset()
		{
			m_PersonList.Clear();
			m_ManualResetEvent.Reset();
		}

		public IEnumerable<Person> GetList()
        {
			return m_PersonList;
        }

		public void AddPerson(Person person)
		{
			m_PersonList.Add(person);
			if (m_PersonList.Count == 1)
            {
				m_ManualResetEvent.Set();
			}
		}

		public async Task WaitForReceiveMessage(int millisecond)
        {
			var success = m_ManualResetEvent.WaitOne(10 * 1000);
			if (!success)
            {
				m_Logger.LogWarning("Timeout detected");
            }
			await Task.Delay(millisecond);
		}
	}
}
