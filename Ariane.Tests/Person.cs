using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests 
{
	public class Person
	{
		public Person()
		{
			IsProcessed = false;
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsProcessed { get; set; }

		public static Person CreateTestPerson()
		{
			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();
			person.IsProcessed = false;
			return person;
		}
	}
}
