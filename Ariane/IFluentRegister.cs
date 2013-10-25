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
		IFluentRegister AddQueue<T>(QueueSetting settings, Action<T> predicate);
		IFluentRegister AddMemoryReader(string queueName, Type typeReader);
		IFluentRegister AddMemoryReader<T>(string queueName, Action<T> predicate);
		IFluentRegister AddMemoryWriter(string queueName);
		IFluentRegister AddMSMQReader(string queueName, Type typeReader);
		IFluentRegister AddMSMQReader<T>(string queueName, Action<T> predicate);
		IFluentRegister AddMSMQWriter(string queueName);
		IFluentRegister AddFileReader(string queueName, Type typeReader);
		IFluentRegister AddFileReader<T>(string queueName, Action<T> predicate);
		IFluentRegister AddFileWriter(string queueName);
	}
}
