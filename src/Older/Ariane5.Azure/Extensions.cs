using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	public static class Extensions
	{
		public static IRegister AddAzureQueueReader(this IRegister register, string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(AzureQueueMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IRegister AddAzureQueueReader<T>(this IRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeof(T),
				TypeMedium = typeof(AzureQueueMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IRegister AddAzureQueueWriter(this IRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(AzureQueueMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IRegister AddAzureTopicReader(this IRegister register, string queueName, string subscriptionName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(AzureTopicMedium),
				SubscriptionName = subscriptionName,
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IRegister AddAzureTopicReader<T>(this IRegister register, string queueName, string subscriptionName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeof(T),
				TypeMedium = typeof(AzureTopicMedium),
				SubscriptionName = subscriptionName,
			};
			register.AddQueue(queueSetting);
			return register;
		}


		public static IRegister AddAzureTopicWriter(this IRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(AzureTopicMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

	}
}
