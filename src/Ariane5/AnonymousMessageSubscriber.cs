using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	internal class AnonymousMessageSubscriber<T> : MessageReaderBase<T>
	{
		private Func<T, Task> m_Predicate;

		public AnonymousMessageSubscriber(Func<T, Task> predicate)
		{
			m_Predicate = predicate;
		}

		public override async Task ProcessMessageAsync(T message)
		{
			await m_Predicate.Invoke(message);
		}
	}
}
