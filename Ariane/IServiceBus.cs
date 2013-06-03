using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IServiceBus
	{
		void RegisterQueuesFromConfig(string configFileName = null);
		void RegisterQueue(QueueSetting queue);
		void StartReading();
		void StopReading();
		void PauseReading();
		void Send(string queueName, object body, string label = null);
	}
}
