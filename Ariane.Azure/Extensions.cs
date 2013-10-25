using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	public static class Extensions
	{
		public static IFluentRegister AddAzureReader(this IFluentRegister register, string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(AzureMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IFluentRegister AddAzureReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(AzureMedium),
			};
			register.AddQueue(queueSetting, predicate);
			return register;
		}

		public static IFluentRegister AddAzureWriter(this IFluentRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(AzureMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

	}
}
