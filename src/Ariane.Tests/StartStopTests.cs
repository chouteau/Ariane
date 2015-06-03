using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	[TestClass]
	public class StartStopTests
	{
		[TestInitialize()]
		public void Initialize()
		{
			BusManager = new BusManager();
			BusManager.Register.AddMemoryReader("test.memory3", typeof(PersonMessageReader));

			BusManager.StartReading();
		}

		protected Ariane.IServiceBus BusManager { get; private set; }

		[TestCleanup]
		public void Stop()
		{
			BusManager.StopReading();
		}

		[TestMethod]
		public void Add_Person()
		{
			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			BusManager.Send("test.memory3", person);
		}

	}
}
