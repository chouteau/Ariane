using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IServiceBus
	{
		IFluentRegister Register { get; }
		void StartReading();
		void StopReading();
		void PauseReading();
		void Send(string queueName, object body, string label = null);
	}
}
