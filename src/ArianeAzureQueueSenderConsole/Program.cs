﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ariane;

namespace ArianeAzureQueueSenderConsole
{
	class Program
	{
		static void Main(string[] args)
		{
			var bus = new BusManager();
			bus.Register.AddAzureQueueWriter("stress.person");

			int count = 0;
			while(count < 1000)
			{
				var person = new Person();
				person.Id = count;
				bus.Send("stress.person", person);
				count++;
				Console.WriteLine($"{count} items sent");
			}

			bus.Dispose();
		}
	}
}
