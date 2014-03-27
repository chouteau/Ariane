using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class ConfigFileSyncBusTests
	{
		private static Ariane.IServiceBus m_Bus;

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			var b = new Ariane.BusManager();
			m_Bus = new Ariane.SyncBusManager(b);
			m_Bus.Register.AddFromConfig();
		}
		// Use ClassCleanup to run code after all tests in a class have run
		[ClassCleanup()]
		public static void MyClassCleanup()
		{
			m_Bus.Dispose();
		}
		//
		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public void MyTestInitialize()
		{

		}

		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup()
		{
			// m_Bus.Register.Clear();
		}

		#endregion


		[TestMethod]
		public void Memory()
		{
			var person = Person.CreateTestPerson();
			m_Bus.Send("test.memory", person);

			Check.That(person.IsProcessed).IsTrue();
		}

		[TestMethod]
		public void Msmq()
		{
			var person = Person.CreateTestPerson();
			m_Bus.Send("test.msmq", person);

			Check.That(person.IsProcessed).IsTrue();
		}

		[TestMethod]
		public void Dynamic_Msmq()
		{
			var person = m_Bus.CreateMessage("test");
			person.IsProcessed = false;
			m_Bus.Send("dynamic.msmq", person);

			Assert.AreEqual(true, person.IsProcessed);
		}

		[TestMethod]
		public void Config_File_Name()
		{
			var bus = new Ariane.BusManager();
			var path = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location);
			var configFileName = System.IO.Path.Combine(path.TrimEnd('\\'), "ariane.config");
			bus.Register.AddFromConfig(configFileName);
			var queueList = bus.Register.GetRegisteredQueues();

			Check.That(queueList).IsNotNull();
			Check.That(queueList.Count()).IsGreaterThan(0);
		}

	}
}
