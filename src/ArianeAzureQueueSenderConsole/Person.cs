using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArianeAzureQueueSenderConsole
{
	public class Person
	{
		public Person()
		{
			FirsName = Guid.NewGuid().ToString();
			LastName = Guid.NewGuid().ToString();
		}

		public string FirsName { get; set; }
		public string LastName { get; set; }
	}
}
