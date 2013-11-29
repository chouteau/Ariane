using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public abstract class DynamicMessageReaderBase : MessageReaderBase<System.Dynamic.ExpandoObject>
	{
		public override void ProcessMessage(System.Dynamic.ExpandoObject message)
		{
			if (!message.IsDynamicPropertyExists("MessageName"))
			{
				return;
			}

			var d = message as dynamic;
			Process(d.MessageName as string, d);
		}

		protected abstract void Process(string messageName, dynamic message);
	}
}
