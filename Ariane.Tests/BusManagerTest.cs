using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;
using System.Threading;

namespace Ariane.Tests
{
	/// <summary>
	/// Summary description for BusManagerTests
	/// </summary>
	[TestClass]
	public class BusManagerTest
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
			GlobalConfiguration.Configuration.Logger = new Ariane.DiagnosticsLogger();
			m_Bus = new Ariane.BusManager();
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
			m_Bus.Register.Clear();
		}

		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public void MyTestCleanup() 
		{
		}
		
		#endregion

		[TestMethod]
		public void Send_Message_To_Unknown_Queue()
		{
			m_Bus.Send("unknowqueue", new object());
		}

		[TestMethod]
		public void Start_Stop_Bus_Manager_With_Empty_Queue_List()
		{
			m_Bus.Register.AddMemoryReader<Person>("test", (m) =>
				{
					// Do nothing
				});
			m_Bus.StartReading();
			m_Bus.StopReading();
		}

		[TestMethod]
		public void Start_Stop_Bus_Manager_With_Unknown_Queue_Name()
		{
			m_Bus.StartReading("unknown");
			m_Bus.StopReading("unknown");
		}

		[TestMethod]
		public void Sync_Process_With_Unknown_Queue()
		{
			var person = Person.CreateTestPerson();
			m_Bus.SyncProcess("unknown", person);
		}

		[TestMethod]
		public void Start_Stop_Start_Reader()
		{
			var bus = new BusManager();
			bus.Register.AddQueue(new QueueSetting()
			{
				AutoStartReading = false,
				Name = "sendreceivetest",
				TypeMedium = typeof(Ariane.InMemoryMedium),
				TypeReader = typeof(PersonMessageReader)
			});

			var personList = new List<Person>();

			for (int i = 0; i < 50; i++)
			{
				var person = new Person();
				person.FirstName = Guid.NewGuid().ToString();
				person.LastName = Guid.NewGuid().ToString();
				bus.Send("sendreceivetest", person);
				personList.Add(person);
			}

			var processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(0, processedCount);

			bus.StartReading("sendreceivetest");

			System.Threading.Thread.Sleep(2 * 1000);

			bus.StopReading("sendreceivetest");

			processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(50, processedCount);

			foreach (var person in personList)
			{
				person.IsProcessed = false;
				bus.Send("sendreceivetest", person);
			}

			processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(0, processedCount);

			bus.StartReading("sendreceivetest");

			System.Threading.Thread.Sleep(2 * 1000);

			bus.StopReading("sendreceivetest");

			processedCount = personList.Where(i => i.IsProcessed).Count();
			Assert.AreEqual(50, processedCount);

			bus.Dispose();
		}

		[TestMethod]
		public void Same_Registration()
		{
			m_Bus.Register.AddMemoryWriter("same");
			m_Bus.Register.AddMemoryWriter("same");

			Check.That(m_Bus.Register.GetRegisteredQueues().Count()).Equals(1);
		}

		[TestMethod]
		public void Add_Null_Registration()
		{
			try
			{
				m_Bus.Register.AddFileReader(null, null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
			m_Bus.Register.AddFileReader<object>(null, null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
			m_Bus.Register.AddFileWriter(null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
			m_Bus.Register.AddMemoryReader(null, null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
			m_Bus.Register.AddMemoryReader<object>(null, null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
			m_Bus.Register.AddMemoryWriter(null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
				m_Bus.Register.AddMSMQReader(null, null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
				m_Bus.Register.AddMSMQReader<object>(null, null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
				m_Bus.Register.AddMSMQWriter(null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
				m_Bus.Register.AddQueue(null);
			}
			catch(Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}
			try
			{
				m_Bus.Register.AddQueue<object>(null, null);
			}
			catch (Exception ex)
			{
				Check.That(ex).IsInstanceOf<ArgumentNullException>();
			}

			Check.That(m_Bus.Register.GetRegisteredQueues().Count()).IsEqualTo(0);
		}

	}
}
