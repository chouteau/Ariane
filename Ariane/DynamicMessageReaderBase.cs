using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Dynamic message reader base
	/// </summary>
	public abstract class DynamicMessageReaderBase : MessageReaderBase<System.Dynamic.ExpandoObject>
	{
		/// <summary>
		/// Process dynamic message
		/// </summary>
		/// <param name="message"></param>
		public override void ProcessMessage(System.Dynamic.ExpandoObject message)
		{
			if (!message.IsDynamicPropertyExists("MessageName"))
			{
				return;
			}

			var d = message as dynamic;
			Process(d.MessageName as string, d);
		}

		/// <summary>
		/// Process dynamic named message
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="message"></param>
		protected abstract void Process(string messageName, dynamic message);
	}
}
