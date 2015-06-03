using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class ConfigFileManualStartTests
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
			m_Bus = new Ariane.BusManager();
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
		public void Msmq_Without_AutoStart()
		{
			var person = Person.CreateTestPerson();
			m_Bus.Send("test2.msmq", person);

			System.Threading.Thread.Sleep(2 * 1000);

			Check.That(person.IsProcessed).IsFalse();

			m_Bus.StartReading();
		}

	}
}
