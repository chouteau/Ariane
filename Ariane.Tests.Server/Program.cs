using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests.Server
{
	class Program 
	{
		static void Main(string[] args)
		{
			var m_Bus = new BusManager();
			var configFileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location), "ariane.config");

			m_Bus.Register.AddFromConfig(configFileName);

			m_Bus.StartReading();

			Console.ReadKey();

			m_Bus.StopReading();

		}
	}
}
