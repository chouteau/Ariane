using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArianeAzureTopicSenderConsole
{
	public class Person
	{
		public Person()
		{
			FirsName = Guid.NewGuid().ToString();
			LastName = Guid.NewGuid().ToString();
		}

		public int Id { get; set; }
		public string FirsName { get; set; }
		public string LastName { get; set; }

		public override string ToString()
		{
			return $"Person:{Id}";
		}
	}
}
