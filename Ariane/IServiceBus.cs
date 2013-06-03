using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IServiceBus
	{
		void RegisterReadersFromConfig(string configFileName = null);
		void RegisterReader(string queueName, Type readerType, Type mediumType = null);
		void StartReading();
		void StopReading();
		void PauseReading();
		void Send(string queueName, object body, string label = null);
	}
}
