﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ariane;

namespace Ariane.Tests.Server
{
	class Program 
	{
		static void Main(string[] args)
		{
			Ariane.IServiceBus bus = null;
			// bus = DefaultServer();
			// bus = AzureServer();
			bus = FileReaderServer();

			bus.Register.AddFileReader<Person>("test.file", (message) =>
			{
				Console.Write(message.FirstName);
			});

			bus.StartReading();

			Console.ReadKey();

			bus.StopReading();
		}

		static Ariane.IServiceBus AzureServer()
		{
			var bus = new BusManager();
			bus.Register.AddAzureQueueReader("test.azure", typeof(PersonMessageReader));

			return bus;
		}

		static Ariane.IServiceBus DefaultServer()
		{
			var bus = new BusManager();
			var configFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "ariane.config");

			bus.Register.AddFromConfig(configFileName);

			return bus;
		}

		static Ariane.IServiceBus FileReaderServer()
		{
			var bus = new BusManager();
			bus.Register.AddFileReader("test.file", typeof(PersonMessageReader));

			return bus;
		}
	}
}
