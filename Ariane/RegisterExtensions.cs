using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public static class RegisterExtensions
	{
		public static IFluentRegister AddMemoryReader(this IFluentRegister register, string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IFluentRegister AddMemoryReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue<T>(queueSetting, predicate);
			return register;
		}

		public static IFluentRegister AddMemoryWriter(this IFluentRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IFluentRegister AddMSMQReader(this IFluentRegister register, string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IFluentRegister AddMSMQReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue<T>(queueSetting, predicate);
			return register;
		}

		public static IFluentRegister AddMSMQWriter(this IFluentRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IFluentRegister AddFileReader(this IFluentRegister register, string queueName, Type typeReader)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(FileMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IFluentRegister AddFileReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(FileMedium),
			};
			register.AddQueue(queueSetting, predicate);
			return register;
		}

		public static IFluentRegister AddFileWriter(this IFluentRegister register, string queueName)
		{
			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(FileMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

	}
}
