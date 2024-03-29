﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NFluent;

namespace Ariane.Tests
{
	[TestClass]
	public class AzureQueueTests
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			ServiceProvider = RootTests.Initialize(services =>
			{
				services.ConfigureArianeAzure();
				services.ConfigureAriane(register =>
				{
					register.AddAzureQueueReader<PersonMessageReader>("test.azure2");
					register.AddQueue(new QueueSetting()
					{
						AutoStartReading = false,
						Name = "test.azure3",
						TypeMedium = typeof(AzureQueueMedium),
						TypeReader = typeof(PersonMessageReader)
					});
				});
			});
		}

		private static IServiceProvider ServiceProvider { get; set; }

		[TestMethod]
		public async Task Send_And_Receive_Person_Queue()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var messageCollector = ServiceProvider.GetRequiredService<MessageCollector>();
			messageCollector.Reset();

			var person = new Person();
			person.FirstName = Guid.NewGuid().ToString();
			person.LastName = Guid.NewGuid().ToString();

			await bus.SendAsync("test.azure2", person);

			await bus.StartReadingAsync();

			await messageCollector.WaitForReceiveMessage(5 * 1000);
			Check.That(messageCollector.Count).IsStrictlyGreaterThan(0);

			await bus.StopReadingAsync();
		}

		[TestMethod]
		public async Task Write_In_Azure_Queue_And_Receive()
		{
			var bus = ServiceProvider.GetRequiredService<IServiceBus>();

			var person = Person.CreateTestPerson();
			await bus.SendAsync("test.azure3", person);

			await Task.Delay(5 * 1000);

			var list = await bus.ReceiveAsync<Person>("test.azure3", 10);

			Check.That(list).IsNotNull();
			Check.That(list.Count()).IsStrictlyGreaterThan(0);
		}

	}
}
