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
	}
}
