using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public static IRegister AddMemoryReader(this IRegister register, string queueName, Type typeReader)
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

		public static IRegister AddMemoryReader<T>(this IRegister register, string queueName)
		{
			if (queueName == null || register == null)
			{
				throw new ArgumentNullException();
			}

			var queueSetting = new QueueSetting()
			{
				Name = queueName,
				TypeReader = typeof(T),
				TypeMedium = typeof(InMemoryMedium),
			};
			register.AddQueue(queueSetting);
			return register;
		}


		/// <summary>
		/// Add memory writer
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public static IRegister AddMemoryWriter(this IRegister register, string queueName)
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
		/// Add File Reader
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <param name="typeReader"></param>
		/// <returns></returns>
		public static IRegister AddFileReader(this IRegister register, string queueName, Type typeReader)
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
		/// Add File Writer
		/// </summary>
		/// <param name="register"></param>
		/// <param name="queueName"></param>
		/// <returns></returns>
		public static IRegister AddFileWriter(this IRegister register, string queueName)
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
