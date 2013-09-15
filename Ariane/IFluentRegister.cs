using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public interface IFluentRegister
	{
		IFluentRegister AddFromConfig(string configFileName = null);
		IFluentRegister AddQueue(QueueSetting queueSetting);
		IFluentRegister AddMemoryReader(string queueName, Type typeReader);
		IFluentRegister AddMemoryWriter(string queueName);
		IFluentRegister AddMSMQReader(string queueName, Type typeReader);
		IFluentRegister AddMSMQWriter(string queueName);
		IFluentRegister AddAzureReader(string queueName, Type typeReader);
		IFluentRegister AddAzureWriter(string queueName);
	}
}
