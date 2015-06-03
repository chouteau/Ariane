using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	public static class Extensions
	{
		public static IRegister AddAzureReader(this IRegister register, string queueName, Type typeReader)
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

		public static IRegister AddAzureReader<T>(this IRegister register, string queueName, Action<T> predicate)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(AzureMedium),
			};
			register.AddQueue(queueSetting, predicate);
			return register;
		}

		public static IRegister AddAzureWriter(this IRegister register, string queueName)
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
