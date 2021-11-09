using Ariane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
    public static class Extensions
    {
		/// <summary>
		/// Add MSMQ Reader
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="typeReader"></param>
		/// <returns></returns>
		public static IRegister AddMSMQReader(this IRegister register, string queueName, Type typeReader)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		public static IRegister AddMSMQReader<T>(this IRegister register, string queueName)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeof(T),
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}


		/// <summary>
		/// Add MSMQ Writer
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public static IRegister AddMSMQWriter(this IRegister register, string queueName)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

	}
}
