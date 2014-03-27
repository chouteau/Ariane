using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public static class RegisterExtensions
	{
		/// <summary>
		/// Add memory reader
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="typeReader"></param>
		/// <returns></returns>
		public static IFluentRegister AddMemoryReader(this IFluentRegister register, string queueName, Type typeReader)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		/// <summary>
		/// Add anonymous memory reader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static IFluentRegister AddMemoryReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			if (queueName == null || register == null || predicate == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue<T>(queueSetting, predicate);
			return register;
		}

		/// <summary>
		/// Add memory writer
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public static IFluentRegister AddMemoryWriter(this IFluentRegister register, string queueName)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		/// <summary>
		/// Add MSMQ Reader
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="typeReader"></param>
		/// <returns></returns>
		public static IFluentRegister AddMSMQReader(this IFluentRegister register, string queueName, Type typeReader)
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

		/// <summary>
		/// Add anonymous MSMQ Reader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static IFluentRegister AddMSMQReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			if (queueName == null || register == null || predicate == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(MSMQMedium),
			};
			register.AddQueue<T>(queueSetting, predicate);
			return register;
		}

		/// <summary>
		/// Add MSMQ Writer
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public static IFluentRegister AddMSMQWriter(this IFluentRegister register, string queueName)
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

		/// <summary>
		/// Add File Reader
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="typeReader"></param>
		/// <returns></returns>
		public static IFluentRegister AddFileReader(this IFluentRegister register, string queueName, Type typeReader)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeReader,
				TypeMedium = typeof(FileMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}

		/// <summary>
		/// Add anonymous File Reader
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static IFluentRegister AddFileReader<T>(this IFluentRegister register, string queueName, Action<T> predicate)
		{
			if (queueName == null || register == null || predicate == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeMedium = typeof(FileMedium),
			};
			register.AddQueue(queueSetting, predicate);
			return register;
		}

		/// <summary>
		/// Add File Writer
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public static IFluentRegister AddFileWriter(this IFluentRegister register, string queueName)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

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
