using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	internal class AnonymousMessageSubscriber<T> : MessageReaderBase<T>
	{
		private Action<T> m_Predicate;

		public AnonymousMessageSubscriber(Action<T> predicate)
		{
			m_Predicate = predicate;
		}

		public override void ProcessMessage(T message)
		{
			m_Predicate(message);
		}
	}
}
