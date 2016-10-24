using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Azure.Tests 
{
	public class Person
	{
		public Person()
		{
			IsProcessed = false;
		}
		public string FirsName { get; set; }
		public string LastName { get; set; }
		public bool IsProcessed { get; set; }
		public string TopicName { get; set; }
	}
}
