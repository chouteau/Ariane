using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public abstract class MessageReaderBase<T>
	{
		public abstract void ProcessMessage(T message);
	}
}
